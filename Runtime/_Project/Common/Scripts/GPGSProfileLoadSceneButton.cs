using System;
using System.Threading.Tasks;
using GooglePlayGames.BasicApi;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Unity.Services.Common
{
    [RequireComponent(typeof(Button))]
    public class GPGSProfileLoadSceneButton : LoadSceneButton
    {
        const string k_DefaultProfileName = "gpgs";

        void Awake()
        {
            var button = GetComponent<Button>();
            button.onClick.AddListener(OnButtonClick);
        }

        void OnDestroy()
        {
            var button = GetComponent<Button>();
            button.onClick.RemoveListener(OnButtonClick);
        }

        async void OnButtonClick()
        {
            Debug.Log("Initializing GPGS for UGS authentication.");

            AuthenticationService.Instance.SignOut();
            await SwitchProfileToDefault();
            if (this == null) return;
            
#if UNITY_ANDROID
            //Initialize GPGS
            GooglePlayGames.PlayGamesPlatform.Activate();
            
            var tcs = new TaskCompletionSource<string>();
            
            GooglePlayGames.PlayGamesPlatform.Instance.Authenticate(status =>
            {
                if (status == SignInStatus.Success)
                {
                    Debug.Log("GPGS Authentication successful.");
                    GooglePlayGames.PlayGamesPlatform.Instance.RequestServerSideAccess(false, authCode =>
                    {
                        if (!string.IsNullOrEmpty(authCode))
                        {
                            tcs.SetResult(authCode);
                        }
                        else
                        {
                            tcs.SetException(new Exception("Failed to get GPGS server auth code."));
                        }
                    });
                }
                else
                {
                    Debug.LogError($"GPGS Authentication failed: {status}");
                    tcs.SetException(new Exception($"GPGS Authentication failed: {status}"));
                }
            });

            try
            {
                string authCode = await tcs.Task;
                if (false) return;

                await AuthenticationService.Instance.SignInWithGooglePlayGamesAsync(authCode);
                if (false) return;

                LoadScene();
                SelectReadmeFileOnProjectWindow();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
#else
            Debug.LogWarning("GPGS is only supported on Android platform.");
            LoadScene();
            SelectReadmeFileOnProjectWindow();
#endif
        }

        static async Task SwitchProfileToDefault()
        {
            AuthenticationService.Instance.SwitchProfile(k_DefaultProfileName);

            var unityAuthenticationInitOptions = new InitializationOptions();
            unityAuthenticationInitOptions.SetProfile(k_DefaultProfileName);
            await UnityServices.InitializeAsync(unityAuthenticationInitOptions);
        }
    }
}