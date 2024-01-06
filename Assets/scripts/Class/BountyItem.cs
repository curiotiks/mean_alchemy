using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public enum RewardType
{
    Gold,
    Exp,
    Reputation
}

[Serializable]
public class BountyItem
{
    public string name;
    public Sprite image;
    public float mean;
    public float sd;
    public Dictionary<RewardType, int> rewardList;

    public BountyItem(string name, Sprite image, float mean, float sd, Dictionary<RewardType, int> rewardList)
    {
        this.name = name;
        this.image = image;
        this.rewardList = rewardList;
        this.mean = mean;
        this.sd = sd;
    }

    public BountyItem deepCopy(){
        return new BountyItem(name, image, mean, sd, rewardList);
    }
}
