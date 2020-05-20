using NUnit.Framework;
using System;
namespace UserApiServiceTests
{
    [TestFixture()]
    public class Test
    {
        private Wired_Brain_UserService.UserApiService _userApiService;

        [SetUp]
        public void Setup()
        {
            _userApiService = new Wired_Brain_UserService.UserApiService();

        }

        [Test()]
        [TestCase(1,"Adam")]
        [TestCase(2, "Simon")]
        [TestCase(3, "Lucy")]
        [TestCase(4, "Phoebe")]
        public void GetUserById_returnsValidFirstName(int userId, string expectedFirstName)
        {
            var user = _userApiService.Get(userId);

            Assert.AreEqual(expectedFirstName, user.FirstName);
        }

        [Test()]
        public void RegisterUserReturns_True()
        {
            // Arrange
            var user = new User()
            {
                FirstName = "John",
                LastName = "Davidsom",
                EmailAddress = "John@ps.com",
                Password = "123456"
            };

            // Act
            var result = _userApiService.Register(user);

            //Assert

            Assert.IsTrue(result.EntityState == Status.Created);

        }

        [Test()]
        public void RegisterUserReturns_False_WithInvalidEmail()
        {
            // Arrange
            var user = new User()
            {
                FirstName = "John",
                LastName = "Davidsom",
                EmailAddress = "someInvalidEmailAddress",
                Password = "123456"
            };

            // Act
            var result = _userApiService.Register(user);

            //Assert

            Assert.IsTrue(result.EntityState == Status.Invalid);

        }

        [Test()]
        public void RegisterUserReturns_False_WithShortPassword()
        {
            // Arrange
            var user = new User()
            {
                FirstName = "John",
                LastName = "Davidsom",
                EmailAddress = "John@ps.com",
                Password = "12345"
            };

            // Act
            var result = _userApiService.Register(user);

            //Assert

            Assert.IsTrue(result.EntityState == Status.Invalid);

        }

        [Test()]
        public void ResetPasswordReturns_6DigitString()
        {
           

            // Act
            var result = _userApiService.ResetPassword(1);

            //Assert

            Assert.IsTrue(result.Length == 6);

        }
    }
}
