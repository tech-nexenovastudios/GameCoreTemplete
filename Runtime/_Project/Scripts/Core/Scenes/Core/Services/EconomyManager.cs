using UnityEngine;

namespace com.birdhunter.core.Services
{
    public interface IEconomyManager
    {
        void SpendCurrency(int amount);
        void EarnCurrency(int amount);  
    }
    public class EconomyManager : IEconomyManager
    {
        public void SpendCurrency(int amount)
        {
            Debug.Log($"Spending {amount} currency");
        }

        public void EarnCurrency(int amount)
        {
            Debug.Log($"Earning {amount} currency");
        }
    }
}