using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    //if want more buttons, use field
    [SerializeField]
    private Button actionButton;
    private KeyCode action;

    public Canvas abilityShopTM;
    private KeyCode openAbility;

    void Start()
    {
        //keybinds
        action = KeyCode.Q;
        openAbility = KeyCode.Tab;
    }

    void Update()
    {
        if (Input.GetKeyDown(action)) {
            ActionOnClick();
        }

        if (Input.GetKeyDown(openAbility)) {
            if (abilityShopTM.enabled)
            {
                abilityShopTM.enabled = false;
            }
            else {
                abilityShopTM.enabled = true;
            }
        }
        

    }

    //more buttons - send index as param
    private void ActionOnClick() {
        actionButton.onClick.Invoke();
    }
}
