using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Web;

namespace Wired_Brain_UserService
{
    public class UserApiService : IUserApiService
    {

        List<User> _users;

        public UserApiService()
        {
            _users = new List<User>();

            PopulateUsers();
        }

        private void PopulateUsers()
        {
            var user1 = new User()
            {Id = 1,
                FirstName = "Adam",
                LastName = "Robertson",
                EmailAddress = "Adam@ps.com",
                Password = "gshajd12!"
            };

            var user2 = new User()
            { Id = 2,
                FirstName = "Simon",
                LastName = "Hargreaves",
                EmailAddress = "Simon@ps.com",
                Password = "hgdjsks!4"
            };

            var user3 = new User()
            { Id = 3,
                FirstName = "Lucy",
                LastName = "Jones",
                EmailAddress = "Lucy@ps.com",
                Password = "78Yhgs;l$"
            };

            var user4 = new User()
            {
                Id = 4,
                FirstName = "Phoebe",
                LastName = "Adams",
                EmailAddress = "Phoebe@ps.com",
                Password = "Rt53tasu!"
            };
            _users.Add(user1);
            _users.Add(user2);
            _users.Add(user3);
            _users.Add(user4);


        }

        public User Get(int id)
        {
            try
            {

                return _users.First(x => x.Id == id);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public UserResponse Register(User user)
        {
            if (!UserIsValid(user))
            {
                return new UserResponse { UserEntity = user, EntityState = Status.Invalid };
            }
            // set the ID
            user.Id = _users.Count + 1;
            _users.Add(user);
            return new UserResponse { UserEntity = user, EntityState = Status.Created };
        }

        private bool UserIsValid(User user)
        {
        if(!string.IsNullOrEmpty(user.FirstName) && !string.IsNullOrEmpty(user.LastName) && PasswordValid(user.Password) && EmailValid(user.EmailAddress))
            {
                return true;
            }
            return false;
        }

        private bool EmailValid(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                // Normalize the domain
                email = Regex.Replace(email, @"(@)(.+)$", DomainMapper,
                                      RegexOptions.None, TimeSpan.FromMilliseconds(200));

                // Examines the domain part of the email and normalizes it.
                string DomainMapper(Match match)
                {
                    // Use IdnMapping class to convert Unicode domain names.
                    var idn = new IdnMapping();

                    // Pull out and process domain name (throws ArgumentException on invalid)
                    var domainName = idn.GetAscii(match.Groups[2].Value);

                    return match.Groups[1].Value + domainName;
                }
            }
            catch (RegexMatchTimeoutException e)
            {
                return false;
            }
            catch (ArgumentException e)
            {
                return false;
            }

            try
            {
                return Regex.IsMatch(email,
                    @"^(?("")("".+?(?<!\\)""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))" +
                    @"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-0-9a-z]*[0-9a-z]*\.)+[a-z0-9][\-a-z0-9]{0,22}[a-z0-9]))$",
                    RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250));
            }
            catch (RegexMatchTimeoutException)
            {
                return false;
            }
        }

        private bool PasswordValid(string password)
        {
            // passwords must be at least 6 charachters in length
            if(password.Length >= 6)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private static Random random = new Random();

        public string ResetPassword(int id)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, 6)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }

    public interface IUserApiService
    {
        /// <summary>
        /// Register a new user
        /// </summary>
        /// <param name="user">The user to register</param>
        /// <returns>UserResponse Object containing Entity and Entity Status</returns>
        UserResponse Register(User user);

        /// <summary>
        /// Get a user by ID
        /// </summary>
        /// <param name="id">The user ID to return</param>
        /// <returns>User object</returns>
        User Get(int id);

        /// <summary>
        /// Reset a users password
        /// </summary>
        /// <param name="id">User ID</param>
        /// <returns>password reset token</returns>
        string ResetPassword(int id);
    }
}

public class User
{
    public int Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string EmailAddress { get; set; }
    public string Password { get; set; }
}

public class UserResponse
{
    public User UserEntity { get; set; }
    public Status EntityState { get; set; }
}

public enum Status
{
    Created,
    Updated,
    Deleted,
    Invalid
}


