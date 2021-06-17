using System;
using System.Collections.Generic;
using Vortex;

namespace Crossplay
{
    public class Crossplay : Plugin
    {
        public static List<Client> MobileClients = new List<Client>();
        public override string Name => "Crossplay";
        public override string Author => "Moneylover3246";
        public override Version Version => new Version(1, 4, 2, 3);

        public override void Initalize()
        {
            DataHandlers.Add(new ClientDataHandler());
            DataHandlers.Add(new ServerDataHandler());
        }
    }
}
