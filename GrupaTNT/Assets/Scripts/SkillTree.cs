using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkillTree : MonoBehaviour
{
    public static float playerAttackRange = 1;
    public static bool range1Purchased = false;
    public static bool range2Purchased = false;
    public static bool range3Purchased = false;
    public static bool range4Purchased = false;
    public static bool doubleShotPurchased = false;
    public static bool tripleShotPurchased = false;
    public static bool x5ShotPurchased = false;
    public static bool wheelShotPurchased = false;

    public Button attackRange1;
    public Button attackRange2;
    public Button attackRange3;
    public Button attackRange4;
    public Button doubleShot;
    public Button tripleShot;
    public Button x5Shot;
    public Button wheelShot;


    //needs to be in player script
    public static int totalSkillPoints = 6;
    public static int totalSpentSkillPoints = 0;
    public static int coins = 50;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log(totalSpentSkillPoints);

        //Attack

        //Range
        if ((totalSkillPoints >= 2)) {
            attackRange1.interactable = true;
        }
        if ((totalSkillPoints >= 4) && range1Purchased)
        {
            attackRange2.interactable = true;
        }
        if ((totalSkillPoints >= 6) && range2Purchased)
        {
            attackRange3.interactable = true;
        }
        if ((totalSkillPoints >= 8) && range3Purchased)
        {
            attackRange4.interactable = true;
        }

        //Shots
        if (coins >= 25) {
            doubleShot.interactable = true;
        }
        if (coins >= 50 && doubleShotPurchased) {
            tripleShot.interactable = true;
        }
        if (coins >= 100 && tripleShotPurchased) {
            x5Shot.interactable = true;
        }
        if (coins >= 200 && x5ShotPurchased) {
            wheelShot.interactable = true;
        }



    }

    public void increaseAttackRange(int increase) {
        int i = 1;
        switch (i) {
            case 1:
                range1Purchased = true;
                i++;
                totalSkillPoints -= 2;
                break;
            case 2:
                range2Purchased = true;
                i++;
                totalSkillPoints -= 4;
                break;
            case 3:
                range3Purchased = true;
                i++;
                totalSkillPoints -= 6;
                break;
            case 4:
                range4Purchased = true;
                totalSkillPoints -= 8;
                break;
        }
        playerAttackRange += increase;
        
        if (totalSkillPoints < 0) totalSkillPoints = 0;
        totalSpentSkillPoints += 1;
        Debug.Log(playerAttackRange);
        
    }

    public void DoubleShot() {
        doubleShotPurchased = true;
        coins -= 25;
        if (coins < 0) coins = 0;
        
        Debug.Log("Has Double Shot");
    }

    public void TripleShot()
    {
        tripleShotPurchased = true;
        coins -= 50;
        if (coins < 0) coins = 0;

        Debug.Log("Has Triple Shot");
    }

    public void X5Shot()
    {
        x5ShotPurchased = true;
        coins -= 100;
        if (coins < 0) coins = 0;

        Debug.Log("Has x5 Shot");
    }

    public void WheelShot()
    {
        wheelShotPurchased = true;
        coins -= 200;
        if (coins < 0) coins = 0;

        Debug.Log("Has Wheel Shot");
    }
}
