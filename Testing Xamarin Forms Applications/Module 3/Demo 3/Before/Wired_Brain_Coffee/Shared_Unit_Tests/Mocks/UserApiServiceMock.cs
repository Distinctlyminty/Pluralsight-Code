using System;
using System.Collections.Generic;
using Wired_Brain_UserService;

namespace Shared_Unit_Tests.Mocks
{
    public class UserApiServiceMock: IUserApiService
    {
        List<User> _users = new List<User>();
        
        public UserApiServiceMock()
        {
        }

        public User Get(int id)
        {
            throw new NotImplementedException();
        }

        public UserResponse Register(User user)
        {
           _users.Add(user);

          return new UserResponse() { UserEntity = user, EntityState = Status.Created };
        
        }


        public string ResetPassword(int id)
        {
            throw new NotImplementedException();
        }
    }
}
