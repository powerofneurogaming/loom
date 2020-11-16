using System;
using System.Collections.Generic;
using System.Text;
using System.Numerics;

namespace LoomServer
{
    class Player
    {
        public int id;
        public string username;

        public Vector3 position;
        public Quaternion rotation;

        public Vector3 positionLH;
        public Quaternion rotationLH;
        public Vector3 positionRH;
        public Quaternion rotationRH;

        public int indexHead = 0;
        public int indexLH = 1;
        public int indexRH = 2;

        //private float moveSpeed = 5f / Constants.TICKS_PER_SEC;
        //private bool[] inputs;

        public Player(int _id, string _username, Vector3 _spawnPosition)
        {
            id = _id;
            username = _username;

            position = _spawnPosition;
            positionLH = _spawnPosition;
            positionRH = _spawnPosition;
            rotation = Quaternion.Identity;
            rotationLH = Quaternion.Identity;
            rotationRH = Quaternion.Identity;

            //inputs = new bool[4];
        }

        public void Update()
        {
            //Vector2 _inputDirection = Vector2.Zero;
            //if (inputs[0])
            //{
            //    _inputDirection.Y += 1;
            //}
            //if (inputs[1])
            //{
            //    _inputDirection.Y -= 1;
            //}
            //if (inputs[2])
            //{
            //    _inputDirection.X += 1;
            //}
            //if (inputs[3])
            //{
            //    _inputDirection.X -= 1;
            //}

            //Move(_inputDirection);
            ServerSend.PlayerPosition(this);
            ServerSend.PlayerRotation(this);

        }

        public void SetStats(Vector3[] _positions, Quaternion[] _rotations)
        {
            position = _positions[indexHead];
            positionLH = _positions[indexLH];
            positionRH = _positions[indexRH];

            rotation = _rotations[indexHead];
            rotationLH = _rotations[indexLH];
            rotationRH = _rotations[indexRH];
        }

        //private void Move(Vector2 _inputDirection)
        //{
        //    Vector3 _forward = Vector3.Transform(new Vector3(0, 0, 1), rotation);
        //    Vector3 _right = Vector3.Normalize(Vector3.Cross(_forward, new Vector3(0, 1, 0)));

        //    Vector3 _moveDirection = _right * _inputDirection.X + _forward * _inputDirection.Y;
        //    position += _moveDirection * moveSpeed;

        //    //ServerSend.PlayerPosition(this);
        //    //ServerSend.PlayerRotation(this);
        //}

        //public void SetInput(bool[] _inputs, Quaternion _rotation)
        //{
        //    inputs = _inputs;
        //    rotation = _rotation;
        //}
    }
}
