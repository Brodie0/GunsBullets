using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections.Generic;

namespace GunsBullets {
    sealed class Guest {
        TcpClient clientSocket;
        IPEndPoint serverEndPoint;
        private Int32 playerID;
        private List<Player> _otherPlayers;
        private List<Player> _allPlayers;

        internal Player PlayerToSend { get; set; }

        public Guest(ref List<Player> allPlayers) {
            // Create a TcpClient.
            // Note, for this client to work you need to have a TcpServer 
            // connected to the same address as specified by the server, port
            // combination.
            serverEndPoint = new IPEndPoint(IPAddress.Parse(Config.IPHostname), Config.Port);
            clientSocket = new TcpClient();
            playerID = -1;
            _otherPlayers = new List<Player>(Config.MaxNumberOfGuests);

            _allPlayers = allPlayers;
            PlayerToSend = allPlayers[0];
        }
        
        public void StartCommunicationThread() {
            try {
                Byte[] data;
                clientSocket.Connect(serverEndPoint);

                // Get a client stream for reading and writing.
                using (NetworkStream stream = clientSocket.GetStream()) {
                    // Send the message to the connected TcpServer.
                    lock (PlayerToSend) data = Serialization.ObjectToByteArray(PlayerToSend);
                    stream.Write(data, 0, data.Length);

                    //get unique key from host
                    data = new Byte[sizeof(Int32)];
                    stream.Read(data, 0, data.Length);
                    playerID = BitConverter.ToInt32(data, 0);
                    Console.WriteLine("Received unique key: {0}", playerID);

                    while (true) {
                        PlayerToSend.PlayerID = playerID;
                        lock (PlayerToSend) data = Serialization.ObjectToByteArray(PlayerToSend);
                        stream.Write(data, 0, data.Length);

                        if (!clientSocket.Connected) break;

                        GetListOfOtherPlayerFromHost(stream);
                        Thread.Sleep(Config.SendingPackagesDelay);

                        if (!clientSocket.Connected) break;
                    }
                }
            } catch (ArgumentNullException e) {
                Console.WriteLine($"ArgumentNullException: {e}");
            } catch (SocketException e) {
                Console.WriteLine($"SocketException: {e}");
            }
        }

        private void GetListOfOtherPlayerFromHost(NetworkStream stream) {
            _otherPlayers = Serialization.ReadListOfPlayersData(stream);
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
