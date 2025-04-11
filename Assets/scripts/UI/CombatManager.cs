using System.Net.Mime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using BountyItemData;

public class CombatManager : MonoBehaviour
{
    public BountyItem bountyItem_info;
    public Button attackBtn, surrenderBtn;
    public UserInfo userInfo_temp_for_combat;
    public Slider userHPbar, enemyHPbar;
    public TextMeshProUGUI hpText_user, hpText_enemy;
    public TextMeshProUGUI combatLog;
    private bool isExecuted = false;
    public GameObject playerGO, enemyGO;
    public GameObject attackImage;


    public CombatManager setBountyItem(BountyItem bountyItem)
    {
        this.bountyItem_info = bountyItem.deepCopy();
        return this;
    }

    public void StartCombat()
    {
        Debug.Log("Start Combat");
        userInfo_temp_for_combat = GameManager.instance.userInfo.deepCopy();

        Debug.Log(userInfo_temp_for_combat+"userHPbar: "+userHPbar);
        Debug.Log("User Info: " + userInfo_temp_for_combat.mean + " " + userInfo_temp_for_combat.sd);

        userHPbar.maxValue = userInfo_temp_for_combat.mean;
        userHPbar.value = userInfo_temp_for_combat.mean;

        enemyHPbar.maxValue = bountyItem_info.mean;
        enemyHPbar.value = bountyItem_info.mean;
        hpText_user.text = "HP: " + userInfo_temp_for_combat.mean.ToString();
        hpText_enemy.text = "HP: " + bountyItem_info.mean.ToString();
    }

    private void Awake() {

        GameObject go = GameObject.Find("CombatManager_Temp");
        if (go != null){
            bountyItem_info = go.GetComponent<CombatManager>().bountyItem_info.deepCopy();
            Destroy(go); 
        }


        attackBtn.onClick.AddListener(Attack);
        surrenderBtn.onClick.AddListener(Surrender);
    }

    void Start()
    {
        StartCombat();
    }

    public void Surrender()
    {
        Debug.Log("Surrender");
        combatLog.text = "Surrender";
    }

    IEnumerator AttackedByTheEnemy(){
        yield return new WaitForSeconds(0.2f);
        Debug.Log("Attacked by the enemy");
        float calulatedDefenceDamage = getAttackedDamage(userInfo_temp_for_combat, bountyItem_info);
        if (calulatedDefenceDamage >= userInfo_temp_for_combat.mean){
            //this means the user lose
            Debug.Log("User Lose");
            combatLog.text = "User Lose";
        }
        animateAttack(false, 0.2f);
        yield return new WaitForSeconds(0.2f);
        changeHPbar(true, calulatedDefenceDamage);
        isExecuted = false;
    }
    
    public void Attack()
    {
        if (isExecuted){
            return;
        }
        isExecuted = true;
        StartCoroutine(Attack_Coroutine()); 
    }

    IEnumerator Attack_Coroutine(){
        Debug.Log("Attack");
        float calulatedAttackDamage = getAttackDamage(userInfo_temp_for_combat, bountyItem_info);
        if (calulatedAttackDamage >= bountyItem_info.mean){
            //this means the user win and HP of the enemy must be 0
            animateAttack(true, 0.2f);
            yield return new WaitForSeconds(0.2f);
            changeHPbar(false, calulatedAttackDamage);
            Debug.Log("User Win");
            combatLog.text = "User Win";
            isExecuted = false;
            yield break;
        }else{
            //this means that it's just normal attack to the enemy
            animateAttack(true, 0.2f);
            yield return new WaitForSeconds(0.2f);
            changeHPbar(false, calulatedAttackDamage);
            //animate and change HP bar
            Debug.Log("Normal Attack");
            combatLog.text = "Normal Attack";
            //now being attacked by the enemy
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
}
