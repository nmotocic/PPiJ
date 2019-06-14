using System.Collections.Generic;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [Header("Main Player Stats")]
    public string playerName;
    
    
    public int playerHP = 50;

    [Header("Player Attributes")]
    public List<PlayerAttributes> attributes = new List<PlayerAttributes>();

    [Header("Player Skills")]
    public List<Skills> skills = new List<Skills>();

    [SerializeField]
    private int m_PlayerXP = 0;
    public int playerXP {
        get {
            return m_PlayerXP;
        }
        set {
            m_PlayerXP = value;
            if (onXPChange != null) {
                onXPChange();
            }
        }
    }

    [SerializeField]
    private int m_PlayerLvl = 1;
    public int playerLevel {
        get { return m_PlayerLvl; }
        set { m_PlayerLvl = value;
            if (onLevelChange != null)
                {
                onLevelChange();
            } }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public delegate void OnXpChange();
    public event OnXpChange onXPChange;

    public delegate void OnLevelChange();
    public event OnLevelChange onLevelChange;


    //for testing purposes
    public void UpdateLevel(int amount) {
        playerLevel += amount;
    }

    public void UpdateXP(int amount)
    {
        playerXP += amount;
    }
}
