namespace Assets.Scripts.Movement
{
    public class CollisionState
    {
        public bool Right;
        public bool Left;
        public bool Above;
        public bool Below;
        public bool BecameGroundedThisFrame;
        public bool WasGroundedLastFrame;
        public bool MovingDownSlope;
        public float SlopeAngle;

        public CollisionState()
        {
            Reset();
        }

        public bool HasCollision()
        {
            return Below || Right || Left || Above;
        }

        public void Reset()
        {
            Right = Left = Above = Below = BecameGroundedThisFrame = MovingDownSlope = false;
            SlopeAngle = 0f;
        }

        public override string ToString()
        {
            return string.Format(
                "[CharacterCollisionState2D] r: {0}, l: {1}, a: {2}, b: {3}, movingDownSlope: {4}, angle: {5}, wasGroundedLastFrame: {6}, becameGroundedThisFrame: {7}",
                 Right,
                 Left,
                 Above,
                 Below,
                 MovingDownSlope,
                 SlopeAngle,
                 WasGroundedLastFrame,
                 BecameGroundedThisFrame);
        }
    }
}