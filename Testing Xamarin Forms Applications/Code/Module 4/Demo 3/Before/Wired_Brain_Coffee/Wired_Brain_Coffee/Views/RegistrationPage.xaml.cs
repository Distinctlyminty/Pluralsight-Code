using System;
using System.Collections.Generic;
using Wired_Brain_Coffee.ViewModels;
using Xamarin.Forms;

namespace Wired_Brain_Coffee.Views
{
    public partial class RegistrationPage : ContentPage
    {
        RegistrationViewModel viewmodel;

        public RegistrationPage()
        {
            InitializeComponent();
            BindingContext = viewmodel = new RegistrationViewModel();
        }
    }
}
