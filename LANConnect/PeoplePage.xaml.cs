using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System.Threading.Tasks;
using System.Diagnostics;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace LANConnect
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class PeoplePage : Page
    {
        public PeoplePage()
        {
            this.InitializeComponent();
        }

        private async Task<string> SendToMainWindow(string Name)
        {
            if (Application.Current is App app && app.MainWindow is MainWindow mainWindow)
            {
                string result = await mainWindow.OtherPagesEvents(Name);
                return result;
            }
            else
            {
                return "Error";
            }
        }

        private async void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            string status = await SendToMainWindow("SearchButtonClicked");
            Debug.WriteLine(status);
        }

        private void MessageSelectedUserButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void CallSelectedUserButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void MoreSelectedUserButton_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
