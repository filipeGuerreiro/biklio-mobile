﻿using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Plugin.Geolocator;
using Plugin.Geolocator.Abstractions;
using Xamarin.Forms.Maps;

namespace Trace {

	public class Geolocator {

		public const int LOCATOR_GOOD_ACCURACY = 50;
		private const int MOTION_ONLY_ACCURACY = 1;

		private static IGeolocator locator;

		public static bool IsTrackingInProgress { get; set; }
		private static TraceMap Map;

		public double MaxSpeed;
		public double AvgSpeed;


		public Geolocator(TraceMap map) {
			Map = map;
			IsTrackingInProgress = false;
		}


		public async Task Start() {
			locator = CrossGeolocator.Current;
			/*if(!locator.IsGeolocationEnabled) {
				await DisplayAlert("", "GPS is disabled, please enable it and come back", "Return");
				return;
			}*/
			if(locator.IsListening)
				await locator.StopListeningAsync();

			locator.DesiredAccuracy = LOCATOR_GOOD_ACCURACY;
			var locationSettings = new ListenerSettings {
				AllowBackgroundUpdates = true
				//PauseLocationUpdatesAutomatically = true
			};

			// Get first position
			var position = await GeoUtils.GetCurrentUserLocation();
			UpdateMap(position);

			// Listen to position changes and update map
			try {
				if(!locator.IsListening)
					await locator.StartListeningAsync(minTime: 5000, minDistance: 15, includeHeading: false, settings: locationSettings);
			}
			catch(Exception e) { Debug.WriteLine(e); return; }

			locator.PositionChanged += (sender, e) => {
				if(IsTrackingInProgress) {
					UpdateMap(e.Position);
					if(e.Position.Speed > MaxSpeed) MaxSpeed = e.Position.Speed;
					AvgSpeed += e.Position.Speed;
					Map.RouteCoordinates.Add(e.Position);
				}
			};
		}


		public static async Task Stop() {
			await locator?.StopListeningAsync();
		}


		public void UpdateMap(Plugin.Geolocator.Abstractions.Position position) {
			Map.MoveToRegion(
				MapSpan.FromCenterAndRadius(
					new Xamarin.Forms.Maps.Position(position.Latitude, position.Longitude), Distance.FromKilometers(2)));
		}


		public static void UpdateMap(Xamarin.Forms.Maps.Position position) {
			Map.MoveToRegion(
				MapSpan.FromCenterAndRadius(position, Distance.FromKilometers(2)));
		}


		public static void ImproveAccuracy() {
			if(locator != null)
				locator.DesiredAccuracy = LOCATOR_GOOD_ACCURACY;
		}


		public static void TryLowerAccuracy() {
			if(locator != null && !IsTrackingInProgress)
				locator.DesiredAccuracy = MOTION_ONLY_ACCURACY;
		}
	}
}
