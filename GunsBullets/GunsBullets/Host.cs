using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace GunsBullets {
    sealed class Host {
        public TcpListener ServerSocket;
        public List<Player> Guests { get; private set; }

        private List<Player> players;
        
        public Host(ref List<Player> players) {
            ServerSocket = new TcpListener(Config.Port);
            Guests = new List<Player>(Config.MaxNumberOfGuests);

            this.players = players;
            ServerSocket.Start();
        }
        
        public void AddListeners() {
            try {
                for (int i = 0; i < Config.MaxNumberOfGuests; i++) {
                    Task.Factory.StartNew(() => Listen());
                }
            } catch (SocketException e) {
                Console.WriteLine($"SocketException: {e}");
            }
        }

        private void Listen() {
            while (true) {
                Console.WriteLine("Waiting for a connection... ");
                // Perform a blocking call to accept requests.
                // You could also user server.AcceptSocket() here.
                TcpClient client = ServerSocket.AcceptTcpClient();
                client.ReceiveTimeout = 1000; // in ms
                client.SendTimeout = 1000;

                Console.WriteLine("Connected!");
                using (NetworkStream stream = client.GetStream()) {
                    while (client.Connected) {
                        Player guest = Serialization.ReadPlayerData(stream);
                        bool newGuest = true;
                        lock (Guests) {
                            foreach (Player actualGuest in Guests) {
                                if (actualGuest.PlayerID == guest.PlayerID) {
                                    Guests[guest.PlayerID - 1] = guest;
                                    newGuest = false;
                                    break;
                                }
                            }

                            if (newGuest && Guests.Count < Config.MaxNumberOfGuests) {
                                // Send a unique key to each newly connected client.
                                // All clients know which key is theirs and from that
                                // know which player to drop from incoming player objects.
                                guest.PlayerID = Guests.Count + 1;

                                Guests.Add(guest);
                                WritePlayerKey(guest.PlayerID, stream);
                            }
                        }

                        if (!client.Connected) break;

                        // Update players                     
                        lock (players) {
                            players[0].PlayerID = 0;
                            if (players.Count > 1)
                                players.RemoveRange(1, players.Count - 1);
                            lock (Guests) players.AddRange(Guests);
                        }

                        if (!client.Connected) break;

                        //return info
                        WriteAllPlayersExceptGuest(guest, stream);
                    }
                }

                client.Close();
                Console.WriteLine("Client disconnected!");
            }
        }

        private void WritePlayerKey(int key, NetworkStream stream) {
            //send the key
            byte[] keyBytes = BitConverter.GetBytes(key);
            stream.Write(keyBytes, 0, keyBytes.Length);
            Console.WriteLine("Host sent a unique key: {0}", key);
        }

        private void WriteAllPlayersExceptGuest(Player guest, NetworkStream stream) {
            List<Player> otherGuests;
            lock (players) otherGuests = players.FindAll(player => player != guest);

            Byte[] b = Serialization.ObjectToByteArray(otherGuests);
            stream.Write(b, 0, b.Length);
        }
    }
}
