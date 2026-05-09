using System;
using System.Collections;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Networking;
using BestHTTP;
using BestHTTP.SecureProtocol.Org.BouncyCastle.Ocsp;
using LitJson;


namespace UnityWebSocket.Demo
{
    public class UnityWebSocketDemo : MonoBehaviour
    {
        public string httpsAddress = "https://test.strawartist.com/api/chat";
        public string address = "ws://test.strawartist.com/connection/websocket";
        // public string address = "wss://echo.websocket.events";
        public string sendText = "Hello UnityWebSocket!";
        public string PUTText = "push";
        public string GetText = "https://test.strawartist.com/api/chat/v1/users/me";
        public string PostText = "";
        public string idtokenText = "eyJhbGciOiJSUzI1NiIsImtpZCI6ImE3MWI1MTU1MmI0ODA5OWNkMGFkN2Y5YmZlNGViODZiMDM5NmUxZDEiLCJ0eXAiOiJKV1QifQ.eyJpc3MiOiJodHRwczovL3NlY3VyZXRva2VuLmdvb2dsZS5jb20vbnV6emxlLWFpLXdhcm0tdmlydHVhbC1wLWEzY2QyIiwiYXVkIjoibnV6emxlLWFpLXdhcm0tdmlydHVhbC1wLWEzY2QyIiwiYXV0aF90aW1lIjoxNzM1NTQ0MjkxLCJ1c2VyX2lkIjoiRU1NRnQ0cW1CQ2RIS2dpQXNDejd5OWxDM3ZzMiIsInN1YiI6IkVNTUZ0NHFtQkNkSEtnaUFzQ3o3eTlsQzN2czIiLCJpYXQiOjE3MzU1NDQyOTEsImV4cCI6MTczNTU0Nzg5MSwiZW1haWwiOiIxODAyMjQ5MzEwQHFxLmNvbSIsImVtYWlsX3ZlcmlmaWVkIjpmYWxzZSwiZmlyZWJhc2UiOnsiaWRlbnRpdGllcyI6eyJlbWFpbCI6WyIxODAyMjQ5MzEwQHFxLmNvbSJdfSwic2lnbl9pbl9wcm92aWRlciI6InBhc3N3b3JkIn19.QbBarRIlLbaImTifzPSA2rwCNSygCMRaz8Lcou1AU1cSEuV6FGw15Wphu3xddr69i5fmX3ImubkP0ciXhqD8tKeLBf9eRqO6YcVBDuNViTArGkAfiZ-AivBW8ncqjVElEk2pI4XmnXx8tddy8ppVY5cI-Z_iG1xDBb-eUbD6hlpyRnSOj4ffbPJBshJ5lCG1ucMWXJkJwUqi_ufrUxnQHyXVH3RFWHnAFQkR5AensEurWkSR0attcTffmtWg7UYjH1SDoxEpoxbeIo_bhp2lEK57q4HNo4GzfW1O3ZQU1zkEE3lyGK3zaMHwKI3lm0ZsZzeZJGxL2hySGIYGFHnXrQ";

        private IWebSocket socket;
        private UnityWebRequest request ;

        private bool logMessage = true;
        private bool isLoginHttps = false;
        private string log = "";
        private int sendCount;
        private int receiveCount;
        private Vector2 scrollPos;
        private Color green = new Color(0.1f, 1, 0.1f);
        private Color red = new Color(1f, 0.1f, 0.1f);
        private Color wait = new Color(0.7f, 0.3f, 0.3f);

