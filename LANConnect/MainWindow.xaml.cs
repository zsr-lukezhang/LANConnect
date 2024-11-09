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
        }

        private async void checkServerButton_Click(object sender, RoutedEventArgs e)
        {
            // Debug ��ʾ����
            Debug.WriteLine("========================");

            // �趨����
            string serverName = "";
            string serverBio = "";

            // ��ȡ������������
            serverName = await getServerName(serverURLBox.Text,"name");

            // ������ֵ
            // �жϿ�ͷ�Ƿ�Ϊ Error

            if (serverName.StartsWith("Error") ) 
            {
                Debug.WriteLine("This is checkServerButton speaking ");
                Debug.WriteLine("getServerName returns a bad one: Error");
                Debug.WriteLine("The string is showed below:");
                Debug.WriteLine(serverName);
            }
            else
            {
                Debug.WriteLine("This is checkServerButton speaking ");
                Debug.WriteLine("getServerName returns a good one:" + serverName);
            }

            serverBio = await getServerName(serverURLBox.Text, "bio");
            if (serverBio.StartsWith("Error"))
            {
                Debug.WriteLine("This is checkServerButton speaking ");
                Debug.WriteLine("getServerName returns a bad one: Error");
                Debug.WriteLine("The string is showed below:");
                Debug.WriteLine(serverBio);
            }
            else
            {
                Debug.WriteLine("This is checkServerButton speaking ");
                Debug.WriteLine("getServerName returns a good one:" + serverBio);
            }
        }

        public async Task<int> PostRequestAsync(string url, string apiKey, string content)
        {
            using (HttpClient client = new HttpClient())
            {
                // �趨 Header
                client.DefaultRequestHeaders.Add("accept", "*/*");
                client.DefaultRequestHeaders.Add("X-API-Key", apiKey);
                client.DefaultRequestHeaders.Add("Content-Type", "application/json; charset=utf-8");
                
                // ������ʽΪ UTF-8
                HttpContent httpContent = new StringContent(content, Encoding.UTF8, "application/json");
                
                // ��������                
                HttpResponseMessage response = await client.PostAsync(url, httpContent);

                // ����״̬��
                return (int)response.StatusCode;
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

        private async void changeServerName(object sender, RoutedEventArgs e)
        {
            try
            {
                // �趨���ڵ��� PostRequestAsync �ı���ֵ
                string url = "https://familychat.lukezhang.win/api/admin/system/organization";
                string apiKey = "eyJhbGciOiJIUzI1NiJ9.eyJkIjp7InVpZCI6MSwiZGV2aWNlIjoid2ViOjRUWFhVQS1KRWdxb0F1dVlvUmNndiIsImlzX2FkbWluIjp0cnVlLCJpc19ndWVzdCI6ZmFsc2V9LCJlIjoiMjAyNC0xMS0wOFQxMDoyODozOS40MTQyODI3NjVaIiwibiI6ImFYU3hHS3ZtTFdjQUFBQUEiLCJ0IjoiQWNjZXNzVG9rZW4ifQ.KX-AJjQ3lkVwiNko8gXVwWr52TcJ20FkPXoJYhGhI_o";
                string content = "{\"name\": \"Family Chat\", \"description\": \"Woof!\"}";

                // �������趨����ֵ
                int statusCode = await PostRequestAsync(url, apiKey, content);

                // ����״̬�룬������ʾ�� UI ��
                Debug.WriteLine($"HTTP Status Code: {statusCode}");
            }
            catch (Exception ex)
            {
                // �����쳣��������ʾ������Ϣ
                Debug.WriteLine($"Error: {ex.Message}");
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

    }
}
