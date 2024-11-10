using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Windows.ApplicationModel.Contacts;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml.Markup;
using System.Reflection;
using Microsoft.UI.Dispatching;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace LANConnect
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            this.InitializeComponent();

            Assembly.Load("LANConnect");

            // 将标题栏隐去
            ExtendsContentIntoTitleBar = true;

            // 实时更新 featuresNV
            this.SizeChanged += MainWindow_SizeChanged;
            UpdateFeaturesNVSize();
        }

        private void MainWindow_SizeChanged(object sender, WindowSizeChangedEventArgs e)
        {
            UpdateFeaturesNVSize();
        }


        // featuresNV: 主面板。
        // SelectionChanged: 选中项更改。
        private void featuresNV_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            // 设置页独有
            if (args.IsSettingsSelected)
            {
                contentFrame.Navigate(typeof(SettingsPage));
            }
            // 不是设置页？
            else
            {
                NavigationViewItem selectedItem = args.SelectedItemContainer as NavigationViewItem;
                // 省得卡死了
                if (selectedItem != null)
                {
                    // 更改选中项
                    switch (selectedItem.Tag)
                    {
                        // 各个项的 Tag
                        case "HomePage":
                            contentFrame.Navigate(typeof(HomePage));
                            break;
                        case "PeoplePage":
                            contentFrame.Navigate(typeof(PeoplePage));
                            break;
                        case "FilesPage":
                            contentFrame.Navigate(typeof(FilesPage));
                            break;
                        case "FavouritesPage":
                            contentFrame.Navigate(typeof(FavouritesPage));
                            break;
                    }
                }
            }
        }

        // UpdateFeaturesNVSize: 实时保证 featuresNV 的 UI 的全屏
        private void UpdateFeaturesNVSize()
        {
            if (featuresNV != null)
            {
                featuresNV.Width = this.Bounds.Width - 10;
                featuresNV.Height = this.Bounds.Height - 35;
            }
        }


        private async void checkServerButton_Click(object sender, RoutedEventArgs e)
        {
            // Debug 显示分行
            Debug.WriteLine("========================");

            // 判断地址是否为空
            if (serverURLBox.Text != string.Empty)
            {

                // 正常时

                // 设定变量
                string serverName = "";
                string serverBio = "";

                // 处理返回值
                // 判断开头是否为 Error
                serverName = await getServerName(serverURLBox.Text, "name");
                if (serverName.StartsWith("Error"))
                {
                    Debug.WriteLine("This is checkServerButton speaking ");
                    Debug.WriteLine("getServerName name returns a bad one: Error");
                    Debug.WriteLine("The string is showed below:");
                    Debug.WriteLine(serverName);
                }
                else
                {
                    Debug.WriteLine("This is checkServerButton speaking ");
                    Debug.WriteLine("getServerName name returns a good one:" + serverName);
                    serverNameTextBlock.Text = serverName;
                    SP_CheckServer.Visibility = Visibility.Collapsed;
                    SP_Login.Visibility = Visibility.Visible;
                    SP_ServerName.Visibility = Visibility.Visible;
                }

                serverBio = await getServerName(serverURLBox.Text, "bio");
                if (serverBio.StartsWith("Error"))
                {
                    Debug.WriteLine("This is checkServerButton speaking ");
                    Debug.WriteLine("getServerName bio returns a bad one: Error");
                    Debug.WriteLine("The string is showed below:");
                    Debug.WriteLine(serverBio);
                }
                else
                {
                    Debug.WriteLine("This is checkServerButton speaking ");
                    Debug.WriteLine("getServerName bio returns a good one:" + serverBio);
                    serverBioTextBlock.Text = serverBio;
                }
            }
            else
            {
                Debug.WriteLine("Server URL seems to be nothing");
                return;
            }
        }

        public async Task<object> PostRequestAsync(string url, string apiKey, string content, string returnType)
        {
            using (HttpClient client = new HttpClient())
            {
                // 设定 Header
                client.DefaultRequestHeaders.Add("accept", "application/json; charset=utf-8");
                if (!string.IsNullOrEmpty(apiKey))
                {
                    client.DefaultRequestHeaders.Add("X-API-Key", apiKey);
                }

                // 设编码格式为 UTF-8
                HttpContent httpContent = new StringContent(content, Encoding.UTF8, "application/json");

                // 发送请求                
                HttpResponseMessage response = await client.PostAsync(url, httpContent);

                if (returnType == "text")
                {
                    return await response.Content.ReadAsStringAsync();
                }
                else
                {
                    // 返回状态码
                    return (int)response.StatusCode;
                }
            }
        }


        public async Task<string> GetRequestAsync(string url, string apiKey)
        {
            using (HttpClient client = new HttpClient())
            {

                // 设定 Header
                client.DefaultRequestHeaders.Add("accept", "*/*");
                client.DefaultRequestHeaders.Add("X-API-Key", apiKey);

                // 请求 URL
                HttpResponseMessage response = await client.GetAsync(url);

                // 对是否成功做出判断
                if (response.IsSuccessStatusCode)
                {
                    // 返回获得的内容
                    return await response.Content.ReadAsStringAsync();
                }
                else
                {
                    // 返回空文本
                    return string.Empty;
                }
            }
        }


        private async Task<string> getServerName(string URL, string type)
        {
            try
            {
                // 设定用于调用 GetRequestAsync 的变量值
                string url = URL + "/api/admin/system/organization";
                string apiKey = "";

                // 调用与设定返回值
                string response = await GetRequestAsync(url, apiKey);

                // 处理响应内容
                Debug.WriteLine("This is getServerName speaking");
                Debug.WriteLine($"Response Content: {response}");

                // 是判断否为正常响应
                if (response == null) // 先判断是否为不正常
                {
                    Debug.WriteLine("This is getServerName speaking");
                    Debug.WriteLine("null: Server not found");

                    // 显示服务器返回空值
                    return ("Error: Server returns null");
                }
                else
                {
                    // 创建变量
                    if (type == "name")
                    {
                        string serverName = "";
                        serverName = JsonDecode(response, "name");
                        Debug.WriteLine("This is getServerName speaking");
                        Debug.WriteLine("Server Name:" + serverName);
                        return (serverName);
                    }
                    if (type == "bio")
                    {
                        string serverBio = "";
                        serverBio = JsonDecode(response, "description");
                        Debug.WriteLine("This is getServerName speaking");
                        Debug.WriteLine("Server Bio:" + serverBio);
                        return (serverBio);

                    }
                    else
                    {
                        Debug.WriteLine("This is getServerName speaking");
                        return ("Error: Type not found");
                    }
                }
            }
            catch (Exception ex)
            {
                // 有问题
                Debug.WriteLine("This is getServerName speaking");
                Debug.WriteLine("Error: " + ex.Message);
                return ($"Error: {ex.Message}");
            }
        }

        private async Task<int> changeServerName(string url, string apiKey, string newServerName, string NewServerBio)
        {
            try
            {
                // 设定用于调用 PostRequestAsync 的变量值
                // 设定 JSON 原内容
                var Json = $@"{{
    ""name"": ""{newServerName}"",
    ""description"": ""{NewServerBio}""
}}";

                // 调用与设定返回值
                int statusCode = (int)await PostRequestAsync(url, apiKey, Json, "int");

                // 处理状态码
                Debug.WriteLine($"HTTP Status Code: {statusCode}");
                return (statusCode);
            }
            catch (Exception ex)
            {
                // 处理异常，例如显示错误消息
                Debug.WriteLine($"Error: {ex.Message}");
                return (0);
            }
        }

        public static string JsonEncode(string text)
        {
            return JsonSerializer.Serialize(text);
        }



        // JsonDecode: 输入需要解码的文本,要查找的属性，输出指定的属性的值。
        private string JsonDecode(string jsonString, string key)
        {
            // 解析JSON字符串
            JsonDocument jsonDocument = JsonDocument.Parse(jsonString);
            JsonElement root = jsonDocument.RootElement;

            // 尝试获取指定键的值
            if (root.TryGetProperty(key, out JsonElement valueElement))
            {
                // 根据值的类型返回字符串形式的值
                switch (valueElement.ValueKind)
                {
                    case JsonValueKind.String:
                        return valueElement.GetString();
                    case JsonValueKind.Number:
                        return valueElement.GetRawText(); // 直接返回数值的字符串形式
                    case JsonValueKind.True:
                    case JsonValueKind.False:
                        return valueElement.GetBoolean().ToString(); // 返回 "True" 或 "False"
                    case JsonValueKind.Null:
                        return "null"; // 返回 "null" 字符串
                    case JsonValueKind.Object:
                        return valueElement.GetRawText(); // 返回对象的字符串形式
                    case JsonValueKind.Array:
                        return valueElement.GetRawText(); // 返回数组的字符串形式
                                                          // 可以根据需要添加更多类型处理
                }
            }

            // 如果键不存在，返回空字符串
            return string.Empty;
        }

        private async void userPasswordLoginButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 获取 Token
                string Token = await UserPasswordLogin(userEmailBox.Text, userPasswordBox.Password, "token");

                // 调用 setupServerWindow
                string setupStatus = await setupServerWindow(Token);
                if (setupStatus == "Success")
                {
                    Debug.WriteLine("This is userPasswordLoginButton Speaking");
                    Debug.WriteLine("setupServerWindow returned 'Successful'. ");
                }
                else
                {
                    Debug.WriteLine("This is userPasswordLoginButton speaking");
                    Debug.WriteLine("Error when using setupServerWindow.");
                    Debug.WriteLine("The error message is shown below:");
                    Debug.WriteLine(setupStatus);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("This is userPasswordLoginButton speaking");
                Debug.WriteLine("Error: " + ex);
            }
        }

        private async Task<string> UserPasswordLogin(string Email, string Password, string Type)
        {
            // 设定 JSON 原内容
            var Json = $@"{{
    ""credential"": {{
        ""email"": ""{Email}"",
        ""password"": ""{Password}"",
        ""type"": ""password""
    }},
    ""device"": ""web"",
    ""device_token"": null
}}";

            // 调用 PostRequestAsync 并设定变量
            string URL = serverURLBox.Text + "/api/token/login";

            // HTTP Status Code
            int Code = (int)await PostRequestAsync(URL, "", Json, "int");

            // 获取API返回的内容
            string Content = (string)await PostRequestAsync(URL, "", Json, "text");

            Debug.WriteLine("This is UserPasswordLogin speaking");
            Debug.WriteLine("PostRequestAsync returned the following content:");
            Debug.WriteLine(Content);
            Debug.WriteLine("With the following HTTP status code: " + $"{Code}");

            // 检验 HTTP Status Code的正确性
            if (Code == 200)
            {
                string Returned = JsonDecode(Content, Type);
                Debug.WriteLine("This is UserPasswordLogin speaking");
                Debug.WriteLine("JsonDecode returned:" + Returned);
                return Returned;
            }
            else
            {
                int TryAgain = (int)await PostRequestAsync(URL, "", Json, "int");
                return ("Error: " + $"TryAgain");
            }
        }


        private async Task<string> setupServerWindow(string Token)
        {
            try
            {
                // 获取服务器所有信息
                string serverInfo = await GetRequestAsync($"{serverURLBox.Text}/api/admin/system/organization", Token);
                string serverName = JsonDecode(serverInfo, "name");
                string serverBio = JsonDecode(serverInfo, "description");

                // 获取当前用户的所有信息
                string UserInfo = await GetRequestAsync(serverURLBox.Text + "/api/user/me", Token);
                Debug.WriteLine("This is setupServerWindow speaking");
                Debug.WriteLine("UserInfo:");
                Debug.WriteLine(UserInfo);

                // 分别获取所有的信息
                string Name = JsonDecode(UserInfo, "name");
                Debug.WriteLine("Name: " + Name);
                string UID = JsonDecode(UserInfo, "uid");
                Debug.WriteLine("UID: " + UID);
                string Email = JsonDecode(UserInfo, "email");
                Debug.WriteLine("Email:" + Email);
                string IsAdmin = JsonDecode(UserInfo, "is_admin");
                Debug.WriteLine("IsAdmin: " + IsAdmin);
                string IsBot = JsonDecode(UserInfo, "is_bot");
                Debug.WriteLine("IsBot: " + IsBot);
                string Gender = JsonDecode(UserInfo, "gender");
                Debug.WriteLine("Gender: " + Gender);
                string Language = JsonDecode(UserInfo, "language");
                Debug.WriteLine("Language: " + Language);
                string Birthday = JsonDecode(UserInfo, "birthday");
                Debug.WriteLine("Birthday: " + Birthday);
                string AvatarUpdatedAt = JsonDecode(UserInfo, "avatar_updated_at");
                Debug.WriteLine("AvatatUpdateAt: " + AvatarUpdatedAt);
                string DecodedAvatarUpdatedAt = TimestampDecode(Int64.Parse(AvatarUpdatedAt));
                Debug.WriteLine("DECODED AvatarUpdateAt: " + DecodedAvatarUpdatedAt);
                string CreateBy = JsonDecode(UserInfo, "create_by");
                Debug.WriteLine("CreateBy: " + CreateBy);

                // 更改主面版的主页设置
                string NVSetHomePageStatus = NVSetHomePage(featuresNV, "HomePage");
                if (NVSetHomePageStatus == "Success")
                {
                    Debug.WriteLine("This is setupServerWindow speaking");
                    Debug.WriteLine("NVSetHomePage returns Success");
                }
                else
                {
                    Debug.WriteLine("This is setupServerWindow speaking");
                    Debug.WriteLine("NVSetHomePage returns an error.");
                    Debug.WriteLine("The error message is shown below:");
                    Debug.WriteLine(NVSetHomePageStatus);
                }


                // 更改主面板的可视状态
                featuresNV.Visibility = Visibility.Visible;
                SP_CheckServer.Visibility = Visibility.Collapsed;
                SP_Login.Visibility = Visibility.Collapsed;
                SP_Logo.Visibility = Visibility.Collapsed;
                SP_ServerName.Visibility = Visibility.Collapsed;


                // 更改服务器名称
                string ChangeOtherPagesStatus = await ChangeOtherPagesAsync("HomePage", "serverNameTextBlock", "Text", serverName);
                if (ChangeOtherPagesStatus == "Success")
                {
                    Debug.WriteLine("This is setupServerWindow speaking");
                    Debug.WriteLine("ChangeOtherPages returns Success");
                }
                else
                {
                    Debug.WriteLine("This is setupServerWindow speaking");
                    Debug.WriteLine("ChangeOtherPages returns an error.");
                    Debug.WriteLine("The error message is shown below:");
                    Debug.WriteLine(ChangeOtherPagesStatus);
                }

                // 更改服务器简介
                string ChangeOtherPages_Bio_Status = await ChangeOtherPagesAsync("HomePage", "serverBioTextBlock", "Text", serverBio);
                if (ChangeOtherPages_Bio_Status == "Success")
                {
                    Debug.WriteLine("This is setupServerWindow speaking");
                    Debug.WriteLine("ChangeOtherPages returns Success");
                }
                else
                {
                    Debug.WriteLine("This is setupServerWindow speaking");
                    Debug.WriteLine("ChangeOtherPages returns an error.");
                    Debug.WriteLine("The error message is shown below:");
                    Debug.WriteLine(ChangeOtherPages_Bio_Status);
                }

                // 返回正确的状态
                Debug.WriteLine("This is setupServerWindow speaking");
                Debug.WriteLine("JSON decode and set visibility success");
                return ("Success");

            }
            catch (Exception ex)
            {
                Debug.WriteLine("This is setupServerWindow speaking");
                Debug.WriteLine("Error. The error message is shown below:");
                Debug.WriteLine(ex.Message);
                return ("Error: " + ex.Message);
            }
        }

        // DocodeTime: 输入时间戳，输出格式化的时间
        private static string TimestampDecode(long timestamp)
        {
            // 将时间戳从毫秒转换为秒
            DateTime decodedTime = DateTimeOffset.FromUnixTimeMilliseconds(timestamp).DateTime;

            // 返回格式化的日期时间字符串
            return decodedTime.ToString("yyyy/MM/dd HH:mm:ss.fff");
        }


        private string RewriteDataTemplate(Page targetPage, string jsonContent)
        {
            try
            {
                if (targetPage == null || string.IsNullOrEmpty(jsonContent))
                {
                    return "Error: Target page or JSON content is null or empty.";
                }

                // 解析 JSON 数据
                var contacts = JsonSerializer.Deserialize<List<Contact>>(jsonContent);

                // 创建新的 DataTemplate
                string xamlTemplate = @"
                <DataTemplate xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'>
                    <StackPanel>
                    <TextBlock Text='{Binding TargetInfo.Name}' />
                    <TextBlock Text='{Binding TargetInfo.Email}' />
                    </StackPanel>
                </DataTemplate>";

                var dataTemplate = (DataTemplate)XamlReader.Load(xamlTemplate);

                // 更新页面的资源字典
                targetPage.Resources["ListViewTemplate"] = dataTemplate;

                // 获取 ListBox 并设置数据源
                var listBox = targetPage.FindName("BaseExample") as ListBox;
                if (listBox != null)
                {
                    listBox.ItemTemplate = dataTemplate;
                    listBox.ItemsSource = contacts;
                }

                return "Success";
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }

        public string NVSetHomePage(NavigationView navigationView, string pageName)
        {
            try
            {
                foreach (var item in navigationView.MenuItems)
                {
                    if (item is NavigationViewItem navItem && navItem.Tag.ToString() == pageName)
                    {
                        navigationView.SelectedItem = navItem;

                        // 调试输出
                        Debug.WriteLine($"Navigating to page: {pageName}");

                        Type pageType;
                        if (pageName == "HomePage")
                        {
                            pageType = typeof(HomePage);
                        }
                        else
                        {
                            var fullPageName = $"LANConnect.Pages.{pageName}";
                            pageType = Type.GetType(fullPageName);
                        }

                        if (pageType == null)
                        {
                            Debug.WriteLine("This is NVSetHomePage speaking");
                            Debug.WriteLine("Error: Page type not found");
                            return $"Error: Page type {pageName} not found";
                        }

                        contentFrame.Navigate(pageType);
                        return "Success";
                    }
                }
                Debug.WriteLine("This is NVSetHomePage speaking");
                Debug.WriteLine("Error: Page Not Found.");
                return $"Error: Page {pageName} in {navigationView} not found";
            }
            catch (Exception ex)
            {
                Debug.WriteLine("This is NVSetHomePage speaking");
                Debug.WriteLine("Error. The error message is shown below:");
                Debug.WriteLine(ex.Message);
                return $"Error: {ex.Message}";
            }
        }

        // 重要方法！
        // ChangeOtherPagesAsync 方法
        // 输入 页面名，元素名，属性名，要更改为的属性值
        // 输出 Success（成功）或 Error（失败）以及错误消息。

        private async Task<string> ChangeOtherPagesAsync(string pageName, string name, string property, object value)
        {
            // 遍历 featuresNV 的 MenuItems 以找到对应的页面
            foreach (var item in featuresNV.MenuItems)
            {
                if (item is NavigationViewItem navItem && navItem.Tag.ToString() == pageName)
                {
                    // 确保 contentFrame 中已经加载了正确的页面
                    if (contentFrame.Content is Page page && page.GetType().Name == pageName)
                    {
                        // 查找指定的元素
                        var element = page.FindName(name) as FrameworkElement;
                        if (element == null)
                        {
                            return $"Element {name} not found on page {pageName}.";
                        }

                        // 获取属性信息
                        var propertyInfo = element.GetType().GetProperty(property);
                        if (propertyInfo == null)
                        {
                            return $"Property {property} not found on element {name}.";
                        }

                        // 更改属性值
                        propertyInfo.SetValue(element, value);

                        return $"Successfully changed {property} of {name} on {pageName} to {value}.";
                    }
                    else
                    {
                        return $"Page {pageName} not found in contentFrame.";
                    }
                }
            }

            return $"Page {pageName} not found in featuresNV.";
        }



        private string RefreshPage(Page page)
        {
            try
            {
                // 获取 contentFrame
                var frame = contentFrame;
                if (frame == null)
                {
                    Debug.WriteLine("This is RefreshPage speaking");
                    Debug.WriteLine("Error: Frame not found");
                    return "Error: Frame not found";
                }

                // 获取当前页面类型
                var pageType = page.GetType();

                // 重新导航到当前页面，使用 SuppressNavigationTransitionInfo 来避免动画
                frame.Navigate(pageType, null, new Microsoft.UI.Xaml.Media.Animation.SuppressNavigationTransitionInfo());
                frame.Navigate(pageType);

                Debug.WriteLine("This is RefreshPage speaking");
                Debug.WriteLine("Success!");
                return "Success";
            }
            catch (Exception ex)
            {
                Debug.WriteLine("This is RefreshPage speaking");
                Debug.WriteLine("Error. The error message is shown below:");
                Debug.WriteLine(ex.ToString()); // 输出完整的异常信息
                return $"Error: {ex.Message}";
            }
        }

        private static readonly string BasePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "LCNT");

        private async Task<string> SaveFileAsync(string content, string path)
        {
            try
            {
                string fullPath = Path.Combine(BasePath, path);
                using (StreamWriter writer = new StreamWriter(fullPath))
                {
                    await writer.WriteAsync(content);
                }
                return "Success";
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }

        private async Task<string> ReadFileAsync(string path)
        {
            try
            {
                string fullPath = Path.Combine(BasePath, path);
                using (StreamReader reader = new StreamReader(fullPath))
                {
                    string content = await reader.ReadToEndAsync();
                    return content;
                }
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }
    }
}
