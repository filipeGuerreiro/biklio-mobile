﻿using System;
using System.Collections.Generic;
using Xamarin.Forms;

namespace Trace {
	public partial class FirstPage : ContentPage {
		public FirstPage() {
			InitializeComponent();
		}

		async void OnRegister(object sender, EventArgs e) {
			await Navigation.PushAsync(new RegistrationPage());
		}

		async void OnLogin(object sender, EventArgs e) {
			await Navigation.PushAsync(new SignInPage());
		}

		async void OnSkip(object sender, EventArgs e) {
			User.Instance.Username = "test";
			User.Instance.Email = "test@provider.com";
			//User.Instance.Trajectories = new List<Trajectory>();
			new SQLiteDB();
			await Navigation.PushAsync(new HomePage());
		}
	}
}
