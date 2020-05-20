using System;
using Wired_Brain_UserService;
using Xamarin.Forms;

namespace Wired_Brain_Coffee.Services
{
    public class UserService :IUserService
    {
        private readonly IDependencyService _dependencyService;

        public UserService() : this(new DependencyServiceWrapper())
        {
        }

        public UserService(IDependencyService dependencyService)
        {
            _dependencyService = dependencyService;
        }


        public UserResponse Register(User user)
        {
           var apiService = _dependencyService.Get<IUserApiService>();
            throw new NotImplementedException();
        }
    }
}
