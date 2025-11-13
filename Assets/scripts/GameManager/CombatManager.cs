using System.Collections;
using System.Collections.Generic;
using Stopwatch = System.Diagnostics.Stopwatch;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using BountyItemData;
using UnityEngine.SceneManagement;
using MeanAlchemy;

public class CombatManager : MonoBehaviour
{
    // ─────────────────────────────────────────────────────────────────────────────
    // Catalog reference for EventRef dropdowns
    // ─────────────────────────────────────────────────────────────────────────────
    [Header("Catalog Reference")]
    [Tooltip("Catalog that supplies the Category→Event options for the dropdown below")] 
    [SerializeField] private EventPayloadCatalog catalog;

    // ─────────────────────────────────────────────────────────────────────────────
    // UI References
    // ─────────────────────────────────────────────────────────────────────────────
    [Header("UI")]
    [SerializeField] private Button attackBtn;
    [SerializeField] private Slider userHPbar;
    [SerializeField] private Slider enemyHPbar;
    [SerializeField] private TextMeshProUGUI hpText_user;
    [SerializeField] private TextMeshProUGUI hpText_enemy;
    [SerializeField] private TextMeshProUGUI combatLog;

    // ─────────────────────────────────────────────────────────────────────────────
    // Scene Objects
    // ─────────────────────────────────────────────────────────────────────────────
    [Header("Scene Objects")]
    [SerializeField] private GameObject playerGO;
    [SerializeField] private GameObject enemyGO;
    [SerializeField] private GameObject attackImage;

    // ─────────────────────────────────────────────────────────────────────────────
    // Familiar Sprites (either SpriteRenderer or UI Image)
    // ─────────────────────────────────────────────────────────────────────────────
    [Header("Familiar Sprites")]
    [SerializeField] private SpriteRenderer playerFamiliarSR;
    [SerializeField] private SpriteRenderer enemyFamiliarSR;
    [SerializeField] private Image playerFamiliarImage;
    [SerializeField] private Image enemyFamiliarImage;
    [SerializeField] private Sprite defaultPlayerFamiliar;
    [SerializeField] private Sprite defaultEnemyFamiliar;

    // ─────────────────────────────────────────────────────────────────────────────
    // Runtime Data
    // ─────────────────────────────────────────────────────────────────────────────
    [Header("Runtime Data")]
    [HideInInspector] public BountyItem bountyItem_info; // set via SetBountyItem or BountyBoardManager
    [HideInInspector] public UserInfo userInfo_temp_for_combat;
    private bool isExecuted = false;

    // ─────────────────────────────────────────────────────────────────────────────
    // Logging (battle report)
    // ─────────────────────────────────────────────────────────────────────────────
    [Header("Logging (Battle Report)")]
    [SerializeField] private EventRef combatReportEvent; // Catalog: Combat/combat_report (for example)

    [System.Serializable]
    public class BattleReportSummary
    {
        public string playerName;
        public string enemyName;
        public string startTimeUtc;
        public string endTimeUtc;
        public float durationSeconds;
        public float avgClickInterval;
        public string outcome; // "win" | "lose"
        public float playerMean;
        public float playerSd;
        public float enemyMean;
        public float enemySd;
        public int playerReputationBefore; // reputation at battle start (or before reward)
        public int playerReputationAfter;  // reputation after battle outcome and rewards
    }

    [System.Serializable]
    public class BattleTurnRow
    {
        public int turnIndex;
        public string actor;          // "player" | "enemy"
        public string action;         // "attack" | "defend"
        public float damageDealt;
        public float playerHpAfter;
        public float enemyHpAfter;
        public int   tClickIntervalMs; // interval between player button presses; 0 for enemy rows
        public int   tElapsedMs;       // since battle start
    }

    private BattleReportSummary _summary;
    private readonly List<BattleTurnRow> _turns = new List<BattleTurnRow>();
    private Stopwatch _sw;
    private int _lastPlayerClickMs = 0;
    private int _turnIdx = 0;

