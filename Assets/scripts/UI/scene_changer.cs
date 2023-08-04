using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class scene_changer : MonoBehaviour
{
 private void OnCollisionEnter2D(Collision2D collision) {
    if (collision.gameObject.name == "Player"){
        SceneManager.LoadScene("HouseMd");
        Debug.Log("Scene Triggered");
 }
}
}
