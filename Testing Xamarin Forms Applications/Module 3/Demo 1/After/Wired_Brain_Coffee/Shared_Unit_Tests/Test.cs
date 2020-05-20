using NUnit.Framework;
using System;
using Wired_Brain_Coffee.Services;

namespace Shared_Unit_Tests
{
    [TestFixture()]
    public class UserServiceTests
    {
        [TestCase]
        public void User_Created_If_All_Fields_Valid()
        {
            User user = new User()
            {
                 FirstName="Adam",
                 LastName="Smith",
                 EmailAddress="adam@test.com",
                 Password="ABC123"
            };
            
            IUserService userService = new UserService();

            UserResponse response = userService.Register(user);

            Assert.AreEqual(Status.Created, response.EntityState);
        }

        [TestCase]
        public void User_Is_Not_Created_If_FirstName_NULL()
        {
            User user = new User()
            {
                LastName = "Smith",
                EmailAddress = "adam@test.com",
                Password = "ABC123"
            };

            IUserService userService = new UserService();

            UserResponse response = userService.Register(user);

            Assert.AreEqual(Status.Invalid, response.EntityState);
        }

        [TestCase]
        public void User_Is_Not_Created_If_Last_Name_NULL()
        {
            User user = new User()
            {
                FirstName = "Adam",
                EmailAddress = "adam@test.com",
                Password = "ABC123"
            };

            IUserService userService = new UserService();

            UserResponse response = userService.Register(user);

            Assert.AreEqual(Status.Invalid, response.EntityState);
        }

        [TestCase(".email@example.com")]
        [TestCase("email@example..com")]
        [TestCase(".email@example.com")]
        [TestCase("ABC..123@example.com")]
        [TestCase("")]
        public void User_Is_Not_Created_If_Email_Invalid(string emailAddress)
        {

            User user = new User()
            {
                FirstName = "Adam",
                LastName="Smith",
                EmailAddress = emailAddress,
                Password = "ABC123"
            };

            IUserService userService = new UserService();

            UserResponse response = userService.Register(user);

            Assert.AreEqual(Status.Invalid, response.EntityState);


        }

        [TestCase("1")]
        [TestCase("ABC12")]
        [TestCase("")]
        public void User_Is_Not_Created_If_Password_Invalid(string password)
        {

            User user = new User()
            {
                FirstName = "Adam",
                LastName = "Smith",
                EmailAddress = "ABC123",
                Password = password
            };

            IUserService userService = new UserService();

            UserResponse response = userService.Register(user);

            Assert.AreEqual(Status.Invalid, response.EntityState);
        }
    }
}
