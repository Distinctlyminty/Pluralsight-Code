using NUnit.Framework;
using UI_Tests.Pages;
using Xamarin.UITest;

namespace UI_Tests
{
    [TestFixture(Platform.Android)]
    [TestFixture(Platform.iOS)]

    public abstract class BaseTest
    {
        protected IApp app;
        protected Platform platform;

        protected RegistrationPage registrationPage;
        protected WelcomePage welcomePage;

        protected BaseTest(Platform platform)
        {
            this.platform = platform;
        }

        [SetUp]
        virtual public void BeforeEachTest()
        { 
            app = AppInitializer.StartApp(platform);
           

           var file= app.Screenshot("App Initialized");

            registrationPage = new RegistrationPage(app, platform);
            welcomePage = new WelcomePage(app, platform);
        }
    }
}
