using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Quest : MonoBehaviour
{
    public List<Goal> Goals { get; set; } = new List<Goal>();
    public string QuestNames { get; set; }
    public string Description { get; set; }
    public int ExperienceReward { get; set; }
    public bool Completed { get; set; }

    public void CheckGoals()
    {
        if (Goals.All(g => g.Completed))
        {
            Completed = true;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
