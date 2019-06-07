using System;
using MoreMountains.Tools;
using Sirenix.OdinInspector;
using UnityEngine;

namespace MoreMountains.CorgiEngine
{
    public struct RhythmicComboEvent
    {
        public float ExecutionWindowTime { get; private set; }

        public float DropComboDelay { get; private set; }

        public RhythmicComboWeapon.ComboStatuses ComboState;

        public RhythmicComboEvent(float dropComboDelay, float executionWindowTime,
            RhythmicComboWeapon.ComboStatuses state)
        {
            DropComboDelay = dropComboDelay;
            ExecutionWindowTime = executionWindowTime;
            ComboState = state;
        }

        private static RhythmicComboEvent e;

        public static void Trigger(float dropComboDelay, float executionWindowTime,
            RhythmicComboWeapon.ComboStatuses state)
        {
            e.ExecutionWindowTime = executionWindowTime;
            e.DropComboDelay = dropComboDelay;
            e.ComboState = state;
            MMEventManager.TriggerEvent(e);
        }
    }

    /// <summary>
    ///     ***This script free to use and no credit is required.
    ///     ***This is intended to be used with More Mountain's Corgi Engine 4.2+
    ///     ***Written by: Jeff Feenstra. Any questions can be sent to feenstra32@gmail.com
    ///     Add this component to an object the same way you would a standard Combo Weapon
    ///     In the inspector added the Rhythmic Weapon Data to add in the properties for your Rhythm.
    ///     This should always be one less than the total number of Weapons you have on the object. As you don't need data for
    ///     the last weapon in the combo
    ///     While this class could clearly be more optimal, I'd chosen to have it inherit direct from the Corgi classes as a
    ///     more solid implementation would require changes in other Corgi classes. My goal was to preserve the Corgi
    ///     inheritance chain to make integration with updates to Corgi easily managed. Even at the cost of the overhead.
    /// </summary>
    [AddComponentMenu("Corgi Engine/Weapons/Rhythmic Combo Weapon")]
    public class RhythmicComboWeapon : ComboWeapon
    {
        public enum ComboStatuses
        {
            Start,
            Success,
            Fail,
            TimedOut
        }

        public enum StartComboState
        {
            WeaponStart,
            WeaponStop
        }

        private ComboWeaponIndicator _rhythmIndicator;

        // At what point in the window does is the next weapon in the combo ready to be executed
        protected float ExecutionWindowTime;

        public GameObject indicatorPrefab;

        public StartComboState startComboInState = StartComboState.WeaponStop;

        // The rhythm data for your weapon combo. It should always ben one less than the number of weapons in your combo.
        public RhythmicWeaponData[] weaponRhythmData;

        private void OnEnable()
        {
            if (indicatorPrefab != null)
            {
                _rhythmIndicator = Instantiate(indicatorPrefab, transform).GetComponent<ComboWeaponIndicator>();
                _rhythmIndicator.gameObject.SetActive(false);
            }
        }

        protected override void Update()
        {
            CheckIfWeaponShouldBeSwitched();
            ResetCombo();
        }

        private void CheckIfWeaponShouldBeSwitched()
        {
            if (Weapons.Length <= 1 || !_countdownActive) return;

            if (TimeSinceLastWeaponStopped < ExecutionWindowTime &&
                TimeSinceLastWeaponStopped + Time.deltaTime > ExecutionWindowTime)
            {
                _currentWeaponIndex += 1;
                //Weapons are swapped like this because Corgi handles weapon in a way that doesn't easily support this without deeper edits
                OwnerCharacterHandleWeapon.CurrentWeapon = Weapons[_currentWeaponIndex];
                OwnerCharacterHandleWeapon.ChangeWeapon(Weapons[_currentWeaponIndex],
                    Weapons[_currentWeaponIndex].WeaponID, true);
            }
        }

