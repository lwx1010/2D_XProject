using UnityEngine;
using System.Collections;
using System;

namespace AL
{
	public class EventArgs<T> : EventArgs
	{
		public T Data { get; set; }
	}
}