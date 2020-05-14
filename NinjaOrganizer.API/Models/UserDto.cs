﻿using NinjaOrganizer.API.Entities;
using NinjaOrganizer.API.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NinjaOrganizer.API.Models
{
    public class UserDto
    {

        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Username { get; set; }

        public int NumberOfTaskboards
        {
            get
            {
                return Taskboards.Count;
            }
        }

        public ICollection<TaskboardDto> Taskboards { get; set; } = new List<TaskboardDto>();

    }
}
