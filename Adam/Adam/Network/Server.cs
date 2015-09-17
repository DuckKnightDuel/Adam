﻿using Adam;
using Adam.GameData;
using Adam.Network.Packets;
using Microsoft.Xna.Framework.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;

namespace Adam.Network
{
    public class Server
    {
        UdpClient udpServer;
        TcpListener tcpListener;
        IPEndPoint serverIP;
        

        public List<IPEndPoint> clientIPs = new List<IPEndPoint>();
        public List<Client> clients = new List<Client>();
        public bool IsWaitingForPlayers { get; set; }

        BinaryFormatter formatter;

        public X509Certificate2 cert = new X509Certificate2();

        public Server()
        {
            Console.WriteLine("Starting server...");
            
            //This is where the game will be hosted.
            IPEndPoint serverIP = new IPEndPoint(IPAddress.Parse("127.0.0.1"),42555);
            Console.WriteLine("Server IP: {0}", serverIP);


            //The listener is to get incoming connections and the udp is for sending game data.
            udpServer = new UdpClient(new IPEndPoint(serverIP.Address,42557));

            Console.WriteLine("UDP server set up.");
            tcpListener = new TcpListener(serverIP);
            tcpListener.Start();
            Console.WriteLine("TCP listener set up.");


            //Starts a new thread that will continually look for clients while the game has not started.
            IsWaitingForPlayers = true;
            new Thread(new ThreadStart(ListenForClients)).Start();
            Console.WriteLine("Waiting for players...");
        }

        private void ListenForClients()
        {
            while (IsWaitingForPlayers)
            {
                Console.WriteLine("Waiting...");
                TcpClient newTcpClient = tcpListener.AcceptTcpClient();
                Console.WriteLine("A player is attempting to connect to the server...");
                Client newClient = new Client(this, newTcpClient);
            }
        }

        /// <summary>
        /// Sends an entity packet to all clients.
        /// </summary>
        /// <param name="gameWorld"></param>
        public void SendEntityPacket(GameWorld gameWorld)
        {
            EntityPacket en = new EntityPacket(gameWorld);
            byte[] packet = CalcHelper.ToByteArray(en);
            SendToClients(packet);
        }

        /// <summary>
        /// Send a level packet of the specified world config file.
        /// </summary>
        /// <param name="config"></param>
        public void SendLevelPacket(WorldConfigFile config)
        {
            byte[] data = CalcHelper.ToByteArray(new LevelPacket(config));
            foreach (Client c in clients)
            {
                c.SendLevelOverTCP(data);
            }
        }

        /// <summary>
        /// Used to send a byte packet to all clients via UDP.
        /// </summary>
        /// <param name="packet"></param>
        private void SendToClients(byte[] packet)
        {
            foreach (var ip in clientIPs)
            {
                IPEndPoint actualIP = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 42559);
                Console.WriteLine("Packet sent to {0}, from: {1}", actualIP, udpServer.Client.LocalEndPoint);
                udpServer.Send(packet, packet.Length, actualIP);
            }
        }

        public void SendMessage(string message)
        {          
            foreach (Client c in clients)
            {
                c.SendMessageOverTCP(message);
            }
        }

    }
}
