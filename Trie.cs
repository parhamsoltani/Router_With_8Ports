using System;

namespace _Router_8Ports
{
    enum NodeType { White, Gray };
    class TrieNode
    {
        public NodeType Type;
        public TrieNode LeftNode;
        public TrieNode RightNode;
        public string Label;
        public int Skip;

        public TrieNode(NodeType nodeType)
        {
            Type = nodeType;
            LeftNode = null;
            RightNode = null;
            Label = null;
            Skip = 0;
        }
    }

    class Trie
    {
        public TrieNode RootNode;
        public Trie()
        {
            RootNode = new TrieNode(NodeType.White);
        }

        public void InsertNode(string nodeAddress, string nodeLabel)
        {
            try
            {
                int i = 0;
                TrieNode currentNode = RootNode;
                while (nodeAddress[i] != '*')
                {
                    for (int j = 0; j < currentNode.Skip; j++)
                        i++;
                    if (nodeAddress[i] == '0')
                    {
                        if (currentNode.LeftNode == null)
                        {
                            currentNode.LeftNode = new TrieNode(NodeType.White);
                        }

                        currentNode = currentNode.LeftNode;
                    }
                    else
                    {
                        if (currentNode.RightNode == null)
                        {
                            currentNode.RightNode = new TrieNode(NodeType.White);
                        }
                        currentNode = currentNode.RightNode;
                    }
                    i++;
                }
                currentNode.Label = nodeLabel;
                currentNode.Type = NodeType.Gray;
            }
            catch (Exception) { }
        }

        public void DeleteNode(String nodeAddress)
        {
            try
            {
                int i = 0;
                TrieNode currentNode = RootNode;
                TrieNode prevNode = null;
                while (nodeAddress[i] != '*')
                {
                    for (int j = 0; j < currentNode.Skip; j++)
                        i++;
                    if (nodeAddress[i] == '0')
                    {
                        prevNode = currentNode;
                        currentNode = currentNode.LeftNode;
                    }
                    else
                    {
                        prevNode = currentNode;
                        currentNode = currentNode.RightNode;
                    }
                    i++;
                }
                if (currentNode.LeftNode == null && currentNode.RightNode == null)
                {
                    currentNode = null;
                    if (nodeAddress[i - 1] == '0')
                        prevNode.LeftNode = null;
                    else
                        prevNode.RightNode = null;
                }
                else if (currentNode.LeftNode == null)
                {
                    if (nodeAddress[i - 1] == '0')
                        prevNode.LeftNode = currentNode.RightNode;
                    else
                        prevNode.RightNode = currentNode.RightNode;
                    currentNode = null;
                }
                else if (currentNode.RightNode == null)
                {
                    if (nodeAddress[i - 1] == '0')
                        prevNode.LeftNode = currentNode.LeftNode;
                    else
                        prevNode.RightNode = currentNode.LeftNode;
                    currentNode = null;
                }
                else
                {
                    currentNode.Label = null;
                    currentNode.Type = NodeType.White;
                }
            }
            catch (Exception) { }
        }

        public TrieNode PathCompress(TrieNode trieNode)
        {
            try
            {
                if (trieNode.LeftNode == null && trieNode.RightNode == null)
                    return trieNode;
                else
                {
                    if (trieNode.Type == NodeType.Gray)
                    {
                        if (trieNode.RightNode != null)
                            trieNode.RightNode = PathCompress(trieNode.RightNode);
                        if (trieNode.LeftNode != null)
                            trieNode.LeftNode = PathCompress(trieNode.LeftNode);
                    }
                    else
                    {
                        if (trieNode.RightNode != null && trieNode.LeftNode != null)
                        {
                            trieNode.LeftNode = PathCompress(trieNode.LeftNode);
                            trieNode.RightNode = PathCompress(trieNode.RightNode);
                        }
                        else if (trieNode.RightNode == null)
                        {
                            trieNode = PathCompress(trieNode.LeftNode);
                            trieNode.Skip++;
                        }
                        else
                        {
                            trieNode = PathCompress(trieNode.RightNode);
                            trieNode.Skip++;
                        }
                    }
                    return trieNode;
                }
            }
            catch (Exception)
            {
                return trieNode;
            }
        }

        public void PrintTrie(TrieNode trieNode, string address)
        {
            try
            {
                if (trieNode.Type == NodeType.Gray)
                    Console.WriteLine(address + "* , " + trieNode.Label);
                if (trieNode.LeftNode != null)
                    PrintTrie(trieNode.LeftNode, address + '0');
                if (trieNode.RightNode != null)
                    PrintTrie(trieNode.RightNode, address + '1');
            }
            catch (Exception)
            {
                Console.WriteLine("An error occurred while printing the trie.");
            }
        }
    }
}
