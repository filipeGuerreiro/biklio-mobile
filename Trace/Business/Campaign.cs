﻿using System.Collections.Generic;
using Trace.Localization;

namespace Trace {
	/// <summary>
	/// A Campaign is originated by a Manager that wants to send notifications to the user.
	/// The user may choose to subscribe and unsubscribe from these notifications at any time.
	/// </summary>
	public class Campaign : UserItemBase {

		public string Name { get; set; }

		public string Description { get; set; }

		public bool IsSubscribed { get; set; }

		public string ImageURL { get; set; }

		public string Website { get; set; }

		public long Start { get; set; }
		public long End { get; set; }

		// Bounding box
		public float NElongitude { get; set; }
		public float NElatitude { get; set; }

		public float SWlongitude { get; set; }
		public float SWlatitude { get; set; }
	}


	/// <summary>
	/// The Campaign Visual Model is used to bind the lists of campaigns for display in the Campaigns Page.
	/// </summary>
	class CampaignVM {
		public IList<Campaign> NewCampaigns { get; set; }
		public IList<Campaign> RegisteredCampaigns { get; set; }

		// TODO strings are for challenges, replace.
		public string Summary {
			get {
				int count = NewCampaigns.Count + RegisteredCampaigns.Count;
				if(count == 0) {
					return Language.NoChallengesFound;
				}
				if(count != 1)
					return Language.ThereAre + " " + count + " " + Language.ChallengesNear;
				else
					return Language.OneChallengeFound;
			}
		}
	}
}
