using System;

namespace _Router_8Ports
{
    class Mesh
    {
        private MeshSwitch[,] switches;
        public Mesh(int inputCount, int outputCount)
        {
            switches = new MeshSwitch[inputCount, outputCount];
            for (int y = 0; y < inputCount; y++)
                for (int x = 0; x < outputCount; x++)
                {
                    switches[y, x] = new MeshSwitch(y, x);
                    switches[y, x].PacketReceived += Mesh_PacketReceived;
                }
        }

        private void Mesh_PacketReceived(object sender, PacketReceivedEventArgs e)
        {
            switches[e.NextSwitch_X, e.NextSwitch_Y].Route(e.packet);
        }

        public void Route(Packet packet)
        {
            switches[packet.InputPort, 0].Route(packet);
        }

        class MeshSwitch
        {
            private int _switch_X;
            private int _switch_Y;

            public MeshSwitch(int switch_X, int switch_Y)
            {
                _switch_X = switch_X;
                _switch_Y = switch_Y;
            }

            public void Route(Packet packet)
            {
                if (packet.GetDesX() > _switch_X) // Right
                {
                    packet.MeshRoute += "R-";
                    PacketReceivedEventArgs args = new PacketReceivedEventArgs();
                    args.packet = packet;
                    args.NextSwitch_X = _switch_X + 1;
                    args.NextSwitch_Y = _switch_Y;
                    OnPacketReceived(args);
                }
                else if (packet.GetDesX() < _switch_X) // Left
                {
                    packet.MeshRoute += "L-";
                    PacketReceivedEventArgs args = new PacketReceivedEventArgs();
                    args.packet = packet;
                    args.NextSwitch_X = _switch_X - 1;
                    args.NextSwitch_Y = _switch_Y;
                    OnPacketReceived(args);
                }
                else if (packet.GetDesY() < _switch_Y) // Down
                {
                    packet.MeshRoute += "D-";
                    PacketReceivedEventArgs args = new PacketReceivedEventArgs();
                    args.packet = packet;
                    args.NextSwitch_X = _switch_X;
                    args.NextSwitch_Y = _switch_Y - 1;
                    OnPacketReceived(args);
                }
                else if (packet.GetDesY() > _switch_Y) // Up
                {
                    packet.MeshRoute += "U-";
                    PacketReceivedEventArgs args = new PacketReceivedEventArgs();
                    args.packet = packet;
                    args.NextSwitch_X = _switch_X;
                    args.NextSwitch_Y = _switch_Y + 1;
                    OnPacketReceived(args);
                }
            }


            public event EventHandler<PacketReceivedEventArgs> PacketReceived;
            protected virtual void OnPacketReceived(PacketReceivedEventArgs e)
            {
                PacketReceived?.Invoke(this, e);
            }
        }
    }
}
