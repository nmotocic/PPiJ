using UnityEngine;
using UnityEngine.UI;

public class SkillDisplay : MonoBehaviour
{
    public Skills skill;

    public Text skillName;
    public Text skillDescription;
    public Text skillLevel;
    public Text skillXP;
    public Image skillIcon;

    [SerializeField]
    private PlayerStats m_PlayerHandler;

    private void Start()
    {
        m_PlayerHandler = this.GetComponentInParent<PlayerHandler>().Player;

        m_PlayerHandler.onXPChange += ReactToChange;

        m_PlayerHandler.onLevelChange += ReactToChange;

        if (skill) {
            skill.SetValues(this.gameObject, m_PlayerHandler);
        }

        EnableSkills();
    }

    public void EnableSkills() {
        if (m_PlayerHandler && skill && skill.EnableSkill(m_PlayerHandler))
        {
            TurnOnSkillIcon();
        }
        else if (m_PlayerHandler && skill & skill.CheckSkills(m_PlayerHandler))
        {
            this.GetComponent<Button>().interactable = true;
            this.transform.Find("IconParent").Find("Disabled").gameObject.SetActive(false);
        }
        else {
            TurnOffSkillIcon();
        }
    }

    private void OnEnable()
    {
        EnableSkills();
    }

    public void GetSkill() {
        if (skill.GetSkill(m_PlayerHandler)) TurnOnSkillIcon();
    }

    private void TurnOnSkillIcon() {
        this.GetComponent<Button>().interactable = false;
        this.transform.Find("IconParent").Find("Avaliable").gameObject.SetActive(false);
        this.transform.Find("IconParent").Find("Disabled").gameObject.SetActive(false);
    }

    private void TurnOffSkillIcon() {
        this.GetComponent<Button>().interactable = false;
        this.transform.Find("IconParent").Find("Avaliable").gameObject.SetActive(true);
        this.transform.Find("IconParent").Find("Disabled").gameObject.SetActive(true);
    }

    public void OnDisable()
    {
        m_PlayerHandler.onXPChange -= ReactToChange;
    }

    void ReactToChange() {
        EnableSkills();
    }
}
