using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Xamarin.Forms;
using DxfNet;


namespace App8
{
	public partial class App : Application
	{
        public CollPages m_doc;
  


        internal static CollPages GetCurrentDoc
        {
            get
            {
                if (Xamarin.Forms.Application.Current is App8.App l_app)
                {
                    return l_app.m_doc;
                }
                return null;
            }
        }

        public App ()
		{
			InitializeComponent();
			MainPage = new NavigationPage(new App8.MainPage());
        }

        protected override void OnStart ()
		{
			// Handle when your app starts
		}

		protected override void OnSleep ()
		{
			// Handle when your app sleeps
		}

		protected override void OnResume ()
		{
			// Handle when your app resumes
		}
	}
}
