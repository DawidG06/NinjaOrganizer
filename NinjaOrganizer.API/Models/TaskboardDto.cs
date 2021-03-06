﻿using Microsoft.EntityFrameworkCore.Query.ExpressionVisitors.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NinjaOrganizer.API.Models
{
    public class TaskboardDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int NumberOfCards
        {
            get
            {
                return Cards.Count;
            }
        }

        public int NumberOfNotReadyCards
        {
            get
            {
                int result = Cards.Where(c => c.State != Entities.CardState.Ready).Count();
                return result;
            }
        }

        public DateTime Created { get; set; }
        public DateTime? Updated { get; set; }

        public ICollection<CardDto> Cards { get; set; } = new List<CardDto>();

    }
}
