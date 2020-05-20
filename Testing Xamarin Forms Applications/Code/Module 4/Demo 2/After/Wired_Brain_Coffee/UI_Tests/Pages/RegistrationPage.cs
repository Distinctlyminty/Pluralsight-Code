using System.Linq;
using Xamarin.UITest;

using Query = System.Func<Xamarin.UITest.Queries.AppQuery, Xamarin.UITest.Queries.AppQuery>;


namespace UI_Tests.Pages
{
    public class RegistrationPage : BasePage
    {
        readonly Query firstNameEntry, lastNameEntry, emailEntry, passwordEntry, signupButton;

        public RegistrationPage(IApp app, Platform platform) : base(app, platform, "Registration Page")
        {
            if (OniOS)
            {
                firstNameEntry = x => x.Marked("firstName");
                lastNameEntry = x => x.Marked("lastName");
                emailEntry = x => x.Marked("emailAddress");
                passwordEntry = x => x.Marked("password");
            }
            else
            {
                firstNameEntry = x => x.Class("FormsEditText").Index(0);
                lastNameEntry = x => x.Class("FormsEditText").Index(1);
                emailEntry = x => x.Class("FormsEditText").Index(2);
                passwordEntry = x => x.Class("FormsEditText").Index(3);
            }

            signupButton = x => x.Marked("signUpButton");


        }

        public void EnterFirstName(string text)
        {
            EnterText(firstNameEntry, text);

            app.Screenshot("Entered Firstname");
        }

        public void EnterLastName(string text)
        {
            EnterText(lastNameEntry, text);

            app.Screenshot("Entered Lastname");
        }

        public void EnterEmailAddress(string text)
        {
            EnterText(emailEntry, text);

            app.Screenshot("Entered Emailaddress");
        }

        public void EnterPassword(string text)
        {
            EnterText(passwordEntry, text);

            app.Screenshot("Entered Password");
        }






        void EnterText(Query textBoxQuery, string text)
        {
            app.ClearText(textBoxQuery);
            app.EnterText(textBoxQuery, text);
            app.DismissKeyboard();
        }

    }
}
