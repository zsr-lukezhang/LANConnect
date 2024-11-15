// ======================== LANConnect by Luke Zhang ========================
//         �����Ŀʹ�� MainWindow.xaml.cs �������к�� ��������׸��´���
// =========================== Contribution Notes ===========================
//                                 ���õķ�����
//                             �Լ���������д�� (?)

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

            // �����Լ�������
            Assembly.Load("LANConnect");

            // ����������ȥ
            ExtendsContentIntoTitleBar = true;

            // ʵʱ���� featuresNV
            this.SizeChanged += MainWindow_SizeChanged;
            UpdateFeaturesNVSize();
        }

        private void MainWindow_SizeChanged(object sender, WindowSizeChangedEventArgs e)
        {
            UpdateFeaturesNVSize();
        } 

        private async Task<int> AutoLogin()
        {
            // ����Ԫ���Է�����

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

        // featuresNV: ����塣
        // SelectionChanged: ѡ������ġ�
        private async void featuresNV_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            // ����ҳ����
            if (args.IsSettingsSelected)
            {
                contentFrame.Navigate(typeof(SettingsPage));
            }
            // ��������ҳ��
            else
            {
                NavigationViewItem selectedItem = args.SelectedItemContainer as NavigationViewItem;
                // ʡ�ÿ�����
                if (selectedItem != null)
                {
                    // ����ѡ����
                    switch (selectedItem.Tag)
                    {
                        // ������� Tag
                        case "HomePage":
                            contentFrame.Navigate(typeof(HomePage));
                            // ��ȥ ContentDialog
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
                            // ��ȥ ContentDialog
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

        // UpdateFeaturesNVSize: ʵʱ��֤ featuresNV �� UI ��ȫ��
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

        // featuresNV_BackRequested: ������󵼺��İ�ť�¼�
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

            // Debug ��ʾ����
            Debug.WriteLine("========================");

            LoadingRing.Visibility = Visibility.Visible;

            // �жϵ�ַ�Ƿ�Ϊ��
            if (serverURLBox.Text != string.Empty)
            {

                // ����ʱ

                // ��ʾ serverURL
                string serverURL = await ReadFileAsync("User/serverURL.txt");
                serverURLTextBlock.Text = serverURL;

                // �趨����
                string serverName = "";
                string serverBio = "";

                // ������ֵ
                // �жϿ�ͷ�Ƿ�Ϊ Error
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
                // �趨 Header
                // һ��� accept: "application/json; charset=utf-8"
                client.DefaultRequestHeaders.Add("accept", "application/json; charset=utf-8");
                if (!string.IsNullOrEmpty(apiKey))
                {
                    client.DefaultRequestHeaders.Add("X-API-Key", apiKey);
                }

                // ������Ϊ UTF-8
                // һ��ı����ʽ��"application/json"
                HttpContent httpContent = new StringContent(content, Encoding.UTF8, "application/json");

                // ��������                
                HttpResponseMessage response = await client.PostAsync(url, httpContent);

                if (returnType == "text")
                {
                    return await response.Content.ReadAsStringAsync();
                }
                else
                {
                    // ����״̬��
                    return (int)response.StatusCode;
                }
            }
        }

        // PostRequestAsyncWithContentType: רΪ����Ϣ��Ƶ� PostRequestAsync

        public static async Task<int> PostRequestWithCurlAsync(string url, string contentType, string content, string apiKey)
        {
            // ���� cURL ����
            string curlCommand = $"curl -X POST \"{url}\" -H \"Content-Type: {contentType}\" -H \"accept: application/json; charset=utf-8\" -d \"{content}\" -w \"%{{http_code}}\" -o /dev/null -s";

            if (!string.IsNullOrEmpty(apiKey))
            {
                curlCommand += $" -H \"X-API-Key: {apiKey}\"";
            }

            // ��������
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

            // ��ȡ���
            string output = await process.StandardOutput.ReadToEndAsync();
            string error = await process.StandardError.ReadToEndAsync();

            process.WaitForExit();

            // �����־
            Console.WriteLine($"Output: {output}");
            Console.WriteLine($"Error: {error}");

            // ���� HTTP ״̬��
            if (int.TryParse(output, out int statusCode))
            {
                return statusCode;
            }
            else
            {
                Console.WriteLine("Failed to parse HTTP status code.");
                return -1; // ����һ��������
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

        // RenewToken: ���� ��������ַ��Token �� Refresh_Token���õ��µ� Token �� Refresh_Token��
        // ���ܲ���ʹ�ã���Ϊ UserPasswordLogin �����ˣ��������ķ�
        public async Task<string> RenewToken(string serverURL, string refreshToken, string token)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    // �趨 Header
                    client.DefaultRequestHeaders.Add("accept", "application/json; charset=utf-8");

                    // ������������
                    var requestBody = $"{{\"token\":\"{token}\",\"refresh_token\":\"{refreshToken}\"}}";
                    HttpContent httpContent = new StringContent(requestBody, Encoding.UTF8, "application/json");

                    // ��������
                    HttpResponseMessage response = await client.PostAsync($"{serverURL}/api/token/renew", httpContent);

                    // ȷ������ɹ�
                    response.EnsureSuccessStatusCode();

                    // ������Ӧ����
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
                // �趨 Header
                client.DefaultRequestHeaders.Add("accept", "*/*");
                client.DefaultRequestHeaders.Add("X-API-Key", apiKey);

                // ���� URL
                HttpResponseMessage response = await client.GetAsync(url);

                // ���Ƿ�ɹ������ж�
                if (response.IsSuccessStatusCode)
                {
                    if (returnType == "text")
                    {
                        // ���ػ�õ�����
                        return await response.Content.ReadAsStringAsync();
                    }
                    else
                    {
                        // ����״̬��
                        return (int)response.StatusCode;
                    }
                }
                else
                {
                    if (returnType == "text")
                    {
                        // ���ؿ��ı�
                        return string.Empty;
                    }
                    else
                    {
                        // ����״̬��
                        return (int)response.StatusCode;
                    }
                }
            }
        }

        public async Task<BitmapImage> GetRequestImageAsync(string url, string apiKey)
        {
            using (HttpClient client = new HttpClient())
            {
                // �趨 Header
                client.DefaultRequestHeaders.Add("accept", "image/*");
                client.DefaultRequestHeaders.Add("X-API-Key", apiKey);

                // ���� URL
                HttpResponseMessage response = await client.GetAsync(url);

                // ���Ƿ�ɹ������ж�
                if (response.IsSuccessStatusCode)
                {
                    // ��ȡͼ����ֽ�����
                    byte[] imageData = await response.Content.ReadAsByteArrayAsync();

                    // ���ֽ�����ת��Ϊ MemoryStream
                    using (MemoryStream ms = new MemoryStream(imageData))
                    {
                        // ���� BitmapImage ����
                        BitmapImage bitmapImage = new BitmapImage();
                        await bitmapImage.SetSourceAsync(ms.AsRandomAccessStream());
                        return bitmapImage;
                    }
                }
                else
                {
                    // ���� null ��ʾ����ʧ��
                    return null;
                }
            }
        }

        private async Task<string> getServerName(string URL, string type)
        {
            try
            {
                // �趨���ڵ��� GetRequestAsync �ı���ֵ
                string url = URL + "/api/admin/system/organization";
                string apiKey = "";

                // �������趨����ֵ
                string response = (string) await GetRequestAsync(url, apiKey, "text");

                // ������Ӧ����
                Debug.WriteLine("This is getServerName speaking");
                Debug.WriteLine($"Response Content: {response}");

                // ���жϷ�Ϊ������Ӧ
                if (response == null) // ���ж��Ƿ�Ϊ������
                {
                    Debug.WriteLine("This is getServerName speaking");
                    Debug.WriteLine("null: Server not found");

                    // ��ʾ���������ؿ�ֵ
                    return ("Error: Server returns null");
                }
                else
                {
                    // ��������
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
                // ������
                Debug.WriteLine("This is getServerName speaking");
                Debug.WriteLine("Error: " + ex.Message);
                return ($"Error: {ex.Message}");
            }
        }

        private async Task<int> changeServerName(string url, string apiKey, string newServerName, string NewServerBio)
        {
            try
            {
                // �趨���ڵ��� PostRequestAsync �ı���ֵ
                // �趨 JSON ԭ����
                var Json = $@"{{
    ""name"": ""{newServerName}"",
    ""description"": ""{NewServerBio}""
}}";

                // �������趨����ֵ
                int statusCode = (int)await PostRequestAsync(url, apiKey, Json, "int");

                // ����״̬��
                Debug.WriteLine($"HTTP Status Code: {statusCode}");
                return (statusCode);
            }
            catch (Exception ex)
            {
                // �����쳣��������ʾ������Ϣ
                Debug.WriteLine($"Error: {ex.Message}");
                return (0);
            }
        }

        public static string JsonEncode(string text)
        {
            return JsonSerializer.Serialize(text);
        }



        // JsonDecode: ������Ҫ������ı�,Ҫ���ҵ����ԣ����ָ�������Ե�ֵ��
        private string JsonDecode(string jsonString, string key)
        {
            // ����JSON�ַ���
            JsonDocument jsonDocument = JsonDocument.Parse(jsonString);
            JsonElement root = jsonDocument.RootElement;

            // ���Ի�ȡָ������ֵ
            if (root.TryGetProperty(key, out JsonElement valueElement))
            {
                // ����ֵ�����ͷ����ַ�����ʽ��ֵ
                switch (valueElement.ValueKind)
                {
                    case JsonValueKind.String:
                        return valueElement.GetString();
                    case JsonValueKind.Number:
                        return valueElement.GetRawText(); // ֱ�ӷ�����ֵ���ַ�����ʽ
                    case JsonValueKind.True:
                    case JsonValueKind.False:
                        return valueElement.GetBoolean().ToString(); // ���� "True" �� "False"
                    case JsonValueKind.Null:
                        return "null"; // ���� "null" �ַ���
                    case JsonValueKind.Object:
                        return valueElement.GetRawText(); // ���ض�����ַ�����ʽ
                    case JsonValueKind.Array:
                        return valueElement.GetRawText(); // ����������ַ�����ʽ
                                                          // ���Ը�����Ҫ��Ӹ������ʹ���
                }
            }

            // ����������ڣ����ؿ��ַ���
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

                // ��ȡ Token
                string Token = await UserPasswordLogin(serverURLBox.Text, userEmailBox.Text, userPasswordBox.Password, "token");

                // ���� setupServerWindow
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
            // �趨 JSON ԭ����
            var Json = $@"{{
    ""credential"": {{
        ""email"": ""{Email}"",
        ""password"": ""{Password}"",
        ""type"": ""password""
    }},
    ""device"": ""web"",
    ""device_token"": null
}}";

            // ���� PostRequestAsync ���趨����
            string URL = serverURL + "/api/token/login";

            // HTTP Status Code
            int Code = (int)await PostRequestAsync(URL, "", Json, "int");

            // ��ȡAPI���ص�����
            string Content = (string)await PostRequestAsync(URL, "", Json, "text");

            Debug.WriteLine("This is UserPasswordLogin speaking");
            Debug.WriteLine("PostRequestAsync returned the following content:");
            Debug.WriteLine(Content);
            Debug.WriteLine("With the following HTTP status code: " + $"{Code}");

            // ���� HTTP Status Code����ȷ��
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
                // ��ȡ������������Ϣ
                string serverInfo = (string) await GetRequestAsync($"{serverURL}/api/admin/system/organization", "", "text");
                string serverName = JsonDecode(serverInfo, "name");
                Debug.WriteLine($"serverName: {serverName}");
                string serverBio = JsonDecode(serverInfo, "description");
                Debug.WriteLine($"serverBio: {serverBio}");
                string serverVersion = (string)await GetRequestAsync($"{serverURL}/api/admin/system/version", "", "text");
                Debug.WriteLine($"serverVersion: {serverVersion}");

                // ��ȡ��ǰ�û���������Ϣ
                string UserInfo = (string) await GetRequestAsync($"{serverURL}/api/user/me", Token, "text");
                Debug.WriteLine("This is setupServerWindow speaking");
                Debug.WriteLine("UserInfo:");
                Debug.WriteLine(UserInfo);

                // �ֱ��ȡ���е���Ϣ
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
                    // �����û���������
                    string saveEmailStatus = await SaveFileAsync(Email, "User/userEmail.txt");
                    Debug.WriteLine($"saveEmailStatus:{saveEmailStatus}");
                    string savePasswordStatus = await SaveFileAsync(userPasswordBox.Password, "User/userPassword.txt");
                    Debug.WriteLine($"savePasswordStatus:{savePasswordStatus}");
                }
                else
                {
                    Debug.WriteLine($"Skipped save email & password because the string Save = {Save}");
                }                

                // ������������ҳ����
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


                // ���������Ŀ���״̬
                featuresNV.Visibility = Visibility.Visible;
                SP_CheckServer.Visibility = Visibility.Collapsed;
                SP_Login.Visibility = Visibility.Collapsed;
                SP_Logo.Visibility = Visibility.Collapsed;
                SP_ServerName.Visibility = Visibility.Collapsed;


                // ���ķ���������
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

                // ���ķ��������
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

                // ���ķ������汾��
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

                // ���ķ����� URL
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

                // ������ȷ��״̬
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

        // DocodeTime: ����ʱ����������ʽ����ʱ��
        private static string TimestampDecode(long timestamp)
        {
            // ��ʱ����Ӻ���ת��Ϊ��
            DateTime decodedTime = DateTimeOffset.FromUnixTimeMilliseconds(timestamp).DateTime;

            // ���ظ�ʽ��������ʱ���ַ���
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

                // ���� JSON ����
                var contacts = JsonSerializer.Deserialize<List<Contact>>(jsonContent);

                // �����µ� DataTemplate
                string xamlTemplate = @"
                <DataTemplate xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'>
                    <StackPanel>
                    <TextBlock Text='{Binding TargetInfo.Name}' />
                    <TextBlock Text='{Binding TargetInfo.Email}' />
                    </StackPanel>
                </DataTemplate>";

                var dataTemplate = (DataTemplate)XamlReader.Load(xamlTemplate);

                // ����ҳ�����Դ�ֵ�
                targetPage.Resources["ListViewTemplate"] = dataTemplate;

                // ��ȡ ListBox ����������Դ
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
                        // ������� Pane
                        navigationView.SelectedItem = navItem;

                        // �������
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

        // ��Ҫ������
        // ChangeOtherPagesAsync ����
        // ���� ҳ������Ԫ��������������Ҫ����Ϊ������ֵ
        // ��� Success���ɹ����� Error��ʧ�ܣ��Լ�������Ϣ��

        private async Task<string> ChangeOtherPagesAsync(string pageName, string name, string property, object value)
        {
            // ���� featuresNV �� MenuItems ���ҵ���Ӧ��ҳ��
            foreach (var item in featuresNV.MenuItems)
            {
                if (item is NavigationViewItem navItem && navItem.Tag.ToString() == pageName)
                {
                    // ȷ�� contentFrame ���Ѿ���������ȷ��ҳ��
                    if (contentFrame.Content is Page page && page.GetType().Name == pageName)
                    {
                        // ����ָ����Ԫ��
                        var element = page.FindName(name) as FrameworkElement;
                        if (element == null)
                        {
                            return $"Element {name} not found on page {pageName}.";
                        }

                        // ��ȡ������Ϣ
                        var propertyInfo = element.GetType().GetProperty(property);
                        if (propertyInfo == null)
                        {
                            return $"Property {property} not found on element {name}.";
                        }

                        // ��������ֵ
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
            // ���� featuresNV �� MenuItems ���ҵ���Ӧ��ҳ��
            foreach (var item in featuresNV.MenuItems)
            {
                if (item is NavigationViewItem navItem && navItem.Tag.ToString() == pageName)
                {
                    // ��ȡҳ������
                    var pageType = Type.GetType(pageName);
                    if (pageType == null)
                    {
                        return $"Page type {pageName} not found.";
                    }

                    // ȷ�� contentFrame ���Ѿ���������ȷ��ҳ��
                    if (contentFrame.Content is Page page && page.GetType() == pageType)
                    {
                        // ��ȡ������Ϣ
                        var methodInfo = pageType.GetMethod(methodName);
                        if (methodInfo == null)
                        {
                            return $"Method {methodName} not found on page {pageName}.";
                        }

                        // ���÷���
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
                // ��ȡ contentFrame
                var frame = contentFrame;
                if (frame == null)
                {
                    Debug.WriteLine("This is RefreshPage speaking");
                    Debug.WriteLine("Error: Frame not found");
                    return "Error: Frame not found";
                }

                // ��ȡ��ǰҳ������
                var pageType = page.GetType();

                // ���µ�������ǰҳ�棬ʹ�� SuppressNavigationTransitionInfo �����⶯��
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
                Debug.WriteLine(ex.ToString()); // ����������쳣��Ϣ
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


        // ��Ҫ������OtherPagesEvents()
        // �ַ�����
        // ���룺��ʶ��
        // �ɹ�����õ�����Ϣ��ʧ�����Error
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
                    serverURL = await ReadFileAsync("User/serverURL.txt");
                    userEmail = await ReadFileAsync("User/userEmail.txt");
                    userPassword = await ReadFileAsync("User/userPassword.txt");
                    Token = await UserPasswordLogin(serverURL, userEmail, userPassword, "token");
                    string FilePath = await PickAFile();
                    Content = await ReadFileAsyncFromOtherLocations(FilePath);
                    UID = await GetOtherPages("PeoplePage", "ChatHeadingUIDTextBlock", "Text");
                    return (await SendMessageTo(Token, Content, "vocechat/file", UID));
                default:
                    Debug.WriteLine($"OtherPagesEvents got an unknown event {eventName}");
                    return "Error. Unknown event: " + eventName;
            }
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
                // �Ƴ����ܵ���Ч�ַ�
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

        // ��Ҫ������JsonGetPropertyWithKey()
        // ���� JSON ԭ���ݣ����ڱ����������ƣ����ڱ�������ֵ��Ҫ�õ�������ֵ����������
        // ��� �õ�������ֵ

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

        // ��Ҫ������GetOtherPages()

        private async Task<string> GetOtherPages(string pageName, string elementName, string propertyName)
        {
            try
            {
                // ���� featuresNV �� MenuItems ���ҵ���Ӧ��ҳ��
                foreach (var item in featuresNV.MenuItems)
                {
                    if (item is NavigationViewItem navItem && navItem.Tag.ToString() == pageName)
                    {
                        // ȷ�� contentFrame ���Ѿ���������ȷ��ҳ��
                        if (featuresNV.Content is Frame contentFrame)
                        {
                            if (contentFrame.Content is Page page && page.GetType().Name == pageName)
                            {
                                // ����ָ����Ԫ��
                                var element = page.FindName(elementName) as FrameworkElement;
                                if (element == null)
                                {
                                    return $"Error: Element {elementName} not found on page {pageName}.";
                                }

                                // ��ȡ������Ϣ
                                var propertyInfo = element.GetType().GetProperty(propertyName);
                                if (propertyInfo == null)
                                {
                                    return $"Error: Property {propertyName} not found on element {elementName}.";
                                }

                                // ��ȡ����ֵ
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


        private async Task<string> PickAFile()
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
                Output = "Picked file: " + file.Path;
                Debug.WriteLine(Output);
                return (Output);
            }
            else
            {
                Output = "Error: Operation cancelled.";
                Debug.WriteLine(Output);
                return (Output);
            }
        }
    }
}
