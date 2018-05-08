using UnityEngine;
using System.Collections;
using System;

public sealed class OnDestroyAction : MonoBehaviour
{
	public Action Action { get; set; }
	void OnDestroy()
	{
		if (Action != null)
			Action();
	}
}
