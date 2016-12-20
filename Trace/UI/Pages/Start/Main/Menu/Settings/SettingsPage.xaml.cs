﻿using System;
using System.Diagnostics;
using Xamarin.Forms;

namespace Trace {
	public partial class SettingsPage : ContentPage {

		public SettingsPage() {
			InitializeComponent();
			//list.Add($"New Spot {DateTime.Now.ToLocalTime()}");
		}

		// TODO: need to perform input validation
		void OnSettingChanged(object sender, EventArgs e) {
			SQLiteDB.Instance.SaveItem(User.Instance);
		}

		void OnGenderChanged(object sender, EventArgs e) {
			var picker = (BindablePicker) sender;
			string item = picker.Items[picker.SelectedIndex];
			User.Instance.Gender = EnumUtil.ParseEnum<SelectedGender>(item);
			SQLiteDB.Instance.SaveItem(User.Instance);
		}

		void OnLanguageChanged(object sender, EventArgs e) {
			var picker = (BindablePicker) sender;
			string item = picker.Items[picker.SelectedIndex];
			User.Instance.UserLanguage = EnumUtil.ParseEnum<SelectedLanguage>(item);
			SQLiteDB.Instance.SaveItem(User.Instance);
			// TODO: change text UI for the new language.
		}

		async void OnDeleteCache(object sender, EventArgs args) {
			var action = await DisplayActionSheet("Warning:\n This will delete all trajectories and challenges. They will NOT be saved.", "Back", "Delete");
			switch(action) {
				case ("Delete"): SQLiteDB.Instance.DropAllTables(); break;
				default: return;
			}
		}

		// TODO OnLicensesClicked
		async void OnLicensesClicked(object sender, EventArgs e) {
			await DisplayAlert("Error", "Not yet implemented.", "Ok");
		}

		// TODO OnTermsOfServiceClicked
		async void OnTermsOfServiceClicked(object sender, EventArgs e) {
			await DisplayAlert("Error", "Not yet implemented.", "Ok");
		}

		// TODO OnPrivacyPolicyClicked
		async void OnPrivacyPolicyClicked(object sender, EventArgs e) {
			await DisplayAlert("Error", "Not yet implemented.", "Ok");
		}

		// TODO OnAboutUsClicked
		async void OnAboutUsClicked(object sender, EventArgs e) {
			await DisplayAlert("Error", "Not yet implemented.", "Ok");
		}
	}
}
