using NinjaOrganizer.API.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NinjaOrganizer.API.Services
{
    /// <summary>
    /// Interface for define managment of Taskboards and Cards.
    /// </summary>
    public interface INinjaOrganizerRepository
    {

        Taskboard GetTaskboard(int taskboardId, bool includeCards);
        IEnumerable<Taskboard> GetTaskboardsForUser(int userId);

        void AddTaskboard(Taskboard taskboard);
        void DeleteTaskboard(Taskboard taskboard);

        bool TaskboardExists(int taskboardId);

        void UpdateTaskboard(int taskboardId, Taskboard taskboard);


        IEnumerable<Card> GetCardsForTaskboard(int taskboardId);

        Card GetCardForTaskboard(int taskboardId, int cardId);
        void AddCardForTaskboard(int taskboardId, Card card);


        void UpdateCard(int taskboardId, Card card);


        void DeleteCard(Card card);

        bool Save();

        bool UserExist(string userName); // TODO przeniesc do userservice

    }
}
