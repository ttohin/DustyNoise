using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Checkbox : MonoBehaviour {

    public bool targetIsActive = true;
    void Start () {
        targetIsActive = PlayerPrefs.GetInt (getPrefKey (), targetIsActive ? 1 : 0) == 1;
        var toggle = GetComponent<Toggle> ();
        toggle.onValueChanged.AddListener (Toggle);
        toggle.isOn = targetIsActive;
    }

    public void Toggle (bool value) {
        targetIsActive = value;
        PlayerPrefs.SetInt (getPrefKey (), targetIsActive ? 1 : 0);
    }
    private string getPrefKey () {
        return name + "PrefKey";
    }
}