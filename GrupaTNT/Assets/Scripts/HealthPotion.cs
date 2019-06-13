using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthPotion : MonoBehaviour
{
    [SerializeField]
    private int health;
    [SerializeField]
    private int stackSize;
    [SerializeField]
    private Text stackText;
    [SerializeField]
    private Button hpButton;
    

    [SerializeField]
    private Button shopButton;
    [SerializeField]
    private int currentAmount;

    void Start()
    {
        currentAmount = 5;
        stackText.text = currentAmount.ToString() + "/" + stackSize.ToString();
    }

    void Update()
    {
        if (currentAmount < stackSize && shopButton.enabled == false) shopButton.enabled = true;
    }
    public void Use() {
        //player.heath += health; -- add
        currentAmount--;
        if (currentAmount == 0) hpButton.enabled = false;
        stackText.text = currentAmount.ToString() + "/" + stackSize.ToString();
        Debug.Log("Health potion used!");
    }

    public void Buy() {
        if (!hpButton.enabled)  hpButton.enabled = true;

        currentAmount++;
        if (currentAmount == stackSize) shopButton.enabled = false;
        stackText.text = currentAmount.ToString() + "/" + stackSize.ToString();
    }
    
}
