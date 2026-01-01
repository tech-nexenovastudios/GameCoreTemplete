using System.Threading.Tasks;
using UnityEngine;

namespace com.birdhunter.core.Services
{
    public interface ICloudSaveManager
    {
        void SaveData(string key, object data);
        object LoadData(string key);
        Task<bool> LoadAllDataAsync();
    }
    public class CloudSaveManager : ICloudSaveManager
    {
        public void SaveData(string key, object data)
        {
            Debug.Log($"Saving data with key: {key}");
        }

        public object LoadData(string key)
        {
            Debug.Log($"Loading data with key: {key}");
            return null;
        }

        public async Task<bool> LoadAllDataAsync()
        {
            Debug.Log("Loading all cloud save data...");
            await Task.Delay(100);
            return true;
        }
    }
}