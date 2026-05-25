using UnityEngine;

namespace EchoShift
{
    /// <summary>One sampled instant of the player, captured each FixedUpdate while recording.</summary>
    public struct RecordedFrame
    {
        public Vector2 position;
        public bool facingRight;
        public float moveX;
        public bool grounded;

        public RecordedFrame(Vector2 position, bool facingRight, float moveX, bool grounded)
        {
            this.position = position;
            this.facingRight = facingRight;
            this.moveX = moveX;
            this.grounded = grounded;
        }
    }
}