        private void OnGUI()
        {
            Debug.Log(isLoginHttps);
            var scale = Screen.width / 800f;
            GUI.matrix = Matrix4x4.TRS(new Vector3(0, 0, 0), Quaternion.identity, new Vector3(scale, scale, 1));
            var width = GUILayout.Width(Screen.width / scale - 10);

            WebSocketState state = socket == null ? WebSocketState.Closed : socket.ReadyState;

            // draw header
            GUILayout.BeginHorizontal();
            GUILayout.Label("SDK Version: " + Settings.VERSION, GUILayout.Width(Screen.width / scale - 100));
            GUI.color = green;
            GUILayout.Label($"FPS: {fps:F2}", GUILayout.Width(80));
            GUI.color = Color.white;
            GUILayout.EndHorizontal();

            // draw websocket state WebSocket的状态
            GUILayout.BeginHorizontal();
            GUILayout.Label("State: ", GUILayout.Width(36));
            GUI.color = WebSocketState.Closed == state ? red : WebSocketState.Open == state ? green : wait;
            GUILayout.Label($"{state}", GUILayout.Width(120));
            GUI.color = Color.white;
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Http State: ", GUILayout.Width(36));
            GUI.color = WebSocketState.Closed == state ? red : WebSocketState.Open == state ? green : wait;
            GUILayout.Label($"https {state}", GUILayout.Width(120));
            GUI.color = Color.white;
            GUILayout.EndHorizontal();

            // draw address
            GUI.enabled = state == WebSocketState.Closed;
            GUILayout.Label("https address: ", width);
            GUILayout.Label("websocket Address: ", width);
            address = GUILayout.TextField(address, width);
            httpsAddress = GUILayout.TextField(httpsAddress, width);

            // draw connect button
            GUILayout.BeginHorizontal();
            GUI.enabled = state == WebSocketState.Closed;
            if (GUILayout.Button(state == WebSocketState.Connecting ? "Connecting..." : "Connect"))
            {
                socket = new WebSocket(address);
                socket.OnOpen += Socket_OnOpen;
                socket.OnMessage += Socket_OnMessage;
                socket.OnClose += Socket_OnClose;
                socket.OnError += Socket_OnError;
                AddLog(string.Format("Connecting..."));
                socket.ConnectAsync();
            }

            // draw close button
            GUI.enabled = state == WebSocketState.Open;
            if (GUILayout.Button(state == WebSocketState.Closing ? "Closing..." : "Close"))
            {
                AddLog(string.Format("Closing..."));
                socket.CloseAsync();
            }
            GUILayout.EndHorizontal();

            // 链接Https
            GUILayout.BeginHorizontal();    // 开始
            GUI.enabled = true;
            if(GUILayout.Button("Https Test Connect"))
            {
                var uri = new Uri(httpsAddress);
                // var useruri = new Uri("https://test.strawartist.com/v1/users/me");
                AddLog("开始https连接 + " + uri.ToString());
                // 创建HTTPRequest对象，指定请求的URL、方法（这里是GET）以及回调函数来处理响应
                var httprequest = new HTTPRequest(uri, (request, response) =>
                {
                    AddLog("State Code: " + response.StatusCode.ToString());
                    AddLog("Data: " + response.Data.ToString());
                    // 处理响应
                    switch(request.State)
                    {
                        case HTTPRequestStates.Finished:
                            if(response.IsSuccess)
                            {
                                //{"code":200,"data":{"uuid":"7c728860-9154-44f0-9307-e3e013c8e6d2","info":{"pet_name":"","gender":"","birthday":"","timezone":"","language":"","location":""},"connect_token":"eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiI3YzcyODg2MC05MTU0LTQ0ZjAtOTMwNy1lM2UwMTNjOGU2ZDIiLCJleHAiOjE3MzU2OTk5NzB9.QDdQKSNEuaxgoQXtR4I06QwylWYNBALkUk23K1wTKNo"}}
                                // 使用JsonMapper.ToObject方法将JSON字符串转换为JsonData对象

                                AddLog("https连接成功！");
                                AddLog("Request Finished! Text received: " + response.DataAsText);
                            } else
                            {
                                Debug.LogWarning(string.Format("Request finished Successfully, but the server sent an error. Status Code: {0}-{1} Message: {2}",
                                                                response.StatusCode,
                                                                response.Message,
                                                                response.DataAsText));
                            }
                            // 处理成功的响应
                            break;
                        case HTTPRequestStates.Error:
                            Debug.LogError("Request Finished with Error! " + (request.Exception != null ? (request.Exception.Message + "\n" + request.Exception.StackTrace) : "No Exception"));
                            // 处理失败的响应
                            break;
                        case HTTPRequestStates.ConnectionTimedOut:
                            // 处理超时的响应
                            break;
                        case HTTPRequestStates.Aborted:
                            Debug.LogWarning("Request Aborted!");
                            // 处理中断的响应
                            break;
                        case HTTPRequestStates.TimedOut:
                            Debug.LogError("Processing the request Timed Out!");
                            break;
                    }
                });

                // httprequest.Credentials = new BestHTTP.Authentication.Credentials("Bearer", "");
                httprequest.SetHeader("Authorization", idtokenText);
                httprequest.Send();
                AddLog("Authorization header : " + httprequest.GetHeaderValues("Authorization header")[0]);
            }

            GUILayout.EndHorizontal();      // 结束

            // draw input message
            GUILayout.Label("Message: ");
            sendText = GUILayout.TextArea(sendText, GUILayout.MinHeight(50), width);

            // draw send message button
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Send") && !string.IsNullOrEmpty(sendText))
            {
                socket.SendAsync(sendText);
                AddLog(string.Format("Send: {0}", sendText));
                sendCount += 1;
            }
            if (GUILayout.Button("Send Bytes") && !string.IsNullOrEmpty(sendText))
            {
                var bytes = System.Text.Encoding.UTF8.GetBytes(sendText);
                socket.SendAsync(bytes);
                AddLog(string.Format("Send Bytes ({1}): {0}", sendText, bytes.Length));
                sendCount += 1;
            }
            if (GUILayout.Button("Send x100") && !string.IsNullOrEmpty(sendText))
            {
                for (int i = 0; i < 100; i++)
                {
                    var text = (i + 1).ToString() + ". " + sendText;
                    socket.SendAsync(text);
                    AddLog(string.Format("Send: {0}", text));
                    sendCount += 1;
                }
            }
            if (GUILayout.Button("Send Bytes x100") && !string.IsNullOrEmpty(sendText))
            {
                for (int i = 0; i < 100; i++)
                {
                    var text = (i + 1).ToString() + ". " + sendText;
                    var bytes = System.Text.Encoding.UTF8.GetBytes(text);
                    socket.SendAsync(bytes);
                    AddLog(string.Format("Send Bytes ({1}): {0}", text, bytes.Length));
                    sendCount += 1;
                }
            }
            GUILayout.EndHorizontal();

            // firebase idtoken
            GUILayout.Label("FireBase IdToken");
            idtokenText = GUILayout.TextArea(idtokenText, GUILayout.MinHeight(50), width);

            // draw input message
            GUILayout.Label("Push Message: ");
            PUTText = GUILayout.TextArea(PUTText, GUILayout.MinHeight(50), width);

            GUILayout.BeginHorizontal();
            // push消息，并且消息提示
            if(GUILayout.Button("Push") && !string.IsNullOrEmpty(PUTText))
            {
                AddLog("Push消息");
                string pushstring = PUTText;
                socket.SendAsync(sendText);
            }
            GUILayout.EndHorizontal();

            GUILayout.Label("Get Http adress :");
            GetText = GUILayout.TextField(GetText, width);

            GUILayout.BeginHorizontal();

            // Get请求，并且消息提示
            if(GUILayout.Button("GET"))
            {
                AddLog("Get 请求 ：" + GetText);
                var uri = new Uri(GetText);
                // 测试获取用户信息
                var httprequest = new HTTPRequest(uri, (req, resp) =>
                {
                    AddLog("GetUser state code" + resp.StatusCode);
                    AddLog("GetUser data" + resp.DataAsText);
                    Debug.Log("Request Finished! Text received: " + resp.DataAsText);
                });
                httprequest.SetHeader("Authorization", idtokenText);
                httprequest.Send();
            }
            GUILayout.EndHorizontal();

            GUILayout.Label("POST https address");

            GUILayout.BeginHorizontal();
            // push消息，并且消息提示
            if(GUILayout.Button("PUT") && !string.IsNullOrEmpty(PUTText))
            {
                AddLog("POST 消息");
                string pushstring = PUTText;
                socket.SendAsync(sendText);
            }
            GUILayout.EndHorizontal();

            // draw message count
            GUI.enabled = true;
            GUILayout.BeginHorizontal();
            logMessage = GUILayout.Toggle(logMessage, "Log Message");
            GUILayout.Label(string.Format("Send Count: {0}", sendCount));
            GUILayout.Label(string.Format("Receive Count: {0}", receiveCount));
            GUILayout.EndHorizontal();

            // draw clear button
            if (GUILayout.Button("Clear"))
            {
                log = "";
                receiveCount = 0;
                sendCount = 0;
            }

            // draw message content
            scrollPos = GUILayout.BeginScrollView(scrollPos, GUILayout.MaxHeight(Screen.height / scale - 270), width);
            GUILayout.Label(log);
            GUILayout.EndScrollView();
        }


