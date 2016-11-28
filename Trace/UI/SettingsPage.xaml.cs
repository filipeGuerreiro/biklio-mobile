﻿using System;
using System.Collections.Generic;

using Xamarin.Forms;

namespace Trace {
	public partial class SettingsPage : ContentPage {
		public SettingsPage() {
			InitializeComponent();
		}

		async void OnDeleteCache(object sender, EventArgs args) {
			var action = await DisplayActionSheet("Warning:\n This will delete all trajectories and challenges. They will NOT be saved.", "Delete", "Nevermind");
			switch(action) {
				case ("Delete"): SQLiteDB.Instance.DeleteAll(); break;
				default: return;
			}
		}
	}
}
