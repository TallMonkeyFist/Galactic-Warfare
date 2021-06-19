using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu (menuName = "AI/Decisions/Heard Sound")]
public class AISoundHeard : AIDecision
{
    public override bool Decide(AIStateController controller)
    {
        return aiHearSound(controller);
    }

    private bool aiHearSound(AIStateController controller)
    {
        if (controller.hearing.TryGetLastAudioEvent(out AudioEvent audioEvent))
        {
            int hearChance = Random.Range(0, 500);
            if (Time.time <= controller.enemyStats.TimeToIgnoreSound + audioEvent.timeStamp && hearChance <= 4)
            {
                controller.lastAudioPosition = audioEvent.position;
                return true;
            }
        }
        return false;
    }
}
