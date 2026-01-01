using System;
using UnityEngine;
using Unity.Services.Core;

namespace com.birdhunter.scenes.core
{
    public class BootController : MonoBehaviour
    {
        private async void Awake()
        {
            try
            {
                if (UnityServices.Instance.State != ServicesInitializationState.Initialized)
                {
                    await UnityServices.Instance.InitializeAsync();
                    Debug.Log("UGS Initialized");
                }
            }
            catch (Exception e)
            {
                Debug.Log($"UGS Initialization failed: {e.Message}");
                
            }
        }
    }
}