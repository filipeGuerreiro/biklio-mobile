﻿using System.Collections.Generic;
using System.Linq;
using SQLite;

namespace Trace {

	/// <summary>
	/// A Challenge is issued by a particular Checkpoint.
	/// They have a 'reward', e.g., 10% discount, which is unlocked upon completing a given 'distance', e.g, cycle 2000 m.
	/// </summary>
	public class Challenge : UserItemBase {

		public string Reward { get; set; }
		public int NeededCyclingDistance { get; set; }
		[Indexed]
		public bool isComplete { get; set; }

		public long CreatedAt { get; set; }
		public long ExpiresAt { get; set; }

		public string CheckpointName { get; set; }
		[Ignore]
		public Checkpoint ThisCheckpoint { get; set; }
		public long CheckpointId { get; set; }

		// These properties are used to display information when listed in the ChallengesPage.
		[Ignore]
		public string Description { get { return Reward + " at " + CheckpointName; } }
		[Ignore]
		public string Image { get { return ThisCheckpoint?.LogoURL ?? "default_shop.png"; } }

		public override string ToString() {
			return string.Format("[Challenge Id->{0} UserId->{1} CheckpointId->{2} Reward->{3} Checkpoint->{4} Distance->{5}]",
								 Id, UserId, CheckpointId, Reward, CheckpointName, NeededCyclingDistance);
		}
	}

	/// <summary>
	/// The Challenge Visual Model is used to bind a list of challenges for display in the Challenge Page and Reward Page.
	/// </summary>
	class ChallengeVM {
		public IEnumerable<Challenge> Challenges { get; set; }
		public string Summary {
			get {
				int count = Challenges.Count();
				if(count == 0) {
					return "No challenges found.";
				}
				if(count != 1)
					return "There are " + count + " challenges near you.";
				else
					return "There is 1 challenge near you.";
			}
		}
	}
}
