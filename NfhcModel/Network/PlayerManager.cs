using NfhcModel.DataStructures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NfhcModel.Network
{
    public class PlayerManager
    {
        public Dictionary<int, PlayerData> Players = new Dictionary<int, PlayerData>();

        public PlayerData LocalPlayer = new PlayerData();
    }
}
