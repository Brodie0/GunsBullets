using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace GunsBullets {
    sealed class Guest {
        public static readonly Guest instance = new Guest();
        TcpClient clientSocket;
        IPEndPoint serverEndPoint;
        private Int32 playerID;
        private ContentManager _content;
        private List<Player> _otherPlayers;
        private List<Player> _allPlayers;

        internal Player PlayerToSend { get; set; }

        private Guest() {
            // Create a TcpClient.
            // Note, for this client to work you need to have a TcpServer 
            // connected to the same address as specified by the server, port
            // combination.
            serverEndPoint = new IPEndPoint(IPAddress.Loopback, Config.Port);
            clientSocket = new TcpClient();
            playerID = -1;
            _otherPlayers = new List<Player>(Config.MaxNumberOfGuests);
        }

        public void Start(List<Player> allPlayers, ContentManager content) {
            _allPlayers = allPlayers;
            _content = content;
            PlayerToSend = allPlayers[0];
        }

        public void Stop() {
            clientSocket.Close();
            Console.WriteLine("Guest finished working");
        }

        public void StartCommunicationThread() {
            try {
                Byte[] data;
                // Get a client stream for reading and writing.
                using (NetworkStream stream = LookForHost()) {
                    // Send the message to the connected TcpServer.
                    lock (PlayerToSend) {
                        data = Serialization.ObjectToByteArray(PlayerToSend);
                    }               
                    stream.Write(data, 0, data.Length);

                    //get unique key from host
                    data = new Byte[sizeof(Int32)];
                    stream.Read(data, 0, data.Length);
                    playerID = BitConverter.ToInt32(data, 0);
                    Console.WriteLine("Received unique key: {0}", playerID);

                    while (true) {
                        PlayerToSend.PlayerID = playerID;
                        data = Serialization.ObjectToByteArray(PlayerToSend);
                        stream.Write(data, 0, data.Length);

                        GetListOfOtherPlayerFromHost(stream);
                        Thread.Sleep(Config.SendingPackagesDelay);
                    }
                }
            }
            catch (ArgumentNullException e) {
                Console.WriteLine("ArgumentNullException: {0}", e);
            }
            catch (SocketException e) {
                Console.WriteLine("SocketException: {0}", e);
            }
        }

        private NetworkStream LookForHost() {
            NetworkStream stream = null;
            try {
                clientSocket.Connect(serverEndPoint);
                stream = clientSocket.GetStream();
            }
            catch (SocketException) {
                Console.WriteLine($"NIE ZNALEZIONO HOSTA POD ADRESEM: {serverEndPoint}");
            }
            return stream;
        }

        private void GetListOfOtherPlayerFromHost(NetworkStream stream) {
            _otherPlayers = Serialization.ReadListOfPlayersData(stream);
            Console.WriteLine("                      L I S T A:                 \n");
            _otherPlayers.ForEach(Console.WriteLine);

            AddOrRefreshPlayers();
        }

        private void AddOrRefreshPlayers() {
            lock (_allPlayers) {
                if (_allPlayers.Count > 1)
                    _allPlayers.RemoveRange(1, _allPlayers.Count - 1);
                _allPlayers.AddRange(_otherPlayers);
            }
        }
    }
}
