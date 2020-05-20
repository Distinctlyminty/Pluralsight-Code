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
        }

        [Test]
        public void AppLaunches()
        {
            app.Screenshot("First screen.");
        }

    }
}
