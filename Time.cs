using System;

public static class CyclesForRhinoConstants
{
#if DEBUG
	public static string BuiltAgainst => "6.0.17137.1000";
#else
	public static string BuiltAgainst => "6.0.17136.10381";
#endif
	public static bool Ok {
		get {
			bool rc = false;

			var ba = BuiltAgainst.Split('.')[2];
			var yr = int.Parse(ba.Substring(0, 2)) + 2000;
			var dyr = int.Parse(ba.Substring(2)) + 7;

			DateTime dt = new DateTime(yr, 1, 1);
			dt = dt.AddDays(dyr);

			rc = dt < DateTime.Now;

			return rc;
		}
	}
}