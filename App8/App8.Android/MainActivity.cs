using System;

using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android.Content.Res;
using Android.Content;


using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;




namespace App8.Droid
{
    

        [Activity (Label = "ProfiCAD", Icon = "@drawable/icon", Theme="@style/MainTheme", MainLauncher = true, 
        ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    [IntentFilter(new[] { Intent.ActionView }, Categories = new[] { Intent.CategoryDefault,Intent.CategoryDefault,
            Intent.CategoryBrowsable }, DataMimeType = "application/*", DataPathPattern = "*.sxe")]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
	{
		protected override void OnCreate (Bundle bundle)
		{
            AppCenter.Start("android=cd668d09-c993-4773-a9f7-cb30b395dc8f;" + "uwp={Your UWP App secret here};" +
                   "ios={Your iOS App secret here}",
                   typeof(Analytics), typeof(Crashes));

            TabLayoutResource = Resource.Layout.Tabbar;
			ToolbarResource = Resource.Layout.Toolbar; 

			base.OnCreate (bundle);


            global::Xamarin.Forms.Forms.Init (this, bundle);

			LoadApplication (new App8.App ());


            Android.Net.Uri uri = Intent.Data;

            System.IO.Stream l_input_stream;
           
            if(uri == null)
            {
                l_input_stream = Assets.Open("Elektroinstallation.sxe");
                //l_input_stream = Assets.Open("test_dim.sxe");
            }
            else
            {
                l_input_stream = ContentResolver.OpenInputStream(uri);
            }

            

            Xamarin.Forms.NavigationPage l_nav_page = Xamarin.Forms.Application.Current.MainPage as Xamarin.Forms.NavigationPage;
            if (l_nav_page.CurrentPage is App8.MainPage l_main_page)
            {
                l_main_page.Load_Drawing(l_input_stream);
            }
           

        }
            
        /*
        protected override void OnActivityResult(int requestCode, Result resultCode, Intent intent)
        {
            base.OnActivityResult(requestCode, resultCode, intent);

            Android.Net.Uri uri = intent.Data;
            System.IO.Stream stream = ContentResolver.OpenInputStream(uri);
        }
        */


    }
}

