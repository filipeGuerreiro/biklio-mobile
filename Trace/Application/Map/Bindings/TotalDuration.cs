﻿namespace Trace {
	public class TotalDuration {

		public int Hours { get; set; }
		public int Minutes { get; set; }
		public int Seconds { get; set; }

		public string Duration { get { return Hours + ":" + Minutes + ":" + Seconds; } }
	}
}