    private bool _battleEnded = false;

    [Header("Flow / Navigation")]
    [SerializeField] private string nextSceneName = "TheLab"; // scene to load after battle ends

    [Header("Navigation Logging")]
    [Tooltip("Location event for returning from combat to the Lab (Location/toLab).")]
    [SerializeField] private EventRef labWarpEvent;

    public CombatManager SetBountyItem(BountyItem bountyItem)
    {
        this.bountyItem_info = bountyItem.deepCopy();
        return this;
    }

    private void Awake()
    {

        GameObject go = GameObject.Find("CombatManager_Temp");
        if (go != null)
        {
           bountyItem_info = go.GetComponent<CombatManager>().bountyItem_info.deepCopy();
           Destroy(go);
        }

    }

    void Start()
    {
        if (attackBtn != null)
        {
            attackBtn.onClick.RemoveAllListeners();
            attackBtn.onClick.AddListener(() =>
            {
                // interval measured when we create the turn entry in Attack_Coroutine
                Attack();
            });
            attackBtn.interactable = true;
        }

        StartCombat();
    }

    public void StartCombat()
    {
        Debug.Log("Start Combat");
        // Always apply defaults first (works even if no bounty is selected yet)
        ApplyFamiliarSprites();
        //userInfo_temp_for_combat = GameManager.instance.userInfo.deepCopy();
        userInfo_temp_for_combat = new UserInfo("Player1",
                                                "1102",
                                                500,
                                                3,
                                                1000,
                                                3,
                                                Random.Range(20, 31),
                                                Random.Range(-2f, 2f),
                                                SpawnLocation.Default); // Default Spawn Location
        // If a bounty is available, load it and re-apply sprites; otherwise keep defaults
        if (BountyBoardManager.instance != null && BountyBoardManager.instance.currentBounty != null)
        {
            bountyItem_info = BountyBoardManager.instance.currentBounty.bountyItem.deepCopy();
            ApplyFamiliarSprites(); // updates enemy to bounty sprite
        }
        else
        {
            Debug.LogWarning("CombatManager: No selected bounty found. Using default enemy familiar sprite.");
        }

        Debug.Log(userInfo_temp_for_combat + "userHPbar: " + userHPbar);
        Debug.Log("User Info: " + userInfo_temp_for_combat.mean + " " + userInfo_temp_for_combat.sd);

        userHPbar.maxValue = userInfo_temp_for_combat.mean;
        userHPbar.value = userInfo_temp_for_combat.mean;

        enemyHPbar.maxValue = bountyItem_info.mean;
        enemyHPbar.value = bountyItem_info.mean;
        hpText_user.text = "HP: " + userInfo_temp_for_combat.mean.ToString();
        hpText_enemy.text = "HP: " + bountyItem_info.mean.ToString();

        // Initialize battle report
        _summary = new BattleReportSummary
        {
            playerName = "Player1", // replace with profile if available
            enemyName = "Enemy",
            startTimeUtc = System.DateTime.UtcNow.ToString("o"),
            playerMean = userInfo_temp_for_combat.mean,
            playerSd = userInfo_temp_for_combat.sd,
            enemyMean = bountyItem_info.mean,
            enemySd = bountyItem_info.sd
        };
        _turns.Clear();
        _turnIdx = 0;
        _lastPlayerClickMs = 0;
        _sw = new Stopwatch();
        _sw.Start();
    }

    private void RecordTurn(string actor, string action, float damage)
    {
        int now = _sw != null ? (int)_sw.ElapsedMilliseconds : 0;
        int interval = 0;
        if (actor == "player")
        {
            interval = (_turnIdx == 0) ? 0 : (now - _lastPlayerClickMs);
            _lastPlayerClickMs = now;
        }
        _turnIdx++;
        _turns.Add(new BattleTurnRow
        {
            turnIndex = _turnIdx,
            actor = actor,
            action = action,
            damageDealt = damage,
            playerHpAfter = userHPbar != null ? userHPbar.value : 0f,
            enemyHpAfter = enemyHPbar != null ? enemyHPbar.value : 0f,
            tClickIntervalMs = actor == "player" ? interval : 0,
            tElapsedMs = now
        });
    }

