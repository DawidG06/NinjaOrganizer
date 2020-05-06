using NinjaOrganizer.API.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NinjaOrganizer.API.Services
{
    public interface IUserService
    {

        User Authenticate(string email, string password);
        IEnumerable<User> GetAll();
        User GetByEmail(string email);
        User Create(User user, string password);
        void Update(User user, string password = null);
        void Delete(string email);

    }
}
