using Assets.Scripts.Helpers;
using UnityEngine;

namespace Assets.Scripts.Interactable
{
    public class ChimneyLid : MonoBehaviour
    {
        public bool IsOpen { get; private set; }
        private Animator _anim;
        private FirePlace _plSpot;

        void Awake()
        {
            _anim = GetComponent<Animator>();
            _plSpot = GetComponentInChildren<FirePlace>();
        }

        public void Switch()
        {
            if (_plSpot.IsAccessible)
            {
                _anim.Play(Animator.StringToHash(Animations.Close));
                _plSpot.IsAccessible = false;
                IsOpen = false;
            }
            else
            {
                _anim.Play(Animator.StringToHash(Animations.Open));
                _plSpot.IsAccessible = true;
                IsOpen = true;
            }
        }
    }
}
