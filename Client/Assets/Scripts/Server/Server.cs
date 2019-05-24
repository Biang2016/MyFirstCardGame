﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Threading;
using System.Xml;

internal class Server
{
    private static Server _sv;

    public static Server SV
    {
        get { return _sv; }
        set { _sv = value; }
    }

    private Server()
    {
    }

    public Server(string ip, int port)
    {
        IP = ip;
        Port = port;
    }

    public string IP;
    public int Port;
    public static string ServerVersion = "1.0.1";
    private Socket SeverSocket;
    public Dictionary<int, ClientProxy> ClientsDict = new Dictionary<int, ClientProxy>();
    private Queue<ReceiveSocketData> ReceiveDataQueue = new Queue<ReceiveSocketData>();

    public ServerGameMatchManager SGMM;

    public void Start()
    {
        Utils.DebugLog = ServerLog.Instance.PrintError;
        AllSideEffects.CurrentAssembly = Assembly.GetAssembly(typeof(Server));
        AllBuffs.CurrentAssembly = Assembly.GetAssembly(typeof(Server));
        LoadAllBasicXMLFiles.Load(ServerConsole.ServerRoot + "Config/");

        ServerLog.Instance.PrintServerStates("CardDeck Loaded");

        //Here to test cards, sideEffects, buffs

        //LanguageManager_Common.GetCurrentLanguage = LanguageDebug;

        //CardInfo_Base ci = AllCards.GetCard(50272);
        //string a = ci.GetCardDescShow();
        //        string text;
        //        using (StreamReader sr = new StreamReader(ServerConsole.ServerRoot + "Config/Cards.xml"))
        //        {
        //            text = sr.ReadToEnd();
        //        }
        //
        //        XmlDocument doc = new XmlDocument();
        //        doc.LoadXml(text);
        //        XmlElement allCards = doc.DocumentElement;
        //        ci.BaseExportToXML(allCards);

        //End

        SGMM = new ServerGameMatchManager();
        OnRestartProtocols();
        OnRestartSideEffects();
        StartSeverSocket();
    }

    public string LanguageDebug()
    {
        return "en";
    }

    private void OnRestartProtocols()
    {
        foreach (NetProtocols num in Enum.GetValues(typeof(NetProtocols)))
        {
            Common.ProtoManager.AddRequestDelegate((int) num, Response);
        }
    }

    private void OnRestartSideEffects() //所有的副作用在此注册
    {
        List<Type> types = Utils.GetClassesByNameSpace("SideEffects");
        MethodInfo mi = typeof(SideEffectManager).GetMethod("AddSideEffectTypes");
        foreach (Type type in types)
        {
            if (type == typeof(SideEffectBase))
            {
                MethodInfo mi_temp = mi.MakeGenericMethod(type);
                mi_temp.Invoke(null, null);
            }
        }
    }

    /// <summary>
    /// 所有的客户端提前异常退出、正常退出都走此方法
    /// </summary>
    /// <param name="clientProxy"></param>
    public void ClientProxyClose(ClientProxy clientProxy)
    {
        SGMM.OnClientCancelMatch(clientProxy);
        SGMM.RemoveGame(clientProxy);
        clientProxy.OnClose();
        ClientsDict.Remove(clientProxy.ClientId);
        UserDatabase.Instance.RemoveLoginUser(clientProxy.ClientId);
    }

    public void StartSeverSocket()
    {
        try
        {
            SeverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            //将服务器的ip捆绑
            SeverSocket.Bind(new IPEndPoint(IPAddress.Parse(IP), Port));
            //为服务器sokect添加监听
            SeverSocket.Listen(200);

            ServerLog.Instance.PrintServerStates("------------------ Server Start ------------------\n");

            //开始服务器时 一般接受一个服务就会被挂起所以要用多线程来解决
            Thread threadAccept = new Thread(Accept);
            threadAccept.IsBackground = true;
            threadAccept.Start();
        }
        catch (Exception e)
        {
            ServerLog.Instance.PrintError(e.Message);
            ServerLog.Instance.PrintError("Server start failed!");
        }
    }

    private int clientIdGenerator = 1000;

    public int GenerateClientId()
    {
        return clientIdGenerator++;
    }

    private void Accept()
    {
        Socket socket = SeverSocket.Accept();
        int clientId = GenerateClientId();
        ClientProxy clientProxy = new ClientProxy(socket, clientId, false);
        ClientsDict.Add(clientId, clientProxy);
        IPEndPoint point = socket.RemoteEndPoint as IPEndPoint;

        ServerLog.Instance.PrintClientStates("New client connection " + point.Address + ":" + point.Port + "  Clients count: " + ClientsDict.Count);

        Thread threadReceive = new Thread(ReceiveSocket);
        threadReceive.IsBackground = true;
        threadReceive.Start(clientProxy);
        Accept();
    }

