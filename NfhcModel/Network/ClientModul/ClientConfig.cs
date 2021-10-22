using NfhcModel.DataStructures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NfhcModel.Network.ClientModul
{
    public class ClientConfig
    {
        public string IP = "localhost";

        public int Port = 29050;

        public string ServerKey = "nfh";

        public float RefreshTime = 0.016f;

        public PlayerData LocalPlayer;

        public ClientConfig()
        {

        }
    }
}
