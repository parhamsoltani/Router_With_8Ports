using System.Security.Cryptography;

namespace _Router_8Ports
{
    class RouterPort
    {
        public int PortNumber;
        public Packet[] Buffer;

        public RouterPort(int portNumber)
        {
            PortNumber = portNumber;
            Buffer = new Packet[4];
            for (int i = 0; i < 4; i++)
                Buffer[i] = null;
        }

        public void AddToBuffer(Packet packet)
        {
            for (int i = 0; i < 4; i++)
                if (Buffer[i] == null)
                    Buffer[i] = packet;
        }

        public bool IsBufferEmpty()
        {
            for (int i = 0; i < 4; i++)
                if (Buffer[i] != null)
                    return false;
            return true;
        }

        public void UpdateBuffer()
        {
            for (int i = 0; i < 3; i++)
                Buffer[i] = Buffer[i + 1];
            Buffer[3] = null;
        }
    }
}
