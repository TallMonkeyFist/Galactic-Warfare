using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "AI/Decisions/Patrol Time Elapsed")]
public class AIPatrolTimeElapsed : AIDecision
{
    public override bool Decide(AIStateController controller)
    {
        return checkTime(controller);
    }

    private bool checkTime(AIStateController controller)
    {
        if (controller.CheckIfCountDownElapsed(controller.patrolTime))
        {
            return true;
        }
        return false;
    }
}
