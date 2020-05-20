using NinjaOrganizer.API.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text;
using System.Net.Http;

namespace NinjaOrganizer.API.Contexts
{
    public class NinjaOrganizerContext : DbContext
    {
        public DbSet<Taskboard> Taskboards { get; set; }
        public DbSet<Card> Cards { get; set; }

        public DbSet<User> Users { get; set; }

        public NinjaOrganizerContext(DbContextOptions<NinjaOrganizerContext> options)
           : base(options)
        {
              //Database.EnsureCreated();
            
        }

        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
                .HasData(
                new User()
                {
                    Id = 1,
                    Username = "User 1",
                    FirstName = "First name of user 1",
                    LastName = "Last name os user 1"
                },
                 new User()
                 {
                     Id = 2,
                     Username = "User 2",
                     FirstName = "First name os user 2",
                     LastName = "Last name of user 2"
                 }
                );

            modelBuilder.Entity<Taskboard>()
                  .HasData(
                 new Taskboard()
                 {
                     Id = 1,
                     Title = "Tablica 1 usera 1",
                     Description = "opis tablicy pierwszej usera 1",
                     UserId = 1

                 },
                 new Taskboard()
                 {
                     Id = 2,
                     Title = "Tablica 2 usera 1",
                     Description = "opis tablicy drugiej usera 1",
                     UserId = 1
                 },
                 new Taskboard()
                 {
                     Id = 3,
                     Title = "tablica 1 usera 2",
                     Description = "opis tablicy pierwszej usera 2",
                     UserId = 2
                 },
                 new Taskboard()
                 {
                     Id = 4,
                     Title = "Tablica 2 usera 2",
                     Description = "opis tablicy pierwszej usera 2",
                     UserId = 2
                 });


            modelBuilder.Entity<Card>()
              .HasData(
                new Card()
                {
                    Id = 1,
                    TaskboardId = 1,
                    Title = "zadanie 1 tablicy 1",
                    Content = "opis zadanie 1 tablicy 1",
                    State = CardState.ToDo,
                    Priority = CardPriority.Low,
                    Created = DateTime.Now
                },
                new Card()
                {
                    Id = 2,
                    TaskboardId = 1,
                    Title = "zadanie 2 tablicy 1",
                    Content = "opis zadanie 2 tablicy 1",
                    State = CardState.InProgress,
                    Priority = CardPriority.Low,
                    Created = DateTime.Now
                },
                new Card()
                {
                    Id = 3,
                    TaskboardId = 2,
                    Title = "zadanie 1 tablicy 2",
                    Content = "opis zadanie 1 tablicy 2",
                    State = CardState.ToDo,
                    Priority = CardPriority.Low,
                    Created = DateTime.Now.AddDays(-1),
                    Updated = DateTime.Now
                },
                new Card()
                {
                    Id = 4,
                    TaskboardId = 2,
                    Title = "zadanie 2 tablicy 2",
                    Content = "opis zadanie 2 tablicy 2",
                    State = CardState.ToDo,
                    Priority = CardPriority.High,
                    Created = DateTime.Now
                }
                );

            base.OnModelCreating(modelBuilder);
        }
        

        //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        //{
        //    optionsBuilder.UseSqlServer("connectionstring");
        //    base.OnConfiguring(optionsBuilder);
        //}

    }
}
