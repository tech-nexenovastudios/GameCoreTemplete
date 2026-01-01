using System;
using UnityEngine;

namespace _Project.Common.Share.Factory
{
    public abstract class NativeShareableBase : IShareable
    {
        protected readonly NativeShare nativeShare = new();

        public virtual void Share()
        {
            nativeShare.Share();
        }

        public IShareable SetText(string text)
        {
            nativeShare.SetText(text);
            return this;
        }

        public IShareable SetSubject(string subject)
        {
            nativeShare.SetSubject(subject);
            return this;
        }

        public IShareable SetCallback(Action<ShareResult, string> callback)
        {
            nativeShare.SetCallback((result, target) =>
            {
                callback?.Invoke((ShareResult)(int)result, target);
            });
            return this;
        }
    }

    public class NativeTextShareable : NativeShareableBase
    {
        public NativeTextShareable(string text)
        {
            SetText(text);
        }
    }

    public class NativeFileShareable : NativeShareableBase
    {
        public NativeFileShareable(string text, string filePath)
        {
            SetText(text);
            nativeShare.AddFile(filePath);
        }
    }

    public class NativeMultipleFilesShareable : NativeShareableBase
    {
        public NativeMultipleFilesShareable(string text, string[] filePaths)
        {
            SetText(text);
            foreach (var path in filePaths)
            {
                nativeShare.AddFile(path);
            }
        }
    }

    public class NativeScreenshotShareable : NativeShareableBase
    {
        private const string DefaultGameName = "My Awesome Game";

        public NativeScreenshotShareable(string text = null)
        {
            SetText(string.IsNullOrEmpty(text) ? $"Check out my progress in {DefaultGameName}!" : text);
        }
        
        public void AddFile(string filePath)
        {
            nativeShare.AddFile(filePath);
        }
    }

    public class NativeInvitationShareable : NativeShareableBase
    {
        private const string DefaultGameName = "My Awesome Game"; //TODO: Get from settings

        public NativeInvitationShareable(string inviteMessage = null)
        {
            string message = string.IsNullOrEmpty(inviteMessage)
                ? $"Join me in {DefaultGameName}! Download now and let's play together!"
                : inviteMessage;

            SetText(message);
        }
    }

    public class NativeShareableFactory : IShareableFactory
    {
        public IShareable CreateTextShare(string text)
        {
            return new NativeTextShareable(text);
        }

        public IShareable CreateFileShare(string text, string filePath)
        {
            return new NativeFileShareable(text, filePath);
        }

        public IShareable CreateMultipleFilesShare(string text, string[] filePaths)
        {
            return new NativeMultipleFilesShareable(text, filePaths);
        }

        public IShareable CreateScreenshotShare(string text = null)
        {
            return new NativeScreenshotShareable(text);
        }

        public IShareable CreateInvitationShare(string inviteMessage = null)
        {
            return new NativeInvitationShareable(inviteMessage);
        }
    }
}
