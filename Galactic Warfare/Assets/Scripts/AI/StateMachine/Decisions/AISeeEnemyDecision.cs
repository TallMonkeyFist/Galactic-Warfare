using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu (menuName = "AI/Decisions/See Enemy")]
public class AISeeEnemyDecision : AIDecision
{
    public override bool Decide(AIStateController controller)
    {
        return seeEnemy(controller);
    }

    private bool seeEnemy(AIStateController controller)
    {
        if(controller.sight.currentTarget == null)
        {
            return false;
        }

        return true;
    }
}
