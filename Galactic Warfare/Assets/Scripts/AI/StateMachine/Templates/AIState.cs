using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "AI/State")]
public class AIState : ScriptableObject
{
	public AIAction[] actions;
	public AITransition[] transitions;
	public Color sceneGizmoColor = Color.grey;

	public void UpdateState(AIStateController controller)
	{
		CheckTransitions(controller);
		DoActions(controller);
	}

	private void DoActions(AIStateController controller)
	{
		foreach (AIAction action in actions)
		{
			action.Act(controller);
		}
	}

	private void CheckTransitions(AIStateController controller)
	{
		foreach(AITransition transition in transitions)
		{
			bool decisionSucceeded = transition.decision.Decide(controller);

			if(decisionSucceeded)
			{
				controller.TransitionToState(transition.trueState);
			}
			else
			{
				controller.TransitionToState(transition.falseState);
			}
		}
	}
}
