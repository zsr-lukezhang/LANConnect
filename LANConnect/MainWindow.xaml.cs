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
            // Debug 显示分行
            Debug.WriteLine("========================");

            // 设定变量
            string serverName = "";
            string serverBio = "";

            // 获取服务器的名称
            serverName = await getServerName(serverURLBox.Text,"name");

            // 处理返回值
            // 判断开头是否为 Error

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
                // 设定 Header
                client.DefaultRequestHeaders.Add("accept", "*/*");
                client.DefaultRequestHeaders.Add("X-API-Key", apiKey);
                client.DefaultRequestHeaders.Add("Content-Type", "application/json; charset=utf-8");
                
                // 设编码格式为 UTF-8
                HttpContent httpContent = new StringContent(content, Encoding.UTF8, "application/json");
                
                // 发送请求                
                HttpResponseMessage response = await client.PostAsync(url, httpContent);

                // 返回状态码
                return (int)response.StatusCode;
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
                    return("Error: Server returns null");
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

        private async void changeServerName(object sender, RoutedEventArgs e)
        {
            try
            {
                // 设定用于调用 PostRequestAsync 的变量值
                string url = "https://familychat.lukezhang.win/api/admin/system/organization";
                string apiKey = "eyJhbGciOiJIUzI1NiJ9.eyJkIjp7InVpZCI6MSwiZGV2aWNlIjoid2ViOjRUWFhVQS1KRWdxb0F1dVlvUmNndiIsImlzX2FkbWluIjp0cnVlLCJpc19ndWVzdCI6ZmFsc2V9LCJlIjoiMjAyNC0xMS0wOFQxMDoyODozOS40MTQyODI3NjVaIiwibiI6ImFYU3hHS3ZtTFdjQUFBQUEiLCJ0IjoiQWNjZXNzVG9rZW4ifQ.KX-AJjQ3lkVwiNko8gXVwWr52TcJ20FkPXoJYhGhI_o";
                string content = "{\"name\": \"Family Chat\", \"description\": \"Woof!\"}";

                // 调用与设定返回值
                int statusCode = await PostRequestAsync(url, apiKey, content);

                // 处理状态码，例如显示在 UI 上
                Debug.WriteLine($"HTTP Status Code: {statusCode}");
            }
            catch (Exception ex)
            {
                // 处理异常，例如显示错误消息
                Debug.WriteLine($"Error: {ex.Message}");
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

    }
}
