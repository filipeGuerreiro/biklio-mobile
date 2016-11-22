﻿using Xamarin.Forms;

namespace Trace {
	public partial class App : Application {

		public static string AppName { get { return "Trace"; } }

		public App() {
			InitializeComponent();

			MainPage = new NavigationPage(new FirstPage());
		}

		protected override void OnStart() {
			// Handle when your app starts
		}

		protected override void OnSleep() {
			// Handle when your app sleeps
		}

		protected override void OnResume() {
			// Handle when your app resumes
		}
	}
}