        private void AddLog(string str)
        {
            if(!logMessage) return;
            Debug.Log(str);
            if(str.Length > 100) str = str.Substring(0, 100) + "...";
            log += str + "\n";
            if (log.Length > 22 * 1024)
            {
                log = log.Substring(log.Length - 22 * 1024);
            }
            scrollPos.y = int.MaxValue;
        }

        private void Socket_OnOpen(object sender, OpenEventArgs e)
        {
            AddLog(string.Format("Connected: {0}", address));
        }

        private void Socket_OnMessage(object sender, MessageEventArgs e)
        {
            AddLog("收到数据推送连接");
            AddLog("WebSocket receive message : " + e.ToString());
            if (e.IsBinary)
            {
                AddLog(string.Format("Receive Bytes ({1}): {0}", e.Data, e.RawData.Length));
            }
            else if (e.IsText)
            {
                AddLog(string.Format("Receive: {0}", e.Data));
            }
            receiveCount += 1;
        }

        private void Socket_OnClose(object sender, CloseEventArgs e)
        {
            AddLog(string.Format("Closed: StatusCode: {0}, Reason: {1}", e.StatusCode, e.Reason));
        }

        private void Socket_OnError(object sender, ErrorEventArgs e)
        {
            AddLog(string.Format("Error: {0}", e.Message));
        }

        private int frame = 0;
        private float time = 0;
        private float fps = 0;
        private void Update()
        {
            frame += 1;
            time += Time.deltaTime;
            if (time >= 0.5f)
            {
                fps = frame / time;
                frame = 0;
                time = 0;
            }
        }
    }
}
