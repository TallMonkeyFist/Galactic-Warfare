using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "AI/Decisions/Target Killed")]
public class AITargetKilled : AIDecision
{
    public override bool Decide(AIStateController controller)
    {
        return targetKilled(controller);
    }

    private bool targetKilled(AIStateController controller)
    {
        if (controller.sight.currentTarget == null)
        {
            return true;
        }

        return false;
    }
}
