using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ToggleButton : MonoBehaviour {
    public GameObject target;
    public bool targetIsActive = false;
    public string activeText;
    public string hiddenText;

    void Start () {
        targetIsActive = PlayerPrefs.GetInt (getPrefKey (), targetIsActive ? 1 : 0) == 1;
        updateMenuState ();
        GetComponent<Button> ().onClick.AddListener (Toggle);
    }

    private string getPrefKey () {
        return name + "PrefKey";
    }

    public void Toggle () {
        targetIsActive = !targetIsActive;
        PlayerPrefs.SetInt (getPrefKey (), targetIsActive ? 1 : 0);
        updateMenuState ();
    }

    private void updateMenuState () {
        GetComponentInChildren<Text> ().text = targetIsActive ? activeText : hiddenText;
        target.SetActive (targetIsActive);
    }
}