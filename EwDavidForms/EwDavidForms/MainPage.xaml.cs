using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui;
using Microsoft.Maui.Controls;

namespace EwDavidForms
{
    public partial class MainPage : ContentPage
    {
        public MainPage ()
        {
            InitializeComponent ();
        }

        void Button_Clicked(System.Object sender, System.EventArgs e)
        {
            Navigation.PushAsync(new BehaviorPage());
        }
    }
}
