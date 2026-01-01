using System;
using System.Threading.Tasks;
using com.birdhunter.core.Services;
using UnityEngine;
using UnityServiceLocator;
using Unity.Services.Core;

public class UnityClient : MonoBehaviour
{
    private IAuthManager authManager;
    private IAnalyticsTracker analyticsTracker;
    private ICloudSaveManager cloudSaveManager;
    private IAdsManager adsManager;
    private IIAPManager iapManager;
    private IEconomyManager economyManager;
    private IEventManager eventManager;
    private IPushManager pushManager;

    private void Awake()
    {
        ServiceLocator.Global.Register<IAuthManager>(authManager = new AuthManager());
        ServiceLocator.Global.Register<IAnalyticsTracker>(analyticsTracker = new AnalyticsTracker());
        ServiceLocator.Global.Register<ICloudSaveManager>(cloudSaveManager = new CloudSaveManager());
        ServiceLocator.Global.Register<IAdsManager>(adsManager = new AdsManager());
        ServiceLocator.Global.Register<IIAPManager>(iapManager = new IAPManager());
        ServiceLocator.Global.Register<IEconomyManager>(economyManager = new EconomyManager());
        ServiceLocator.Global.Register<IEventManager>(eventManager = new EventManager());
        ServiceLocator.Global.Register<IPushManager>(pushManager = new PushManager());
    }

    private async void Start()
    {
        try
        {
            // 1. Init UGS
            if (UnityServices.Instance.State != ServicesInitializationState.Initialized)
            {
                await UnityServices.Instance.InitializeAsync();
                Debug.Log("UGS Initialized");
            }

            // 2. Load Systems
            // Systems are already loaded in Awake via ServiceLocator

            // 3. Auth attempt
            bool isAuthenticated = await AttemptAuth();

            if (isAuthenticated)
            {
                // 4. Load Cloud Save
                await cloudSaveManager.LoadAllDataAsync();
                Debug.Log("Cloud Save Loaded");

                // 5. Signal AppShell ready
                SignalAppShellReady();
            }
            else
            {
                Debug.LogError("Authentication failed, cannot proceed to AppShell");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error during boot sequence: {e.Message}");
        }
    }

    private async Task<bool> AttemptAuth()
    {
        Debug.Log("Starting Auth attempt sequence");

        // Try GPGS
        if (await authManager.LoginWithGPGSAsync())
        {
            Debug.Log("Authenticated via GPGS");
            return true;
        }

        // Try Email (Placeholder - in real case would check for cached credentials)
        // For this task, we follow the described sequence.
        if (await authManager.LoginWithEmailAsync("user@example.com", "password"))
        {
            Debug.Log("Authenticated via Email");
            return true;
        }

        // Anonymous fallback
        if (await authManager.LoginAnonymousAsync())
        {
            Debug.Log("Authenticated Anonymously");
            return true;
        }

        return false;
    }

    private void SignalAppShellReady()
    {
        Debug.Log("Signal AppShell ready");
        // This could be a scene load or an event. 
        // Based on the SceneManagement investigation, loading the AppShell scene group might be appropriate.
        // For now, we log it as per instructions.
    }
}
