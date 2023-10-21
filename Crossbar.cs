using System;

namespace _Router_8Ports
{
    class Crossbar
    {
        private CrossbarSwitch[,] switches;
        public Crossbar(int inputCount, int outputCount)
        {
            switches = new CrossbarSwitch[inputCount, outputCount];
            for (int y = 0; y < inputCount; y++)
                for (int x = 0; x < outputCount; x++)
                {
                    switches[y, x] = new CrossbarSwitch(y, x);
                    switches[y, x].PacketReceived += Crossbar_PacketReceived;
                }
        }

        private void Crossbar_PacketReceived(object sender, PacketReceivedEventArgs e)
        {
            switches[e.NextSwitch_X, e.NextSwitch_Y].Route(e.packet);
        }

        public void Route(Packet packet)
        {
            switches[packet.InputPort, 0].Route(packet);
        }

        class CrossbarSwitch
        {
            private int _switch_X;
            private int _switch_Y;

            public CrossbarSwitch(int switch_X, int switch_Y)
            {
                _switch_X = switch_X;
                _switch_Y = switch_Y;
            }

            public void Route(Packet packet)
            {
                if (packet.GetDesX() > _switch_X) // Right
                {
                    packet.CrossRoute += "R-";
                    PacketReceivedEventArgs args = new PacketReceivedEventArgs();
                    args.packet = packet;
                    args.NextSwitch_X = _switch_X + 1;
                    args.NextSwitch_Y = _switch_Y;
                    OnPacketReceived(args);
                }
                else if (packet.GetDesY() < _switch_Y) // Down
                {
                    packet.CrossRoute += "D-";
                    PacketReceivedEventArgs args = new PacketReceivedEventArgs();
                    args.packet = packet;
                    args.NextSwitch_X = _switch_X;
                    args.NextSwitch_Y = _switch_Y - 1;
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
