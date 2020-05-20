using Microsoft.Extensions.Logging;
using NinjaOrganizer.API.Contexts;
using NinjaOrganizer.API.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace NinjaOrganizer.API.Services
{
   /// <summary>
   /// Desciprion function implemented by IUserService can read in "IUserService.cs"
   /// Class "UserService" allow to manipulate Users.
   /// </summary>
    public class UserService : IUserService
    {
        private readonly NinjaOrganizerContext _context;
        private readonly ILogger<UserService> _logger;

        public UserService(NinjaOrganizerContext context, ILogger<UserService> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public User Create(User user, string password)
        {
            // create password
            byte[] passwordHash, passwordSalt;
            passwordHash = GenerateHashPassword(password, out passwordSalt);

            user.PasswordHash = passwordHash;
            user.PasswordSalt = passwordSalt;

            // add user
            _context.Users.Add(user);

            try
            {
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                _logger.LogCritical("Exception while SaveChanges",ex);
                throw new Exception(ex.Message);
            }

            // created succefful
            return user;
        }

       

        public User Authenticate(string username, string password)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                return null;

            var user = _context.Users.SingleOrDefault(u => u.Username == username);

            // check if username exists
            if (user == null)
                return null;

            // check if password is correct
            if (!checkPasswordHash(password, user.PasswordHash, user.PasswordSalt))
                return null;

            // authentication successful
            return user;
        }

        /// <summary>
        /// Check if password is correct.
        /// </summary>
        /// <param name="password"></param>
        /// <param name="storedHash"></param>
        /// <param name="storedSalt"></param>
        /// <returns>True if password is correct, else false.</returns>
        private bool checkPasswordHash(string password, byte[] storedHash, byte[] storedSalt)
        {
            if (storedHash.Length != 64 || storedSalt.Length != 128)
                throw new ArgumentException("invalid lenght", "checkpassword");

            using (var hmac = new HMACSHA512(storedSalt))
            {
                var resultHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));

                for (int i = 0; i < resultHash.Length; i++)
                    if (resultHash[i] != storedHash[i]) return false;
            }
            return true;
        }

        public void Update(User user, string password = null)
        {
            // find user
            var tmpUser = _context.Users.Find(user.Id);
            if (tmpUser == null)
                throw new Exception("User not found");

            // update username if it has changed
            if (!string.IsNullOrWhiteSpace(user.Username) && user.Username != tmpUser.Username)
            {
                if (_context.Users.Any(x => x.Username == user.Username))
                    throw new Exception("Username " + user.Username + " is already taken");

                tmpUser.Username = user.Username;
            }

            // update user properties if provided
            if (!string.IsNullOrWhiteSpace(user.FirstName))
                tmpUser.FirstName = user.FirstName;

            if (!string.IsNullOrWhiteSpace(user.LastName))
                tmpUser.LastName = user.LastName;

            // update password if provided
            if (!string.IsNullOrWhiteSpace(password))
            {
                byte[] passwordHash, passwordSalt;
                passwordHash = GenerateHashPassword(password, out passwordSalt);

                tmpUser.PasswordHash = passwordHash;
                tmpUser.PasswordSalt = passwordSalt;
            }

            _context.Users.Update(tmpUser);
            _context.SaveChanges();
        }

        /// <summary>
        /// Create password hash.
        /// </summary>
        /// <param name="password"></param>
        /// <param name="passwordHash"></param>
        /// <param name="passwordSalt">return passwordsalt</param>
        private byte[] GenerateHashPassword(string password, out byte[] passwordSalt)
        {
            var hash = new byte[64];

            if (password == null) throw new ArgumentNullException("password");
            if (string.IsNullOrWhiteSpace(password)) throw new ArgumentException("Value cannot be empty or whitespace", "password");

            using (var hmac = new HMACSHA512())
            {
                passwordSalt = hmac.Key;
                hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
            }

            return hash;
        }


        public void Delete(int id)
        {
            var user = _context.Users.Find(id);
            if (user != null)
            {
                _context.Users.Remove(user);
                _context.SaveChanges();
            }
        }

        public IEnumerable<User> GetAll()
        {
            return _context.Users;
        }

        public User GetById(int id)
        {
            return _context.Users.Find(id);
        }

        
    }
}
