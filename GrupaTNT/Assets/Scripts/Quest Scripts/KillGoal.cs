using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KillGoal : Goal
{
    private string enemy;

    public KillGoal(string enemy, string description, bool completed, int currentAmount, int requiredAmount)
    {
        Description = description;
        Completed = completed;
        CurrentAmount = currentAmount;
        RequiredAmount = requiredAmount;
        this.enemy = enemy;
    }

    public void EnemyDied(string diedEnemy)
    {
        if (diedEnemy == enemy)
        {
            CurrentAmount++;
        }
        
        Evaluate();
    }
    
}
