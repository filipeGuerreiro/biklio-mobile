﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SQLite;
using Xamarin.Auth;
using Xamarin.Forms;
using Trace.Localization;
using System.Diagnostics;

namespace Trace {
	/// <summary>
	/// Page used for checking user credentials and entering the application.
	/// </summary>
	public partial class SignInPage : ContentPage {

		private bool isRememberMe;
		static INavigation NavPage;


		public SignInPage() {
			InitializeComponent();
			NavPage = Navigation;
			isRememberMe = true;
			// TODO this gets the first username available, but if there are multiple, it will still pick always the first, ideally, the last one used would show up first
			usernameText.Text = DependencyService.Get<DeviceKeychainInterface>().Username;
			passwordText.Text = DependencyService.Get<DeviceKeychainInterface>().GetPassword(usernameText.Text);
		}

		void OnUsernameInput(object sender, EventArgs e) {
			passwordText.Text = DependencyService.Get<DeviceKeychainInterface>().GetPassword(((Entry) sender).Text);
		}

		async void OnLogin(object sender, EventArgs e) {
			var username = usernameText.Text;
			var password = passwordText.Text;

			if(username == null || password == null) {
				await DisplayAlert(Language.Error, Language.FillEveryField, Language.Ok);
				return;
			}

			//var client = new WebServerClient();
			//WSResult result = await Task.Run(() => client.LoginWithCredentials(username, password));

			// Perform credentials validation against the local keystore/keychain.
			bool doesUsernameExist = DependencyService.Get<DeviceKeychainInterface>().Exists(username);
			string storedPassword = DependencyService.Get<DeviceKeychainInterface>().GetPassword(username);
			bool doesPasswordMatch = storedPassword != null && storedPassword.Equals(password);
			if(!doesUsernameExist || !doesPasswordMatch) {
				await DisplayAlert(Language.Warning, Language.LocalIncorrectCredentialsWarning, Language.Yes, Language.No);
				await LoginManager.TryLogin(isCredentialsLogin: true);
				if(!LoginManager.IsLoginVerified) {
					await DisplayAlert(Language.Error, Language.LoginError, Language.Ok);
					return;
				}
			}

			//if(result.success) {
			// Remember me => Store credentials in keychain.
			LoginManager.IsRememberMe = isRememberMe;
			if(isRememberMe)
				DependencyService.Get<DeviceKeychainInterface>().SaveCredentials(username, password);
			else
				DependencyService.Get<DeviceKeychainInterface>().DeleteCredentials(username);

			// Fetch user information from the database.
			SQLiteDB.Instance.InstantiateUser(username);
			// TODO verify if authtoken is used besides fb and google login -> User.Instance.AuthToken = result.token;
			//SQLiteDB.Instance.SaveItem(User.Instance);

			// Used later for background login when user has internet connection.
			User.Instance.Password = password;

			LoginManager.TryLogin(isCredentialsLogin: true).DoNotAwait();

			Application.Current.MainPage = new MainPage();
			//}
			//else
			//	await DisplayAlert("Error", result.error, "Ok");
		}


		void OnGoogleLogin(object sender, EventArgs e) {
			// Use a custom renderer to display the Google auth UI
			OAuthConfigurationManager.SetConfig(new GoogleOAuthConfig());
			Navigation.PushModalAsync(new GoogleOAuthUIPage());
		}


		void OnFacebookLogin(object sender, EventArgs e) {
			// Use a custom renderer to display the Facebook auth UI
			OAuthConfigurationManager.SetConfig(new FacebookOAuthConfig());
			Navigation.PushModalAsync(new FacebookOAuthUIPage());
		}


		/// <summary>
		/// If the OAuthLogin is successful, finish the login by sending the token to Web Server.
		/// </summary>
		public static Action SuccessfulOAuthLoginAction {
			get {
				return new Action(async () => {

					LoginManager.TryLogin(isCredentialsLogin: false).DoNotAwait();

					//var client = new WebServerClient();
					//WSResult result = await Task.Run(() => client.LoginWithToken(User.Instance.AuthToken));
					//if(result.success) {
					//	User.Instance.Name = result.payload.name;
					//	User.Instance.Email = result.payload.email;
					//	User.Instance.PictureURL = result.payload.picture;
					//	SQLiteDB.Instance.SaveItem(User.Instance);
					//await navigation.PopModalAsync();

					//Application.Current.MainPage = new MainPage();

					// Leave OAuth provider page.
					Debug.WriteLine("ModalPage - start: " + NavPage.ModalStack.FirstOrDefault().GetType().Name);
					Debug.WriteLine("Page - start: " + NavPage.NavigationStack.FirstOrDefault().GetType().Name);
					//NavPage.PopModalAsync();

					//Application.Current.MainPage = new MainPage();
					//Debug.WriteLine("ModalPage after OAuth: " + NavPage.ModalStack.FirstOrDefault().GetType().Name);
					//Debug.WriteLine("Page after OAuth: " + NavPage.NavigationStack.FirstOrDefault().GetType().Name);

					// Set root page as MainPage, so user can't use Back button to leave app.
					//NavPage.InsertPageBefore(new MainPage(), NavPage.NavigationStack.First());

					// Remove all pages from the navigation stack but the MainPage.
					//NavPage.PopToRootAsync();

					// HACK sorry, spent 4 hours trying to make this pretty. I gave up. 
					// If you remove this, the Android app crashes at this point.
					await Task.Delay(1000);
					Application.Current.MainPage = new MainPage();
				});
			}
		}


		/// <summary>
		/// If the OAuthLogin is unsuccessful, simply come back to sign-in page.
		/// </summary>
		public static Action UnsuccessfulOAuthLoginAction {
			get {
				return new Action(() => {
					NavPage.PopModalAsync();
					NavPage.NavigationStack.First().DisplayAlert(Language.Error, Language.OAuthError, Language.Ok);
				});
			}
		}


		void OnRememberMe(object sender, EventArgs e) {
			isRememberMe = !isRememberMe;
		}
	}
}