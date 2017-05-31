using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace GunsBullets {
    sealed class Host {
        public static readonly Host instance = new Host(); // Only one instance!

        TcpListener serverSocket;
        TcpClient client = null;
        List<Player> players;

        internal List<Player> Guests { get; private set; }

        private Host() {
            serverSocket = new TcpListener(Config.Port);
            Guests = new List<Player>(Config.MaxNumberOfGuests);
        }

        public void Start(ref List<Player> players) {
            this.players = players;
            serverSocket.Start();
        }

        public void Stop() {
            if (players.Count > 1) players.RemoveRange(1, players.Count - 1);
            if (client != null) {
                client.Close();
                client = null;
            }

            if (serverSocket != null) {
                serverSocket.Stop();
                serverSocket = null;
            }
            
            Console.WriteLine("Host stopped.");
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
                Console.WriteLine("Waiting for a connection... ");
                // Perform a blocking call to accept requests.
                // You could also user server.AcceptSocket() here.
                client = serverSocket.AcceptTcpClient();
                Console.WriteLine("Connected!");
                using (NetworkStream stream = client.GetStream()) {

                    while (client != null) {
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
                        lock (players) {
                            players[0].PlayerID = 0;
                            if (players.Count > 1)
                                players.RemoveRange(1, players.Count - 1);
                            players.AddRange(Guests);
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
            var otherGuests = new List<Player>(players);
            otherGuests.Remove(guest);
            Byte[] b = Serialization.ObjectToByteArray(otherGuests);
            stream.Write(b, 0, b.Length);
        }
    }
}
