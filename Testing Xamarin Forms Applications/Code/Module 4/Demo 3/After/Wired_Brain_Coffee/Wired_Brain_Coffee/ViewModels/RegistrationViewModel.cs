using System;
using System.Text;
using Wired_Brain_Coffee.Views;
using Xamarin.Forms;

namespace Wired_Brain_Coffee.ViewModels
{
    public class RegistrationViewModel : BaseViewModel
    {
        private string firstName;
        private string lastName;
        private string emailAddress;
        private string password;
        private string errorText;

        public RegistrationViewModel()
        {

        }


public string FirstName { get => firstName; set => SetProperty(ref firstName, value); }

        public string LastName { get => lastName; set => SetProperty(ref lastName, value); }

        public string EmailAddress { get => emailAddress; set => SetProperty(ref emailAddress, value); }

        public string Password { get => password; set => SetProperty(ref password, value); }

        public string ErrorText { get => errorText; set => SetProperty(ref errorText, value); }

        public Command SignupCommand
        {
            get
            {
                return new Command(SignupAsync);
            }
        }


        private async void SignupAsync()
        {
            if (string.IsNullOrEmpty(FirstName) || string.IsNullOrEmpty(LastName) || string.IsNullOrEmpty(EmailAddress) || string.IsNullOrEmpty(Password))
            {

                ErrorText = "Some values are not correct, please correct any errors and try again.";

            }
            else
            {
                User user = new User()
                {
                    FirstName = FirstName,
                    LastName = LastName,
                    EmailAddress = EmailAddress,
                    Password = Password
                };

                var userResponse = UserService.Register(user);

                if (userResponse.EntityState == Status.Created)
                {
                    await App.Current.MainPage.Navigation.PushAsync(new WelcomePage());

                }
                else
                {
                    await App.Current.MainPage.DisplayAlert("Error", "Unable to register the user", "OK");

                }

            }
        }
    }
}