    public void Stop()
    {
        foreach (KeyValuePair<int, ClientProxy> kv in ClientsDict)
        {
            ClientProxy clientProxy = kv.Value;
            if (clientProxy.Socket != null && clientProxy.Socket.Connected)
            {
                ServerLog.Instance.PrintClientStates("Client " + clientProxy.ClientId + " quit");

                ClientProxyClose(clientProxy);
            }
        }

        ClientsDict.Clear();
    }

    #region 接收

    //接收客户端Socket连接请求
    private void ReceiveSocket(object obj)
    {
        ClientProxy clientProxy = obj as ClientProxy;
        clientProxy.DataHolder.Reset();
        while (!clientProxy.IsStopReceive)
        {
            if (!clientProxy.Socket.Connected)
            {
                //与客户端连接失败跳出循环  

                ServerLog.Instance.PrintClientStates("Client connect failed, ID: " + clientProxy.ClientId + " IP: " + clientProxy.Socket.RemoteEndPoint);

                ClientProxyClose(clientProxy);
                break;
            }

            try
            {
                byte[] bytes = new byte[1024];
                int i = clientProxy.Socket.Receive(bytes);
                if (i <= 0)
                {
                    ServerLog.Instance.PrintClientStates("Client shutdown, ID: " + clientProxy.ClientId + " IP: " + clientProxy.Socket.RemoteEndPoint + "  Clients count: " + ClientsDict.Count);

                    ClientProxyClose(clientProxy);
                    break;
                }

                clientProxy.DataHolder.PushData(bytes, i);
                while (clientProxy.DataHolder.IsFinished())
                {
                    ReceiveSocketData rsd = new ReceiveSocketData(clientProxy.Socket, clientProxy.DataHolder.mRecvData);
                    ReceiveDataQueue.Enqueue(rsd);
                    clientProxy.DataHolder.RemoveFromHead();
                    OnReceiveMessage();
                }
            }
            catch (Exception e)
            {
                ServerLog.Instance.PrintError("Failed to ServerSocket error,ID: " + clientProxy.ClientId + " Error:" + e.ToString());

                ClientProxyClose(clientProxy);
                break;
            }
        }
    }

    private void OnReceiveMessage()
    {
        Thread threadReceive = new Thread(ReceiveMessage);
        threadReceive.IsBackground = true;
        threadReceive.Start();
    }

    private void ReceiveMessage()
    {
        if (ReceiveDataQueue.Count > 0)
        {
            ReceiveSocketData rsd = ReceiveDataQueue.Dequeue();
            DataStream stream = new DataStream(rsd.Data, true);
            Common.ProtoManager.TryDeserialize(stream, rsd.Socket);
        }
    }

    private void Response(Socket socket, RequestBase r)
    {
        if (r is ClientRequestBase)
        {
            ServerLog.Instance.PrintReceive("GetFrom clientId: " + ((ClientRequestBase) r).clientId + " <" + r.GetProtocol() + "> " + r.DeserializeLog());

            ClientRequestBase request = (ClientRequestBase) r;
            if (ClientsDict.ContainsKey(request.clientId))
            {
                ClientsDict[request.clientId].ReceiveMessage(request);
            }
        }
    }

    #endregion

    #region 发送信息

    //对特定客户端发送信息
    public void DoSendToClient(object obj)
    {
        SendMsg sendMsg = (SendMsg) obj;
        if (sendMsg == null)
        {
            ServerLog.Instance.PrintError("SendMsg is null");

            return;
        }

        if (sendMsg.Client == null)
        {
            ServerLog.Instance.PrintError("Client socket is null");

            return;
        }

        if (!sendMsg.Client.Connected)
        {
            ServerLog.Instance.PrintError("Not connected to client socket");

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

            writer.WriteSInt32(msg.Length); //增加数据长度
            writer.WriteRaw(msg);

            byte[] data = writer.ToByteArray();
            IAsyncResult asyncSend = sendMsg.Client.BeginSend(data, 0, data.Length, SocketFlags.None, new AsyncCallback(SendCallback), sendMsg.Client);
            bool success = asyncSend.AsyncWaitHandle.WaitOne(1000, true);
            if (!success)
            {
                ServerLog.Instance.PrintError("Send failed");
            }

            string log = "SendTo clientId: " + sendMsg.ClientId + sendMsg.Req.DeserializeLog();

            ServerLog.Instance.PrintSend(log);
        }
        catch (Exception e)
        {
            ServerLog.Instance.PrintError("Send Exception : " + e);
        }
    }

    private void SendCallback(IAsyncResult asyncConnect)
    {
        //ServerLog.Instance.Print("发送信息成功");
    }

    #endregion
}