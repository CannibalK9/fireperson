using Assets.Scripts.Player.Config;
using System;
using UnityEngine;

namespace Assets.Scripts.Player.Abilities
{
    public struct AbilityPoint
    {
        public Vector2 Point { get; set; }
        public int[] Connections { get; set; }
        public bool IsActivated { get; set; }
        public Ability Ability { get; set; }

        public AbilityPoint(Vector2 point, int[] connections, bool isActivated, Ability ability)
            :this()
        {
            Point = point;
            Connections = connections;
            IsActivated = isActivated;
            Ability = ability;
        }
    }
}
