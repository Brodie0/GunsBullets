using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GunsBullets {
    sealed class Host {

        //singleton example, only one instance exists at runtime
        public static readonly Host instance = new Host();
        TcpListener serverSocket;
        IPEndPoint serverEndPoint;
        TcpClient client;
        List<Player> guests;

        internal List<Player> Guests { get => guests;}

        private Host() {
            serverEndPoint = new IPEndPoint(IPAddress.Loopback, Config.Port);
            serverSocket = new TcpListener(serverEndPoint);
            client = null;
            guests = new List<Player>(Config.MaxNumberOfGuests);
        }

        public void Start() {
            serverSocket.Start();
        }

        public void Stop() {
            client.Close();
            serverSocket.Stop();
            client = null;
            serverSocket = null;
            Console.WriteLine("Host finished working");
        }

        public void AddNewListeningThread() {
            try {
                ParallelOptions p = new ParallelOptions();
                p.MaxDegreeOfParallelism = Config.MaxNumberOfGuests;
                using (Task listeningTask = Task.Factory.StartNew(() => Listen()))
                    listeningTask.Start();
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
                client = serverSocket.AcceptTcpClient();
                Console.WriteLine("Connected!");
                NetworkStream stream = client.GetStream();

                while (client!=null) {
                    guest = ReadPlayerData(ref client, ref stream);
                    bool newGuest = true;
                    //there's second foreach loop in maingame (in other thread) so its necessary to lock both
                    lock (guests) {
                        foreach (Player actualGuest in guests) {
                            if (actualGuest.ServerIdentificationNumber == guest.ServerIdentificationNumber) {
                                guests[guest.ServerIdentificationNumber - 1] = guest;
                                newGuest = false;
                                break;
                            }
                        }
                    }
                    //TODO consider monitors usage 
                    Thread.Sleep(1);
                    if (newGuest && guests.Count < Config.MaxNumberOfGuests) {
                        //give him a key here
                        guest.ServerIdentificationNumber = guests.Count + 1;
                        //add guest to actual guests
                        guests.Add(guest);
                        WritePlayerKey(guest.ServerIdentificationNumber, ref stream);
                    }
                }
            }
        }

        private Player ReadPlayerData(ref TcpClient client, ref NetworkStream stream) {
            // Get a stream object for reading and writing
            Player guest = null;
            try {
                BinaryFormatter formatter = new BinaryFormatter();
                guest = (Player)formatter.Deserialize(stream);
            }
            catch(SerializationException e) {
                Console.WriteLine("Serialization failed. Reason: " + e.Message);
            }
            return guest;
        }

        private void WritePlayerKey(int key, ref NetworkStream stream) {
            //send the key
            byte[] keyBytes = BitConverter.GetBytes(key);
            stream.Write(keyBytes, 0, keyBytes.Length);
            Console.WriteLine("Host send a unique key: {0}", key);
        }
    }
}
