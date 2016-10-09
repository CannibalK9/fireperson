﻿using UnityEngine;

namespace Assets.Scripts.Helpers
{
    public static class OrientationHelper
    {
        public static Orientation GetOrientation(float rotation)
        {
            float slopeLimit = ConstantVariables.DefaultPlayerSlopeLimit;
			rotation = Mathf.Abs(rotation);

            if (rotation < slopeLimit || rotation > 360f - slopeLimit)
                return Orientation.Flat;
            else if (rotation < 180f + slopeLimit && rotation > 180f - slopeLimit)
                return Orientation.UpsideDown;
            else if (rotation < 180f)
                return Orientation.UprightAntiClockwise;
            else
                return Orientation.UprightClockwise;
        }

        public static Vector3 GetSurfaceVectorTowardsRight(Orientation orientation, Transform trans)
        {
            switch (orientation)
            {
                case Orientation.Flat:
                    return trans.right;
                case Orientation.UpsideDown:
                    return -trans.right;
                case Orientation.UprightAntiClockwise:
                    return -trans.up;
                case Orientation.UprightClockwise:
                    return trans.up;
                default:
                    return Vector3.zero;
            }
        }

		public static Vector3 GetDownwardVector(Orientation orientation, Transform trans)
		{
			switch (orientation)
			{
				case Orientation.Flat:
					return -trans.up;
				case Orientation.UpsideDown:
					return trans.up;
				case Orientation.UprightAntiClockwise:
					return -trans.right;
				case Orientation.UprightClockwise:
					return trans.right;
				default:
					return Vector3.zero;
			}
		}
	}
}
