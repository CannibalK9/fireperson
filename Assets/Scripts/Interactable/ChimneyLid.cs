using Assets.Scripts.Helpers;
using UnityEngine;

namespace Assets.Scripts.Interactable
{
    public class ChimneyLid : FirePlace
    {
        public bool IsOpen { get; private set; }
        private Animator _anim;

        void Start()
        {
            _anim = GetComponentInParent<Animator>();
        }

        public void Switch()
        {
            if (IsAccessible)
            {
                _anim.Play(Animator.StringToHash(Animations.Close));
                IsAccessible = false;
                IsOpen = false;
            }
            else
            {
                _anim.Play(Animator.StringToHash(Animations.Open));
                IsAccessible = true;
                IsOpen = true;
            }
        }
    }
}
