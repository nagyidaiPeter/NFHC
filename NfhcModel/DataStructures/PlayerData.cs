using LiteNetLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace NfhcModel.DataStructures
{
    public class PlayerData
    {
        public NetPeer Connection;

        public int ID;

        public string Name = "Woody";

        public Vector3 Position;

        public string Room;

        public GameObject Woody;
    }
}
