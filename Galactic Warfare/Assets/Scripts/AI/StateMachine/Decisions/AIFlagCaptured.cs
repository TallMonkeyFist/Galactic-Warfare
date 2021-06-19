using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu (menuName = "AI/Decisions/Flag Captured")]
public class AIFlagCaptured : AIDecision
{
    public override bool Decide(AIStateController controller)
    {
        return isFlagCaptured(controller);
    }

    private bool isFlagCaptured(AIStateController controller)
    {
        switch(controller.team)
        {
            case 1:
                return teamOneControlFlag(controller);

            case 2:
                return teamTwoControlFlag(controller);

            default:
                return false;
        }
    }

    private bool teamOneControlFlag(AIStateController controller)
    {
        if(controller.flagController.TryGetAssignedFlag(out Flag flag))
        {
            if(100 <= flag.FlagValue)
            {
                return true;
            }
            return false;
        }

        return true;
    }

    private bool teamTwoControlFlag(AIStateController controller)
    {
        if (controller.flagController.TryGetAssignedFlag(out Flag flag))
        {
            if (flag.FlagValue <= -100)
            {
                return true;
            }
            return false;
        }

        return true;
    }
}
