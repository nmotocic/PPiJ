using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName ="EnterTheDungeon/Player/Create Skill")]
public class Skills : ScriptableObject
{
    public string descrpiton;
    public Sprite icon;
    public int levelNeeded;
    public int xpNeeded;

    public List<PlayerAttributes> affectedAttributes = new List<PlayerAttributes>();

    public void SetValues(GameObject SkillDisplayObject, PlayerStats Player) {
        if (SkillDisplayObject) {
            SkillDisplay SD = SkillDisplayObject.GetComponent<SkillDisplay>();
            SD.skillName.text = name;
            if (SD.skillDescription) SD.skillDescription.text = descrpiton;
            if (SD.skillIcon) SD.skillIcon.sprite = icon;
            if (SD.skillXP) SD.skillXP.text = xpNeeded.ToString() + " XP";
            if (SD.skillLevel) SD.skillLevel.text = levelNeeded.ToString();
        }
    }

    public bool CheckSkills(PlayerStats player) {
        if (player.playerLevel < levelNeeded) {
            return false;
        }
        if (player.playerXP < xpNeeded) {
            return false;
        }
        return true;
    }

    public bool EnableSkill(PlayerStats player) {
        List<Skills>.Enumerator skills = player.skills.GetEnumerator();
        while (skills.MoveNext()) {
            var CurrSkill = skills.Current;
            if (CurrSkill.name == this.name) {
                return true;
            }
            
        }
        return false;
    }

    public bool GetSkill(PlayerStats player) {
        int i = 0;
        List<PlayerAttributes>.Enumerator attributes = affectedAttributes.GetEnumerator();
        while (attributes.MoveNext()) {
            List<PlayerAttributes>.Enumerator playerAttr = player.attributes.GetEnumerator();
            while (playerAttr.MoveNext()) {
                if (attributes.Current.attribute.name.ToString() != playerAttr.Current.attribute.name.ToString()) {
                    playerAttr.Current.amount += attributes.Current.amount;
                    i++;
                }
            }
            if (i > 0) {
                player.playerXP -= this.xpNeeded;
                player.skills.Add(this);
                return true;
            }
        }
        return false;
    }
}
