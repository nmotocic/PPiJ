using UnityEngine;
using UnityEngine.UI;

public class Health : MonoBehaviour
{
    public int health;
    public int numOfHearts;

    public Image[] lives;
    public Sprite heartFull;
    public Sprite heartEmpty;

    private void Update()
    {
        if (health > numOfHearts)
        {
            health = numOfHearts;
        }

        for (int i = 0; i < lives.Length; i++)
        {


            if (i < health)
            {
                lives[i].sprite = heartFull;
            }
            else
            {
                lives[i].sprite = heartEmpty;
            }
            if (numOfHearts > i)
            {
                lives[i].enabled = true;
            }
            else
            {
                lives[i].enabled = false;
            }
        }
    }
}
