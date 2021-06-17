using System;
using System.Collections.Generic;
using System.Text;
using Vortex;
using Vortex.DataHandlers;
using Vortex.Enums;

namespace Crossplay
{
    internal class ServerDataHandler : IDataHandler
    {
        public static Dictionary<PacketTypes, ClientGetDataDelegate> DataHandlerDelegates => new Dictionary<PacketTypes, ClientGetDataDelegate>()
        {
            { PacketTypes.WorldInfo, HandleWorldInfo },
            { PacketTypes.TileSendSquare, HandleSendTileSquare },
            { PacketTypes.NpcUpdate, HandleNPCUpdate },
            { PacketTypes.ProjectileNew, HandleProjectileUpdate },
            { PacketTypes.LoadNetModule, HandleLoadNetModule }
        };

        public DataHandlerType HandlerType => DataHandlerType.Server;

        public int Priority => 10;

        public bool OnReceive(PacketTypes type, Client client, ref byte[] packet)
        {
            ClientGetDataArgs args = new ClientGetDataArgs(client, packet);
            if (Crossplay.MobileClients.Contains(client))
            {
                if ((int)client.Server.SenderType == (int)HandlerType)
                {
                    if (DataHandlerDelegates.TryGetValue(type, out ClientGetDataDelegate handler))
                    {
                        handler(args);
                    }
                }
                packet = args.Data;
            }
            return args.Handled;
        }
        private static void HandleSendTileSquare(ClientGetDataArgs args)
        {
            short tileX = args.Reader.ReadInt16();
            short tileY = args.Reader.ReadInt16();
            ushort width = args.Reader.ReadByte();
            ushort length = args.Reader.ReadByte();
            byte tileChangeType = args.Reader.ReadByte();
            ushort size = Math.Min(width, length);
            PacketFactory data = new PacketFactory()
                .SetType(20)
                .PackUInt16(size);
            if (tileChangeType != 0)
            {
                data.PackByte(tileChangeType);
            }
            data.PackInt16(tileX);
            data.PackInt16(tileY);
            data.PackBuffer(args.Reader.ReadBytes((int)(args.Reader.BaseStream.Length - args.Reader.BaseStream.Position)));
            byte[] buffer = data.GetByteData();
            args.Data = buffer;
        }

        private static void HandleWorldInfo(ClientGetDataArgs args)
        {
            byte[] buffer = args.Reader.ReadBytes(22);
            string worldName = args.Reader.ReadString();
            byte[] buffer2 = args.Reader.ReadBytes(103);
            args.Reader.ReadByte(); // Main.tenthAnniversaryWorld
            byte[] buffer3 = args.Reader.ReadBytes(27);
            byte[] data = new PacketFactory()
                .SetType(7)
                .PackBuffer(buffer)
                .PackString(worldName)
                .PackBuffer(buffer2)
                .PackBuffer(buffer3)
                .GetByteData();
            args.Data = data;
        }

        private static void HandleNPCUpdate(ClientGetDataArgs args)
        {
            short who = args.Reader.ReadInt16();
            if (args.Server.World.NPC[who] != null && args.Server.World.NPC[who].Type > 662)
            {
                args.Handled = true;
            }
        }

        private static void HandleProjectileUpdate(ClientGetDataArgs args)
        {
            short who = args.Reader.ReadInt16();
            if (args.Server.World.Projectile[who] != null && args.Server.World.Projectile[who].ProjNetID > 948)
            {
                args.Handled = true;
            }
        }

        private static void HandleLoadNetModule(ClientGetDataArgs args)
        {
            ushort netModuleID = args.Reader.ReadUInt16();
            switch (netModuleID)
            {
                case 4:
                    byte unlockType = args.Reader.ReadByte();
                    short npcID = args.Reader.ReadInt16();
                    if (npcID > 662)
                    {
                        Array.Clear(args.Data, 0, args.Data.Length);
                    }
                    break;
                case 5:
                    short itemID = args.Reader.ReadInt16();
                    if (itemID > 5044)
                    {
                        Array.Clear(args.Data, 0, args.Data.Length);
                    }
                    break;
            }
        }
    }
}
