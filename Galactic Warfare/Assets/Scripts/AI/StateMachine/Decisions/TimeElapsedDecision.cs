using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu (menuName = "AI/Decisions/Time Elapsed")]
public class TimeElapsedDecision : AIDecision
{
    public override bool Decide(AIStateController controller)
    {
        return checkTime(controller);
    }

    private bool checkTime(AIStateController controller)
    {
        if(controller.CheckIfCountDownElapsed(controller.enemyStats.ChaseTime))
        {
            return true;
        }
        return false;
    }
}
