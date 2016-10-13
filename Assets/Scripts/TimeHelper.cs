using System;
public static class TimeHelper {
	public static readonly DateTime _epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

	public static double Now() {
		return (DateTime.UtcNow - _epoch).TotalSeconds;
	}

	public static double UnitTimestamp(DateTime time) {
		return (time - _epoch).TotalSeconds;	
	}
}