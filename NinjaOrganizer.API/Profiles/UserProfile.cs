using AutoMapper;
using Microsoft.AspNetCore.JsonPatch;
using NinjaOrganizer.API.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NinjaOrganizer.API.Profiles
{
    public class UserProfile : Profile
    {
        public UserProfile()
        {
            CreateMap<Entities.User, Models.UserDto>();
            CreateMap<Models.UserForRegisterDto, Entities.User>().ReverseMap();
            CreateMap<Models.UserForUpdateDto, Entities.User>().ReverseMap();
        }


    }
}
