using System;
using System.Collections.Generic;
using System.Text;
using System.Numerics;

namespace GameServer.Player
{
    class Player
    {
        public int id;
        public string userName;

        public Vector3 position;
        public Quaternion rotation;

        private float moveSpeed = 5f / Constants.TICKS_PER_SEC;
        private bool[] inputs;
        public Player(int playerId, string playerName, Vector3 spawnPosition)
        {
            id = playerId;
            userName = playerName;
            position = spawnPosition;
            rotation = Quaternion.Identity;

            inputs = new bool[4];
        }

        public void Update()
        {
            Vector2 inputDir = Vector2.Zero;

            if (inputs[0])
            {
                inputDir.Y += 1;
            }
            if (inputs[1])
            {
                inputDir.Y -= 1;
            }
            if (inputs[2])
            {
                inputDir.X += 1;
            }
            if (inputs[3])
            {
                inputDir.X -= 1;
            }

            Move(inputDir);
        }

        private void Move(Vector2 input)
        {
            Vector3 forward = Vector3.Transform(new Vector3(0, 0, 1), rotation);
            Vector3 right = Vector3.Normalize(Vector3.Cross(forward, new Vector3(0, 1, 0)));

            Vector3 moveDir = right * input.X + forward * input.Y;
            position += moveDir * moveSpeed;

            ServerSend.PlayerPosition(this);
            ServerSend.PlayerRotation(this);
        }

        public void SetInput(bool[] playerInputs, Quaternion playerRotation)
        {
            inputs = playerInputs;
            rotation = playerRotation;
        }

    }
}
