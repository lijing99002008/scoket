using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SocketClass
{
    public enum SocketAction
    {
        登陆, 注销, 发送, 重连
    }
    /// <summary>
    /// socket消息
    /// </summary>
    public class SocketMessage
    {
        /// <summary>
        /// 动作
        /// </summary>
        /// <value>
        /// The action.
        /// </value>
        public SocketAction ActionEnum { get; set; }
        public string UserName { get; set; }
        public string WorkLocation { get; set; }
        public string TextMessage { get; set; }
    }
    public class SocketClient
    {
        public delegate void ReceiveHandler(object sender, MsgEventArgs e);
        public event ReceiveHandler OnReceive;
        protected void Receive(MsgEventArgs e)
        {
            OnReceive?.Invoke(this, e);
        }
        public delegate void ConnectedHandler(object sender, EventArgs e);
        public event ConnectedHandler OnConnected;
        protected void Connected(EventArgs e)
        {
            OnConnected?.Invoke(this, e);
        }
        ClientWebSocket clientWebSocket = new ClientWebSocket();
        private static string userMessage;
        /// <summary>
        /// Gets or sets the connect time out.
        /// </summary>
        /// <value>
        /// The connect time out.
        /// </value>
        public int ConnectTimeOut { get; set; } = 5;
        /// <summary>
        /// Gets or sets the ws URL.
        /// </summary>
        /// <value>
        /// The ws URL.
        /// </value>
        public string WsUrl { get; set; }
        /// <summary>
        /// Gets a value indicating whether this instance is connect.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is connect; otherwise, <c>false</c>.
        /// </value>
        public bool IsConnect { get; private set; }
        public SocketClient(string url = "ws://172.16.1.17/websocket/api/WSChat")
        {
            WsUrl = url;
            Connect();
            
        }
        public void Abort()
        {
            clientWebSocket.Abort();
        }
        /// <summary>
        /// Connects this instance.
        /// </summary>
        /// <exception cref="Exception">连接超时!</exception>
        public bool Connect(string url = "ws://172.16.1.17/websocket/api/WSChat")
        {
            if (clientWebSocket.State == WebSocketState.Aborted)
            {
                clientWebSocket.Dispose();
                clientWebSocket = new ClientWebSocket();
            }
            WsUrl = url;
            if(clientWebSocket.State != WebSocketState.Connecting && clientWebSocket.State != WebSocketState.Open)
                clientWebSocket.ConnectAsync(new Uri(WsUrl), System.Threading.CancellationToken.None).Wait();
            DateTime dts = DateTime.Now;
            while (clientWebSocket.State != WebSocketState.Open)
            {
                if (DateTime.Now > dts.AddSeconds(ConnectTimeOut))
                {
                    throw new Exception("连接超时!");
                }
            }
            EventArgs eventArgs = new EventArgs();
            Connected(eventArgs);
            StartReceiving();
            IsConnect = true;
            return true;
        }
        /// <summary>
        /// Logins the specified user name.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <param name="workLoc">The work loc.</param>
        /// <returns></returns>
        public void Login(string userName, string workLoc)
        {
            if (clientWebSocket.State != WebSocketState.Open || clientWebSocket.State != WebSocketState.Connecting)
            {
                Connect();
            }
            SocketClass.SocketMessage socketMessage = new SocketMessage()
            {
                ActionEnum = SocketAction.登陆,
                UserName = userName,
                WorkLocation = workLoc
            };
            string msg = Newtonsoft.Json.JsonConvert.SerializeObject(socketMessage);
            clientWebSocket.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(msg)), WebSocketMessageType.Text, true, CancellationToken.None).Wait();
        }
        /// <summary>
        /// Sends the MSG.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <param name="workLoc">The work loc.</param>
        /// <param name="msg">The MSG.</param>
        /// <returns></returns>
        public void SendMsg(string userName, string workLoc, string msg)
        {
            if (clientWebSocket.State != WebSocketState.Open && clientWebSocket.State != WebSocketState.Connecting)
            {
                Connect();
            }
            SocketClass.SocketMessage socketMessage = new SocketMessage()
            {
                ActionEnum = SocketAction.发送,
                UserName = userName,
                WorkLocation = workLoc,
                TextMessage = msg
            };
            string sendmsg = Newtonsoft.Json.JsonConvert.SerializeObject(socketMessage);
            try
            {
                clientWebSocket.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(sendmsg)), WebSocketMessageType.Text, true, CancellationToken.None).Wait();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        /// <summary>
        /// Starts the receiving.
        /// </summary>
        private async void StartReceiving()
        {
            WebSocketReceiveResult result;
            if (clientWebSocket.State == WebSocketState.Aborted) return;
            while (clientWebSocket.State == WebSocketState.Connecting) {
                Thread.Sleep(100);
            }

            while (clientWebSocket != null && clientWebSocket.State == WebSocketState.Open)
            {
                try
                {
                    //接收信息
                    ArraySegment<byte> buffer = new ArraySegment<byte>(new byte[1024]);
                    if (clientWebSocket.State == WebSocketState.Aborted) return;
                    result = await clientWebSocket.ReceiveAsync(buffer, CancellationToken.None);
                    userMessage = Encoding.UTF8.GetString(buffer.Array, 0, result.Count);
                    while (!result.EndOfMessage)
                    {
                        result = await clientWebSocket.ReceiveAsync(buffer, CancellationToken.None);
                        userMessage += Encoding.UTF8.GetString(buffer.Array, 0, result.Count);

                    }
                    if (userMessage.Length > 0)
                        Receive(new MsgEventArgs() { Message = userMessage });
                }
                catch (System.Net.WebSockets.WebSocketException e)
                {
                    System.Diagnostics.Debug.WriteLine(e.Message +"\r\n"+ e.InnerException.Message);
                    return;
                }
                catch (Exception ex)
                {
                    string msg = ex.Message;
                }
            }
            IsConnect = false;
            Connect(WsUrl);
        }
    }
    public class MsgEventArgs : EventArgs
    {
        public string Message { get; set; }
    }

}