    IEnumerator AttackedByTheEnemy(){
        yield return new WaitForSeconds(0.2f);
        Debug.Log("Attacked by the enemy");
        float calculatedDefenseDamage = getAttackedDamage(userInfo_temp_for_combat, bountyItem_info);
        if (calculatedDefenseDamage >= userInfo_temp_for_combat.mean)
        {
            // This means the user loses
            Debug.Log("User Lose");
            combatLog.text = "User Lose";
            // TODO: Implement an "end game" method for when a familiar is defeated.
            FinishAndSend("lose");
        }
        animateAttack(false, 0.2f);
        yield return new WaitForSeconds(0.2f);
        changeHPbar(true, calculatedDefenseDamage);
        // record enemy attack turn (after HP updates)
        RecordTurn("enemy", "attack", calculatedDefenseDamage);

        // if player died as a result of this hit, finish now
        if (userHPbar != null && userHPbar.value <= 0f && !_battleEnded)
        {
            Debug.Log("Player HP reached 0 after enemy attack. Finishing battle as LOSE.");
            FinishAndSend("lose");
            isExecuted = false;
            yield break;
        }

        isExecuted = false;
    }
    
    public void Attack()
    {
        Debug.Log("Attack Button Clicked"); 
        if (isExecuted){
            Debug.Log("There is death.");
            return;
        }
        isExecuted = true;
        StartCoroutine(Attack_Coroutine()); 
    }

    /// <summary>
    /// Flee out of combat back to the Lab (or the configured next scene) without logging turns.
    /// Clears familiar/warps and then loads the configured next scene.
    /// Wire your Flee button's OnClick to this (instead of SceneChanger).
    /// </summary>
    public void OnFleeClicked()
    {
        Debug.Log("[CombatManager] Flee clicked. nextSceneName=" + nextSceneName);

        // Mark the battle as ended so OnDisable guard doesn't double-run
        _battleEnded = true;
        if (attackBtn != null) attackBtn.interactable = false;
        if (_sw != null && _sw.IsRunning) _sw.Stop();

        // Optional: set a summary outcome for consistency
        if (_summary == null) _summary = new BattleReportSummary
        {
            playerName = "Player1",
            enemyName = bountyItem_info != null ? (bountyItem_info.name ?? "Enemy") : "Enemy",
            startTimeUtc = System.DateTime.UtcNow.ToString("o")
        };
        _summary.outcome = "flee";
        _summary.endTimeUtc = System.DateTime.UtcNow.ToString("o");
        _summary.durationSeconds = _sw != null ? (float)(_sw.ElapsedMilliseconds / 1000.0) : 0f;

        // No battle-turn logging on flee for now (keep it simple)
        ClearFamiliarAndRelockWarp();
        SafeLoadNextScene();
    }

    [ContextMenu("TEST: Flee Now")] private void __TestFleeNow() => OnFleeClicked();

