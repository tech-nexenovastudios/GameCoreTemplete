using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Unity.Services.Common.ProjectInbox
{
    public class AddressablesManager : MonoBehaviour
    {
        // This manager class can't be static because if it holds references to handles when the OTA use case is
        // interacted with, the OTA use case can't clear the cache.
        public static AddressablesManager instance { get; private set; }

        public Dictionary<string, (Sprite sprite, AsyncOperationHandle<Texture2D> handle)>
            AddressableSpriteContent { get; } =
            new Dictionary<string, (Sprite sprite, AsyncOperationHandle<Texture2D> handle)>();

        void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(this);
            }
            else
            {
                instance = this;
            }
        }

        public async void LoadImageForMessage(string imageAddress, string messageId)
        {
            try
            {
                if (string.IsNullOrEmpty(imageAddress) || AddressableSpriteContent.ContainsKey(messageId))
                {
                    return;
                }

                var imageLoadHandle = Addressables.LoadAssetAsync<Texture2D>(imageAddress);
                await imageLoadHandle.Task;
                if (this == null) return;

                var texture2D = imageLoadHandle.Result;

                if (!(texture2D is null))
                {
                    var sprite = Sprite.Create(texture2D, new Rect(0, 0, texture2D.width, texture2D.height), Vector2.zero, 100);
                    AddressableSpriteContent.Add(messageId, (sprite, imageLoadHandle));
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"There was a problem downloading the image for message {messageId}: {e}");
            }
        }

        public void TryReleaseHandle(string spriteContentKey)
        {
            if (AddressableSpriteContent.TryGetValue(spriteContentKey, out var spriteContent))
            {
                Addressables.Release(spriteContent.handle);
                AddressableSpriteContent.Remove(spriteContentKey);
            }
        }

        void OnDestroy()
        {
            if (instance == this)
            {
                foreach (var spriteContent in AddressableSpriteContent.Values)
                {
                    Addressables.Release(spriteContent.handle);
                }

                instance = null;
            }
        }
    }
}
