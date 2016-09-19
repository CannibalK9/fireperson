using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts.Player
{
    public static  class PlayerAnimBool
    {
        public static string Falling { get { return "falling"; } }
        public static string Moving { get { return "moving"; } }
        public static string Upright { get { return "upright"; } }
        public static string Squashed { get { return "squashed"; } }
        public static string Corner { get { return "corner"; } }
        public static string IsGrabbing { get { return "isGrabbing"; } }
        public static string Inverted { get { return "inverted"; } }
        public static string IsJumping { get { return "isJumping"; } }
        public static string Forward { get { return "forward"; } }
    }
}
