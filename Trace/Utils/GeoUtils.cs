﻿using System;
using System.Threading.Tasks;
using Plugin.Geolocator;
using Plugin.Geolocator.Abstractions;

namespace Trace {

	public static class GeoUtils {

		const int GET_LOCATION_TIMEOUT = 1500;
		static Position prevLocation = new Position { Longitude = 0D, Latitude = 0D };

		/// <summary>
		/// Attempts to get the user location within a specified timeout.
		/// </summary>
		/// <returns>The current user location.</returns>
		public static async Task<Position> GetCurrentUserLocation(int timeout = GET_LOCATION_TIMEOUT) {
			Position location;
			var locator = CrossGeolocator.Current;
			var prevAccuracy = locator.DesiredAccuracy;
			locator.DesiredAccuracy = Geolocator.LOCATOR_GOOD_ACCURACY;

			try {
				prevLocation = location = await locator.GetPositionAsync(timeout);
			}
			catch(Exception) {
				locator.DesiredAccuracy = prevAccuracy;
				return prevLocation;
			}

			locator.DesiredAccuracy = prevAccuracy;
			return location;
		}


		/// <summary>
		/// Calculates the distance between points using Equirectangular approximation.
		/// This does not take into account the arc of the Earth, but is much quicker than more accurate options and acceptable for our purposes.
		/// </summary>
		/// <returns>The distance in meters.</returns>
		/// <param name="pos1">Position 1.</param>
		/// <param name="pos2">Position 2.</param>
		public static double DistanceBetweenPoints(Position pos1, Position pos2) {
			const int EARTH_RADIUS_METERS = 6371009;

			var lng1 = Math.Abs(pos1.Longitude);
			var lng2 = Math.Abs(pos2.Longitude);
			var alpha = lng2 - lng1;
			var lat1 = Math.Abs(pos1.Latitude);
			var lat2 = Math.Abs(pos2.Latitude);
			var x = (alpha * (Math.PI / 180)) * Math.Cos((lat1 + lat2) * (Math.PI / 180) / 2);
			var y = (Math.PI / 180) * (lat1 - lat2);
			return Math.Sqrt(x * x + y * y) * EARTH_RADIUS_METERS;
		}
	}
}
