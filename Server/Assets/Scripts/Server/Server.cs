﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class Server
{
    public static int Main(string[] args)
    {
        return 0;
    }

    private static Server _sv;

    public static Server SV
    {
        get
        {
            if (!_sv) _sv = FindObjectOfType<Server>();
            return _sv;
        }
    }

    string ip = "127.0.0.1";
    int port = 9999;

    private Socket severSocket;
    private List<ClientData> clientList = new List<ClientData>();

    Queue<byte[]> receiveDataQueue = new Queue<byte[]>();
    Dictionary<int, Queue<Request>> sendDataQueueDict = new Dictionary<int, Queue<Request>>();

    public ProtoManager ServerProtoManager;
    private ServerGameMatchManager sgmm;


    void Awake()
    {
        OnRestartProtocols();
        sgmm = new ServerGameMatchManager();
    }

    void Start()
    {
        StartSever();
    }

    //接收到数据放入数据队列，按顺序取出
    void Update()
    {
        if (receiveDataQueue.Count > 0)
        {
            Response response = ServerProtoManager.TryDeserialize(receiveDataQueue.Dequeue());
        }

        foreach (KeyValuePair<int, Queue<Request>> kv in sendDataQueueDict)
        {
            Send(kv.Key);
        }
    }

    private void OnDisable()
    {
        Closed();
    }

    void OnDestroy()
    {
        Closed();
    }

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

    public void Closed()
    {
        foreach (ClientData clientData in clientList)
        {
            if (clientData.Client != null && clientData.Client.Connected)
            {
                clientData.Client.Shutdown(SocketShutdown.Both);
                clientData.Client.Close();
            }
        }

        clientList.Clear();
    }

    public void StartSever()
    {
        try
        {
            severSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            //将服务器的ip捆绑
            severSocket.Bind(new IPEndPoint(IPAddress.Parse(ip), port));
            //为服务器sokect添加监听
            severSocket.Listen(10);
            Debug.Log("[S]服务器启动成功");
            clientList = new List<ClientData>();
            //开始服务器时 一般接受一个服务就会被挂起所以要用多线程来解决
            Thread threadAccept = new Thread(Accept);
            threadAccept.IsBackground = true;
            threadAccept.Start();
        }
        catch
        {
            Debug.Log("[S]创建服务器失败");
        }
    }

    public void Accept()
    {
        Socket client = severSocket.Accept();
        ClientData clientData = new ClientData(client, 0, new DataHolder(), false);
        clientList.Add(clientData);
        IPEndPoint point = client.RemoteEndPoint as IPEndPoint;
        Debug.Log(point.Address + ":【" + point.Port + "】连接成功");
        Debug.Log("[S]客户端总数:" + clientList.Count);

        Thread threadReceive = new Thread(ReceiveSocket);
        threadReceive.IsBackground = true;
        threadReceive.Start(clientData);
        Accept();
    }

    #region 接收

    private void ReceiveSocket(object obj)
    {
        ClientData clientData = obj as ClientData;
        clientData.DataHolder.Reset();
        while (!clientData.IsStopReceive)
        {
            if (!clientData.Client.Connected)
            {
                //与客户端连接失败跳出循环  
                Debug.Log("[S]连接客户端失败,客户端ID" + clientData.ClientId);
                clientData.Client.Close();
                break;
            }

            try
            {
                byte[] bytes = new byte[1024];
                int i = clientData.Client.Receive(bytes);
                if (i <= 0)
                {
                    clientData.Client.Close();
                    Debug.Log("[S]客户端关闭,客户端ID" + clientData.ClientId);
                    break;
                }

                clientData.DataHolder.PushData(bytes, i);
                while (clientData.DataHolder.IsFinished())
                {
                    receiveDataQueue.Enqueue(clientData.DataHolder.mRecvData);
                    clientData.DataHolder.RemoveFromHead();
                }
            }
            catch (Exception e)
            {
                Debug.Log("[S]Failed to ServerSocket error." + e);
                clientData.Client.Close();
                break;
            }
        }
    }

    void Response(Response r)
    {
        string Log = "";
        Log += "服务器收到信息：[协议]" + r.GetProtocolName() + "[内容]";
        Log += r.DeserializeLog();
        Debug.Log(Log);

        if (r is ClientIdResponse)
        {
            ClientIdResponse resp = (ClientIdResponse) r;
            if (resp.purpose == ClientIdPurpose.RegisterClientId)
            {
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

    public void SendMessage(Request req, int clientId)
    {
        if (sendDataQueueDict.ContainsKey(clientId))
        {
            sendDataQueueDict[clientId].Enqueue(req);
        }
        else
        {
            Debug.Log("发送消息失败，客户端未连接到服务器,客户端ID" + clientId);
        }
    }

    public void Send(int clientId)
    {
        if (sendDataQueueDict.ContainsKey(clientId))
        {
            Request req = sendDataQueueDict[clientId].Dequeue();
            foreach (ClientData clientData in clientList)
            {
                Thread threadSend = new Thread(Send);
                threadSend.IsBackground = true;
                threadSend.Start(new SendMsg(clientData.Client, req));
            }
        }
        else
        {
            Debug.Log("发送消息失败，客户端未连接到服务器,客户端ID" + clientId);
        }
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

    //对特定客户端发送信息
    private void Send(object obj)
    {
        SendMsg sendMsg = (SendMsg) obj;

        if (sendMsg == null)
        {
            Debug.Log("[S]sendMsg is null");
            return;
        }

        if (sendMsg.Client == null)
        {
            Debug.Log("[S]client socket is null");
            return;
        }

        if (!sendMsg.Client.Connected)
        {
            Debug.Log("[S]Not connected to client socket");
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
                Closed();
            }
        }
        catch (Exception e)
        {
            Debug.Log("[S]Send error : " + e.ToString());
        }
    }

    private void SendCallback(IAsyncResult asyncConnect)
    {
        Debug.Log("[S]发送信息成功");
    }

    #endregion
}

public class ClientData
{
    public Socket Client;
    public int ClientId;
    public DataHolder DataHolder;
    public bool IsStopReceive;

    public ClientData(Socket client, int clientId, DataHolder dataHolder, bool isStopReceive)
    {
        Client = client;
        ClientId = clientId;
        DataHolder = dataHolder;
        IsStopReceive = isStopReceive;
    }
}