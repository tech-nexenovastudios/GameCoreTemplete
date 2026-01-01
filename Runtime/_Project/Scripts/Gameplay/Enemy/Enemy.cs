using System;
using UnityEngine;
using UnityEngine.U2D;
using Spine;
using Spine.Unity;

namespace Com.Gameplay.Enemy
{
    public class Enemy : MonoBehaviour
    {
        [SerializeField] SkeletonData skeletonData;
        [SerializeField] BirdEgg egg;

        SkeletonAnimation skeletonAnimation;
        
        public SkeletonAnimation SkeletonAnimation
        {
            get => skeletonAnimation;
            set => skeletonAnimation = value;
        }
        
        SkeletonRootMotion rootMotion;

        public SkeletonRootMotion RootMotion
        {
            get => rootMotion;
            set => rootMotion = value;
        }

        public void Start()
        {
            TryGetComponent(out rootMotion);
            
            TryGetComponent(out skeletonAnimation);
            skeletonAnimation.Initialize(true);
            
            //skeletonAnimation.skele = skeletonData;
        }
    }
    public class BirdEgg : MonoBehaviour
    {
        
    }
}