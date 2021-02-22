using System;
using System.Collections.Generic;
using System.Text;

using System.Net;
using System.Net.Sockets;

namespace GameServer
{
    class Server
    {
        public static int maxPlayers
        {
            get;
            private set;
        }

        public static int port
        {
            get;
            private set;
        }

        private static TcpListener tcpListener;
        public static Dictionary<int, Client> clients = new Dictionary<int, Client>();

        private static UdpClient udpListener;

        public delegate void PacketHandler(int fromClient, Packet packet);

        public static Dictionary<int, PacketHandler> packetHandlers;
        public static void Start(int _maxPlayer, int _port)
        {
            maxPlayers = _maxPlayer;
            port = _port;

            Console.WriteLine("Starting server...");
            InitializeServerData();

            tcpListener = new TcpListener(IPAddress.Any, port);
            tcpListener.Start();
            tcpListener.BeginAcceptTcpClient(new AsyncCallback(TCPConnectCallback), null);

            udpListener = new UdpClient(port);
            udpListener.BeginReceive(UDPReceiveCallback, null);

            Console.WriteLine($"Server started on {port}");
        }

        //If user enter, this CallBack function will executed.
        //This will find & assign empty slot for incomming user.
        private static void TCPConnectCallback(IAsyncResult result)
        {
            //result will represent connection with Unity?


            TcpClient client = tcpListener.EndAcceptTcpClient(result);
            tcpListener.BeginAcceptTcpClient(new AsyncCallback(TCPConnectCallback), null);

            Console.WriteLine($"Incomming! : {client.Client.RemoteEndPoint}");

            for (int i = 1; i <= maxPlayers; ++i)
            {
                //means slot is empty
                if (clients[i].tcp.socket == null)
                {
                    clients[i].tcp.Connect(client);
                    return;
                }
            }

            //If this line is executed, means server slot is full.
            Console.WriteLine($"{client.Client.RemoteEndPoint} failed to connect : server is full!");
        }

        private static void UDPReceiveCallback(IAsyncResult result)
        {
            try
            {
                IPEndPoint clientEndPoint = new IPEndPoint(IPAddress.Any, 0);
                byte[] data = udpListener.EndReceive(result, ref clientEndPoint);
                udpListener.BeginReceive(UDPReceiveCallback, null);

                if (data.Length < 4)
                {
                    return;
                }

                using (Packet packet = new Packet(data))
                {
                    int clientId = packet.ReadInt();

                    if (clientId == 0)
                    {
                        return;
                    }

                    if (clients[clientId].udp.endPoint == null)
                    {
                        clients[clientId].udp.Connect(clientEndPoint);
                        return;
                    }

                    if (clients[clientId].udp.endPoint.ToString() == clientEndPoint.ToString())
                    {
                        clients[clientId].udp.HandleData(packet);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error receiving UDP Data : {ex}");
            }
        }

        public static void SendUDPData(IPEndPoint clientEndPoint, Packet packet)
        {
            try
            {
                if (clientEndPoint != null)
                {
                    udpListener.BeginSend(packet.ToArray(), packet.Length(), clientEndPoint, null, null);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending data to {clientEndPoint} via UDP : {ex}");
            }
        }

        private static void InitializeServerData()
        {
            for (int i = 1; i <= maxPlayers; ++i)
            {
                clients.Add(i, new Client(i));
            }

            packetHandlers = new Dictionary<int, PacketHandler>()
            {
                { (int)ClientPackets.welcomeReceived, ServerHandle.WelcomeReceived},
                { (int)ClientPackets.playerMovement, ServerHandle.PlayerMovement},
            };
            
        }
    }
}
