using System;
using System.IO;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace GunsBullets {
    class Serialization {
        public static byte[] ObjectToByteArray(object obj) {
            if (obj == null) return null;

            BinaryFormatter bf = new BinaryFormatter();
            using (MemoryStream ms = new MemoryStream()) {
                bf.Serialize(ms, obj);
                return ms.ToArray();
            }
        }

        public static Player ReadPlayerData(NetworkStream stream) {
            // Get a stream object for reading and writing
            Player guest = null;

            try {
                BinaryFormatter formatter = new BinaryFormatter();
                guest = (Player)formatter.Deserialize(stream);
            } catch (SerializationException e) {
                Console.WriteLine("Serialization failed. Reason: " + e.Message);
            }

            return guest;
        }

        public static List<Player> ReadListOfPlayersData(NetworkStream stream) {
            // Get a stream object for reading and writing
            List<Player> guests = null;

            try {
                BinaryFormatter formatter = new BinaryFormatter();
                guests = (List<Player>)formatter.Deserialize(stream);
            } catch (SerializationException e) {
                Console.WriteLine("Serialization failed. Reason: " + e.Message);
            }

            return guests;
        }
    }
}
