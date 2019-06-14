using UnityEngine;

public abstract class AiScriptBase : MonoBehaviour
{
    public abstract bool isDangerous();
    public abstract void setState(int set);
    public abstract void setAlarm(float duration);
    public abstract void updateAnimation(float flip);
    public abstract void getStats(ref int health,ref int armor,ref int poise,ref int meleeDamage,ref int rangeDamage);
    public abstract void setDanger(bool level);
}
