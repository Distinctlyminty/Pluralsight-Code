﻿using System;
using System.Collections.Generic;
using System.Linq;

using Foundation;
using UIKit;
using Wired_Brain_Coffee.Services;
using Xamarin.Forms;

namespace Wired_Brain_Coffee.iOS
{
    public class Application
    {
        // This is the main entry point of the application.
        static void Main(string[] args)
        {
            // if you want to use a different Application Delegate class from "AppDelegate"
            // you can specify it here.
            UIApplication.Main(args, null, "AppDelegate");
            var userService = DependencyService.Resolve<IUserService>();
        }
    }
}
