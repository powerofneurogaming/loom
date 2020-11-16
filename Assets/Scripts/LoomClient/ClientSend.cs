using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClientSend : MonoBehaviour
{
    private static void SendTCPData(Packet _packet)
    {
        _packet.WriteLength();
        Client.instance.tcp.SendData(_packet);
    }

    private static void SendUDPData(Packet _packet)
    {
        _packet.WriteLength();
        Client.instance.udp.SendData(_packet);
    }

    #region Packets
    public static void WelcomeReceived()
    {
        using (Packet _packet = new Packet((int)ClientPackets.welcomeReceived))
        {
            _packet.Write(Client.instance.myId);
            _packet.Write("RobinTest");

            SendTCPData(_packet);
        }
    }

    //public static void UDPTestReceived()
    //{
    //    using (Packet _packet = new Packet((int)ClientPackets.udpTestReceived))
    //    {
    //        _packet.Write("Received a UDP packet.");

    //        SendUDPData(_packet);
    //    }
    //}
    /// <summary>Sending Loom Player Status of position & rotation to server.</summary>
    /// <param name="_positions">The Player positions [1 - headset; 2 - lefthand; 3 - righthand].</param>
    /// <param name="_rotations">The Player rotations [1 - headset; 2 - lefthand; 3 - righthand].</param>
    public static void PlayerStats(Vector3[] _positions, Quaternion[] _rotations)
    {
        int size = _positions.Length;
        if (size != _rotations.Length) // currently have to be 3
        {
            Debug.LogError($"PlayerStats() receive different length of data (position {_positions.Length}/ rotation {_rotations.Length}) RETURN");
            return;
        }
        using (Packet _packet = new Packet((int)ClientPackets.playerStats))
        {
            _packet.Write(size);
            for (int i = 0; i < size; i++)
            {
                _packet.Write(_positions[i]);
                _packet.Write(_rotations[i]);
            }

            SendUDPData(_packet);

        }
    }
    #endregion
}