    IEnumerator Attack_Coroutine(){
        Debug.Log("Attack");
        float calculatedAttackDamage = getAttackDamage(userInfo_temp_for_combat, bountyItem_info);
        if (calculatedAttackDamage >= bountyItem_info.mean){
            // This means the user wins and HP of the enemy must be 0
            animateAttack(true, 0.2f);
            yield return new WaitForSeconds(0.2f);
            changeHPbar(false, calculatedAttackDamage);
            // record player attack turn (after HP updates)
            RecordTurn("player", "attack", calculatedAttackDamage);
            Debug.Log("User Win");
            // combatLog.text = "User Win";
            FinishAndSend("win");
            isExecuted = false;
            yield break;
        }else{
            // This means that it's just a normal attack to the enemy
            animateAttack(true, 0.2f);
            yield return new WaitForSeconds(0.2f);
            changeHPbar(false, calculatedAttackDamage);
            // record player attack turn (after HP updates)
            RecordTurn("player", "attack", calculatedAttackDamage);

            // if enemy died as a result of this hit, finish now
            if (enemyHPbar != null && enemyHPbar.value <= 0f && !_battleEnded)
            {
                Debug.Log("Enemy HP reached 0 after player attack. Finishing battle as WIN.");
                FinishAndSend("win");
                isExecuted = false;
                yield break;
            }

            Debug.Log("Normal attack");
            // now being attacked by the enemy
            yield return AttackedByTheEnemy();
        }
    }

    public void animateAttack(bool isFromUser, float time){
        attackImage.gameObject.SetActive(true);
        if (isFromUser){
            //first move the attack image to the user
            //then move the attack image to the enemy using DOTween, using interpolation
            attackImage.transform.position = playerGO.transform.position;
            playerGO.transform.DOShakePosition(0.1f, 0.5f, 10, 90, false, true);
            attackImage.transform.DOMove(enemyGO.transform.position, time).onComplete += () => {
                attackImage.gameObject.SetActive(false); 
            };
        }else{
            //first move the attack image to the enemy
            //then move the attack image to the user using DOTween, using interpolation
            attackImage.transform.position = enemyGO.transform.position;
            enemyGO.transform.DOShakePosition(0.1f, 0.5f, 10, 90, false, true);
            attackImage.transform.DOMove(playerGO.transform.position, time).onComplete += () => {
                attackImage.gameObject.SetActive(false);
            };
        } 
    }

    public void changeHPbar(bool isForUser, float damage)
    {
        Debug.Log("Change HP bar: " + damage.ToString() + " isForUser: " + isForUser.ToString() + "");
        //animate the HP bar
        //if isForUser is true, then animate the user's HP bar
        //else animate the enemy's HP bar
        if (isForUser){
            //animate the user's HP bar
            userHPbar.value -= damage;
            if (userHPbar.value <= 0)
                userHPbar.value = 0;

            userInfo_temp_for_combat.mean = userHPbar.value;
        }else{
            //animate the enemy's HP bar
            enemyHPbar.value -= damage;
            if (enemyHPbar.value <= 0)
                enemyHPbar.value = 0;

            bountyItem_info.mean = enemyHPbar.value;
        }

        hpText_user.text = "HP: " + userInfo_temp_for_combat.mean.ToString();
        hpText_enemy.text = "HP: " + bountyItem_info.mean.ToString();
    }

    public float getAttackDamage(UserInfo userInfo, BountyItem bountyItem)
    {
        return Random.Range(userInfo.mean - userInfo.sd, userInfo.mean + userInfo.sd)/10; 
    }

    public float getAttackedDamage(UserInfo userInfo, BountyItem bountyItem)
    {
        return Random.Range(bountyItem.mean - bountyItem.sd, bountyItem.mean + bountyItem.sd)/10;
    }
    public float getDefenceDamage(UserInfo userInfo, BountyItem bountyItem)
    {
        return Random.Range(bountyItem.mean - bountyItem.sd, bountyItem.mean + bountyItem.sd)/10;
    }

