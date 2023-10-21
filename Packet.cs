using System;

namespace _Router_8Ports
{
    class Packet
    {
        public int ID;
        public int InputPort;
        public int Time;
        public int DestinationPort;
        public string CrossRoute;
        public string MeshRoute;

        public Packet()
        {
            CrossRoute = "";
            MeshRoute = "";
        }

        public int GetDesX()
        {
            return DestinationPort;
        }

        public int GetDesY()
        {
            return InputPort;
        }
    }

    class PacketReceivedEventArgs : EventArgs
    {
        public Packet packet { get; set; }
        public int NextSwitch_X { get; set; }
        public int NextSwitch_Y { get; set; }
    }
}