        protected override void ResetCombo()
        {
            if (Weapons.Length > 1)
                if (_countdownActive && DroppableCombo)
                {
                    TimeSinceLastWeaponStopped += Time.deltaTime;
                    if (TimeSinceLastWeaponStopped > DropComboDelay)
                    {
                        _countdownActive = false;

                        _currentWeaponIndex = 0;
                        OwnerCharacterHandleWeapon.CurrentWeapon = Weapons[_currentWeaponIndex];
                        OwnerCharacterHandleWeapon.ChangeWeapon(Weapons[_currentWeaponIndex],
                            Weapons[_currentWeaponIndex].WeaponID, true);
                        RhythmicComboEvent.Trigger(DropComboDelay, ExecutionWindowTime, ComboStatuses.TimedOut);

                        StopIndicator();
                    }
                }
        }

        public override void WeaponStarted(Weapon weaponThatStarted)
        {
            //We are in the middle of a Combo
            if (_countdownActive && _currentWeaponIndex > 0)
                RhythmicComboEvent.Trigger(DropComboDelay, ExecutionWindowTime, ComboStatuses.Success);
            //Debug.Log("SUCCESS >> Time since last Weapon Stopped:"+TimeSinceLastWeaponStopped);
            else
                RhythmicComboEvent.Trigger(DropComboDelay, ExecutionWindowTime, ComboStatuses.Fail);
            //Debug.Log("FAIL >> Time since last Weapon Stopped:"+TimeSinceLastWeaponStopped);

            StopIndicator();

            base.WeaponStarted(weaponThatStarted); //_countdownActive = false

            ProcessComboByState(StartComboState.WeaponStart);
        }

        public override void WeaponStopped(Weapon weaponThatStopped)
        {
            OwnerCharacterHandleWeapon = Weapons[_currentWeaponIndex].CharacterHandleWeapon;
            DroppableCombo = true; // Todo: remove when inspector no longer offers this as an option

            if (OwnerCharacterHandleWeapon == null) return;

            ProcessComboByState(StartComboState.WeaponStop);

            //Reset the weapon back to the first one. This is so the player can execute out of combo and start over from the first weapon in the combo
            OwnerCharacterHandleWeapon.CurrentWeapon = Weapons[0];
            OwnerCharacterHandleWeapon.ChangeWeapon(Weapons[0], Weapons[0].WeaponID, true);
        }

        private void ProcessComboByState(StartComboState state)
        {
            if (state == startComboInState) StartWeaponCombo();
        }

        private void StartWeaponCombo()
        {
            if (Weapons.Length > 0 && _currentWeaponIndex < weaponRhythmData.Length)
            {
                //The total time to wait for the combo input
                DropComboDelay = weaponRhythmData[_currentWeaponIndex].DropComboWindow +
                                 weaponRhythmData[_currentWeaponIndex].ExecutionWindow;

                ExecutionWindowTime = DropComboDelay - weaponRhythmData[_currentWeaponIndex].ExecutionWindow;

                if (_currentWeaponIndex == 0)
                    RhythmicComboEvent.Trigger(DropComboDelay, ExecutionWindowTime, ComboStatuses.Start);

                if (TimeSinceLastWeaponStopped <= ExecutionWindowTime) _currentWeaponIndex = 0;

                TimeSinceLastWeaponStopped = 0f;
                _countdownActive = true;

                StartIndicator();
            }
            else
            {
                //This was the last Weapon in the combo
                _countdownActive = false;
                _currentWeaponIndex = 0;
            }
        }

        protected void StartIndicator()
        {
            if (_rhythmIndicator == null) return;
            _rhythmIndicator.gameObject.SetActive(true);
            //calculate the % of the execution indicator
            var percentOfExecutionWindow = DropComboDelay - ExecutionWindowTime;
            percentOfExecutionWindow = percentOfExecutionWindow / DropComboDelay;
            _rhythmIndicator.StartIndicator(percentOfExecutionWindow, DropComboDelay);
        }

        protected void StopIndicator()
        {
            if (_rhythmIndicator == null) return;
            _rhythmIndicator.ResetIndicator();
            _rhythmIndicator.gameObject.SetActive(false);
        }

        [Serializable]
        public struct RhythmicWeaponData
        {
            [HorizontalGroup("Group 1")]
            //The window of time, in seconds, after an attack that another input would reset the combo
            public float DropComboWindow;

            [HorizontalGroup("Group 1")]
            //The window of time, in seconds, after the DropWindow where the combo can be executed perfectly
            public float ExecutionWindow;
        }
    }
}