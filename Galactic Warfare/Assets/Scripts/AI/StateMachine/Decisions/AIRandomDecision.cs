using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "AI/Decisions/Random Decision")]
public class AIRandomDecision : AIDecision
{
    public override bool Decide(AIStateController controller)
    {
        return randomDecision();
    }

    private bool randomDecision()
    {
        int value = Random.Range(0, 100);

        if(value >= 50)
        {
            return true;
        }
        return false;
    }
}
