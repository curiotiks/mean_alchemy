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
        public Sprite image;
        public float mean;
        public float sd;
        public RewardType rewardType;
        public string difficulty;
        //public Dictionary<RewardType, int> rewardList;
        public List<RewardEntry> rewardList; // Use List<RewardEntry> instead of Dictionary

        public BountyItem(string name, Sprite image, float mean, float sd, string difficulty,List<RewardEntry> rewardList)
        {
            this.name = name;
            this.image = image;
            this.rewardList = rewardList;
            this.difficulty = difficulty;
            this.mean = mean;
            this.sd = sd;
        }

        public BountyItem deepCopy()
        {
            return new BountyItem(name, image, mean, sd, difficulty.ToString(), rewardList);
        }
    }

    [Serializable]
    class BountyItemWrapper
    {
        public List<BountyItem> bountyItems;
    }
}
