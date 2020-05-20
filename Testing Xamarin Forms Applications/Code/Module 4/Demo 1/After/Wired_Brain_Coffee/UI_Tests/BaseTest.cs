﻿using System;
using NUnit.Framework;
using Xamarin.UITest;

namespace UI_Tests
{
    [TestFixture(Platform.Android)]
    [TestFixture(Platform.iOS)]

    public abstract class BaseTest
    {
        protected IApp app;
        protected Platform platform;

        protected BaseTest(Platform platform)
        {
            this.platform = platform;
        }

        [SetUp]
        virtual public void BeforeEachTest()
        {
            app = AppInitializer.StartApp(platform);


            app.Screenshot("App Initialized");

        
        }
    }
}
