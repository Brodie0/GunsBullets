using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace GunsBullets {
    sealed class Host {

        //singleton example, only one instance exists at runtime
        public static readonly Host instance = new Host();
        TcpListener _serverSocket;
        IPEndPoint _serverEndPoint;
        TcpClient _client;
        List<Player> _allPlayers;
        ContentManager _content;

        internal List<Player> Guests { get; private set; }

        private Host() {
            _serverEndPoint = new IPEndPoint(IPAddress.Parse(Interface.GetLocalIPAddress()), Config.Port);
            _serverSocket = new TcpListener(_serverEndPoint);
            _client = null;
            Guests = new List<Player>(Config.MaxNumberOfGuests);
        }

        public void Start(ref List<Player> players, ContentManager contentP) {
            _content = contentP;
            _allPlayers = players;
            _serverSocket.Start();
        }

        public void Stop(List<Player> players) {
            if (_allPlayers.Count > 1)
                _allPlayers.RemoveRange(1, _allPlayers.Count - 1);
            _client.Close();
            _serverSocket.Stop();
            _client = null;
            _serverSocket = null;
            Console.WriteLine("Host finished working");
        }

        public void AddNewListeningThread(int count) {
            try {
                ParallelOptions p = new ParallelOptions();
                p.MaxDegreeOfParallelism = Config.MaxNumberOfGuests;
                for (int i = 0; i < count; i++) {
                    var listeningTask = Task.Factory.StartNew(() => Listen());
                }
            }
            catch (SocketException e) {
                Console.WriteLine($"SocketException: {e}");
            }
        }

        private void Listen() {
            Player guest = null;
            while (true) {
                Console.Write("Waiting for a connection... ");
                // Perform a blocking call to accept requests.
                // You could also user server.AcceptSocket() here.
                _client = _serverSocket.AcceptTcpClient();
                Console.WriteLine("Connected!");
                using (NetworkStream stream = _client.GetStream()) {

                    while (_client != null) {
                        guest = Serialization.ReadPlayerData(stream);
                        bool newGuest = true;
                        foreach (Player actualGuest in Guests) {
                            if (actualGuest.PlayerID == guest.PlayerID) {
                                Guests[guest.PlayerID - 1] = guest;
                                newGuest = false;
                                break;
                            }
                        }
                        if (newGuest && Guests.Count < Config.MaxNumberOfGuests) {
                            //give him a key here
                            guest.PlayerID = Guests.Count + 1;
                            //add guest to actual guests
                            Guests.Add(guest);
                            WritePlayerKey(guest.PlayerID, stream);
                        }
                        
                        //update players                     
                        lock (_allPlayers) {
                            _allPlayers[0].PlayerID = 0;
                            if (_allPlayers.Count > 1)
                                _allPlayers.RemoveRange(1, _allPlayers.Count - 1);
                            _allPlayers.AddRange(Guests);
                        }

                        //TODO consider monitors usage 
                        //return control to main thread
                        Thread.Sleep(1);

                        //return info
                        WriteAllPlayersExceptGuest(guest, stream);
                    }
                }
            }
        }

        private void WritePlayerKey(int key, NetworkStream stream) {
            //send the key
            byte[] keyBytes = BitConverter.GetBytes(key);
            stream.Write(keyBytes, 0, keyBytes.Length);
            Console.WriteLine("Host send a unique key: {0}", key);
        }

        private void WriteAllPlayersExceptGuest(Player guest, NetworkStream stream) {
            var otherGuests = new List<Player>(_allPlayers);
            otherGuests.Remove(guest);
            Byte[] b = Serialization.ObjectToByteArray(otherGuests);
            stream.Write(b, 0, b.Length);
        }
    }
}
