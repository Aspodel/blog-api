using AutoMapper;
using BlogApi.Contract;
using BlogApi.Core;
using BlogApi.DTOs;
using BlogApi.Hubs;
using BlogApi.Models;
using BlogApi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace BlogApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationsController : ControllerBase
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly INotificationService _notificationService;
        private readonly IMapper _mapper;
        private readonly IHubContext<NotificationHub> _hubContext;

        public NotificationsController(IHubContext<NotificationHub> hubContext,
                                       INotificationRepository notificationRepository,
                                       INotificationService notificationService,
                                       IMapper mapper)
        {
            _notificationRepository = notificationRepository;
            _notificationService = notificationService;
            _mapper = mapper;
            _hubContext = hubContext;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(CancellationToken cancellationToken = default)
        {
            var notifications = await _notificationRepository.FindAll().ToListAsync(cancellationToken);
            return Ok(_mapper.Map<IEnumerable<NotificationDTO>>(notifications));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var notification = await _notificationRepository.FindByIdAsync(id);
            if (notification is null)
                return NotFound();

            return Ok(_mapper.Map<NotificationDTO>(notification));
        }

        [HttpPost]
        public async Task<IActionResult> Create(NotificationDTO dto, CancellationToken cancellationToken = default)
        {
            var notification = _mapper.Map<Notification>(dto);
            _notificationRepository.Add(notification);
            await _notificationRepository.SaveChangesAsync(cancellationToken);

            NotificationModel model = new()
            {
                SenderId = notification.SenderId,
                Content = notification.Content,
                CreatedAt = notification.CreatedAt
            };
            await _notificationService.SendToUser(notification.RecipientId, model);

            return Ok(_mapper.Map<NotificationDTO>(notification));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(NotificationDTO dto, CancellationToken cancellationToken = default)
        {
            var notification = await _notificationRepository.FindByIdAsync(dto.Id);
            if (notification is null)
                return NotFound();

            _mapper.Map(dto, notification);
            await _notificationRepository.SaveChangesAsync(cancellationToken);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken = default)
        {
            var notification = await _notificationRepository.FindByIdAsync(id);
            if (notification is null)
                return NotFound();

            _notificationRepository.Delete(notification);
            await _notificationRepository.SaveChangesAsync();
            return NoContent();
        }

        [HttpPost("send-to-all")]
        public async Task<IActionResult> SendToAll(NotificationDTO dto, CancellationToken cancellationToken = default)
        {
            var notification = _mapper.Map<Notification>(dto);
            
            _notificationRepository.Add(notification);
            await _notificationRepository.SaveChangesAsync(cancellationToken);

            NotificationModel model = new NotificationModel()
            {
                SenderId = notification.Content,
                Type = notification.Type,
                Content = notification.Content
            };
            await _notificationService.SendToAll(model);

            return Ok();
        }

        [HttpPost("send-to-users")]
        public async Task<IActionResult> SendToUsers(NotificationDTO dto, CancellationToken cancellationToken = default)
        {
            var notification = _mapper.Map<Notification>(dto);
            _notificationRepository.Add(notification);
            await _notificationRepository.SaveChangesAsync(cancellationToken);

            NotificationModel model = new()
            {
                SenderId = notification.Content,
                Type = notification.Type,
                Content = notification.Content
            };
            // await _notificationService.SendToUsers(model);
            return Ok();
        }
    }
}