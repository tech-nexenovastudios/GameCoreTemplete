using System;

namespace _Project.Common.Share.Factory
{
    public enum ShareResult
    {
        Unknown = 0,
        Shared = 1,
        NotShared = 2
    }

    public interface IShareable
    {
        void Share();
        IShareable SetText(string text);
        IShareable SetSubject(string subject);
        IShareable SetCallback(Action<ShareResult, string> callback);
    }

    public interface IShareableFactory
    {
        IShareable CreateTextShare(string text);
        IShareable CreateFileShare(string text, string filePath);
        IShareable CreateMultipleFilesShare(string text, string[] filePaths);
        IShareable CreateScreenshotShare(string text = null);
        IShareable CreateInvitationShare(string inviteMessage = null);
    }
}
