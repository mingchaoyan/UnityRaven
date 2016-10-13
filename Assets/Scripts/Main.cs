using UnityEngine;
using System.Collections;

public class Main : MonoBehaviour {
	public static Main Instance {get; private set;}

	// Use this for initialization
	void Start () {
		Instance = this;
		Raven.Instance.Init(
			"http://xxx:xxx@xxx/xxx.com/1");
		Application.logMessageReceived += HandleLog;

		int i2 = 0;
		int i = 10 / i2;
	}

	static void HandleLog(string message, string stackTrace, LogType type) {
		Raven.Instance.CaptureLog(message, stackTrace, type);
	}
	
}
