﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using NinjaOrganizer.API.Contexts;
using NinjaOrganizer.API.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Serialization;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using NinjaOrganizer.API.Helpers;
using Microsoft.Extensions.Options;
using System.Data.SqlClient;

namespace NinjaOrganizer.API
{
    public class Startup
    {
        private readonly IConfiguration _configuration;

        public Startup(IConfiguration configuration)
        {
            _configuration = configuration ??
                throw new ArgumentNullException(nameof(configuration));

        }


        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc()
                .AddMvcOptions(o =>
                {
                    o.OutputFormatters.Add(new XmlDataContractSerializerOutputFormatter());
                });


            //#if DEBUG
            //            services.AddTransient<IMailService, LocalMailService>();
            //#else
            //            services.AddTransient<IMailService, CloudMailService>();
            //#endif
            var connectionString = _configuration["connectionStrings:NinjaOrganizerDBConnectionString"];
           // services.AddDbContext<NinjaOrganizerContext>(o =>
            //{
                // o.UseSqlServer(connectionString);

               // var connectionStrBuilder = new SqlConnectionStringBuilder { DataSource = "MyDb.db" };
             //   var connectionStr = connectionStrBuilder.ToString();
              //  var connection = new SqlConnection(connectionStr);
              //  o.UseSqlite(connection);
                
           // });

            services.AddEntityFrameworkSqlite().AddDbContext<NinjaOrganizerContext>(o =>
            {
                //o.UseSqlite("Filename=MyDatabase.db"); //ok dziala

               // o.UseSqlite("Filename=MYSQLCONNSTR_localdb");
               string conStr = "Data Source=D:\\home\\site\\wwwroot\\MyDatabase.db";
                o.UseSqlite(conStr);

            });


            services.AddScoped<INinjaOrganizerRepository, NinjaOrganizerRepository>();

            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());


            var appSettingsFromFile = _configuration.GetSection("AppSettings");
            services.Configure<AppSettings>(appSettingsFromFile);
            // configure jwt authentication
            var appSettings = appSettingsFromFile.Get<AppSettings>();
            var key = Encoding.ASCII.GetBytes(appSettings.Secret);
            services.AddAuthentication(confOptions =>
            {
                confOptions.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                confOptions.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(confOptions =>
            {
                confOptions.Events = new JwtBearerEvents
                {
                    OnTokenValidated = context =>
                    {
                        var userService = context.HttpContext.RequestServices.GetRequiredService<IUserService>();
                        var userId = int.Parse(context.Principal.Identity.Name);
                        var user = userService.GetById(userId);
                        if (user == null)
                        {
                            // return unauthorized if user no longer exists
                            context.Fail("Unauthorized");
                        }

                        return Task.CompletedTask;
                    }

                };
                confOptions.RequireHttpsMetadata = false;
                confOptions.SaveToken = true;
                confOptions.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false
                };
            });


            services.AddScoped<IUserService, UserService>();
            services.AddCors(o =>
            {
                o.AddPolicy("AllowAll",
                    p => p.AllowAnyOrigin()
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials());
            });
        }


        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {

            if (env.IsDevelopment())
                app.UseDeveloperExceptionPage();
            else
            {
                // app.UseExceptionHandler();
                app.UseExceptionHandler("/error");

            }


            /*
            app.UseCors(c => c
             .AllowAnyOrigin()
             .AllowAnyMethod()
             .AllowAnyHeader());
             */
            app.UseCors("AllowAll");

            app.UseAuthentication();

            app.UseStatusCodePages();
            app.UseMvc();
        }
    }
}
