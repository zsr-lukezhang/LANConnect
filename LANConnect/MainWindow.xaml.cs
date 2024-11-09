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
using Windows.Foundation;
using Windows.Foundation.Collections;


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

            // ����������ȥ
            ExtendsContentIntoTitleBar = true;
        }

        private async void checkServerButton_Click(object sender, RoutedEventArgs e)
        {
            // Debug ��ʾ����
            Debug.WriteLine("========================");

            // �жϵ�ַ�Ƿ�Ϊ��
            if(serverURLBox.Text != string.Empty)
            {
                
                // ����ʱ

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
                // �趨 Header
                client.DefaultRequestHeaders.Add("accept", "application/json; charset=utf-8");
                if (!string.IsNullOrEmpty(apiKey))
                {
                    client.DefaultRequestHeaders.Add("X-API-Key", apiKey);
                }

                // ������ʽΪ UTF-8
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


        public async Task<string> GetRequestAsync(string url, string apiKey)
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
                    // ���ػ�õ�����
                    return await response.Content.ReadAsStringAsync();
                }
                else
                {
                    // ���ؿ��ı�
                    return string.Empty;
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
                string response = await GetRequestAsync(url, apiKey);

                // ������Ӧ����
                Debug.WriteLine("This is getServerName speaking");
                Debug.WriteLine($"Response Content: {response}");

                // ���жϷ�Ϊ������Ӧ
                if (response == null) // ���ж��Ƿ�Ϊ������
                {
                    Debug.WriteLine("This is getServerName speaking");
                    Debug.WriteLine("null: Server not found");

                    // ��ʾ���������ؿ�ֵ
                    return("Error: Server returns null");
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
                // ��ȡ Token
                await UserPasswordLogin(userEmailBox.Text, userPasswordBox.Password, "token");
            }
            catch (Exception ex)
            {
                Debug.WriteLine("This is userPasswordLoginButton speaking");
                Debug.WriteLine("Error: " + ex);
            }
        }

        private async Task<string> UserPasswordLogin(string Email, string Password, string Type)
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
            string URL = serverURLBox.Text + "/api/token/login";

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
                return Returned;
            }
            else
            {
                int TryAgain = (int)await PostRequestAsync(URL, "", Json, "int");
                return ("Error: " + $"TryAgain");
            }
        }
    }
}
