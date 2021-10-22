using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NfhcModel.Network.ServerModul
{
    public class ServerConfig
    {
        public int Port = 29050;

        public string ServerName = "NFH - Coop";

        public string ServerKey = "nfh";

        public int MaxPlayers = 4;

        public float RefreshTime = 0.016f;
    }
}
