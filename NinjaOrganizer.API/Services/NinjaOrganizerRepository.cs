using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NinjaOrganizer.API.Contexts;
using NinjaOrganizer.API.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace NinjaOrganizer.API.Services
{
    /// <summary>
    /// Desciprion function implemented by INinjaOrganizerRepository can read in "INinjaOrganizerRepository.cs"
    /// Class "NinjaOrganizerRepository" allow to manipulate Taskboards and Cards.
    /// </summary>
    public class NinjaOrganizerRepository : INinjaOrganizerRepository
    {
        private readonly NinjaOrganizerContext _context;

        public NinjaOrganizerRepository(NinjaOrganizerContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }


        public Taskboard GetTaskboard(int taskboardId, bool includeCards)
        {
            if (includeCards)
            {
                return _context.Taskboards.Include(c => c.Cards)
                    .Where(t => t.Id == taskboardId).FirstOrDefault();
            }

            return _context.Taskboards.Where(t => t.Id == taskboardId).FirstOrDefault();
        }

        public IEnumerable<Taskboard> GetTaskboardsForUser(int userId)
        {
            var taskboards = _context.Taskboards.Where(u => u.UserId == userId).ToList();

            foreach (var taskboard in taskboards)
                taskboard.Cards = _context.Cards.Where(t => t.TaskboardId == taskboard.Id).ToList();

            return taskboards;

        }

        public Card GetCardForTaskboard(int taskboardId, int cardId)
        {
            return _context.Cards.Where(t => t.TaskboardId == taskboardId && t.Id == cardId).FirstOrDefault();
        }

        public IEnumerable<Card> GetCardsForTaskboard(int taskboardId)
        {
            return _context.Cards.Where(t => t.TaskboardId == taskboardId).ToList();
        }

        

        public bool TaskboardExists(int taskboardId)
        {
            return _context.Taskboards.Any(t => t.Id == taskboardId);
        }

        public bool UserExist(string userName)
        {
            return _context.Users.Any(u => u.Username == userName);
        }

        public void AddCardForTaskboard(int taskboardId, Card card)
        {
            var taskboard = GetTaskboard(taskboardId, false);
            taskboard.Cards.Add(card);
        }

        public void UpdateCard(int taskboardId, Card card)
        {
            _context.Cards.Update(card);
        }

        public void UpdateTaskboard(int taskboardId, Taskboard taskboard)
        {
            _context.Taskboards.Update(taskboard);
        }

        public void DeleteCard(Card card)
        {
            _context.Cards.Remove(card);
        }

        public bool Save()
        {
            var AddedEntities = _context.ChangeTracker.Entries().Where(E => E.State == EntityState.Added).ToList();
            AddedEntities.ForEach(E =>
            {
                string displayName = E.Metadata.DisplayName();
                if (displayName == "Card" || displayName == "Taskboard")
                    E.Property("Created").CurrentValue = DateTime.Now;
            });

            var EditedEntities = _context.ChangeTracker.Entries().Where(E => E.State == EntityState.Modified).ToList();
            EditedEntities.ForEach(E =>
            {
                string displayName = E.Metadata.DisplayName();
                if (displayName == "Card" || displayName == "Taskboard")
                    E.Property("Updated").CurrentValue = DateTime.Now;
            });

            return (_context.SaveChanges() >= 0);
        }

        public void AddTaskboard(Taskboard taskboard)
        {
            _context.Taskboards.Add(taskboard);
        }

        public void DeleteTaskboard(Taskboard taskboard)
        {
            _context.Taskboards.Remove(taskboard);
        }

    }
}
