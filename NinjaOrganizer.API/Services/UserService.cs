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
    public class UserService : IUserService
    {
        private readonly NinjaOrganizerContext _context;

        public UserService(NinjaOrganizerContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public User Authenticate(string email, string password)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
                return null;

            var user = _context.Users.SingleOrDefault(u => u.Email == email);

            //if user not exists
            if (user == null)
                return null;

            //check password
            if (!checkPasswordHash(password, user.PasswordHash, user.PasswordSalt))
                return null;
            else
                return user;
        }

        private bool checkPasswordHash(string password, byte[] storedHash, byte[] storedSalt)
        {
            // TODO sprawdzic password

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

        private byte[] createPasswordHash(string password, out byte[] passwordSalt)
        {
            // TODO sprawdzic czy haslo jest puste


            var hash = new byte[64];

            using (var hmac = new HMACSHA512())
            {
                passwordSalt = hmac.Key;
                hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
            }

            return hash;
        }

        public User Create(User user, string password)
        {
            // TODO sprawdzic czy jest wpisane haslo i czy uzytkownik juz istnieje

            byte[] passwordHash, passwordSalt;
            passwordHash = createPasswordHash(password, out passwordSalt);

            user.PasswordHash = passwordHash;
            user.PasswordSalt = passwordSalt;

            _context.Users.Add(user);
            _context.SaveChanges();

            return user;
        }

        public void Delete(string email)
        {
            var user = _context.Users.Find(email);
            if(user != null)
            {
                _context.Users.Remove(user);
                _context.SaveChanges();
            }
        }

        public IEnumerable<User> GetAll()
        {
            return _context.Users;
        }

        public User GetByEmail(string email)
        {
            return _context.Users.Find(email);
        }

        public void Update(User user, string password)
        {
            var tmpUser = _context.Users.Find(user.Email);

            //TODO sprawdzic czy istnieje i czy sa argumenty podane

            tmpUser.FirstName = user.FirstName;
            tmpUser.LastName = user.LastName;
            tmpUser.Username = user.Username;
            byte[] salt;
            tmpUser.PasswordHash = createPasswordHash(password, out salt);
            tmpUser.PasswordHash=salt;

            _context.Users.Update(tmpUser);
            _context.SaveChanges();
        }
    }
}
