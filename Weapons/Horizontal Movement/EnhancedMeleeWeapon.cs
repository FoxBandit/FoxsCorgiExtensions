using UnityEngine;

namespace MoreMountains.CorgiEngine
{
    /// <summary>
    /// ***This script free to use and no credit is required.
    /// ***This is intended to be used with More Mountain's Corgi Engine 4.2+
    ///
    /// This script adds functionality to a weapon to give it horizontal force when used. Use the Animation Curve to
    /// create the timing and force desired. It's designed to accompany an animation. My primary use if for a forward
    /// when swinging a sword.
    /// 
    /// This code is best added to the MoreMountains.CorgiEngine.Weapon base class so all weapons benefit from this
    /// behaviour.
    /// </summary>
    [AddComponentMenu("Corgi Engine/Weapons/Enhanced Melee Weapon")]
    public class EnhancedMeleeWeapon : MeleeWeapon
    {
        private float _endTime;

        private bool _executingMoveOverTime;
        private float _startTime;

        public bool allowInAir;

        [Header("Force Horizontal Movement")]
        // A curve of how much force to apply and when throughout the animation
        // X is time and Y is force
        public AnimationCurve moveOverTime;

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

            var time = Time.time - _startTime;

            var direction = transform.lossyScale.normalized.x > 0 ? 1 : -1;

            //Get new force from curve at that point
            var force = moveOverTime.Evaluate(time);
            force *= direction;

            //apply force to wielder
            _controller.SetForce(new Vector2(force, 0f));
        }

        public override void WeaponInputStart()
        {
            if (!allowInAir && (Owner.MovementState.CurrentState == CharacterStates.MovementStates.Jumping ||
                                Owner.MovementState.CurrentState == CharacterStates.MovementStates.Falling))
                return;
            if (Owner.MovementState.CurrentState == CharacterStates.MovementStates.Dashing)
                return;

            base.WeaponInputStart();
        }
    }
}