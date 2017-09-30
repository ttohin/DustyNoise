using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuScript : MonoBehaviour {
    private GameObject mouseModeDropdown;
    bool menuIsActive = false;

    void Start () {
        mouseModeDropdown = GameObject.Find ("Panel");
        mouseModeDropdown.SetActive (false);
    }

    // Update is called once per frame
    void Update () { }

    public void OnClick () {
        menuIsActive = !menuIsActive;
        updateMenuState ();
    }

    private void updateMenuState () {
        GetComponentInChildren<Text> ().text = menuIsActive ? "Hide" : "Menu";
        mouseModeDropdown.SetActive (menuIsActive);
    }
}