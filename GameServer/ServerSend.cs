using System;
using System.Collections.Generic;
using System.Text;

namespace GameServer
{
    class ServerSend
    {
        
        
        #region TCP
        private static void SendTCPData(int toClient, Packet packet)
        {
            packet.WriteLength();
            Server.clients[toClient].tcp.SendData(packet);
        }

        private static void SendTCPDataToAll(Packet packet)
        {
            packet.WriteLength();
            for (int i = 1; i <= Server.maxPlayers; ++i)
            {
                Server.clients[i].tcp.SendData(packet);
            }
        }

        private static void SendTCPDataToAll(int exceptClient, Packet packet)
        {
            packet.WriteLength();
            for (int i = 1; i <= Server.maxPlayers; ++i)
            {
                if (i != exceptClient)
                {
                    Server.clients[i].tcp.SendData(packet);
                }
            }
        }
        #endregion
        #region UDP
        private static void SendUDPData(int toClient, Packet packet)
        {
            packet.WriteLength();
            Server.clients[toClient].udp.SendData(packet);
        }
        private static void SendUDPDataToAll(Packet packet)
        {
            packet.WriteLength();
            for (int i = 1; i <= Server.maxPlayers; ++i)
            {
                Server.clients[i].udp.SendData(packet);
            }
        }

        private static void SendUDPDataToAll(int exceptClient, Packet packet)
        {
            packet.WriteLength();
            for (int i = 1; i <= Server.maxPlayers; ++i)
            {
                if (i != exceptClient)
                {
                    Server.clients[i].udp.SendData(packet);
                }
            }
        }
        #endregion
        public static void Welcome(int toClient, string msg)
        {
            using (Packet packet = new Packet((int) ServerPackets.welcome))
            {
                packet.Write(msg);
                packet.Write(toClient);

                SendTCPData(toClient, packet);
            }
        }
        

        public static void SpawnPlayer(int toClient, Player.Player player)
        {
            using (Packet packet = new Packet((int) ServerPackets.spawnPlayer))
            {
                packet.Write(player.id);
                packet.Write(player.userName);
                packet.Write(player.position);
                packet.Write(player.rotation);

                SendTCPData(toClient, packet);
            }
        }

        public static void PlayerPosition(Player.Player player)
        {
            using (Packet packet = new Packet((int) ServerPackets.playerPosition))
            {
                packet.Write(player.id);
                packet.Write(player.position);

                SendUDPDataToAll(packet);
            }
        }
        public static void PlayerRotation(Player.Player player)
        {
            using (Packet packet = new Packet((int)ServerPackets.playerRotation))
            {
                packet.Write(player.id);
                packet.Write(player.rotation);

                SendUDPDataToAll(player.id, packet);
            }
        }
    }
}
