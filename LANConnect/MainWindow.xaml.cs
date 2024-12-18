// ======================== LANConnect by Luke Zhang ========================
//         这个项目使用 MainWindow.xaml.cs 处理所有后端 以求更容易更新代码
// =========================== Contribution Notes ===========================
//                                 可用的方法：
//                             自己看，不想写了 (?)

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
using Microsoft.UI.Xaml.Media.Imaging;
using Windows.ApplicationModel;
using Windows.Storage;
using static System.Net.Mime.MediaTypeNames;
using System.Security.Cryptography;
using System.Net.Http.Headers;
using Windows.Storage.Pickers;
using System.Net.Mime;

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

            // 加载自己（？）
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

        private async Task<int> AutoLogin()
        {
            // 禁用元素以防更改

            string serverURL = await ReadFileAsync("User/serverURL.txt");
            string userEmail = await ReadFileAsync("User/userEmail.txt");
            string userPassword = await ReadFileAsync("User/userPassword.txt");
            if (serverURL.StartsWith("Error") || userEmail.StartsWith("Error") || userPassword.StartsWith("Error"))
            {
                return 404;
            }
            else
            {
                string userToken = await UserPasswordLogin(serverURL, userEmail, userPassword, "token");
                if (userToken != null)
                {
                    string setupStatus = await setupServerWindow(serverURL, userToken, "false", true);
                    Debug.WriteLine("AutoLogin setupServerWindow completed");

                    // string setupStatus = "Successful";

                    if (setupStatus.StartsWith("Success"))
                    {
                        return 200;
                    }
                    else
                    {
                        return 500;
                    }
                }
                else
                {
                    return 401;
                }
            }
        }

        public bool FirstStarted = false;

        // featuresNV: 主面板。
        // SelectionChanged: 选中项更改。
        private async void featuresNV_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
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
                            // 舍去 ContentDialog
                            // string ShowLoadingRingStatus = await InOtherPagesAsync("HomePage", "ShowLoadingRing");
                            // Debug.WriteLine(ShowLoadingRingStatus);
                            if (FirstStarted != true)
                            {
                                string showLoadingRingStatus = await ChangeOtherPagesAsync("HomePage", "LoadingRing", "Visibility", Visibility.Visible);
                                Debug.WriteLine(showLoadingRingStatus);
                                string serverURL = await ReadFileAsync("User/serverURL.txt");
                                string Email = await ReadFileAsync("User/userEmail.txt");
                                string Password = await ReadFileAsync("User/userPassword.txt");
                                string userToken = await UserPasswordLogin(serverURL, Email, Password, "token");
                                await SaveFileAsync(userToken, "user/userToken.txt");
                                await setupServerWindow(serverURL, userToken, "false", false);
                                string hideLoadingRingStatus = await ChangeOtherPagesAsync("HomePage", "LoadingRing", "Visibility", Visibility.Collapsed);
                                Debug.WriteLine(hideLoadingRingStatus);
                            }
                            else
                            {
                                FirstStarted = false;
                                Debug.WriteLine("FirstStart = true dectected.");
                                Debug.WriteLine("Skipped featuresNV_SelectionChanged except Navigate");
                            }
                            // 舍去 ContentDialog
                            // string HideLoadingRingStatus = await InOtherPagesAsync("HomePage", "HideLoadingRing");
                            // Debug.WriteLine(HideLoadingRingStatus);
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
                    featuresNV.SelectedItem = selectedItem;
                    
                }
            }
        }

        // UpdateFeaturesNVSize: 实时保证 featuresNV 的 UI 的全屏
        private void UpdateFeaturesNVSize()
        {
            if (featuresNV != null)
            {
                if (this.Bounds.Width > 15 && this.Bounds.Height > 40) 
                {
                    featuresNV.Width = this.Bounds.Width - 10;
                    featuresNV.Height = this.Bounds.Height - 35;
                }
                else
                {
                    featuresNV.Width = this.Bounds.Width;
                    featuresNV.Height = this.Bounds.Height;
                }
            }
        }

        // featuresNV_BackRequested: 处理向后导航的按钮事件
        private void featuresNV_BackRequested(NavigationView sender, NavigationViewBackRequestedEventArgs args)
        {
            if (contentFrame.CanGoBack)
            {
                contentFrame.GoBack();
                UpdateSelectedItem();
            }
        }

        private async void UpdateSelectedItem()
        {
            foreach (NavigationViewItemBase item in featuresNV.MenuItems)
            {
                if (item is NavigationViewItem navItem && navItem.Tag.ToString() == contentFrame.CurrentSourcePageType.Name)
                {
                    featuresNV.SelectedItem = navItem;
                    Debug.WriteLine($"navItem:{navItem}");
                    if (navItem.Tag.ToString().Equals("HomePage"))
                    {
                        string Token = await ReadFileAsync("User/userToken.txt");
                        string serverURL = await ReadFileAsync("User/serverURL.txt");
                        await setupServerWindow(serverURL, Token, "false", false);
                    }
                    break;
                }
            }
        }

        private async void checkServerButton_Click(object sender, RoutedEventArgs e)
        {
            checkServerButton.IsEnabled = false;
            serverURLBox.IsEnabled = false;
            AutoLoginServerButton.IsEnabled = false;

            // Debug 显示分行
            Debug.WriteLine("========================");

            LoadingRing.Visibility = Visibility.Visible;

            // 判断地址是否为空
            if (serverURLBox.Text != string.Empty)
            {

                // 正常时

                // 显示 serverURL
                string serverURL = await ReadFileAsync("User/serverURL.txt");
                serverURLTextBlock.Text = serverURL;

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
                    checkServerButton.IsEnabled = true;
                    serverURLBox.IsEnabled = true;
                    AutoLoginServerButton.IsEnabled = true;
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
                    checkServerButton.IsEnabled = true;
                    serverURLBox.IsEnabled = true;
                    AutoLoginServerButton.IsEnabled = true;
                }
                else
                {
                    Debug.WriteLine("This is checkServerButton speaking ");
                    Debug.WriteLine("getServerName bio returns a good one:" + serverBio);
                    serverBioTextBlock.Text = serverBio;

                    userPasswordLoginButton.IsEnabled = true;
                    userEmailBox.IsEnabled = true;
                    userPasswordBox.IsEnabled = true;
                    AutoLoginPasswordButton.IsEnabled = true;

                    string saveURLStatus =  await SaveFileAsync(serverURLBox.Text, "User/serverURL.txt");
                    if (saveURLStatus.StartsWith("Success"))
                    {
                        Debug.WriteLine("This is checkServerButton speaking");
                        Debug.WriteLine("Save server URL success");
                    }
                    else
                    {
                        Debug.WriteLine("This is checkServerButton speaking");
                        Debug.WriteLine("Error when saving server URL.");
                        Debug.WriteLine("The error message is shown below:");
                        Debug.WriteLine(saveURLStatus);
                    }
                }
                LoadingRing.Visibility = Visibility.Collapsed;
            }
            else
            {
                LoadingRing.Visibility = Visibility.Collapsed;
                Debug.WriteLine("Server URL seems to be nothing");
                checkServerButton.IsEnabled = true;
                serverURLBox.IsEnabled = true;
                AutoLoginServerButton.IsEnabled = true;
                return;
            }
        }

        private async Task<object> PostRequestAsync(string url, string apiKey, string content, string returnType)
        {
            using (HttpClient client = new HttpClient())
            {
                // 设定 Header
                // 一般的 accept: "application/json; charset=utf-8"
                client.DefaultRequestHeaders.Add("accept", "application/json; charset=utf-8");
                if (!string.IsNullOrEmpty(apiKey))
                {
                    client.DefaultRequestHeaders.Add("X-API-Key", apiKey);
                }

                // 设内容为 UTF-8
                // 一般的编码格式："application/json"
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

        // PostRequestAsyncWithContentType: 专为发消息设计的 PostRequestAsync

        public static async Task<int> PostRequestWithCurlAsync(string url, string contentType, string content, string apiKey)
        {
            // 创建 cURL 命令
            string curlCommand = $"curl -X POST \"{url}\" -H \"Content-Type: {contentType}\" -H \"accept: application/json; charset=utf-8\" -d \"{content}\" -w \"%{{http_code}}\" -o /dev/null -s";

            if (!string.IsNullOrEmpty(apiKey))
            {
                curlCommand += $" -H \"X-API-Key: {apiKey}\"";
            }

            // 启动进程
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/C {curlCommand}",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.Start();

            // 读取输出
            string output = await process.StandardOutput.ReadToEndAsync();
            string error = await process.StandardError.ReadToEndAsync();

            process.WaitForExit();

            // 输出日志
            Console.WriteLine($"Output: {output}");
            Console.WriteLine($"Error: {error}");

            // 解析 HTTP 状态码
            if (int.TryParse(output, out int statusCode))
            {
                return statusCode;
            }
            else
            {
                Console.WriteLine("Failed to parse HTTP status code.");
                return -1; // 返回一个错误码
            }
        }



        private async Task<int> PostRequestAsyncWithContentType(string url, string contentType, string content, string apiKey)
        {
            using (HttpClient client = new HttpClient())
            {
                // Create the request
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, url)
                {
                    Content = new StringContent(content)
                };
                request.Content.Headers.ContentType = new MediaTypeHeaderValue(contentType);
                request.Version = HttpVersion.Version10; // Set HTTP version to 1.0

                Debug.WriteLine($"Content: {content}");

                // Set headers
                request.Headers.Add("accept", "application/json");
                request.Headers.Add("accept-charset", "utf-8");

                // Generate Referer header from the URL
                Uri uri = new Uri(url);
                string refererUrl = $"{uri.Scheme}://{uri.Host}";
                request.Headers.Add("Referer", refererUrl);

                if (!string.IsNullOrEmpty(apiKey))
                {
                    request.Headers.Add("X-API-Key", apiKey);
                }

                Debug.WriteLine("Request: ");
                Debug.WriteLine(request);

                // Send the request
                HttpResponseMessage response = await client.SendAsync(request);
                Debug.WriteLine($"Response: {response}");
                Debug.WriteLine($"Status code:{response.StatusCode}");

                // Return the status code
                return (int)response.StatusCode;
            }
        }

        // RenewToken: 输入 服务器地址、Token 与 Refresh_Token，得到新的 Token 与 Refresh_Token。
        // 可能不会使用，因为 UserPasswordLogin 够用了，而这个真的烦
        public async Task<string> RenewToken(string serverURL, string refreshToken, string token)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    // 设定 Header
                    client.DefaultRequestHeaders.Add("accept", "application/json; charset=utf-8");

                    // 构建请求内容
                    var requestBody = $"{{\"token\":\"{token}\",\"refresh_token\":\"{refreshToken}\"}}";
                    HttpContent httpContent = new StringContent(requestBody, Encoding.UTF8, "application/json");

                    // 发送请求
                    HttpResponseMessage response = await client.PostAsync($"{serverURL}/api/token/renew", httpContent);

                    // 确保请求成功
                    response.EnsureSuccessStatusCode();

                    // 返回响应内容
                    return await response.Content.ReadAsStringAsync();
                }
            }
            catch (Exception ex) 
            {
                Debug.WriteLine("This is RenewToken speaking");
                Debug.WriteLine("Error. The error message is shown below:");
                Debug.WriteLine(ex.Message);
                return ($"Error: {ex.Message}");
            }
        }

        public async Task<object> GetRequestAsync(string url, string apiKey, string returnType)
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
                    if (returnType == "text")
                    {
                        // 返回获得的内容
                        return await response.Content.ReadAsStringAsync();
                    }
                    else
                    {
                        // 返回状态码
                        return (int)response.StatusCode;
                    }
                }
                else
                {
                    if (returnType == "text")
                    {
                        // 返回空文本
                        return string.Empty;
                    }
                    else
                    {
                        // 返回状态码
                        return (int)response.StatusCode;
                    }
                }
            }
        }

        public async Task<BitmapImage> GetRequestImageAsync(string url, string apiKey)
        {
            using (HttpClient client = new HttpClient())
            {
                // 设定 Header
                client.DefaultRequestHeaders.Add("accept", "image/*");
                client.DefaultRequestHeaders.Add("X-API-Key", apiKey);

                // 请求 URL
                HttpResponseMessage response = await client.GetAsync(url);

                // 对是否成功做出判断
                if (response.IsSuccessStatusCode)
                {
                    // 获取图像的字节数组
                    byte[] imageData = await response.Content.ReadAsByteArrayAsync();

                    // 将字节数组转换为 MemoryStream
                    using (MemoryStream ms = new MemoryStream(imageData))
                    {
                        // 创建 BitmapImage 对象
                        BitmapImage bitmapImage = new BitmapImage();
                        await bitmapImage.SetSourceAsync(ms.AsRandomAccessStream());
                        return bitmapImage;
                    }
                }
                else
                {
                    // 返回 null 表示请求失败
                    return null;
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
                string response = (string) await GetRequestAsync(url, apiKey, "text");

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
                userEmailBox.IsEnabled = false;
                userPasswordBox.IsEnabled = false;
                userPasswordLoginButton.IsEnabled = false;
                AutoLoginPasswordButton.IsEnabled = false;

                LoadingRing.Visibility = Visibility.Visible;

                // 获取 Token
                string Token = await UserPasswordLogin(serverURLBox.Text, userEmailBox.Text, userPasswordBox.Password, "token");

                // 调用 setupServerWindow
                string setupStatus = await setupServerWindow(serverURLBox.Text, Token, "true", true);
                if (setupStatus.StartsWith("Success"))
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
                    userPasswordLoginButton.IsEnabled = true;
                    userEmailBox.IsEnabled = true;
                    userPasswordBox.IsEnabled = true;
                    AutoLoginPasswordButton.IsEnabled = true;
                }
                LoadingRing.Visibility = Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                LoadingRing.Visibility = Visibility.Collapsed;
                Debug.WriteLine("This is userPasswordLoginButton speaking");
                Debug.WriteLine("Error: " + ex);
            }
        }

        private async Task<string> UserPasswordLogin(string serverURL, string Email, string Password, string Type)
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
            string URL = serverURL + "/api/token/login";

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
                string saveTokenStatus = await SaveFileAsync(Returned, "User/userToken.txt");
                if(saveTokenStatus.StartsWith("Success"))
                {
                    Debug.WriteLine("Save user token success");
                }
                else
                {
                    Debug.WriteLine("Save user token failed with the error message below:");
                    Debug.WriteLine(saveTokenStatus);
                }
                return Returned;
            }
            else
            {
                int TryAgain = (int)await PostRequestAsync(URL, "", Json, "int");
                return ("Error: " + $"TryAgain");
            }
        }


        private async Task<string> setupServerWindow(string serverURL, string Token, string Save, bool FirstTime)
        {
            try
            {
                if (FirstTime == true)
                {
                    FirstStarted = true;
                }
                else
                {
                    FirstStarted = false;
                }
                // 获取服务器所有信息
                string serverInfo = (string) await GetRequestAsync($"{serverURL}/api/admin/system/organization", "", "text");
                string serverName = JsonDecode(serverInfo, "name");
                Debug.WriteLine($"serverName: {serverName}");
                string serverBio = JsonDecode(serverInfo, "description");
                Debug.WriteLine($"serverBio: {serverBio}");
                string serverVersion = (string)await GetRequestAsync($"{serverURL}/api/admin/system/version", "", "text");
                Debug.WriteLine($"serverVersion: {serverVersion}");

                // 获取当前用户的所有信息
                string UserInfo = (string) await GetRequestAsync($"{serverURL}/api/user/me", Token, "text");
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

                if (Save == "true")
                {
                    // 保存用户名与密码
                    string saveEmailStatus = await SaveFileAsync(Email, "User/userEmail.txt");
                    Debug.WriteLine($"saveEmailStatus:{saveEmailStatus}");
                    string savePasswordStatus = await SaveFileAsync(userPasswordBox.Password, "User/userPassword.txt");
                    Debug.WriteLine($"savePasswordStatus:{savePasswordStatus}");
                }
                else
                {
                    Debug.WriteLine($"Skipped save email & password because the string Save = {Save}");
                }                

                // 更改主面版的主页设置
                string NVSetPageStatus = NVSetPage(featuresNV, "HomePage");

                if (NVSetPageStatus.StartsWith("Success"))
                {
                    Debug.WriteLine("This is setupServerWindow speaking");
                    Debug.WriteLine("NVSetPage returns Success");
                }
                else
                {
                    Debug.WriteLine("This is setupServerWindow speaking");
                    Debug.WriteLine("NVSetPage returns an error.");
                    Debug.WriteLine("The error message is shown below:");
                    Debug.WriteLine(NVSetPageStatus);
                }


                // 更改主面板的可视状态
                featuresNV.Visibility = Visibility.Visible;
                SP_CheckServer.Visibility = Visibility.Collapsed;
                SP_Login.Visibility = Visibility.Collapsed;
                SP_Logo.Visibility = Visibility.Collapsed;
                SP_ServerName.Visibility = Visibility.Collapsed;


                // 更改服务器名称
                string ChangeOtherPagesStatus = await ChangeOtherPagesAsync("HomePage", "serverNameTextBlock", "Text", serverName);
                if (ChangeOtherPagesStatus.StartsWith("Success"))
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
                if (ChangeOtherPages_Bio_Status.StartsWith("Success"))
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

                // 更改服务器版本号
                string ChangeOtherPages_Version_Status = await ChangeOtherPagesAsync("HomePage", "serverVersionTextBlock", "Text", $"VoceChat Server Version: {serverVersion}");
                if (ChangeOtherPages_Version_Status.StartsWith("Success"))
                {
                    Debug.WriteLine("This is setupServerWindow speaking");
                    Debug.WriteLine("ChangeOtherPages returns Success");
                }
                else
                {
                    Debug.WriteLine("This is setupServerWindow speaking");
                    Debug.WriteLine("ChangeOtherPages returns an error.");
                    Debug.WriteLine("The error message is shown below:");
                    Debug.WriteLine(ChangeOtherPages_Version_Status);
                }

                // 更改服务器 URL
                string ChangeOtherPages_URL_Status = await ChangeOtherPagesAsync("HomePage", "serverURLTextBlock", "Text", $"Using server {serverURL}");
                if (ChangeOtherPages_URL_Status.StartsWith("Success"))
                {
                    Debug.WriteLine("This is setupServerWindow speaking");
                    Debug.WriteLine("ChangeOtherPages returns Success");
                }
                else
                {
                    Debug.WriteLine("This is setupServerWindow speaking");
                    Debug.WriteLine("ChangeOtherPages returns an error.");
                    Debug.WriteLine("The error message is shown below:");
                    Debug.WriteLine(ChangeOtherPages_URL_Status);
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

        public string NVSetPage(NavigationView navigationView, string pageName)
        {
            try
            {
                foreach (var item in navigationView.MenuItems)
                {
                    if (item is NavigationViewItem navItem && navItem.Tag.ToString() == pageName)
                    {
                        // 更新左侧 Pane
                        navigationView.SelectedItem = navItem;

                        // 调试输出
                        Debug.WriteLine($"Navigating to page: {pageName}");

                        Type pageType;
                        if (pageName == "HomePage")
                        {
                            pageType = typeof(HomePage);
                        }
                        if (pageName == "SettingsPage")
                        {
                            pageType = typeof(SettingsPage);
                        }
                        if (pageName == "FilesPage")
                        {
                            pageType = typeof(FilesPage);
                        }
                        if (pageName == "PeoplePage")
                        {
                            pageType = typeof(PeoplePage);
                        }
                        if (pageName == "FavouritesPage")
                        {
                            pageType = typeof(FavouritesPage);
                        }
                        else
                        {
                            var fullPageName = $"{pageName}";
                            pageType = Type.GetType(fullPageName);
                        }
                        if (pageType == null)
                        {
                            Debug.WriteLine("This is NVSetPage speaking");
                            Debug.WriteLine("Error: Page type not found");
                            return $"Error: Page type {pageName} not found";
                        }

                        contentFrame.Navigate(pageType);
                        return "Success";
                    }
                }
                Debug.WriteLine("This is NVSetPage speaking");
                Debug.WriteLine("Error: Page Not Found.");
                return $"Error: Page {pageName} in {navigationView} not found";
            }
            catch (Exception ex)
            {
                Debug.WriteLine("This is NVSetPage speaking");
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


        private async Task<string> InOtherPagesAsync(string pageName, string methodName)
        {
            // 遍历 featuresNV 的 MenuItems 以找到对应的页面
            foreach (var item in featuresNV.MenuItems)
            {
                if (item is NavigationViewItem navItem && navItem.Tag.ToString() == pageName)
                {
                    // 获取页面类型
                    var pageType = Type.GetType(pageName);
                    if (pageType == null)
                    {
                        return $"Page type {pageName} not found.";
                    }

                    // 确保 contentFrame 中已经加载了正确的页面
                    if (contentFrame.Content is Page page && page.GetType() == pageType)
                    {
                        // 获取方法信息
                        var methodInfo = pageType.GetMethod(methodName);
                        if (methodInfo == null)
                        {
                            return $"Method {methodName} not found on page {pageName}.";
                        }

                        // 调用方法
                        methodInfo.Invoke(page, null);

                        return $"Successfully called {methodName} on {pageName}.";
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

        private async Task<string> ReadFileAsyncFromOtherLocations(string path)
        {
            try
            {
                string fullPath = Path.Combine(path);
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

        private async Task<string> SendMessageTo(string Token, string Content, string Type, string UID)
        {
            try
            {
                string serverURL = await ReadFileAsync("User/serverURL.txt");
                string APIURL = $"{serverURL}/api/user/{UID}/send";
                Debug.WriteLine($"Send Mssage API URL: {APIURL}");
                int status = (int)await PostRequestAsyncWithContentType(APIURL, Type, Content, Token);
                if (status == 200)
                {
                    Debug.WriteLine("Send Success");
                    return "Success";
                }
                else
                {
                    Debug.WriteLine($"Send error HTTP code: {status}");
                    return ($"Error. {status}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine ($"SendMessageTo Error: {ex.Message}");
                return ($"Error: {ex.Message}"); 
            }
        }


        // 重要方法：OtherPagesEvents()
        // 分发任务
        // 输入：标识符
        // 成功输出得到的信息，失败输出Error
        public async Task<string> OtherPagesEvents(string eventName)
        {
            switch (eventName)
            {
                case "SearchButtonClicked":
                    return await HandleSearchButtonClickAsync();
                case "SendNormalMessage":
                    string serverURL = await ReadFileAsync("User/serverURL.txt");
                    string userEmail = await ReadFileAsync("User/userEmail.txt");
                    string userPassword = await ReadFileAsync("User/userPassword.txt");
                    string Token = await UserPasswordLogin(serverURL, userEmail, userPassword, "token");
                    string Content = await GetOtherPages("PeoplePage", "SendNormalMessageTextBox", "Text");
                    string UID = await GetOtherPages("PeoplePage", "ChatHeadingUIDTextBlock", "Text");
                    await ChangeOtherPagesAsync("PeoplePage", "SendNormalMessageTextBox", "Text", "");
                    return (await SendMessageTo(Token, Content, "text/plain", UID));
                case "SendMarkdownMessage":
                    serverURL = await ReadFileAsync("User/serverURL.txt");
                    userEmail = await ReadFileAsync("User/userEmail.txt");
                    userPassword = await ReadFileAsync("User/userPassword.txt");
                    Token = await UserPasswordLogin(serverURL, userEmail, userPassword, "token");
                    Content = await GetOtherPages("PeoplePage", "SendNormalMessageTextBox", "Text");
                    UID = await GetOtherPages("PeoplePage", "ChatHeadingUIDTextBlock", "Text");
                    await ChangeOtherPagesAsync("PeoplePage", "SendNormalMessageTextBox", "Text", "");
                    return (await SendMessageTo(Token, Content, "text/markdown", UID));
                case "SendFile":
                    try
                    {
                        string uploadResponse = await UploadFileAsync();
                        if (!uploadResponse.StartsWith("Error"))
                        {
                            // 正确的判断时
                            string filepath = JsonDecode(uploadResponse, "path");
                            int status = await SendFileToWithSendMessageTo(filepath);
                            if (status == 200)
                            {
                                return ("Success");
                            }
                            else
                            {
                                return ($"Error: {status}");
                            }
                        }
                        else
                        {
                            return ("Error");
                        }
                    }
                    catch (Exception ex)
                    {
                        return(ex.Message);
                    }
                    return ("");
                default:
                    Debug.WriteLine($"OtherPagesEvents got an unknown event {eventName}");
                    return "Error. Unknown event: " + eventName;
            }
        }

        private async Task<int> SendFileToWithSendMessageTo(string Path)
        {
            try
            {
                string userToken = await ReadFileAsync("User/userToken.txt");
                string UID = await GetOtherPages("PeoplePage", "ChatHeadingUIDTextBlock", "Text");
                string sendMessageStatus = await SendMessageTo(userToken, $"{{\"path\": \"{Path}\"}}", "vocechat/file", UID);
                int sendMessageStatus_int = 0;
                int.TryParse(sendMessageStatus, out sendMessageStatus_int);
                return (sendMessageStatus_int);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return (0);
            }
        }

        private static string JsonEncode(string head, string body)
        {
            var jsonObject = new Dictionary<string, string>
        {
            { head, body }
        };

            return JsonSerializer.Serialize(jsonObject);
        }

        private async Task<string> HandleSearchButtonClickAsync()
        {
            try
            {
                await ChangeOtherPagesAsync("PeoplePage", "SelectedUserAvatarRing", "Visibility", Visibility.Visible);
                await ChangeOtherPagesAsync("PeoplePage", "SelectedUserImage", "Visibility", Visibility.Collapsed);
                Debug.WriteLine("This is HandleSearchButtonClickAsync speaking");
                string serverURL = await ReadFileAsync("User/serverURL.txt");
                Debug.WriteLine($"serverURL: {serverURL}");
                string userToken = await ReadFileAsync("User/userToken.txt");
                Debug.WriteLine($"userToken: {userToken}");
                string ListUsersAPIURL = $"{serverURL}/api/user";
                Debug.WriteLine($"ListUserAPIURL: {ListUsersAPIURL}");
                string UserList = (string) await GetRequestAsync(ListUsersAPIURL, userToken, "text");
                Debug.WriteLine($"UserList: {UserList}");
                string UID = await GetOtherPages("PeoplePage", "SearchTextBox", "Text");
                Debug.WriteLine($"UID: {UID}");

                string UserName = await JsonGetPropertyWithKey(UserList, "uid", UID, "name");
                Debug.WriteLine($"UserName: {UserName}");
                string UserEmail = await JsonGetPropertyWithKey(UserList, "uid", UID, "email");
                Debug.WriteLine($"UserEmail: {UserEmail}");

                if(UserName != "" && UserEmail != "")
                {
                    await ChangeOtherPagesAsync("PeoplePage", "SelectedUserUIDTextBlock", "Text", "#" + UID);
                    await ChangeOtherPagesAsync("PeoplePage", "SelectedUserNameTextBlock", "Text", UserName);
                    await ChangeOtherPagesAsync("PeoplePage", "ChatHeadingTextBlock", "Text", UserName);
                    await ChangeOtherPagesAsync("PeoplePage", "SelectedUserEmailTextBlock", "Text", UserEmail);
                    await ChangeOtherPagesAsync("PeoplePage", "ChatHeadingUIDTextBlock", "Text", UID);
                }
                else
                {
                    Debug.WriteLine("UID invaild");
                }

                string UserAvatarAPIURL = $"{serverURL}/api/resource/avatar?uid={UID}";
                Debug.WriteLine($"ListUserAPIURL: {UserAvatarAPIURL}");
                
                int AvatarStatus = (int) await GetRequestAsync(UserAvatarAPIURL,userToken, "int");
                if (AvatarStatus == 200)
                {
                    BitmapImage UserAvatar = await GetRequestImageAsync(UserAvatarAPIURL, userToken);
                    string ChangeOtherPagesAvatarStatus = await ChangeOtherPagesAsync("PeoplePage", "SeletedUserImage", "Source", UserAvatar);
                    Debug.WriteLine($"Change Avatar Status: {ChangeOtherPagesAvatarStatus}");

                }
                else
                {
                    BitmapImage UserAvatar = await GetRequestImageAsync("https://lcnt-pictures.lukezhang.win/UserAvatar/default.png", "");
                    string ChangeOtherPagesAvatarStatus = await ChangeOtherPagesAsync("PeoplePage", "SeletedUserImage", "Source", UserAvatar);
                    Debug.WriteLine($"Change Avatar Status: {ChangeOtherPagesAvatarStatus}");
                }
                await ChangeOtherPagesAsync("PeoplePage", "SelectedUserAvatarRing", "Visibility", Visibility.Collapsed);
                await ChangeOtherPagesAsync("PeoplePage", "SelectedUserImage", "Visibility", Visibility.Visible);
                return "Success: Search button click handled";
            }
            catch(Exception ex)
            {
                await ChangeOtherPagesAsync("PeoplePage", "SelectedUserAvatarRing", "Visibility", Visibility.Collapsed);
                await ChangeOtherPagesAsync("PeoplePage", "SelectedUserImage", "Visibility", Visibility.Visible);
                return ($"Error:{ex.Message}");
            }
        }

        private async Task<BitmapImage> GetImageAsync(string relativePath)
        {
            try
            {
                StorageFolder localFolder = ApplicationData.Current.LocalFolder;
                StorageFile file = await localFolder.GetFileAsync(relativePath);
                using (var stream = await file.OpenAsync(FileAccessMode.Read))
                {
                    BitmapImage image = new BitmapImage();
                    await image.SetSourceAsync(stream);
                    return image;
                }

            }
            catch(Exception ex)
            {
                Debug.WriteLine("This is GetImageAsync speaking");
                Debug.WriteLine("Error.The error message is shown below:");
                Debug.WriteLine(ex.Message);
                return null;
            }
        }

        private string ConvertUtf8ToBase64(string utf8String)
        {
            byte[] utf8Bytes = System.Text.Encoding.UTF8.GetBytes(utf8String);
            return Convert.ToBase64String(utf8Bytes);
        }

        public BitmapImage StringToImage(string base64String)
        {
            try
            {
                // 移除可能的无效字符
                base64String = base64String.Trim().Replace(" ", "").Replace("\r", "").Replace("\n", "");

                byte[] imageBytes = Convert.FromBase64String(base64String);
                using (var ms = new MemoryStream(imageBytes))
                {
                    var image = new BitmapImage();
                    ms.Position = 0;
                    image.SetSourceAsync(ms.AsRandomAccessStream()).AsTask().Wait();
                    return image;
                }
            }
            catch (FormatException ex)
            {
                Debug.WriteLine($"Error: Invalid Base64 string. {ex.Message}");
                return null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error: {ex.Message}");
                return null;
            }
        }

        private void SetImageSource(Microsoft.UI.Xaml.Controls.Image imageControl, string base64String)
        {
            var imageHelper = new MainWindow();
            BitmapImage bitmapImage = imageHelper.StringToImage(base64String);

            if (bitmapImage != null)
            {
                imageControl.Source = bitmapImage;
                Debug.WriteLine("Image converted and set successfully.");
            }
            else
            {
                Debug.WriteLine("Failed to convert string to image.");
            }
        }

        // 重要方法：JsonGetPropertyWithKey()
        // 输入 JSON 原内容，用于辨别的属性名称，用于辨别的属性值，要得到的属性值的属性名称
        // 输出 得到的属性值

        private async Task<string> JsonGetPropertyWithKey(string jsonContent, string propertyName, string key, string getPropertyName)
        {
            try
            {
                var jsonArray = JsonDocument.Parse(jsonContent).RootElement;

                foreach (var element in jsonArray.EnumerateArray())
                {
                    if (element.TryGetProperty(propertyName, out JsonElement value) && value.ToString() == key)
                    {
                        if (element.TryGetProperty(getPropertyName, out JsonElement result))
                        {
                            return await Task.FromResult(result.ToString());
                        }
                    }
                }

                return await Task.FromResult<string>(null);
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }

        // 重要方法：GetOtherPages()

        private async Task<string> GetOtherPages(string pageName, string elementName, string propertyName)
        {
            try
            {
                // 遍历 featuresNV 的 MenuItems 以找到对应的页面
                foreach (var item in featuresNV.MenuItems)
                {
                    if (item is NavigationViewItem navItem && navItem.Tag.ToString() == pageName)
                    {
                        // 确保 contentFrame 中已经加载了正确的页面
                        if (featuresNV.Content is Frame contentFrame)
                        {
                            if (contentFrame.Content is Page page && page.GetType().Name == pageName)
                            {
                                // 查找指定的元素
                                var element = page.FindName(elementName) as FrameworkElement;
                                if (element == null)
                                {
                                    return $"Error: Element {elementName} not found on page {pageName}.";
                                }

                                // 获取属性信息
                                var propertyInfo = element.GetType().GetProperty(propertyName);
                                if (propertyInfo == null)
                                {
                                    return $"Error: Property {propertyName} not found on element {elementName}.";
                                }

                                // 获取属性值
                                var value = propertyInfo.GetValue(element)?.ToString();
                                return value ?? $"Error: Property value is null for {propertyName} on element {elementName}.";
                            }
                            else
                            {
                                return $"Error: Loaded page is {contentFrame.Content.GetType().Name}, expected {pageName}.";
                            }
                        }
                        else
                        {
                            return $"Error: Content is not a Frame, it is {featuresNV.Content.GetType().Name}.";
                        }
                    }
                }

                return $"Error: Page {pageName} not found in featuresNV.";
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }

        private async void AutoLoginPasswordButton_Click(object sender, RoutedEventArgs e)
        {
            userEmailBox.IsEnabled = false;
            userPasswordBox.IsEnabled = false;
            userPasswordLoginButton.IsEnabled = false;
            AutoLoginPasswordButton.IsEnabled = false;

            LoadingRing.Visibility = Visibility.Visible;
            await AutoLogin();
            LoadingRing.Visibility = Visibility.Collapsed;
        }


        private async void AutoLoginServerButton_Click(object sender, RoutedEventArgs e)
        {
            serverURLBox.IsEnabled = false;
            checkServerButton.IsEnabled = false;
            AutoLoginServerButton.IsEnabled = false;

            LoadingRing.Visibility = Visibility.Visible;
            await AutoLogin();
            LoadingRing.Visibility = Visibility.Collapsed;
        }



        // PickAFile: 打开对话框让用户选择文件，并返回文件地址
        // 从 WinUI 3 Gallery 上借鉴的，稍微改了一下
        private async Task<object> PickAFile()
        {
            string Output = "";

            // Clear previous returned file name, if it exists, between iterations of this scenario
            Output = "";

            // Create a file picker
            var openPicker = new Windows.Storage.Pickers.FileOpenPicker();

            // See the sample code below for how to make the window accessible from the App class.
            var window = this;

            // Retrieve the window handle (HWND) of the current WinUI 3 window.
            var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(window);

            // Initialize the file picker with the window handle (HWND).
            WinRT.Interop.InitializeWithWindow.Initialize(openPicker, hWnd);

            // Set options for your file picker
            openPicker.ViewMode = PickerViewMode.Thumbnail;
            openPicker.FileTypeFilter.Add("*");

            // Open the picker for the user to pick a file
            var file = await openPicker.PickSingleFileAsync();
            if (file != null)
            {
                string FileData = await ReadFileAsync(file.Path);
                Output = file.Path;
                Debug.WriteLine($"Output: {Output}");
                return (file);
            }
            else
            {
                Output = "Error: Operation cancelled.";
                Debug.WriteLine(Output);
                return ("Error: " + Output);
            }
        }

        private async Task<string> UploadFileAsync()
        {
            string serverURL = await ReadFileAsync("User/serverURL.txt");
            string userEmail = await ReadFileAsync("User/userEmail.txt");
            string userPassword = await ReadFileAsync("User/userPassword.txt");
            string apiKey = await UserPasswordLogin(serverURL, userEmail, userPassword, "token");

            // Create a file picker
            var openPicker = new Windows.Storage.Pickers.FileOpenPicker();

            // See the sample code below for how to make the window accessible from the App class.
            var window = this;

            // Retrieve the window handle (HWND) of the current WinUI 3 window.
            var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(window);

            // Initialize the file picker with the window handle (HWND).
            WinRT.Interop.InitializeWithWindow.Initialize(openPicker, hWnd);

            // Set options for your file picker
            openPicker.ViewMode = PickerViewMode.Thumbnail;
            openPicker.FileTypeFilter.Add("*");

            // Open the picker for the user to pick a file
            var file = await openPicker.PickSingleFileAsync();

            if (file != null)
            {
                string file_id = "";

                using (var httpClientForPrepare =new HttpClient())
                {
                    var requestContent = new MultipartFormDataContent();
                    var fileContent = new StreamContent(await file.OpenStreamForReadAsync());

                    httpClientForPrepare.DefaultRequestHeaders.Add("X-API-Key", apiKey);

                    // 获取文件的 MIME 类型
                    var contentType = file.ContentType;
                    string prepareContentType = (string) contentType;
                    string fileName = file.Name;
                    string content = $"{{\"content_type\": \"{prepareContentType}\", \"filename\": \"{fileName}\"}}";

                    var response = await httpClientForPrepare.PostAsync($"{serverURL}/api/resource/file/prepare", requestContent);
                    if (response.IsSuccessStatusCode)
                    {
                        // Handle success
                        file_id = await response.Content.ReadAsStringAsync();
                        Debug.WriteLine($"file_id: {file_id}");
                        // return (await response.Content.ReadAsStringAsync());
                    }
                    else
                    {
                        // Handle failure
                        Debug.WriteLine ("Error when preparing: " + response.StatusCode.ToString());
                    }

                    // SOMETHINGTODISABLECODETRANSPARENT
                }

                using (var httpClient = new HttpClient())
                {

                    httpClient.DefaultRequestHeaders.Add("X-API-Key", apiKey);

                    var requestContent = new MultipartFormDataContent();
                    var fileContent = new StreamContent(await file.OpenStreamForReadAsync());

                    string Data = await ReadFileAsyncFromOtherLocations(file.Path);

                    // 获取文件的 MIME 类型
                    string contentType = "multipart/form-data";

                    fileContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);

                    requestContent.Add(fileContent, "chunk_data", Data);
                    requestContent.Add(new StringContent(file_id), "file_id");
                    requestContent.Add(new StringContent("true"), "chunk_is_last");

                    var response = await httpClient.PostAsync($"{serverURL}/api/resource/file/upload", requestContent);
                    if (response.IsSuccessStatusCode)
                    {
                        // Handle success
                        Debug.WriteLine($"Upload response: {await response.Content.ReadAsStringAsync()}");
                        Debug.WriteLine($"Upload tatus code: {response.StatusCode}");
                        return (await response.Content.ReadAsStringAsync());
                    }
                    else
                    {
                        // Handle failure
                        Debug.WriteLine($"Error when uploading: {response.StatusCode.ToString()}");
                        return($"Error: {response.StatusCode.ToString()}");
                    }
                }
            }
            else
            {
                return("Error: file = null");
            }
        }
    }
}
