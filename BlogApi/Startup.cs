using AutoMapper;
using BlogApi.Contract;
using BlogApi.Core.Database;
using BlogApi.Core.Entities;
using BlogApi.DTOs.Mapping;
using BlogApi.Hubs;
using BlogApi.Repository;
using BlogApi.Services;
using BlogApi.Settings;
using BlogApi.Utils;
using BlogApi.Validators;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Serialization;
using System.Net;
using System.Net.Mail;
using System.Text;

namespace BlogApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<JwtTokenConfig>(Configuration.GetSection("JwtTokenConfig"));
            services.Configure<EmailConfig>(Configuration.GetSection("EmailConfig"));

            services.AddSignalR();
            services
                .AddControllers()
                .AddNewtonsoftJson(options =>
                {
                    options.SerializerSettings.ContractResolver = new DefaultContractResolver
                    {
                        NamingStrategy = new CamelCaseNamingStrategy
                        {
                            ProcessDictionaryKeys = true
                        }
                    };
                })
                .AddFluentValidation(config => config.RegisterValidatorsFromAssemblyContaining<CreateUserValidator>());

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "BlogApi", Version = "v1" });
            });

            services.AddDbContextPool<ApplicationDbContext>(options => options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));
            services.AddScoped<IBlogRepository, BlogRepository>();
            services.AddScoped<ICategoryRepository, CategoryRepository>();
            services.AddScoped<INotificationRepository, NotificationRepository>();

            services.AddIdentity<User, Role>(options =>
            {
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequiredLength = 1;

                options.User.RequireUniqueEmail = true;
                options.SignIn.RequireConfirmedEmail = true;
            })
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddUserManager<UserManager>()
                .AddDefaultTokenProviders();

            services.AddIdentityCore<Author>(options =>
            {
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequiredLength = 1;

                options.User.RequireUniqueEmail = true;
                options.SignIn.RequireConfirmedEmail = true;
            })
                .AddRoles<Role>()
                .AddClaimsPrincipalFactory<UserClaimsPrincipalFactory<Author, Role>>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddUserManager<AuthorManager>()
                .AddTokenProvider<EmailConfirmationTokenProvider<Author>>("emailConfirmation")
                .AddTokenProvider<PasswordResetTokenProvider<Author>>("passwordReset");

            var mapperConfig = new MapperConfiguration(mc =>
            {
                mc.AddProfile(new MappingProfile());
            });
            IMapper mapper = mapperConfig.CreateMapper();
            services.AddSingleton(mapper);

            services.AddScoped(provider =>
            {
                var config = provider.GetRequiredService<IOptionsMonitor<EmailConfig>>().CurrentValue;
                SmtpClient client = new(config.Host, config.Port)
                {
                    EnableSsl = config.EnableSsl,
                    UseDefaultCredentials = config.UseDefaultCredentials,
                    Credentials = new NetworkCredential(config.UserName, config.Password)
                };

                return client;
            });

            // services.AddHttpContextAccessor();
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<INotificationService, NotificationService>();

            services
                .AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = "jwt";
                    options.DefaultChallengeScheme = "jwt";
                })
                .AddCookie(cfg => cfg.SlidingExpiration = true)
                .AddJwtBearer("jwt", cfg =>
                {
                    cfg.RequireHttpsMetadata = false;
                    cfg.SaveToken = true;

                    cfg.TokenValidationParameters = new TokenValidationParameters()
                    {
                        ValidIssuer = Configuration["JwtTokenConfig:Issuer"],
                        ValidAudience = Configuration["JwtTokenConfig:Issuer"],
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["JwtTokenConfig:Key"]))
                    };
                });


            services.AddCors(options =>
            {
                options.AddPolicy("ClientPermission", policy =>
                {
                    policy.AllowAnyHeader()
                        .AllowAnyMethod()
                        .WithOrigins("http://localhost:3000")
                        .AllowCredentials();
                });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "BlogApi v1"));

            app.UseHttpsRedirection();

            app.UseCors("ClientPermission");

            app.UseRouting();

            app.UseAuthentication();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHub<NotificationHub>("/notihub");
            });
        }
    }
}
