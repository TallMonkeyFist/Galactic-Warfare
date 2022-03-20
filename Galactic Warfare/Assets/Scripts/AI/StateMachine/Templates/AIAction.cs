using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AIAction : ScriptableObject
{
	public static bool DisplayLogInfo = false;
	public abstract void Act(AIStateController controller);
}