    /// <summary>
    /// Awards reputation based on the current bounty difficulty.
    /// Easy = 1, Medium = 10, Hard = 100. No-op if Wallet is missing.
    /// </summary>
    private void AwardReputationForCurrentBounty()
    {
        int rep = 1;
        string diff = string.Empty;
        try 
        {
            if (bountyItem_info != null)
                diff = bountyItem_info.difficulty ?? string.Empty;

            if (diff.Equals("Medium", System.StringComparison.OrdinalIgnoreCase)) rep = 10;
            else if (diff.Equals("Hard", System.StringComparison.OrdinalIgnoreCase)) rep = 100;
            else rep = 1; // Easy or unknown
        }
        catch { rep = 1; }

        try
        {
            if (Wallet.Instance != null)
            {
                int before = 0;
                try { before = Wallet.Instance.Reputation; } catch {}

                Wallet.Instance.Add(rep);

                int after = 0;
                try { after = Wallet.Instance.Reputation; } catch {}

                Debug.Log($"[CombatManager] Awarded +{rep} reputation for difficulty '{(string.IsNullOrEmpty(diff)?"Easy/Unknown":diff)}'. Before={before}, After={after}");
            }
            else
            {
                Debug.LogWarning($"[CombatManager] Wallet not present; could not award +{rep} reputation.");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning($"[CombatManager] Failed to award reputation: {ex.Message}");
        }
    }

    private void FinishAndSend(string outcome)
    {
        if (_battleEnded) return; // prevent double send
        _battleEnded = true;

        if (attackBtn != null) attackBtn.interactable = false;

        if (_sw != null && _sw.IsRunning) _sw.Stop();
        if (_summary == null) _summary = new BattleReportSummary();

        _summary.outcome = outcome;
        _summary.endTimeUtc = System.DateTime.UtcNow.ToString("o");
        _summary.durationSeconds = _sw != null ? (float)(_sw.ElapsedMilliseconds / 1000.0) : 0f;

        // average interval across player clicks only
        int playerTurns = 0;
        int sumIntervals = 0;
        for (int i = 0; i < _turns.Count; i++)
        {
            if (_turns[i].actor == "player")
            {
                playerTurns++;
                if (_turns[i].tClickIntervalMs > 0)
                    sumIntervals += _turns[i].tClickIntervalMs;
            }
        }
        _summary.avgClickInterval = (playerTurns > 1) ? (sumIntervals / (float)(playerTurns - 1) / 1000f) : 0f;

        // Capture player reputation before/after awarding any battle rewards.
        int repBefore = 0;
        int repAfter = 0;
        try
        {
            if (Wallet.Instance != null)
            {
                repBefore = Wallet.Instance.Reputation;
            }
        }
        catch {}

        if (outcome == "win")
        {
            // Award reputation exactly once here so the summary reflects the final state.
            AwardReputationForCurrentBounty();
            try
            {
                if (Wallet.Instance != null)
                {
                    repAfter = Wallet.Instance.Reputation;
                }
                else
                {
                    repAfter = repBefore;
                }
            }
            catch
            {
                repAfter = repBefore;
            }
        }
        else
        {
            // No reputation change on lose/flee; after == before.
            repAfter = repBefore;
        }

        _summary.playerReputationBefore = repBefore;
        _summary.playerReputationAfter  = repAfter;

        // Build payload for game_events.event_data
        var payload = new Dictionary<string, object>
        {
            { "battle_report", _summary }
        };

        var logger = GameLogger.Instance != null ? GameLogger.Instance : GameObject.FindObjectOfType<GameLogger>();
        if (logger == null)
        {
            Debug.LogWarning("GameLogger not present; battle report not sent. Loading next scene anyway.");
            if (outcome == "win")
            {
                CompleteBountySelection();
            }
            ClearFamiliarAndRelockWarp();
            SafeLoadNextScene();
            return;
        }

        // Convert turns to list of row dictionaries for /battle_turns bulk insert
        var rows = new List<Dictionary<string, object>>(_turns.Count);
        foreach (var t in _turns)
        {
            rows.Add(new Dictionary<string, object>
            {
                { "turn_index", t.turnIndex },
                { "actor", t.actor },
                { "action", t.action },
                { "damage_dealt", t.damageDealt },
                { "player_hp_after", t.playerHpAfter },
                { "enemy_hp_after", t.enemyHpAfter },
                { "t_click_interval_ms", t.tClickIntervalMs },
                { "t_elapsed_ms", t.tElapsedMs }
            });
        }

        logger.LogEventReturnId(
            combatReportEvent,
            payload,
            onSuccess: (eventId) =>
            {
                logger.LogBattleTurnsBulk(eventId, rows,
                    onSuccess: () => {
                        Debug.Log($"Battle report sent with {rows.Count} turns. Id=" + eventId);
                        if (outcome == "win")
                        {
                            CompleteBountySelection();
                        }
                        ClearFamiliarAndRelockWarp();
                        SafeLoadNextScene();
                    },
                    onError: (err) => {
                        Debug.LogError("Battle turns insert failed: " + err);
                        if (outcome == "win")
                        {
                            CompleteBountySelection();
                        }
                        ClearFamiliarAndRelockWarp();
                        SafeLoadNextScene();
                    });
            },
            onError: (err) => {
                Debug.LogError("Battle summary insert failed: " + err);
                if (outcome == "win")
                {
                    CompleteBountySelection();
                }
                ClearFamiliarAndRelockWarp();
                SafeLoadNextScene();
            }
        );
    }

    /// <summary>
    /// Clears the current bounty selection after a win.
    /// </summary>
    private void CompleteBountySelection()
    {
        // Try to get BountyBoardManager (instance or via FindObjectOfType)
        var bbm = (BountyBoardManager.instance != null) ? BountyBoardManager.instance : GameObject.FindObjectOfType<BountyBoardManager>();
        if (bbm != null)
        {
            var cur = bbm.GetType().GetField("currentBounty");
            if (cur != null)
            {
                var val = cur.GetValue(bbm);
                if (val != null)
                {
                    string bountyName = "?";
                    string bountyDiff = "?";
                    try
                    {
                        var bountyItemField = val.GetType().GetField("bountyItem");
                        object bountyItemObj = bountyItemField != null ? bountyItemField.GetValue(val) : null;
                        if (bountyItemObj != null)
                        {
                            var nameField = bountyItemObj.GetType().GetField("name");
                            var diffField = bountyItemObj.GetType().GetField("difficulty");
                            if (nameField != null)
                                bountyName = System.Convert.ToString(nameField.GetValue(bountyItemObj));
                            if (diffField != null)
                                bountyDiff = System.Convert.ToString(diffField.GetValue(bountyItemObj));
                        }
                    }
                    catch {}
                    Debug.Log($"[CombatManager] Clearing current bounty selection: {bountyName} (difficulty: {bountyDiff})");
                    cur.SetValue(bbm, null);
                }
            }
            // Attempt to refresh UI (if method exists)
            try
            {
                var m = bbm.GetType().GetMethod("RefreshSelectedCardBanner", System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.Public|System.Reflection.BindingFlags.NonPublic);
                if (m != null) m.Invoke(bbm, null);
            }
            catch {}
        }
    }

    /// <summary>
    /// Clears the powered familiar state and refreshes warp gates so Lab's combat warp re-locks.
    /// </summary>
    private void ClearFamiliarAndRelockWarp()
    {
        try
        {
            var tm = GameObject.FindObjectOfType<TransmuteManager>();
            if (tm != null)
            {
                tm.TransmuteEraseNew();
            }
        }
        catch {}
        TransmuteManager.ClearPoweredFamiliar();
        // Ensure any gates in the next scene reflect the cleared state ASAP
        WarpGate.RefreshAllGates();
    }

    private void OnDisable()
    {
        if (!Application.isPlaying) return;
        // Only attempt if battle ended (to avoid clearing on accidental disable)
        if (_battleEnded)
        {
            ClearFamiliarAndRelockWarp();
        }
    }

    private void SafeLoadNextScene()
    {
        if (string.IsNullOrEmpty(nextSceneName))
        {
            Debug.LogWarning("nextSceneName not set on CombatManager. Staying in current scene.");
            return;
        }
        try
        {
            // Log movement back to the Lab (Location/toLab) before changing scenes
            try
            {
                var logger = GameLogger.Instance != null ? GameLogger.Instance : GameObject.FindObjectOfType<GameLogger>();
                if (logger != null)
                {
                    // No extra payload needed here; simple location event
                    logger.LogEventReturnId(
                        labWarpEvent,
                        null,
                        onSuccess: null,
                        onError: (err) => Debug.LogWarning("[CombatManager] Failed to log Lab warp event: " + err)
                    );
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning("[CombatManager] Exception while logging Lab warp event: " + ex.Message);
            }

            // Set spawn location when returning to the Lab
            if (GameManager.instance != null && GameManager.instance.userInfo != null)
            {
                GameManager.instance.userInfo.lastSpawnLocation = SpawnLocation.Default;
                Debug.Log("[CombatManager] Set lastSpawnLocation = FromCombatGate before loading next scene.");
            }
            SceneManager.LoadSceneAsync(nextSceneName, LoadSceneMode.Single);
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Failed to load scene '" + nextSceneName + "': " + ex.Message);
        }
    }

    private void ApplyFamiliarSprites()
    {
        // Player familiar: static for now
        if (playerFamiliarSR != null)
            playerFamiliarSR.sprite = defaultPlayerFamiliar;
        if (playerFamiliarImage != null)
            playerFamiliarImage.sprite = defaultPlayerFamiliar;

        // Enemy familiar: try from selected bounty; fallback to default
        Sprite enemySprite = LoadBountySprite();
        Sprite enemyFinal = enemySprite != null ? enemySprite : defaultEnemyFamiliar;

        if (enemyFamiliarSR != null)
            enemyFamiliarSR.sprite = enemyFinal;
        if (enemyFamiliarImage != null)
            enemyFamiliarImage.sprite = enemyFinal;
    }

    private Sprite LoadBountySprite()
    {
        // Attempt to resolve the bounty sprite from the current bounty info.
        // Preferred path: use a Resources path stored on the bounty item (e.g., "Bounties/Wisp_01").
        if (bountyItem_info == null) return null;

        // Try common string path fields
        try
        {
            var imageField = bountyItem_info.GetType().GetField("image");
            if (imageField != null)
            {
                var val = imageField.GetValue(bountyItem_info) as string;
                if (!string.IsNullOrEmpty(val))
                {
                    var s = Resources.Load<Sprite>(val);
                    if (s != null) return s;
                }
            }
        }
        catch {}
        try
        {
            var imagePathField = bountyItem_info.GetType().GetField("imagePath");
            if (imagePathField != null)
            {
                var val = imagePathField.GetValue(bountyItem_info) as string;
                if (!string.IsNullOrEmpty(val))
                {
                    var s = Resources.Load<Sprite>(val);
                    if (s != null) return s;
                }
            }
        }
        catch {}

        // Direct Sprite field fallback
        try
        {
            var spriteField = bountyItem_info.GetType().GetField("sprite");
            if (spriteField != null)
            {
                var spr = spriteField.GetValue(bountyItem_info) as Sprite;
                if (spr != null) return spr;
            }
        }
        catch {}

        return null;
    }

    private void OnValidate()
    {
        // Auto-assign the catalog in the editor when possible so the EventRef drawer shows dropdowns
#if UNITY_EDITOR
        if (catalog == null)
        {
            var gl = FindObjectOfType<GameLogger>();
            if (gl != null && gl.catalog != null)
            {
                catalog = gl.catalog;
            }
            else
            {
                // Fallback to Resources lookup (Assets/Resources/EventPayloadCatalog.asset)
                var fallback = Resources.Load<EventPayloadCatalog>("EventPayloadCatalog");
                if (fallback != null)
                {
                    catalog = fallback;
                }
            }
        }
#endif
    }
}