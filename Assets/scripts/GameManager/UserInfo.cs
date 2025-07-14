using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

[Serializable]
public class UserInfo
{
    public string userName;
    public string userUid;
    public int userGold;
    public int userLevel;
    public int userExp;
    public int userReputation;
    public float mean;
    public float sd;
    public SpawnLocation lastSpawnLocation = SpawnLocation.Default;

    public UserInfo(){
        
    }

    public UserInfo(string userName, 
                    string userUid, 
                    int userGold, 
                    int userLevel, 
                    int userExp, 
                    int userReputation, 
                    float current_mean, 
                    float current_sd,
                    SpawnLocation lastSpawnLocation
                    )
    {
        this.userName = userName;
        this.userUid = userUid;
        this.userGold = userGold;
        this.userLevel = userLevel;
        this.userExp = userExp;
        this.userReputation = userReputation;
        this.mean = current_mean;
        this.sd = current_sd;
        this.lastSpawnLocation = lastSpawnLocation;
    }

    public UserInfo deepCopy(){
        return new UserInfo(userName, userUid, userGold, userLevel, userExp, userReputation, mean, sd, lastSpawnLocation);
    }
} 