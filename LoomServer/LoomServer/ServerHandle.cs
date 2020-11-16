using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace LoomServer
{
    class ServerHandle
    {
        public static void WelcomeReceived(int _fromClient, Packet _packet)
        {
            int _clientIdCheck = _packet.ReadInt();
            string _username = _packet.ReadString();

            Console.WriteLine($"{Server.clients[_fromClient].tcp.socket.Client.RemoteEndPoint} connected successfully and is now player {_fromClient}.");
            if (_fromClient != _clientIdCheck)
            {
                Console.WriteLine($"Player \"{_username}\" (ID: {_fromClient}) has assumed the wrong client ID ({_clientIdCheck})!");
            }
            // send player into game
            Server.clients[_fromClient].SendIntoGame(_username);
        }

        //public static void UDPTestReceived(int _fromClient, Packet _packet)
        //{
        //    string _msg = _packet.ReadString();

        //    Console.WriteLine($"Received packet via UDP. Contains message: {_msg}");
        //}
        public static void PlayerStats(int _fromClient, Packet _packet)
        {
            int size = _packet.ReadInt();
            Vector3[] _positions = new Vector3[size];
            Quaternion[] _rotations = new Quaternion[size];
            for (int i = 0; i < size; i++)
            {
                _positions[i] = _packet.ReadVector3();
                _rotations[i] = _packet.ReadQuaternion();
            }

            Server.clients[_fromClient].player.SetStats(_positions, _rotations);
        }
    }
}
