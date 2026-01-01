using System.Threading.Tasks;
using UnityEngine;

namespace com.birdhunter.core.Services
{
    public interface IAuthManager
    {
        Task<bool> LoginWithGPGSAsync();
        Task<bool> LoginWithEmailAsync(string email, string password);
        Task<bool> LoginAnonymousAsync();
    }
    
    public class AuthManager : IAuthManager
    {
        public async Task<bool> LoginWithGPGSAsync()
        {
            Debug.Log("Attempting GPGS Login...");
            // Placeholder for GPGS logic
            await Task.Delay(100); 
            return false; // Default to false for now
        }

        public async Task<bool> LoginWithEmailAsync(string email, string password)
        {
            Debug.Log($"Attempting Email Login for: {email}");
            // Placeholder for Email logic
            await Task.Delay(100);
            return false;
        }

        public async Task<bool> LoginAnonymousAsync()
        {
            Debug.Log("Attempting Anonymous Login...");
            // Placeholder for Anonymous logic
            await Task.Delay(100);
            return true;
        }
    }
}