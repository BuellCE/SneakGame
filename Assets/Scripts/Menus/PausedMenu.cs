using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PausedMenu : MonoBehaviour {

    GameController controller;

    private bool isPaused;

    public GameObject menuComponents;
    public Text menuText;

    void Start() {
        isPaused = false;
        controller = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameController>();
    }

    void Update() {
        if (Input.GetKeyUp(KeyCode.Escape)) {
            TogglePause();
        }
    }

    public void TogglePause() {
        if (controller.CanPlayerControl()) {
            isPaused = !isPaused;
            SetMenuVisibility(isPaused);
        }
    }

    public void SetToEndOfLevelMenu() {
        menuText.text = "Level Complete";
    }

    public void SetToPlayingMenu() {
        menuText.text = "Menu";
    }

    public void ContinueButtonPushed() {
        if (controller.IsGameOver()) {
            SetMenuVisibility(false);
            controller.StartNextLevel();
        } else {
            TogglePause();
        }
    }

    public void SetMenuVisibility(bool visible) {
        menuComponents.SetActive(visible);
    }

    public void ReplayButtonPushed() {
        SetMenuVisibility(false);
        isPaused = false;
        controller.RestartLevel();
    }

    public void ExitButtonPushed() {

        controller.ReturnToMenu();
    }

}
