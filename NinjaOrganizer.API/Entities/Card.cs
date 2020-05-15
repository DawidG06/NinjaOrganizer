using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace NinjaOrganizer.API.Entities
{
    public enum CardState
    {
        ToDo = 1,
        InProgress = 2,
        Ready = 3
    }

    public enum CardPriority
    {
        Low = 1,
        Medium = 2,
        High = 3
    }

    public class Card
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Required]
        [MaxLength(50)]
        public string Title { get; set; }
        [MaxLength(200)]
        public string Content { get; set; }

        public CardState State { get; set; }
        public CardPriority Priority { get; set; }


        public DateTime Created { get; set; }
        public DateTime? Updated { get; set; }

        [ForeignKey("TaskboardId")]
        public Taskboard Taskboard { get; set; }
        public int TaskboardId { get; set; }

    }
}
