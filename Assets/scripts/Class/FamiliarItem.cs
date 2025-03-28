
using System;
using UnityEngine;

[Serializable]
public class FamiliarItem
{
    public int id;
    public string name;
    public string description;
    public string createdOn;
    public string iconID;
    public float mean;
    public float sd;
    public float skew;

    public FamiliarItem(int id, string name, string description, string createdOn, string iconID, float mean, float sd, float skew)
    {
        this.id = id;
        this.name = name;
        this.description = description;
        this.createdOn = createdOn;
        this.iconID = iconID;
        this.mean = mean;
        this.sd = sd;
        this.skew = skew;
    }

}
