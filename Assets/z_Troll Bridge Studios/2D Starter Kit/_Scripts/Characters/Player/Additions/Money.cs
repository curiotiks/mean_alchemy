using UnityEngine;
using System.Collections;
using System;

namespace TrollBridge {

	public class Money : MonoBehaviour {
		// The types of currencies
		public Currency[] currency;

        // Use this as the canonical name for the reputation currency
        public const string ReputationCurrencyName = "Reputation";

        /// <summary>
        /// Convenience accessor for the Reputation amount. Returns 0 if not present.
        /// </summary>
        public int Reputation => GetCurrency(ReputationCurrencyName);

        /// <summary>
        /// Adds (or subtracts with negative) to the Reputation currency. Creates the entry if missing.
        /// Automatically saves after change.
        /// </summary>
        public void AddReputation(int amount)
        {
            EnsureCurrencyExists(ReputationCurrencyName);
            AddSubtractMoney(ReputationCurrencyName, amount);
            Save();
        }

        /// <summary>
        /// Sets Reputation to an explicit value. Creates the entry if missing.
        /// Automatically saves after change.
        /// </summary>
        public void SetReputation(int value)
        {
            EnsureCurrencyExists(ReputationCurrencyName);
            for (int i = 0; i < currency.Length; i++)
            {
                if (currency[i].currencyName == ReputationCurrencyName)
                {
                    currency[i].currencyAmount = Mathf.Max(0, value);
                    break;
                }
            }
            Save();
        }

        /// <summary>
        /// Compatibility shim: treat generic Add(int) as adding Reputation.
        /// This lets external systems award reputation without depending on
        /// TrollBridge-specific APIs or namespaces.
        /// </summary>
        public void Add(int amount)
        {
            AddReputation(amount);
        }

        /// <summary>
        /// Compatibility shim: explicit getter for reputation as an int method.
        /// </summary>
        public int GetReputation()
        {
            return Reputation;
        }

        /// <summary>
        /// Ensures a currency with the provided name exists; if not, appends it with amount 0.
        /// </summary>
        private void EnsureCurrencyExists(string name)
        {
            if (currency == null)
                currency = new Currency[0];

            for (int i = 0; i < currency.Length; i++)
            {
                if (currency[i].currencyName == name)
                    return;
            }

            // Grow the array and append a new currency entry.
            Array.Resize(ref currency, currency.Length + 1);
            currency[currency.Length - 1] = new Currency { currencyName = name, currencyAmount = 0 };
        }

		void Awake(){
            if (currency == null)
                currency = new Currency[0];
			Load ();
		}

		/// <summary>
		/// Based on the paramater string currencyName this will return the currency amount.
		/// </summary>
		/// <returns>The money.</returns>
		/// <param name="currencyName">Currency name.</param>
		public int GetCurrency(string currencyName){
			// Loop through all the currenies.
			for(int i = 0; i < currency.Length; i++){
				// IF we find the currency we are looking for.
				if(currency[i].currencyName == currencyName){
					// Return the amount of currency.
					return currency[i].currencyAmount;
				}
			}
			return 0;
		}

		/// <summary>
		/// Adds or subtracts the currency based on the currencyName.
		/// </summary>
		/// <param name="currencyName">Currency name.</param>
		/// <param name="amount">Amount.</param>
		public void AddSubtractMoney(string currencyName, int amount){
            if (currency == null)
                currency = new Currency[0];
			// Loop through all the currenies.
			for(int i = 0; i < currency.Length; i++){
				// IF we find the currency we are looking for.
				if(currency[i].currencyName == currencyName){
					// Add or Subtract the amount of currency.
					currency[i].currencyAmount += amount;
					// We found the match so return.
					return;
				}
			}
		}

		/// <summary>
		/// Save all the types of currencies.
		/// </summary>
		public void Save()
		{
            if (currency == null)
                currency = new Currency[0];
			// Create a new Currency_Data.
			Currency_Data data = new Currency_Data ();
			// Setup the data to be saved.
			string[] currNames = new string[currency.Length];
			int[] currAmount = new int[currency.Length];
			// Loop through the currencies.
			for(int i = 0; i < currency.Length; i++){
				// Set the name and the amount.
				currNames [i] = currency [i].currencyName;
				currAmount [i] = currency [i].currencyAmount;
			}
			// Save the data.
			data.currencyName = currNames;
			data.currencyAmount = currAmount;
			// Turn the Currency_Data to json.
			string currencyToJson = JsonUtility.ToJson(data);
			// Save the information.
			PlayerPrefs.SetString ("Money", currencyToJson);
		}

		/// <summary>
		/// Load the currencies.
		/// </summary>
		private void Load()
		{
			// Get the encrypted json of the Money.
			string currencyJson = PlayerPrefs.GetString ("Money");
			// IF the Json string is null or empty
			if(String.IsNullOrEmpty(currencyJson)){
				// We leave as there is nothing to load.
				return;
			}
            if (currency == null)
                currency = new Currency[0];
			// Turn the Json to Currency_Data.
			Currency_Data data = JsonUtility.FromJson<Currency_Data> (currencyJson);
			// Load the values of the players currency/reputation.
			int count = Mathf.Min(currency.Length, Mathf.Min(data.currencyName.Length, data.currencyAmount.Length));
			for (int i = 0; i < count; i++)
			{
				currency[i].currencyName = data.currencyName[i];
				currency[i].currencyAmount = data.currencyAmount[i];
			}

            // Make sure a Reputation entry exists for downstream UI even if older saves lacked it
            EnsureCurrencyExists(ReputationCurrencyName);
		}
	}

	[Serializable]
	class Currency_Data
	{	
		public string[] currencyName;
		public int[] currencyAmount;
	}
}
