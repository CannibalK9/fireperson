using UnityEngine;

namespace Assets.Scripts.Helpers
{
    public static class OrientationHelper
    {
		public static Orientation GetOrientation(Transform t)
		{
			return GetOrientation(t.rotation.eulerAngles.z);
		}

		public static Orientation GetOrientation(float rotation)
        {
            float slopeLimit = ConstantVariables.DefaultPlayerSlopeLimit;
			rotation = Mathf.Abs(rotation);

            if (rotation <= 90 - slopeLimit || rotation >= 360f - 90 + slopeLimit)
                return Orientation.Flat;
            else if (rotation >= 180f - 90 + slopeLimit && rotation <= 180 + 90 - slopeLimit)
                return Orientation.UpsideDown;
            else if (rotation >= slopeLimit && rotation <= 180f - slopeLimit)
                return Orientation.UprightClockwise;
            else if (rotation >= 180 + slopeLimit && rotation <= 360 - slopeLimit)
                return Orientation.UprightAntiClockwise;
            else if (rotation < 90)
                return Orientation.LeftTilt;
            else if (rotation < 180)
                return Orientation.UpsideDownRightTilt;
            else if (rotation < 270)
                return Orientation.UpsideDownLeftTilt;
            else
                return Orientation.RightTilt;
        }

		public static Vector3 GetSurfaceVectorTowardsRight(Transform trans)
		{
            return trans.right;
		}

		public static Vector3 GetDownwardVector(Transform trans)
		{
			return -trans.up;
		}
	}
}
