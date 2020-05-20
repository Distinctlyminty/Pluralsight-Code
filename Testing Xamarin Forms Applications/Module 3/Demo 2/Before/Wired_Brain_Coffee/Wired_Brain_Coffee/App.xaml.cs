using System;
using Wired_Brain_Coffee.Services;
using Wired_Brain_UserService;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Wired_Brain_Coffee
{
    public partial class App : Application
    {

        public App()
        {
            InitializeComponent();

            DependencyService.Register<IUserApiService, UserApiService>();

        }

        protected override void OnStart()
        {
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}
