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
        int guestsConnected = 0;
        TcpClient client;
        List<Player> guests;

        internal List<Player> Guests { get => guests;}

        private Host() {
            serverEndPoint = new IPEndPoint(IPAddress.Loopback, Config.Port);
            serverSocket = new TcpListener(serverEndPoint);
            client = null;
            guests = new List<Player>(Config.MaxNumberOfPlayers);
        }

        public void Stop() {
            client.Close();
            serverSocket.Stop();
            client = null;
            serverSocket = null;
            Console.WriteLine("Host finished working");
        }

        public void Start() {
            try {
                ParallelOptions p = new ParallelOptions();
                p.MaxDegreeOfParallelism = Config.MaxNumberOfPlayers;
                serverSocket.Start();
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
                guestsConnected++;
                while (client!=null) {
                    Console.WriteLine("petla!");
                    guest = ReadData(ref client);
                    //guests.RemoveAt(0);
                    guests.Add(guest);
                    Console.WriteLine(guest.ToString());
                    Console.WriteLine("Guests Connected: " + guestsConnected);
                }
            }
        }

        private Player ReadData(ref TcpClient client) {
            // Get a stream object for reading and writing
            Player guest = null;
            NetworkStream stream = client.GetStream();
            try {
                BinaryFormatter formatter = new BinaryFormatter();
                guest = (Player)formatter.Deserialize(stream);
            }
            catch(SerializationException e) {
                Console.WriteLine("Serialization failed. Reason: " + e.Message);
            }
            finally {
                stream.Close();
            }
            return guest;
        }
    }
}
