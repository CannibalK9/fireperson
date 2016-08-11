using Assets.Scripts.Helpers;
using UnityEngine;

namespace Assets.Scripts.Interactable
{
    public class ChimneyLid : FirePlace
    {
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
            }
            else
            {
                _anim.Play(Animator.StringToHash(Animations.Open));
                IsAccessible = true;
            }
        }
    }
}
