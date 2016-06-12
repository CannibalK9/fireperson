using Assets.Scripts.Helpers;
using UnityEngine;

namespace Assets.Scripts.Interactable
{
    class StoveDoor : MonoBehaviour
    {
        private Animator _anim;
        private Stove _stove;

        void Awake()
        {
            _anim = GetComponent<Animator>();
            _stove = GetComponentInChildren<Stove>();
        }

        public void Switch()
        {
            if (_stove.IsAccessible)
            {
                _anim.Play(Animator.StringToHash(Animations.Close));
                _stove.IsAccessible = false;
            }
            else
            {
                _anim.Play(Animator.StringToHash(Animations.Open));
                _stove.IsAccessible = true;
            }
        }
    }
}
