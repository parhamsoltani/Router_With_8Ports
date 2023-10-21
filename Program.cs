using System;
using System.Collections.Generic;
using System.IO;

namespace _Router_8Ports
{
    class Program
    {
        static void Main(string[] args)
        {
            #region Initialize
            // Define Trie for Lookup
            Trie trie = new Trie();
            List<string> portAddresses = new List<string>{
                "111*", // 111*
                "1111*", // 240/4
                "11111111*", // 255/8
                "111111111111*", // 255.240/12
                "1111111111111111*", // 255.255/16
                "11111111111111111111*", // 255.255.240/20
                "111111111111111111111111*", // 255.255.255/24
                "1111111111111111111111111111*"  // 255.255.255.240/28
            };
            Dictionary<string, string[]> portTable = new Dictionary<string, string[]>();

            int portCounter = 0;
            Random random = new Random();
            for (int i = 0; i < 8; i++)
            {
                trie.InsertNode(portAddresses[i], portCounter++.ToString());
                portAddresses[i] = GetCompleteAddress(portAddresses[i]);
                AddPortToTable(portTable, "P" + (i + 1), portAddresses[i], portAddresses[i], random.Next(0, 8).ToString());
            }
            Console.WriteLine("Preorder (VLR) tree traversal:");
            trie.PrintTrie(trie.RootNode, "*");

            // Define Output Ports
            RouterPort[] outputPorts = new RouterPort[8];
            for (int i = 0; i < 8; i++)
                outputPorts[i] = new RouterPort(i);

            // Define Input Ports
            RouterPort[] inputPorts = new RouterPort[8];
            for (int i = 0; i < 8; i++)
                inputPorts[i] = new RouterPort(i);

            // Create Crossbar Network
            Crossbar crossbar = new Crossbar(8, 8);

            // Create Mesh Network
            Mesh mesh = new Mesh(8, 8);

            // Set Timer to Zero
            int Timer = 0;
            #endregion

            // Read Input File and Open Output File
            string inputFileAddress = @"D:\packet_Info.txt";
            string outputFileAddress = @"D:\router_output.txt";
            StreamWriter outputFile = new StreamWriter(outputFileAddress);
            if (File.Exists(inputFileAddress))
            {
                string[] lines = File.ReadAllLines(inputFileAddress);
                foreach (string ln in lines)
                {
                    string line = ln.Replace(" ", string.Empty);
                    if (Convert.ToInt32(line.Split(',')[2]) != Timer)
                    {
                        for (int i = 0; i < 8; i++)
                        {
                            if (!inputPorts[i].IsBufferEmpty())
                            {
                                if (inputPorts[i].Buffer[0].Time == Timer)
                                {
                                    string outputString = "";
                                    crossbar.Route(inputPorts[i].Buffer[0]);
                                    mesh.Route(inputPorts[i].Buffer[0]);
                                    outputPorts[inputPorts[i].Buffer[0].GetDesX()].AddToBuffer(inputPorts[i].Buffer[0]);
                                    // Write Packet ID
                                    outputString += outputPorts[inputPorts[i].Buffer[0].GetDesX()].Buffer[0].ID + ", ";
                                    // Write Input and Output Buffer Location
                                    outputString += "0, 0, " + inputPorts[i].Buffer[0].GetDesX() + ", ";
                                    // Write Crossbar Routing Path
                                    outputString += inputPorts[i].Buffer[0].CrossRoute + ", ";
                                    // Write Mesh Routing Path
                                    outputString += inputPorts[i].Buffer[0].MeshRoute;
                                    // Write the String to File
                                    outputFile.WriteLine(outputString);
                                    inputPorts[i].UpdateBuffer();
                                }
                            }
                        }
                        Timer = Convert.ToInt32(line.Split(',')[2]);
                    }
                    Packet packet = new Packet();
                    packet.ID = Convert.ToInt32(line.Split(',')[0]);
                    packet.InputPort = Convert.ToInt32(line.Split(',')[1]);
                    packet.Time = Convert.ToInt32(line.Split(',')[2]);
                    packet.DestinationPort = TrieBasedLookupEngine(line.Split(',')[3], trie);
                    inputPorts[packet.InputPort].AddToBuffer(packet);
                }
                bool hasPacketsFinished = false;
                while (!hasPacketsFinished)
                {
                    hasPacketsFinished = true;
                    for (int i = 0; i < 8; i++)
                    {
                        if (!inputPorts[i].IsBufferEmpty())
                        {
                            hasPacketsFinished = false;
                            string outputString = "";
                            crossbar.Route(inputPorts[i].Buffer[0]);
                            mesh.Route(inputPorts[i].Buffer[0]);
                            outputPorts[inputPorts[i].Buffer[0].GetDesX()].AddToBuffer(inputPorts[i].Buffer[0]);
                            // Write Packet ID
                            outputString += outputPorts[inputPorts[i].Buffer[0].GetDesX()].Buffer[0].ID + ", ";
                            // Write Input Buffer Location, Output Buffer Location and Output Port Number
                            outputString += "0, 0, " + inputPorts[i].Buffer[0].GetDesX() + ", ";
                            // Write Crossbar Routing Path
                            outputString += inputPorts[i].Buffer[0].CrossRoute + ", ";
                            // Write Mesh Routing Path
                            outputString += inputPorts[i].Buffer[0].MeshRoute;
                            // Write the String to File
                            outputFile.WriteLine(outputString);
                            inputPorts[i].UpdateBuffer();
                        }
                    }
                }

            }
            outputFile.Close();

            // TCAM with Extension and Pruning
            Console.WriteLine("Port Table:");
            PrintPortTable(portTable);
            Console.WriteLine("Port Table with TCAM Extension:");
            var pt = TCAM_Extension(portTable);
            PrintPortTable(pt);
            Console.WriteLine("Port Table with TCAM Pruning:");
            pt = TCAM_Pruning(portTable);
            PrintPortTable(pt);
            Console.ReadKey();
        }

