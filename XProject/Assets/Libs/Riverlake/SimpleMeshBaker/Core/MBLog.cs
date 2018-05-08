using System;
using UnityEngine;
using System.Collections;
using System.Text;

namespace Riverlake.SMB.Core{
	 
	public enum MBLogLevel{
		none,
		error,
		warn,
		info,
		debug,
		trace
	}
	
	public class MBLog {
		
		public static void Log(MBLogLevel l, String msg, MBLogLevel currentThreshold){
			if (l <= currentThreshold) {
				if (l == MBLogLevel.error) Debug.LogError(msg);
				if (l == MBLogLevel.warn) Debug.LogWarning(String.Format("frm={0} WARN {1}",Time.frameCount,msg));
				if (l == MBLogLevel.info) Debug.Log(String.Format("frm={0} INFO {1}",Time.frameCount,msg));
				if (l == MBLogLevel.debug) Debug.Log(String.Format("frm={0} DEBUG {1}",Time.frameCount,msg));
				if (l == MBLogLevel.trace) Debug.Log(String.Format("frm={0} TRACE {1}",Time.frameCount,msg));				
			}	
		}

		public static string Error(string msg, params object[] args){
			string s = String.Format(msg, args);
			string s2 = String.Format("f={0} ERROR {1}", Time.frameCount,s);
			Debug.LogError(s2);
			return s2;
		}

		public static string Warn(string msg, params object[] args){
			string s = String.Format(msg, args);
			string s2 = String.Format("f={0} WARN {1}", Time.frameCount,s);
			Debug.LogWarning(s2);
			return s2;
		}		
		
		public static string Info(string msg, params object[] args){
			string s = String.Format(msg, args);
			string s2 = String.Format("f={0} INFO {1}", Time.frameCount,s);
			Debug.Log(s2);
			return s2;
		}
		
		public static string LogDebug(string msg, params object[] args){
			string s = String.Format(msg, args);
			string s2 = String.Format("f={0} DEBUG {1}", Time.frameCount,s);
			Debug.Log(s2);
			return s2;
		}
		
		public static string Trace(string msg, params object[] args){
			string s = String.Format(msg, args);
			string s2 = String.Format("f={0} TRACE {1}", Time.frameCount,s);
			Debug.Log(s2);
			return s2;
		}		
	}
}