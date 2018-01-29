using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.WebSockets;
using SocketClass;
namespace WebScoket.Controllers
{
    public class WSChatController : ApiController
    {
        public static List<ConnectedUser> listConnectedUser = new List<ConnectedUser>();
        public HttpResponseMessage Get()
        {
            if (HttpContext.Current.IsWebSocketRequest)
            {
                HttpContext.Current.AcceptWebSocketRequest(ProcessWSChat);
            }
            return new HttpResponseMessage(HttpStatusCode.SwitchingProtocols);
        }
        private async Task ProcessWSChat(AspNetWebSocketContext context)
        {
            WebSocket socket = context.WebSocket;
            ConnectedUser connectedUser;
            //ConnectedUser connectedUser;
            while (socket.State != WebSocketState.Closed)
            {
                ArraySegment<byte> buffer = new ArraySegment<byte>(new byte[1024]);
                WebSocketReceiveResult result = await socket.ReceiveAsync(buffer, CancellationToken.None);
                string userMessage = Encoding.UTF8.GetString(buffer.Array, 0, result.Count);
                while (!result.EndOfMessage)
                {
                    result = await socket.ReceiveAsync(buffer, CancellationToken.None);
                    userMessage += Encoding.UTF8.GetString(buffer.Array, 0, result.Count);

                }
                //json
                SocketMessage socketMessage = null;
                try
                {
                    socketMessage = Newtonsoft.Json.JsonConvert.DeserializeObject<SocketMessage>(userMessage);
                }
                catch
                {
                    continue;
                }
                if (socketMessage == null)
                    continue;
                switch (socketMessage.ActionEnum)
                {
                    case SocketAction.登陆:
                        connectedUser = listConnectedUser.FirstOrDefault(p => p.WorkLocation == socketMessage.WorkLocation || p.UserName == socketMessage.UserName);
                        if (connectedUser != null)
                        {
                            connectedUser.Socket = socket;
                        }
                        if (listConnectedUser.FirstOrDefault(p => p.Socket == socket) == null)
                        {
                            listConnectedUser.Add(new ConnectedUser { Socket = socket, LoginTime = DateTime.Now, UserName = socketMessage.UserName, WorkLocation = socketMessage.WorkLocation });
                            await socket.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes("OK")), WebSocketMessageType.Text, true, CancellationToken.None);
                        }

                        //List<ConnectedUser> list = listConnectedUser.ToList();
                        //foreach (ConnectedUser item in list)
                        //{
                        //    if (item != connectedUser && item.Socket == socket)
                        //        item.Socket = null;
                        //}
                        listConnectedUser.Add(new ConnectedUser { Socket = socket, LoginTime = DateTime.Now, UserName = socketMessage.UserName, WorkLocation = socketMessage.WorkLocation });

                        await socket.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes("用户已经登陆")), WebSocketMessageType.Text, true, CancellationToken.None);
                        if (connectedUser != null && !String.IsNullOrWhiteSpace(connectedUser.LastMsg))
                        {

                            await socket.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(connectedUser.LastMsg)), WebSocketMessageType.Text, true, CancellationToken.None);
                        }
                        break;
                    case SocketAction.注销:
                        connectedUser = listConnectedUser.FirstOrDefault(p => p.Socket == socket);
                        if (connectedUser != null)
                        {
                            listConnectedUser.Remove(connectedUser);
                            await socket.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes("OK")), WebSocketMessageType.Text, true, CancellationToken.None);
                            socket.Abort();
                        }
                        else
                            await socket.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes("用户不存在或不在线")), WebSocketMessageType.Text, true, CancellationToken.None);
                        break;
                    case SocketAction.发送:
                        ConnectedUser connectedUser1 = listConnectedUser.FirstOrDefault(p => p.WorkLocation == socketMessage.WorkLocation || p.UserName == socketMessage.UserName);
                        if (connectedUser1 != null)
                        {
                            connectedUser1.LastMsg = socketMessage.TextMessage;
                        }
                        if (connectedUser1 != null && connectedUser1.Socket.State == WebSocketState.Open)
                        {
                            await connectedUser1.Socket.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(socketMessage.TextMessage)), WebSocketMessageType.Text, true, CancellationToken.None);
                            await socket.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes("OK")), WebSocketMessageType.Text, true, CancellationToken.None);
                        }
                        else
                        {
                            await socket.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes("用户不存在或不在线")), WebSocketMessageType.Text, true, CancellationToken.None);
                        }
                        break;
                    default:
                        await socket.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes("不支持的指令")), WebSocketMessageType.Text, true, CancellationToken.None);
                        break;
                }
            }
            //else
            //{
            //    break;
            //}
        }
    }
    /// <summary>
    /// 连接的用户
    /// </summary>
    public class ConnectedUser
    {
        /// <summary>
        /// 连接
        /// </summary>
        /// <value>
        /// The web socket.
        /// </value>
        public WebSocket Socket { get; set; }
        /// <summary>
        /// 用户姓名
        /// </summary>
        /// <value>
        /// The name of the user.
        /// </value>
        public string UserName { get; set; }
        /// <summary>
        /// 工位
        /// </summary>
        /// <value>
        /// The work location.
        /// </value>
        public string WorkLocation { get; set; }
        /// <summary>
        /// 登陆时间
        /// </summary>
        /// <value>
        /// The login time.
        /// </value>
        public DateTime LoginTime { get; set; }
        /// <summary>
        /// 最后接收或未接收的消息
        /// </summary>
        /// <value>
        /// The last MSG.
        /// </value>
        public string LastMsg { get; set; }
    }
}
