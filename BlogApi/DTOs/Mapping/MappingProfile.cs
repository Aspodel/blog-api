using AutoMapper;
using BlogApi.Core;
using BlogApi.Core.Entities;
using BlogApi.DTOs.Create;
using System.Linq;

namespace BlogApi.DTOs.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Role, RoleDTO>();
            CreateMap<RoleDTO, Role>()
                .ForMember(d => d.Id, opt => opt.Ignore());


            CreateMap<User, UserDTO>()
                .ForMember(d => d.Roles, opt => opt.MapFrom(s => s.UserRoles.Select(ur => ur.Role!.Name)));
            CreateMap<UserDTO, User>()
                .ForMember(d => d.Guid, opt => opt.Ignore());
            CreateMap<CreateUserDTO, User>();
            
            CreateMap<Author, AuthorDTO>()
                .ForMember(d => d.Roles, opt => opt.MapFrom(s => s.UserRoles.Select(ur => ur.Role!.Name)));
            CreateMap<AuthorDTO, Author>()
                .ForMember(d => d.Guid, opt => opt.Ignore());
            CreateMap<CreateUserDTO, Author>();

            CreateMap<Blog, BlogDTO>();
            CreateMap<BlogDTO, Blog>()
                .ForMember(d => d.Id, opt => opt.Ignore())
                .ForMember(d => d.CreatedAt, opt => opt.Ignore());

            CreateMap<Category, CategoryDTO>();
            CreateMap<CategoryDTO, Category>()
                .ForMember(d => d.Id, opt => opt.Ignore());

            CreateMap<Notification, NotificationDTO>();
            CreateMap<NotificationDTO, Notification>()
                .ForMember(d => d.Id, opt => opt.Ignore())
                .ForMember(d => d.CreatedAt, opt => opt.Ignore());
        }
    }
}
