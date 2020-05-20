using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using Xamarin.UITest;
using Xamarin.UITest.Queries;

namespace UI_Tests
{

    public class Tests : BaseTest
    {

        public Tests(Platform platform):base(platform)
        {

        }

        [SetUp]
        public void BeforeEachTest()
        {
            base.BeforeEachTest();
            registrationPage.WaitForPageToLoad();
        }

       [Test]
       public void RegistrationPageIsVisible()
        {
            Assert.IsTrue(registrationPage.IsPageVisible);
        }


        [Test]
        public void CompletedFormDisplaysWelcomePage()
        {
            registrationPage.EnterFirstName("james");
            registrationPage.EnterLastName("millar");
            registrationPage.EnterEmailAddress("james@null.com");
            registrationPage.EnterPassword("ABC213");

            registrationPage.TapSignupButton();

            welcomePage.WaitForPageToLoad();

            Assert.IsTrue(welcomePage.IsPageVisible);
        }

        [Test]
        public void MissingEmailDisplaysWarning()
        {
         
            registrationPage.TapSignupButton();
      
            Assert.IsTrue(registrationPage.ErrorIsVisible);

            Assert.IsTrue(registrationPage.ErrorDisplaysCorrectText);
        }


    }
}
