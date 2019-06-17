using System.Collections;
using System.Collections.Generic;
using MoreMountains.CorgiEngine;
using UnityEngine;

namespace MoreMountains.CorgiEngine
{
    [AddComponentMenu("Corgi Engine/Weapons/Enhanced Melee Weapon")]
    public class EnhancedProjectileWeapon : ProjectileWeapon
    {
        [Header("Force Horizontal Movement")]
        //A curve of how much force to apply and when throughout the animation
        public AnimationCurve moveOverTime;

        private bool _executingMoveOverTime = false;
        private float _endTime = 0.0f;
        private float _startTime = 0.0f;

        protected override void CaseWeaponStart()
        {
            base.CaseWeaponStart();

            //If there is only a single item in the curve then it's not in use
            if (moveOverTime.length == 1) return;

            //Set the time at which the movement should end
            _startTime = Time.time;
            _endTime = _startTime + moveOverTime[moveOverTime.length - 1].time;
            _executingMoveOverTime = true;
        }

        protected void FixedUpdate()
        {
            if (!_executingMoveOverTime)
                return;

            if (Time.time >= _endTime)
            {
                _executingMoveOverTime = false;
                _controller.SetHorizontalForce(0f);
                return;
            }

            var time = (Time.time) - _startTime;

            var direction = (transform.lossyScale.normalized.x > 0) ? 1 : -1;

            //Get new force from curve at that point
            var force = moveOverTime.Evaluate(time);
            force *= direction;

            //apply force to wielder
            _controller.SetForce(new Vector2(force, 0f));
        }
    }
}