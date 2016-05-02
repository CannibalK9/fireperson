using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.Player
{
    public class FollowingLight : MonoBehaviour
    {
        public GameObject Player;
        private TargetJoint2D _target;

        void Awake()
        {
            _target = GetComponent<TargetJoint2D>();
        }

        void Update()
        {
            _target.target = Player.transform.position;
        }
    }
}
