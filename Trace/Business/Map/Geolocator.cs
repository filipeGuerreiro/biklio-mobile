﻿using System;
using System.Threading.Tasks;
using Plugin.Geolocator;
using Xamarin.Forms.Maps;

namespace Trace {

	public class Geolocator {
		private const int LOCATOR_TIMEOUT = 1000000;
		private const int LOCATOR_ACCURACY = 50;

		public bool IsTrackingInProgress { get; set; }
		private DateTime StartTrackingTime;
		private DateTime StopTrackingTime;
		private TraceMap Map;

		public double MaxSpeed;
		public double AvgSpeed;

		public Geolocator(TraceMap map) {
			Map = map;
			IsTrackingInProgress = false;
		}

		public async void Start() {
			var locator = CrossGeolocator.Current;
			/*if(!locator.IsGeolocationEnabled) {
				await DisplayAlert("", "GPS is disabled, please enable it and come back", "Return");
				return;
			}*/
			if(locator.IsListening)
				await locator.StopListeningAsync();

			locator.DesiredAccuracy = LOCATOR_ACCURACY;
			locator.AllowsBackgroundUpdates = true;
			locator.PausesLocationUpdatesAutomatically = false;

			// Get first position
			var position = await locator.GetPositionAsync(timeoutMilliseconds: LOCATOR_TIMEOUT);
			UpdateMap(position);

			// Listen to position changes and update map
			await locator.StartListeningAsync(minTime: 5000, minDistance: 15);
			locator.PositionChanged += (sender, e) => {
				if(IsTrackingInProgress) {
					UpdateMap(e.Position);
					if(e.Position.Speed > MaxSpeed) MaxSpeed = e.Position.Speed;
					AvgSpeed += e.Position.Speed;
					Map.RouteCoordinates.Add(new Position(e.Position.Latitude, e.Position.Longitude));
				}
			};
		}

		private void UpdateMap(Plugin.Geolocator.Abstractions.Position position) {
			Map.MoveToRegion(
				MapSpan.FromCenterAndRadius(
					new Position(position.Latitude, position.Longitude), Distance.FromKilometers(1)));
		}
	}
}
