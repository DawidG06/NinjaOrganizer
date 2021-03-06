﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace NinjaOrganizer.API.Models
{
    public class TaskboardForCreationDto
    {
        [Required(ErrorMessage = "You should provide a title value.")]
        [MaxLength(50)]
        public string Title { get; set; }

        [Required(ErrorMessage = "You should provide a description value.")]
        [MaxLength(200)]
        public string Description { get; set; }


    }
}
