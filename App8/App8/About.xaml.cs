using System.IO;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace App8
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class About : ContentPage
	{
		public About ()
		{
			InitializeComponent ();

#if __ANDROID__
            var assetManager = Android.App.Application.Context.Assets;
            using (var streamReader = new StreamReader(assetManager.Open("html/about.html")))
            {
                string ls_html = streamReader.ReadToEnd();
                HtmlWebViewSource l_source = new HtmlWebViewSource();
                l_source.Html = ls_html;
                m_web_view. Source = l_source;
            }
#endif
        }
	}
}