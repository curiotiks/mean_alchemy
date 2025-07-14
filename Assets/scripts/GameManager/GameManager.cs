using UnityEngine;

public class GameManager : MonoBehaviour {
    public static GameManager instance;
    public UserInfo userInfo;
 
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this);
        }
        else
        {
            Destroy(gameObject); // avoid duplicates
        }
    }

    private void Start() {
        //let's create userInfo here
        userInfo = new UserInfo();
        userInfo.userName = "Test";
        userInfo.userUid = "Test";
        userInfo.userGold = 100;
        userInfo.userLevel = 1;
        userInfo.userExp = 0;
        userInfo.userReputation = 0;
        userInfo.mean = 10;
        userInfo.sd = 2;
        userInfo.lastSpawnLocation = SpawnLocation.Default;
    }

    public UserInfo getUserInfo()
    {
        return userInfo;
    }

    public void setUserInfo(UserInfo userInfo)
    {
        this.userInfo = userInfo;
    }

    public void updateUserInfo(float newMean, float newSD){
        if (userInfo != null){
            userInfo.mean = newMean;
            userInfo.sd = newSD;
        }else{
            userInfo = new UserInfo();
            userInfo.mean = newMean;
            userInfo.sd = newSD;
        }
    }
}