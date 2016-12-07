﻿namespace Trace {

	/// <summary>
	/// The values are matched to the Google Activity Detection specifications.
	/// See: https://developers.google.com/android/reference/com/google/android/gms/location/DetectedActivity
	/// </summary>
	public enum ActivityType {
		Unknown = 4,
		Walking = 7,
		Running = 8,
		Cycling = 1,
		Automative = 0,
		Stationary = 3
	}


	static class ActivityTypeExtension {
		public static string ToAndroidFormat(this ActivityType a) {
			switch(a) {
				case ActivityType.Walking:
					return "WALKING";
				case ActivityType.Running:
					return "RUNNING";
				case ActivityType.Cycling:
					return "ON_BYCICLE";
				case ActivityType.Automative:
					return "IN_VEHICLE";
				case ActivityType.Stationary:
					return "STILL";
				default:
					return "UNKNOWN";
			}
		}

		public static string ToAndroidFormat(this string activity) {
			if(activity == "Walking")
				return "WALKING";
			if(activity == "Running")
				return "RUNNING";
			if(activity == "Automotive")
				return "IN_VEHICLE";
			if(activity == "Stationary")
				return "STILL";
			if(activity == "Cycling")
				return "ON_BYCICLE";
			return "UNKNOWN";
		}
	}
}