﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Plugin.Geolocator.Abstractions;
using Trace.Localization;
using Xamarin.Forms;

namespace Trace {

	/// <summary>
	/// This class maintains the state of the several conditions (i.e. guards) that are used to determine
	/// if the state machine should transition or not. 
	/// </summary>
	public class RewardEligibilityManager {
		RewardEligibilityStateMachine stateMachine;
		Dictionary<State, Action<int>> transitionGuards;
		Timer timer;
		Timer vehicularTimer;
		Task checkNearbyCheckpointsTask;

		long cyclingEventStart;

		// Successive count threshold for transitioning between states.
		private const int THRESHOLD = 5;
		// Timeout threshold between 'cyclingIneligible' to 'cyclingEligible' in ms. 1,5 min.
		private const int CYCLING_INELIGIBLE_TIMEOUT = 90 * 1000;
		// Timeout threshold between 'unknownEligible' to 'ineligible' in ms. 12 h.
		private const int UNKNOWN_ELIGIBLE_TIMEOUT = 12 * 60 * 60 * 1000;
		// Timeout threshold between 'inAVehicle' to 'ineligible' in ms. 5 min.
		private const int VEHICULAR_TIMEOUT = 5 * 60 * 1000;

		// Timers used when the device is only updated between radio tower changes (iOS w/ background audio off).
		private int backgroundCyclingIneligibleTimer = CYCLING_INELIGIBLE_TIMEOUT;
		private int backgroundUnknownEligibleTimer = UNKNOWN_ELIGIBLE_TIMEOUT;
		private int backgroundVehicularTimer = VEHICULAR_TIMEOUT;

		// Minimum distance between user and checkpoints for reward eligibility.
		private readonly double CKPT_DISTANCE_THRESHOLD = 120;
		// Minimum time between verification attempts.
		private readonly int CHECK_NEARBY_TIMEOUT = 90 * 1000;
		private Timer checkNearbyCheckpointsTimer;

		int cyclingCount;
		int nonCyclingCount;
		int vehicularCount;
		int nonVehicularCount;