        static int TrieBasedLookupEngine(string address, Trie trie)
        {
            int portNumber = -1;
            TrieNode currentNode = trie.RootNode;
            if (currentNode.Type == NodeType.Gray)
                portNumber = int.Parse(currentNode.Label);
            for (int i = 0; address[i] != null; i++)
            {
                if (address[i] == '0')
                    currentNode = currentNode.LeftNode;
                else
                    currentNode = currentNode.RightNode;
                if (currentNode.Type == NodeType.Gray)
                    portNumber = int.Parse(currentNode.Label);
            }
            return portNumber;
        }

        static int TCAM_BasedLookupEngine(string address, List<string> ports)
        {
            int portNumber = -1;
            for (int i = 7; i >= 0; i--)
            {
                int j = 0;
                bool matched = true;
                while (ports[i][j] == '1')
                {
                    if (address[j] != '1')
                        matched = false;
                    j++;
                }
                if (matched)
                {
                    portNumber = i;
                    break;
                }
            }
            return portNumber;
        }

        static Dictionary<string, string[]> TCAM_Extension(Dictionary<string, string[]> portTable)
        {
            Dictionary<string, string[]> pt = new Dictionary<string, string[]>();
            bool[] matchFound = new bool[8];
            for (int i = 0; i < 8; i++)
                matchFound[i] = false;
            for (int i = 1; i <= 8; i++)
            {
                for (int j = i + 1; j <= 8; j++)
                {
                    if (portTable["P" + i][1] == portTable["P" + j][1] && portTable["P" + i][2] == portTable["P" + j][2])
                    {
                        matchFound[i - 1] = true;
                        matchFound[j - 1] = true;
                        int missMatch = 0;
                        string newPrefix = "";
                        string newMask = "";
                        for (int k = 0; k < 32; k++)
                        {
                            if (portTable["P" + i][0][k] != portTable["P" + j][0][k])
                            {
                                missMatch++;
                                newPrefix += '0';
                                newMask += '0';
                            }
                            else
                            {
                                newPrefix += portTable["P" + i][0][k];
                                newMask += portTable["P" + i][1][k];
                            }
                        }
                        if (missMatch <= 1)
                        {
                            string[] portValues = new string[3];
                            portValues[0] = newPrefix;
                            portValues[1] = newMask;
                            portValues[2] = portTable["P" + i][2];
                            pt.Add("P" + i + " & P" + j, portValues);
                        }
                    }
                }
                if (!matchFound[i - 1])
                    pt.Add("P" + i, portTable["P" + i]);
            }
            return pt;
        }

        static Dictionary<string, string[]> TCAM_Pruning(Dictionary<string, string[]> portTable)
        {
            Dictionary<string, string[]> pt = new Dictionary<string, string[]>();
            bool[] matchFound = new bool[8];
            for (int i = 0; i < 8; i++)
                matchFound[i] = false;
            for (int i = 1; i <= 8; i++)
            {
                for (int j = i + 1; j <= 8; j++)
                {
                    if (portTable["P" + i][2] == portTable["P" + j][2])
                    {
                        matchFound[i - 1] = true;
                        matchFound[j - 1] = true;
                        int count = 0;
                        string newPrefix = "";
                        string newMask = "";
                        for (int k = 0; k < 32; k++)
                        {
                            if (portTable["P" + i][1][k] != portTable["P" + j][1][k])
                                break;
                            else
                                count++;
                        }
                        if (count >= 1)
                        {
                            newPrefix += portTable["P" + i][0].Substring(0, count);
                            newMask += portTable["P" + i][1].Substring(0, count);
                            for (int k = 0; k < 32 - count; k++)
                            {
                                newPrefix += '0';
                                newMask += '0';
                            }
                            string[] portValues = new string[3];
                            portValues[0] = newPrefix;
                            portValues[1] = newMask;
                            portValues[2] = portTable["P" + i][2];
                            pt.Add("P" + i + " & P" + j, portValues);
                        }
                    }
                }
                if (!matchFound[i - 1])
                    pt.Add("P" + i, portTable["P" + i]);
            }
            return pt;
        }

        static void AddPortToTable(Dictionary<string, string[]> portTable, string portNumber, string prefix, string mask, string nextHopPort)
        {
            string[] portValues = new string[3];
            portValues[0] = prefix;
            portValues[1] = mask;
            portValues[2] = nextHopPort;
            portTable.Add(portNumber, portValues);
        }

        static void PrintPortTable(Dictionary<string, string[]> portTable)
        {
            Console.WriteLine("Port Number, Prefix, Mask, Next Hop Port");
            foreach (KeyValuePair<string, string[]> keyValuePair in portTable)
            {
                Console.WriteLine(keyValuePair.Key + ", " + keyValuePair.Value[0] + ", " + keyValuePair.Value[1] + ", " + keyValuePair.Value[2]);
            }
        }

        static string GetCompleteAddress(string portAddress)
        {
            int count = 0;
            string address = "";
            while (portAddress[count] != '*')
            {
                address += portAddress[count];
                count++;
            }
            for (int i = 0; i < 32 - count; i++)
            {
                address += '0';
            }
            return address;
        }
    }
}
