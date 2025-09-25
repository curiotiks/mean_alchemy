using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;


namespace BountyItemData
{
    public enum RewardType
    {
        Gold,
        Exp,
        Reputation
    }

    public enum CardDifficulty
    {
        Easy,
        Medium,
        Hard
    }

    [Serializable]
    public class RewardEntry
    {
        public RewardType type;
        public int amount;
    }

    [Serializable]
    public class BountyItem
    {
        public string name;
        [NonSerialized]
        public Sprite image;
        public string imagePath;
        public float mean;
        public float sd;
        public RewardType rewardType;
        public string difficulty;
        //public Dictionary<RewardType, int> rewardList;
        public List<RewardEntry> rewardList; // Use List<RewardEntry> instead of Dictionary

        public BountyItem(string name, string imagePath, float mean, float sd, string difficulty,List<RewardEntry> rewardList)
        {
            this.name = name;
            this.imagePath = imagePath;
            this.rewardList = rewardList;
            this.difficulty = difficulty;
            this.mean = mean;
            this.sd = sd;
        }

        public BountyItem deepCopy()
        {
            var copy = new BountyItem(name, imagePath, mean, sd, difficulty.ToString(), rewardList);
            copy.image = image;
            return copy;
        }
    }

    [Serializable]
    class BountyItemWrapper
    {
        public List<BountyItem> bountyItems;
    }
}
