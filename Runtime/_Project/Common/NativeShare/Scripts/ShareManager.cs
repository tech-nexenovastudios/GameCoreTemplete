using UnityEngine;
using System.Collections;
using _Project.Common.Share.Factory;

namespace _Project.Common.Share
{
    public class ShareManager : MonoBehaviour
    {
        private static IShareableFactory _factory;
        public static IShareableFactory Factory => _factory ??= new NativeShareableFactory();

        private const string DefaultGameName = "My Awesome Game";

        /// <summary>
        /// Shares a screenshot with optional text message
        /// </summary>
        public static IShareable ShareScreenshot(string text = null)
        {
            return Factory.CreateScreenshotShare(text);
        }

        /// <summary>
        /// Shares text content only
        /// </summary>
        public static IShareable ShareText(string text)
        {
            return Factory.CreateTextShare(text);
        }

        /// <summary>
        /// Shares with subject, text and file path
        /// </summary>
        public static IShareable ShareWithFile(string subject, string text, string filePath)
        {
            return Factory.CreateFileShare(text, filePath).SetSubject(subject);
        }

        /// <summary>
        /// Shares multiple files with text
        /// </summary>
        public static IShareable ShareMultipleFiles(string text, params string[] filePaths)
        {
            return Factory.CreateMultipleFilesShare(text, filePaths);
        }

        /// <summary>
        /// Captures and shares screenshot immediately
        /// </summary>
        public static IEnumerator CaptureAndShare(string text = null)
        {
            yield return new WaitForEndOfFrame();

            Texture2D screenshot = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
            screenshot.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
            screenshot.Apply();

            string filePath = System.IO.Path.Combine(Application.temporaryCachePath, "screenshot.png");
            System.IO.File.WriteAllBytes(filePath, screenshot.EncodeToPNG());
            Object.Destroy(screenshot);

            var shareable = ShareScreenshot(text);
            if (shareable is NativeScreenshotShareable nativeScreenshot)
            {
                nativeScreenshot.AddFile(filePath);
            }
            
            shareable.SetCallback((result, shareTarget) => Debug.Log($"Share result: {result}, Target: {shareTarget}"))
                     .Share();
        }

        /// <summary>
        /// Shares game invitation with custom message
        /// </summary>
        public static IShareable ShareGameInvitation(string inviteMessage = null)
        {
            return Factory.CreateInvitationShare(inviteMessage);
        }
    }
}