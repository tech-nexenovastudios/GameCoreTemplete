using Core;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Gameplay.Abilities
{
    [CreateAssetMenu(fileName = "AbilityData", menuName = "ScriptableObjects/AbilityData")]
    class AbilitiesData : ScriptableObject
    {
        public string label;
        
        public AnimationClip animationClip;
        [Range(0.1f, 4f)] public float castTime = 2f;

        //public ProjectileMove vfxPrefab;
        
        [SerializeReference] public List<AbilityEffect> effects; 
    }

    [Serializable]
    abstract class AbilityEffect
    {
        public abstract void Execute(GameObject caster, GameObject target);
    }

    [Serializable]
    class LaserAbility : AbilityEffect
    {
        public int damage;
        public override void Execute(GameObject caster, GameObject target)
        {
            //target.GetComponent<IHealthManager>().TakeDamage(damage);
            Debug.Log($"{caster.name} hit {target.name} and dealt {damage} damage");
        }
    }
}
