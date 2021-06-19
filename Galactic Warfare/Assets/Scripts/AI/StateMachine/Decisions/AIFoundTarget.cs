using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "AI/Decisions/Found Target")]
public class AIFoundTarget : AIDecision
{
    public override bool Decide(AIStateController controller)
    {
        return foundTarget(controller);
    }

    private bool foundTarget(AIStateController controller)
    {
        if(controller.sight.currentTarget != null)
        {
            return true;
        }
        return false;
    }
}
