﻿using Assets.Scripts.Helpers;
using Assets.Scripts.Interactable;
using Assets.Scripts.Movement;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Denizens
{
    [RequireComponent(typeof(BoxCollider2D))]
    public class DenizenController : MonoBehaviour, IMotor
    {
        [Range(0f, 90f)]
        public float _slopeLimit = 30f;
        public float SlopeLimit { get { return _slopeLimit; } }

        public AnimationCurve SlopeSpeedMultiplier
        {
            get
            {
                return new AnimationCurve(new Keyframe(-90f, 1.5f), new Keyframe(0f, 1f), new Keyframe(90f, 0f));
            }
        }

        public Transform Transform { get; set; }
        public Collider2D Collider { get; set; }
		public Rigidbody2D Rigidbody { get; set; }
        public Vector3 Velocity { get; set; }
        public List<RaycastHit2D> RaycastHitsThisFrame { get; set; }
        public bool SatAtFireplace { get; set; }
        public MovementHandler Movement;
		public MovementState MovementState { get; set; }
        private DenizenMotor _motor;
		private Vector2 _eyeLevel { get { return new Vector2(Collider.bounds.center.x, Collider.bounds.max.y - 0.5f); } }

        void Awake()
        {
            Movement = new MovementHandler(this);

            Transform = GetComponent<Transform>();
            Collider = GetComponent<BoxCollider2D>();
			Rigidbody = GetComponent<Rigidbody2D>();
			RaycastHitsThisFrame = new List<RaycastHit2D>(2);
            _motor = GetComponent<DenizenMotor>();
			MovementState = new MovementState();
        }

        public bool SpotPlayer(Vector2 direction)
        {
            LayerMask mask = 1 << LayerMask.NameToLayer(Layers.Player) | Layers.Platforms | 1 << LayerMask.NameToLayer(Layers.Steam);

            RaycastHit2D hit = Physics2D.Raycast(_eyeLevel, direction, 20f, mask);
            if (hit && hit.collider.gameObject.layer == LayerMask.NameToLayer(Layers.Player))
            {
                hit.collider.SendMessage("Spotted", SendMessageOptions.RequireReceiver);
                return true;
            }
            return false;
        }

		public bool CanSeeFlash(Vector2 plLocation)
		{
			LayerMask mask = 1 << LayerMask.NameToLayer(Layers.PL) | Layers.Platforms;

			RaycastHit2D hit = Physics2D.Raycast(_eyeLevel, plLocation - _eyeLevel, 20f, mask);
			return hit && hit.collider.gameObject.layer == LayerMask.NameToLayer(Layers.Flash);
		}

		void OnTriggerStay2D(Collider2D col)
        {
            if (col.gameObject.layer == LayerMask.NameToLayer(Layers.PlSpot))
            {
                var fireplace = col.gameObject.GetComponent<FirePlace>();
                if (fireplace != null && fireplace.IsHeatSource && fireplace.IsLit == false)
                {
                    var stove = fireplace as Stove;
					if (stove != null)
					{
						if (stove.AllChimneysAreClosed())
							_motor.BeginLighting(fireplace);
						else
						{
							SatAtFireplace = false;
							_motor.SetTravelInDirectionFacing();
						}
					}
					else if (stove == null)
						_motor.BeginLighting(fireplace);
				}
				else if (fireplace.IsHeatSource)
                {
                    SatAtFireplace = true;
                }
            }
        }
    }
}