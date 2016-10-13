using UnityEngine;
using System.Collections;
using System;
using FullSerializer;
using System.Collections.Generic;
using System.Text;

public class Raven {
	private static Raven _instance;
	public static Raven Instance {
		get {
			if(_instance == null) {
				_instance = new Raven();
			}
			return _instance;
		}
	}
	Dsn _dsn;

	public void Init(string dsn) {
		_dsn = new Dsn(dsn);
	}

	#region dsn
	class Dsn {
		public readonly string uri;
		public readonly string auth;
		static readonly string AUTH_TEMPLATE = "Sentry sentry_version=5, " +
			"sentry_timestamp={{0}}, sentry_key={0},sentry_secret={1}";	

		public Dsn(string dsn) {
			var dsnUri = new Uri(dsn);
			var userpass = dsnUri.UserInfo.Split(':');
			var pub = userpass[0];
			var pri = userpass[1];
			int lasSlash = dsnUri.AbsolutePath.LastIndexOf("/", StringComparison.Ordinal);
			string path = dsnUri.AbsolutePath.Substring(0, lasSlash);
			string projectId = dsnUri.AbsolutePath.Substring(lasSlash+1);
			uri = string.Format("{0}://{1}:{2}{3}/api/{4}/store/", 
				dsnUri.Scheme,
				dsnUri.DnsSafeHost,
				dsnUri.Port,
				path,
				projectId
			);
			auth = string.Format(AUTH_TEMPLATE, pub, pri);
		}
	}
	#endregion

	#region event
	public class Event {
		public string event_id;
		public string level = "error";
		public DateTime timestamp;
		public string message;
		public string logger;
		public string platform = "c#";
		public string culprit;
		public fsData tags = fsData.Null;
		public fsData modules = fsData.Null;
		public fsData extra = fsData.Null;

		private readonly Dictionary<string, fsData> _interface = new Dictionary<string, fsData>();

		public Event(LogType type) {
			timestamp = DateTime.UtcNow;
			event_id = Guid.NewGuid().ToString("n");
			switch(type) {
				case LogType.Log:
					level = "info"; 
					break;
				case LogType.Warning:
					level = "warning";
					break;
				case LogType.Error:
					level = "error";
					break;
				case LogType.Exception:
					level = "exception";
					break;
			}
		}

		public void SetTag(string name, string value) {
			if(tags.IsNull) {
				tags = fsData.CreateDictionary();
			}
			tags.AsDictionary[name] = new fsData(value);
		}

		public void SetExtra(string name, fsData value) {
			if(extra.IsNull) {
				extra = fsData.CreateDictionary();
			}
			extra.AsDictionary[name] = value;
		}

		public fsData this[string name] {
			get {return _interface[name];}
			set {_interface[name] = value;}
		}

		public string ToJson() {
			this["event_id"] = new fsData(event_id);
			this["timestamp"] = new fsData(timestamp.ToString("s"));
			this["message"] = new fsData(message);
			if(!extra.IsNull) this["extra"] = extra;
			return Json.Generate(new fsData(_interface));
		}

		public byte[] Payload() {
			return Encoding.UTF8.GetBytes(ToJson());
		}
	}
	#endregion

	public void CaptureLog(string logString, string stackTrace, LogType type) {
		Event e = new Event(type);
		e.message = logString + " " + stackTrace;
		e.SetExtra("stacktrace", new fsData(stackTrace));
		SendEvent(e);
	}

	bool SendEvent(Event e) {
		Main.Instance.StartCoroutine(ExecuteWWW(_dsn.uri,e.Payload(), BuildHeaders(e)));
		return true;
	}

	IEnumerator ExecuteWWW(string uri, byte[] data, Dictionary<string, string> headers) {
		WWW www = new WWW(uri, data, headers);
		yield return www;
	}
		
	static readonly Dictionary<string, string> _defaultHeaders = new Dictionary<string, string> { 
		{ "Content-Type", "application/json" }, 
		{ "User-Agent", "unity3d-raven/1.0" }
	};
		
	Dictionary<string, string> BuildHeaders(Event e) {
		Dictionary<string, string> headers = new Dictionary<string, string>(_defaultHeaders);
		headers["X-Sentry-Auth"] = string.Format(_dsn.auth, (long) TimeHelper.UnitTimestamp(e.timestamp));
		return headers;
	}
}
