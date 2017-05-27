using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;

namespace GunsBullets {
    sealed class Guest {
        public static readonly Guest instance = new Guest();
        TcpClient clientSocket;
        IPEndPoint serverEndPoint;
        private volatile Player _playerToSend;
        private Int32 serverIdentificationNumber;
        public Player PlayerToSend { get => _playerToSend; set => _playerToSend = value; }
        public int ServerIdentificationNumber { get => serverIdentificationNumber; set => serverIdentificationNumber = value; }

        private Guest() {
            // Create a TcpClient.
            // Note, for this client to work you need to have a TcpServer 
            // connected to the same address as specified by the server, port
            // combination.
            serverEndPoint = new IPEndPoint(IPAddress.Loopback, Config.Port);
            clientSocket = new TcpClient();
            serverIdentificationNumber = -1;
        }

        public void Stop() {
            clientSocket.Close();
            Console.WriteLine("Guest finished working");
        }

        public void Start() {
            try {
                Byte[] data;

                // Get a client stream for reading and writing.
                using (NetworkStream stream = LookForHost()) {
                    //while (true) {
                    // Send the message to the connected TcpServer. 
                    data = ObjectToByteArray(_playerToSend);
                    stream.Write(data, 0, data.Length);
                    Console.WriteLine("WYSYŁAM JAKIS SZIT");
                    //Thread.Sleep(500);
                    //}
                    //get unique key from host
                    data = new Byte[sizeof(Int32)];
                    stream.Read(data, 0, data.Length);
                    serverIdentificationNumber = BitConverter.ToInt32(data, 0);
                    Console.WriteLine("Received unique key: {0}", serverIdentificationNumber);

                    while (true) {
                        data = ObjectToByteArray(_playerToSend);
                        stream.Write(data, 0, data.Length);
                        Console.WriteLine("WYSYŁAM JAKIS SZIT");
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

        public NetworkStream LookForHost() {
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

        Byte[] ObjectToByteArray(object obj) {
            if (obj == null)
                return null;
            BinaryFormatter bf = new BinaryFormatter();
            using (MemoryStream ms = new MemoryStream()) {
                bf.Serialize(ms, obj);
                return ms.ToArray();
            }
        }
    }
}
