using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameUI : MonoBehaviour
{
    public Button Save, Load, Reset, Exit;
    public Action SaveEvent, LoadEvent, ResetEvent;
    public static GameUI Instance;
    public void Awake()
    {
        if (Instance == null)
            Instance = this;
        Save.onClick.AddListener(() => OnSave());
        Load.onClick.AddListener(() => OnLoad());
        Reset.onClick.AddListener(() => OnReset());
        Exit.onClick.AddListener(() => OnExit());
    }

    private void OnExit()
    {
        Application.Quit();
    }

    private void OnReset()
    {
        ResetEvent?.Invoke();
    }

    private void OnLoad()
    {
        LoadEvent?.Invoke();
    }

    private void OnSave()
    {
        SaveEvent?.Invoke();
    }
    public void SetInteractable(bool status, Button btn, float time = 0)
    {
        if (time < 1)
        {
            btn.interactable = status;
            return;
        }
        else
            StartCoroutine(SetInteractableForTime(btn, time));
    }

    private IEnumerator SetInteractableForTime(Button btn, float time)
    {
        btn.interactable = false;
        yield return new WaitForSeconds(time);
        btn.interactable = true;
    }
}
