using UnityEngine;
using System.Collections;
using System;

namespace Riverlake
{
	public class EventArgs<T> : EventArgs
	{
		public T Data { get; set; }
	}
}