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
            // Initialize familiar powered state from PlayerPrefs once per app run
            FamiliarState.LoadFromPrefs();
            // Ensure any WarpGates already in the scene reflect current state
            WarpGate.RefreshAllGates();
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

    /// <summary>
    /// Returns whether the player currently has a powered familiar (session truth).
    /// </summary>
    public bool HasPoweredFamiliar() => FamiliarState.Powered;

    /// <summary>
    /// Sets the powered familiar state and refreshes all warp gates.
    /// </summary>
    public void SetPoweredFamiliar(bool powered)
    {
        FamiliarState.SetPowered(powered);
        WarpGate.RefreshAllGates();
    }
}