﻿using System;
using System.Threading.Tasks;
using Trace.Localization;
using Xamarin.Forms;

namespace Trace {
	public class MainPage : MasterDetailPage {
		HomeMasterPage masterPage;

		public MainPage() {
			masterPage = new HomeMasterPage();
			Master = masterPage;
			Detail = new NavigationPage(new HomePage());

			masterPage.ListView.ItemSelected += OnItemSelected;
		}

		async void OnItemSelected(object sender, SelectedItemChangedEventArgs e) {
			RewardEligibilityManager.Instance.Input(ActivityType.Cycling); // TODO remove!
			var item = e.SelectedItem as MasterPageItem;
			if(item != null) {
				// If 'logout' was clicked.
				if(item.TargetType.Equals(typeof(SignInPage))) {
					await logout(item);
				}
				// Else load another page from the menu.
				else {
					Detail = new NavigationPage((Page) Activator.CreateInstance(item.TargetType));
					masterPage.ListView.SelectedItem = null;
					IsPresented = false;
				}
			}
		}

		async Task logout(MasterPageItem item) {
			bool isLogout = await DisplayAlert(Language.Logout, Language.AreYouSure, Language.Yes, Language.No);
			await DisplayAlert("Activity Results", App.DEBUG_ActivityLog, "Ok");
			App.DEBUG_ActivityLog = "";
			if(isLogout) {
				await LoginManager.PrepareLogout();
				Application.Current.MainPage = new NavigationPage((Page) Activator.CreateInstance(item.TargetType));
			}
		}
	}
}
