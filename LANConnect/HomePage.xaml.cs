using System;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
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
using System.Net.WebSockets;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace LANConnect
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class HomePage : Page
    {
        public HomePage()
        {
            this.InitializeComponent();
            ErrorContentDialog.XamlRoot = this.Content.XamlRoot;
            ShowErrorDialog();
        }

        public async void ShowErrorDialog()
        {
            try
            {
                // 确保在 UI 线程上执行
                DispatcherQueue.TryEnqueue(() =>
                {
                    // 设置 XamlRoot 属性
                    ErrorContentDialog.XamlRoot = this.Content.XamlRoot;
                });

                var ShowStatus = await ErrorContentDialog.ShowAsync();
                Debug.WriteLine(ShowStatus);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        public async void HideErrorDialog()
        {
            try
            {
                // 确保在 UI 线程上执行
                DispatcherQueue.TryEnqueue(() =>
                {
                    // 设置 XamlRoot 属性
                    ErrorContentDialog.XamlRoot = this.Content.XamlRoot;
                });

                ErrorContentDialog.Hide();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
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
    }
}
