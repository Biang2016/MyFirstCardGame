﻿//#define Console

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
//using UnityEngine;


class Server : MonoBehaviour
//class Server
{
    private static Server _sv;

    public static Server SV
    {
        get
        {
            if (_sv == null)
            {
# if Console
                _sv = new Server("127.0.0.1", 9999);
#else
                _sv = FindObjectOfType<Server>();
#endif
            }

            return _sv;
        }
        set { _sv = value; }
    }

    public string IP;
    public int Port;
    private Socket severSocket;
    private List<ClientData> clientList = new List<ClientData>();
    private Dictionary<int, Socket> socketDict = new Dictionary<int, Socket>();

    private Queue<ClientData> receiveDataQueue = new Queue<ClientData>();
    private Dictionary<int, Queue<Request>> sendDataQueueDict = new Dictionary<int, Queue<Request>>();

    public ProtoManager ServerProtoManager;
    private ServerGameMatchManager sgmm;

#if Console
    public Server(string ip, int port)
    {
        IP = ip;
        Port = port;
    }
#endif

    public void Start()
    {
        OnRestartProtocols();
        sgmm = new ServerGameMatchManager();
        StartSeverSocket();
#if Console
        Thread threadReceiveAndSend = new Thread(ReceiveAndSendMsg);
        threadReceiveAndSend.IsBackground = true;
        threadReceiveAndSend.Start();
#endif
    }

    void Update()
    {
        if (receiveDataQueue.Count > 0)
        {
            ServerProtoManager.TryDeserialize(receiveDataQueue.Dequeue());
        }

        foreach (KeyValuePair<int, Queue<Request>> kv in sendDataQueueDict)
        {
            SendToClient(kv.Key);
        }
    }

#if Console
    private void ReceiveAndSendMsg()
    {
        while (true)
        {
            Update();
        }
    }
#endif

    public void OnRestartProtocols()
    {
        ServerProtoManager = new ProtoManager();
        ServerProtoManager.AddProtocol<TestConnectResponse>(NetProtocols.TEST_CONNECT);
        ServerProtoManager.AddProtocol<ClientIdResponse>(NetProtocols.SEND_CLIENT_ID);
        ServerProtoManager.AddProtocol<ServerInfoResponse>(NetProtocols.INFO_NUMBER);
        ServerProtoManager.AddProtocol<ServerWarningResponse>(NetProtocols.WARNING_NUMBER);
        ServerProtoManager.AddProtocol<GameBeginResponse>(NetProtocols.GAME_BEGIN);
        ServerProtoManager.AddProtocol<PlayerResponse>(NetProtocols.PLAYER);
        ServerProtoManager.AddProtocol<PlayerCostResponse>(NetProtocols.PLAYER_COST_CHANGE);
        ServerProtoManager.AddProtocol<DrawCardResponse>(NetProtocols.DRAW_CARD);
        ServerProtoManager.AddProtocol<SummonRetinueResponse>(NetProtocols.SUMMON_RETINUE);
        ServerProtoManager.AddProtocol<PlayerTurnResponse>(NetProtocols.PLAYER_TURN);
        ServerProtoManager.AddProtocol<ClientEndRoundResponse>(NetProtocols.CLIENT_END_ROUND);
        ServerProtoManager.AddProtocol<CardDeckResponse>(NetProtocols.CARD_DECK_INFO);
        ServerProtoManager.AddRespDelegate(NetProtocols.TEST_CONNECT, Response);
        ServerProtoManager.AddRespDelegate(NetProtocols.SEND_CLIENT_ID, Response);
        ServerProtoManager.AddRespDelegate(NetProtocols.INFO_NUMBER, Response);
        ServerProtoManager.AddRespDelegate(NetProtocols.WARNING_NUMBER, Response);
        ServerProtoManager.AddRespDelegate(NetProtocols.GAME_BEGIN, Response);
        ServerProtoManager.AddRespDelegate(NetProtocols.PLAYER, Response);
        ServerProtoManager.AddRespDelegate(NetProtocols.PLAYER_COST_CHANGE, Response);
        ServerProtoManager.AddRespDelegate(NetProtocols.DRAW_CARD, Response);
        ServerProtoManager.AddRespDelegate(NetProtocols.SUMMON_RETINUE, Response);
        ServerProtoManager.AddRespDelegate(NetProtocols.PLAYER_TURN, Response);
        ServerProtoManager.AddRespDelegate(NetProtocols.CLIENT_END_ROUND, Response);
        ServerProtoManager.AddRespDelegate(NetProtocols.CARD_DECK_INFO, Response);
    }

    public void Stop()
    {
        foreach (ClientData clientData in clientList)
        {
            if (clientData.Socket != null && clientData.Socket.Connected)
            {
                clientData.Socket.Shutdown(SocketShutdown.Both);
                clientData.Socket.Close();
                ServerLog.Print("客户" + clientData.ClientId + "退出");
            }
        }

        clientList.Clear();
    }

    public void StartSeverSocket()
    {
        try
        {
            severSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            //将服务器的ip捆绑
            severSocket.Bind(new IPEndPoint(IPAddress.Parse(IP), Port));
            //为服务器sokect添加监听
            severSocket.Listen(10);
            ServerLog.Print("[S]服务器启动成功");
            clientList = new List<ClientData>();
            //开始服务器时 一般接受一个服务就会被挂起所以要用多线程来解决
            Thread threadAccept = new Thread(Accept);
            threadAccept.IsBackground = true;
            threadAccept.Start();
        }
        catch
        {
            ServerLog.Print("[S]创建服务器失败");
        }
    }

    public void Accept()
    {
        Socket client = severSocket.Accept();
        ClientData clientData = new ClientData(client, 0, new DataHolder(), false);
        clientList.Add(clientData);
        IPEndPoint point = client.RemoteEndPoint as IPEndPoint;
        ServerLog.Print(point.Address + ":【" + point.Port + "】连接成功");
        ServerLog.Print("[S]客户端总数:" + clientList.Count);

        Thread threadReceive = new Thread(ReceiveSocket);
        threadReceive.IsBackground = true;
        threadReceive.Start(clientData);
        Accept();
    }

    #region 接收

    //接收客户端Socket连接请求
    private void ReceiveSocket(object obj)
    {
        ClientData clientData = obj as ClientData;
        clientData.DataHolder.Reset();
        while (!clientData.IsStopReceive)
        {
            if (!clientData.Socket.Connected)
            {
                //与客户端连接失败跳出循环  
                ServerLog.Print("[S]连接客户端失败,客户端ID" + clientData.ClientId);
                clientData.Socket.Close();
                break;
            }

            try
            {
                byte[] bytes = new byte[1024];
                int i = clientData.Socket.Receive(bytes);
                if (i <= 0)
                {
                    clientData.Socket.Close();
                    ServerLog.Print("[S]客户端关闭,客户端ID" + clientData.ClientId);
                    break;
                }

                clientData.DataHolder.PushData(bytes, i);
                while (clientData.DataHolder.IsFinished())
                {
                    receiveDataQueue.Enqueue(clientData);
                    clientData.DataHolder.RemoveFromHead();
                }
            }
            catch (Exception e)
            {
                ServerLog.Print("[S]Failed to ServerSocket error." + e);
                clientData.Socket.Close();
                break;
            }
        }
    }

    void Response(Socket socket, Response r)
    {
        string Log = "";
        Log += "服务器收到信息：[协议]" + r.GetProtocolName() + "[内容]";
        Log += r.DeserializeLog();
        ServerLog.Print(Log);

        if (r is ClientIdResponse)
        {
            ClientIdResponse resp = (ClientIdResponse) r;
            if (resp.purpose == ClientIdPurpose.RegisterClientId)
            {
                socketDict.Add(resp.clientId, socket);
                sendDataQueueDict.Add(resp.clientId, new Queue<Request>());
                sgmm.OnClientRegister(resp.clientId);
            }
            else if (resp.purpose == ClientIdPurpose.MatchGames)
            {
                sgmm.OnClientMatchGames(resp.clientId);
            }
        }
        else if (r is CardDeckResponse)
        {
            CardDeckResponse resp = (CardDeckResponse) r;
            sgmm.OnReceiveCardDeckInfo(resp.clientId, resp.cardDeckInfo);
        }
        else if (r is ClientEndRoundResponse)
        {
            ClientEndRoundResponse resp = (ClientEndRoundResponse) r;
            sgmm.PlayerGamesDictionary[resp.clientId].OnEndRound();
        }
    }

    #endregion


    #region 发送信息

    //将要发送的信息送入队列
    public void SendMessage(Request req, int clientId)
    {
        if (sendDataQueueDict.ContainsKey(clientId))
        {
            sendDataQueueDict[clientId].Enqueue(req);
        }
        else
        {
            ServerLog.Print("发送消息失败，客户端未连接到服务器,客户端ID" + clientId);
        }
    }

    //处理特定客户端的信息队列
    private void SendToClient(int clientId)
    {
        if (sendDataQueueDict.ContainsKey(clientId))
        {
            while (sendDataQueueDict[clientId].Count > 0)
            {
                Request req = sendDataQueueDict[clientId].Dequeue();
                Thread threadSend = new Thread(DoSendToClient);
                threadSend.IsBackground = true;
                threadSend.Start(new SendMsg(socketDict[clientId], req));
            }
        }
        else
        {
            ServerLog.Print("发送消息失败，客户端未连接到服务器,客户端ID" + clientId);
        }
    }


    //对特定客户端发送信息
    private void DoSendToClient(object obj)
    {
        SendMsg sendMsg = (SendMsg) obj;

        if (sendMsg == null)
        {
            ServerLog.Print("[S]sendMsg is null");
            return;
        }

        if (sendMsg.Client == null)
        {
            ServerLog.Print("[S]client socket is null");
            return;
        }

        if (!sendMsg.Client.Connected)
        {
            ServerLog.Print("[S]Not connected to client socket");
            sendMsg.Client.Close();
            return;
        }

        try
        {
            DataStream bufferWriter = new DataStream(true);
            sendMsg.Req.Serialize(bufferWriter);
            byte[] msg = bufferWriter.ToByteArray();

            byte[] buffer = new byte[msg.Length + 4];
            DataStream writer = new DataStream(buffer, true);

            writer.WriteInt32((uint) msg.Length); //增加数据长度
            writer.WriteRaw(msg);

            byte[] data = writer.ToByteArray();

            IAsyncResult asyncSend = sendMsg.Client.BeginSend(data, 0, data.Length, SocketFlags.None, new AsyncCallback(SendCallback), sendMsg.Client);
            bool success = asyncSend.AsyncWaitHandle.WaitOne(5000, true);
            if (!success)
            {
                Stop();
            }
        }
        catch (Exception e)
        {
            ServerLog.Print("[S]Send error : " + e.ToString());
        }
    }

    private void SendCallback(IAsyncResult asyncConnect)
    {
        ServerLog.Print("[S]发送信息成功");
    }

    #endregion
}


public class SendMsg
{
    public SendMsg(Socket client, Request req)
    {
        Client = client;
        Req = req;
    }

    public Socket Client;
    public Request Req;
}