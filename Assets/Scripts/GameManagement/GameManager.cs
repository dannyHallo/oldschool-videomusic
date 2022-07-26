using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public GameObject spectrum;
    public GameObject terminal;

    private void Start()
    {
        Cursor.visible = false;
        terminal.SetActive(true);
        spectrum.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            SwitchView();
        }
    }

    void SwitchView()
    {
        if (terminal.activeInHierarchy)
        {
            terminal.SetActive(false);
            spectrum.SetActive(true);
        }
        else
        {
            terminal.SetActive(true);
            spectrum.SetActive(false);
        }
    }
}
