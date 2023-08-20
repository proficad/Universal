using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using DxfNet;

namespace App8
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class ListOfPages : ContentPage
	{
        public ListOfPages()
        {
            InitializeComponent();

            Title = "List of Pages";

            CollPages l_doc = App8.App.GetCurrentDoc;
            if (l_doc != null)
            {
                List<string> ls_lop = l_doc.GetListOfPages();
                m_ctrl_lop.ItemsSource = ls_lop;
            }
        }

        async private void OnItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            string ls_item_selected = e.SelectedItem.ToString();
            CollPages l_doc = App8.App.GetCurrentDoc;
            if (l_doc != null)
            {
                l_doc.SetCurrentPage(ls_item_selected);
                await Navigation.PopAsync();
            }
        }

        //----------------------
    }
}