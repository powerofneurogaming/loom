using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;

public class Client
{
    public static int dataBufferSize = 4096;

    public int id;
    public TCP tcp;

    public Client(int clientId)
    {
        id = clientId;
        tcp = new TCP(clientId);
    }

    public class TCP
    {
        public TcpClient socket;
        private readonly int id;
        private NetworkStream stream;
        private byte[] receiveBuffer;

        public TCP(int suppliedId)
        {
            id = suppliedId;
        }

        public void Connect(TcpClient suppliedSocket)
        {
            socket = suppliedSocket;
            socket.ReceiveBufferSize = dataBufferSize;
            socket.SendBufferSize = dataBufferSize;

            stream = socket.GetStream();
            receiveBuffer = new byte[dataBufferSize];
            stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);

            // TODO - Send some bingo packet
        }

        private void ReceiveCallback(IAsyncResult result)
        {
            try
            {
                int byteLength = stream.EndRead(result);
                if (byteLength <= 0)
                {
                    // TODO - disconnect
                    return;
                }

                byte[] data = new byte[byteLength];
                Array.Copy(receiveBuffer, data, byteLength);

                // TODO - need to handle data
                stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error receiving TCP data: {ex}");
                // TODO - disconnect
            }
        }
    }
}
