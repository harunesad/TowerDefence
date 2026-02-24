using UnityEngine;
using System.Collections.Generic;

namespace TowerDefence.Combat
{
    public enum StatusEffectType
    {
        None,
        Burn,
        Poison,
        Stun,
        Slow
    }

    [System.Serializable]
    public class StatusEffect
    {
        public StatusEffectType type;
        public float duration;
        public float power;
        public float tickInterval = 1f;
        
        private float nextTickTime;
        private float remainingDuration;

        public StatusEffect(StatusEffectType _type, float _duration, float _power)
        {
            type = _type;
            duration = _duration;
            power = _power;
            remainingDuration = _duration;
            nextTickTime = Time.time + tickInterval;
        }

        public bool Update(Unit unit)
        {
            remainingDuration -= Time.deltaTime;

            if (Time.time >= nextTickTime)
            {
                ApplyTick(unit);
                nextTickTime = Time.time + tickInterval;
            }

            return remainingDuration > 0;
        }

        private void ApplyTick(Unit unit)
        {
            switch (type)
            {
                case StatusEffectType.Burn:
                case StatusEffectType.Poison:
                    unit.TakeHealthDamage(power); // DoT Damage
                    break;
                case StatusEffectType.Stun:
                    // Stun mantığı Unit Update'inde NavMeshAgent.isStopped ile kontrol edilecek
                    break;
            }
        }

        public float GetRemainingDuration() => remainingDuration;
    }
}
