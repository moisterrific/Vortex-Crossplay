using System;
using System.Collections.Generic;
using Vortex;
using Vortex.DataHandlers;
using Vortex.Enums;

namespace Crossplay
{
    public class ClientDataHandler : IDataHandler
    {
        public static Dictionary<PacketTypes, ServerGetDataDelegate> DataHandlerDelegates => new Dictionary<PacketTypes, ServerGetDataDelegate>()
        {
            { PacketTypes.ConnectRequest, HandleConnectRequest },
            { PacketTypes.TileSendSquare, HandleSendTileSquare }
        };

        public DataHandlerType HandlerType => DataHandlerType.Client;

        public int Priority => -1;

        public bool OnReceive(PacketTypes type, Client client, ref byte[] packet)
        {
            ServerGetDataArgs args = new ServerGetDataArgs(client, packet);
            if (type == PacketTypes.ConnectRequest || Crossplay.MobileClients.Contains(client))
            {
                if ((int)client.SenderType == (int)HandlerType)
                {
                    if (DataHandlerDelegates.TryGetValue(type, out ServerGetDataDelegate handler))
                    {
                        handler(args);
                    }
                    packet = args.Data;
                }

            }
            return args.Handled;
        }

        private static void HandleConnectRequest(ServerGetDataArgs args)
        {
            string version = args.Reader.ReadString();
            if (version == "Terraria230")
            {
                Crossplay.MobileClients.Add(args.Client);
                byte[] data = new PacketFactory()
                    .SetType(1)
                    .PackString("Terraria238")
                    .GetByteData();
                args.Data = data;
            }
        }

        private static void HandleSendTileSquare(ServerGetDataArgs args)
        {
            ushort size = args.Reader.ReadUInt16();
            byte tileChangeType = 0;
            if ((size & 32768) > 0)
            {
                tileChangeType = args.Reader.ReadByte();
            }
            short tileX = args.Reader.ReadInt16();
            short tileY = args.Reader.ReadInt16();
            byte[] data = new PacketFactory()
                .SetType(20)
                .PackInt16(tileX)
                .PackInt16(tileY)
                .PackByte((byte)size)
                .PackByte((byte)size)
                .PackByte(tileChangeType)
                .PackBuffer(args.Reader.ReadBytes((int)(args.Reader.BaseStream.Length - args.Reader.BaseStream.Position)))
                .GetByteData();
            args.Data = data;
        }
    }
}