		static RewardEligibilityManager instance;
		public static RewardEligibilityManager Instance {
			get {
				if(instance == null) { instance = new RewardEligibilityManager(); }
				return instance;
			}
			set { instance = value; }
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:Trace.RewardEligibilityManager"/> class.
		/// The 'transitionGuards' is a dictionary that provides constant look-up for the next action to call based on the current state.
		/// </summary>
		public RewardEligibilityManager() {
			stateMachine = new RewardEligibilityStateMachine();
			transitionGuards = new Dictionary<State, Action<int>> {
				{ State.Ineligible, new Action<int>(ineligibleStateGuards) },
				{ State.CyclingIneligible, new Action<int>(cyclingIneligibleStateGuards) },
				{ State.CyclingEligible, new Action<int>(cyclingEligibleStateGuards) },
				{ State.UnknownEligible, new Action<int>(unknownEligibleStateGuards) },
				{ State.Vehicular, new Action<int>(vehicularStateGuards) }
			};
		}

		/// <summary>
		/// Destructor called upon dereferencing (i.e. when user logs out).
		/// </summary>
		~RewardEligibilityManager() {
			timer?.Dispose();
			vehicularTimer?.Dispose();
			checkNearbyCheckpointsTimer?.Dispose();
			stateMachine = null;
		}

		/// <summary>
		/// The feedback from the motion detector goes here, passes the guard checks and goes into the state-machine.
		/// </summary>
		/// <param name="activity">Activity.</param>
		public void Input(ActivityType activity) {
			incrementCounters(activity);
			Action<int> nextAction;
			var state = stateMachine.CurrentState;
			transitionGuards.TryGetValue(state, out nextAction);
			nextAction.Invoke(-1);
		}


		/// <summary>
		/// Same as above but used sporadically by iOS devices when significant location updates occur (cell towers change).
		/// </summary>
		/// <param name="activity">Activity.</param>
		public void Input(ActivityType activity, int elapsedTime) {
			incrementCounters(activity);
			Action<int> nextAction;
			var state = stateMachine.CurrentState;
			switch(state) {
				case State.CyclingIneligible:
					backgroundCyclingIneligibleTimer -= elapsedTime;
					break;
				case State.CyclingEligible:
					backgroundUnknownEligibleTimer -= elapsedTime;
					break;
				case State.Vehicular:
					backgroundVehicularTimer -= elapsedTime;
					break;
			}

			if(backgroundCyclingIneligibleTimer < 1 || backgroundUnknownEligibleTimer < 1 || backgroundVehicularTimer < 1) {
				stateMachine.MoveNext(Command.Timeout);
				state = stateMachine.CurrentState;
				backgroundCyclingIneligibleTimer = CYCLING_INELIGIBLE_TIMEOUT;
				backgroundUnknownEligibleTimer = UNKNOWN_ELIGIBLE_TIMEOUT;
				backgroundVehicularTimer = VEHICULAR_TIMEOUT;
			}

			transitionGuards.TryGetValue(state, out nextAction);
			nextAction.Invoke(elapsedTime);
		}


		/// <summary>
		/// Exposes the state machine state to the rest of the application.
		/// </summary>
		/// <returns>The current state.</returns>
		public State GetCurrentState() {
			return stateMachine.CurrentState;
		}


		/// <summary>
		/// Increments the counters depending on the activity.
		/// Counters count SUCCESSIVE activities.
		/// </summary>
		/// <param name="activity">Activity.</param>
		void incrementCounters(ActivityType activity) {
			if(activity == ActivityType.Cycling) {
				cyclingCount++; nonVehicularCount++; nonCyclingCount = vehicularCount = 0;
			}
			else if(activity == ActivityType.Automative) {
				vehicularCount++; nonCyclingCount++; cyclingCount = 0;
			}
			else {
				nonCyclingCount++; nonVehicularCount++; cyclingCount = vehicularCount = 0;
			}
		}


		void resetCounters() {
			cyclingCount = nonCyclingCount = vehicularCount = nonVehicularCount = 0;
		}


		void ineligibleStateGuards(int elapsedTime) {
			// If user starts using a bycicle, go to: 'cyclingIneligible'.
			if(cyclingCount > THRESHOLD) {
				resetCounters();
				cyclingEventStart = TimeUtil.CurrentEpochTimeSeconds();
				stateMachine.MoveNext(Command.Cycling);
				// Start timer -> If user continues using a bycicle for a certain time, go to: 'cyclingEligible'.
				if(elapsedTime == -1) {
					timer = new Timer(new TimerCallback(goToCyclingEligibleCallback), null, CYCLING_INELIGIBLE_TIMEOUT);
				}
			}
		}


		void cyclingIneligibleStateGuards(int elapsedTime) {
			// If user stops using a bycicle, go back to the start: 'ineligible'.
			if(nonCyclingCount > THRESHOLD) {
				timer.Dispose();
				resetCounters();
				// Ignore small cycling events.
				cyclingEventStart = 0;
				stateMachine.MoveNext(Command.NotCycling);
			}
		}


		void cyclingEligibleStateGuards(int elapsedTime) {
			// If user stops using a bycicle, go to: 'unknownEligible'.
			if(nonCyclingCount > THRESHOLD) {
				resetCounters();

				// Record cycling event.
				User.Instance.GetCurrentKPI().AddCyclingEvent(cyclingEventStart, TimeUtil.CurrentEpochTimeSeconds());

				stateMachine.MoveNext(Command.NotCycling);

				// User likely got off bike, give an audible congratulatory sound to let her know she is eligible.
				DependencyService.Get<ISoundPlayer>().PlayShortSound(User.Instance.CongratulatorySoundSetting, 0);

				// Start a long timer where the user is still eligible for rewards even when not using a bycicle.
				// If the timer goes off (the user goes too long without using a bycicle), go back to 'ineligible'.
				if(elapsedTime == -1) {
					timer = new Timer(new TimerCallback(goToIneligibleCallback), null, UNKNOWN_ELIGIBLE_TIMEOUT);
				}
			}
		}


		void unknownEligibleStateGuards(int elapsedTime) {
			// When user leaves bike, check for nearby checkpoints/shops for reward notification every CHECK_NEARBY_TIMEOUT period.
			if(checkNearbyCheckpointsTask == null) {
				checkNearbyCheckpointsTask = new Task(() => checkNearbyCheckpoints());
				checkNearbyCheckpointsTask.Start();
			}

			// If users start using a vehicle, go to 'inAVehicle' and start a shorter timer that makes her ineligible after it fires.
			if(vehicularCount > THRESHOLD) {
				resetCounters();
				stateMachine.MoveNext(Command.InAVehicle);
				if(elapsedTime == -1) {
					vehicularTimer = new Timer(new TimerCallback(goToIneligibleCallback), null, VEHICULAR_TIMEOUT);
				}
			}
			// If user starts cycling again, go back to 'cyclingEligible'.
			if(cyclingCount > THRESHOLD) {
				resetCounters();
				cyclingEventStart = TimeUtil.CurrentEpochTimeSeconds();
				stateMachine.MoveNext(Command.Cycling);
				DependencyService.Get<ISoundPlayer>().PlaySound(User.Instance.BycicleEligibleSoundSetting);
				timer.Dispose();
			}
		}


		void vehicularStateGuards(int elapsedTime) {
			// If the user stops using a vehicle, go back to 'unknownEligible'.
			if(nonVehicularCount > THRESHOLD) {
				resetCounters();
				vehicularTimer.Dispose();
				stateMachine.MoveNext(Command.NotInAVehicle);
			}
		}


		/// <summary>
		/// When a user goes from 'cyclingEligible' to 'unknownEligible', i.e., stops cycling, 
		/// check for shops nearby with 'distance' condition to see if she is eligible for rewards.
		/// </summary>
		async Task checkNearbyCheckpoints() {
			var now = TimeUtil.CurrentEpochTimeSeconds();
			var nChallengesCompleted = 0;
			// Compare the distance between the user and the checkpoints.
			var userLocation = await GeoUtils.GetCurrentUserLocation();
			foreach(var checkpoint in User.Instance.Checkpoints) {
				var ckptLocation = new Position {
					Longitude = checkpoint.Value.Longitude,
					Latitude = checkpoint.Value.Latitude
				};
				var distance = GeoUtils.DistanceBetweenPoints(userLocation, ckptLocation);

				if(distance < CKPT_DISTANCE_THRESHOLD) {
					// Check their challenges' conditions.
					foreach(var challenge in checkpoint.Value.Challenges) {
						var createdAt = challenge.CreatedAt;
						var expiresAt = challenge.ExpiresAt;
						// For the valid challenges ...
						if(TimeUtil.IsWithinPeriod(now, createdAt, expiresAt)) {
							// Check if distance cycled meets the criteria.
							if(challenge.NeededCyclingDistance <= CycledDistanceBetween(createdAt, expiresAt)) {
								challenge.IsComplete = true;
								challenge.CompletedAt = now;
								SQLiteDB.Instance.SaveItem(challenge);
								nChallengesCompleted++;
								// Record event
								User.Instance.GetCurrentKPI().AddCheckInEvent(now, challenge.CheckpointId);
							}
						}
					}
				}
			}

			sendRewardNotificationUser(nChallengesCompleted);

			// Start a timer that waits a certain period before doing 'checkNearbyCheckpoints' again.
			checkNearbyCheckpointsTimer = new Timer(new TimerCallback(
													(obj) => { checkNearbyCheckpointsTask = null; }),
 													null,
 													CHECK_NEARBY_TIMEOUT);
		}


		public static long CycledDistanceBetween(long start, long end) {
			long res = 0;
			foreach(Trajectory t in User.Instance.Trajectories) {
				if(TimeUtil.IsWithinPeriod(t.EndTime, start, end))
					res += t.CalculateCyclingDistance();
			}
			return res;
		}


		/// <summary>
		/// Called when a user goes from 'cyclingIneligible' to 'cyclingEligible', i.e., 
		/// has been cycling for at least 1.5 mins, 
		/// check the list of challenges for those without 'distance' condition, i.e., the 'cycle to shop' challenges.
		/// </summary>
		void checkForRewards() {
			var now = TimeUtil.CurrentEpochTimeSeconds();
			var cycleToShopChallenges = User.Instance.Challenges.FindAll((x) => x.NeededCyclingDistance == 0);
			var nChallengesCompleted = cycleToShopChallenges.Count;
			foreach(var c in cycleToShopChallenges) {
				c.IsComplete = true;
				c.CompletedAt = now;
				SQLiteDB.Instance.SaveItem(c);
				// Record event
				User.Instance.GetCurrentKPI().AddCheckInEvent(now, c.CheckpointId);
			}

			sendRewardNotificationUser(nChallengesCompleted);
		}


		static void sendRewardNotificationUser(int nChallengesCompleted) {
			if(nChallengesCompleted > 0) {
				DependencyService.Get<INotificationMessage>().Send(
					 "checkForRewards",
					 Language.RewardsEarned,
					 Language.YouHaveEarned + " " + nChallengesCompleted + " " + Language.nRewardsClickToSeeWhat,
					 nChallengesCompleted
				);
			}
		}


		/// <summary>
		/// Timer callback. User is now eligible for rewards.
		/// </summary>
		void goToCyclingEligibleCallback(object state) {
			resetCounters();
			stateMachine.MoveNext(Command.Timeout);
			Task.Run(() => checkForRewards()).DoNotAwait();
			// Start different sound in background if needed.
			var soundPlayer = DependencyService.Get<ISoundPlayer>();
			if(soundPlayer.IsPlaying()) {
				soundPlayer.StopSound();
				soundPlayer.PlaySound(User.Instance.BycicleEligibleSoundSetting);
			}
		}


		/// <summary>
		/// Timer callback. User is now ineligible for rewards.
		/// </summary>
		void goToIneligibleCallback(object state) {
			resetCounters();
			stateMachine.MoveNext(Command.Timeout);
			timer.Dispose();
			vehicularTimer.Dispose();
			DependencyService.Get<ISoundPlayer>().PlayShortSound(User.Instance.NoLongerEligibleSoundSetting);
		}
	}
}
