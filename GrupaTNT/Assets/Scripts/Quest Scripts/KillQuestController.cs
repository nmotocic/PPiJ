using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class KillQuestController : MonoBehaviour
{
    private List<Goal> Goals;
    private SpawnController _spawnController;
    private LocationController _locationController;
    
    public void Init()
    {
        _spawnController = GetComponent<SpawnController>();
        _locationController = GetComponent<LocationController>();
        Goals = new List<Goal>();
        foreach (var enemyType in _spawnController.enemyTypes)
        {
            Goals.Add(new KillGoal(enemyType.Key, "Kill enemies", 
                false, 0, enemyType.Value/10));
        }
        
        Goals.Add(new KillGoal(_spawnController.bossType, "Kill the boss", 
            false, 0, 1));
        
    }

    public void EnemyDeath(string enemy)
    {
        Goals.ForEach(goal =>
        {
            if (goal.GetType() == typeof(KillGoal))
            {
                var killGoal = (KillGoal) goal;
                killGoal.EnemyDied(enemy);
            }
                
        });

        if (Goals.All(goal => goal.Completed))
        {
            _spawnController.SpawnExitInRoomCenter(_locationController.locationOnRoomGrid);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
