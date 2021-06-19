using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu (menuName = "AI/Decisions/Target Lost")]
public class AILostTarget : AIDecision
{
    public override bool Decide(AIStateController controller)
    {
        return lostTarget(controller);
    }

    private bool lostTarget(AIStateController controller)
    {
        if(controller.sight.currentTarget != null && controller.sight.targetSeen)
        {
            return false;
        }

        return true;
    }
}
