using NinjaOrganizer.API.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NinjaOrganizer.API.Services
{
    /// <summary>
    /// Interface for define managment of User Profile.
    /// </summary>
    public interface IUserService
    {
        /// <summary>
        /// Create new user.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="password"></param>
        /// <returns>Created user.</returns>
        User Create(User user, string password);

        /// <summary>
        /// Authenticate exist user.
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns>Return Info about User with bearer token for authorization</returns>
        User Authenticate(string username, string password);

        /// <summary>
        /// Edit exist user.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="password"></param>
        void Update(User user, string password = null);

        /// <summary>
        /// For get all users.
        /// </summary>
        /// <returns></returns>
        IEnumerable<User> GetAll();

        /// <summary>
        /// Get specific user by id.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        User GetById(int id);

        /// <summary>
        /// Delete exist user.
        /// </summary>
        /// <param name="id"></param>
        void Delete(int id);

    }
}
