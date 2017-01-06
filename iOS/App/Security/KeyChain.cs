﻿using System.Diagnostics;
using Xamarin.Auth;

[assembly: Xamarin.Forms.Dependency(typeof(Trace.iOS.KeyChain))]
namespace Trace.iOS {

	public class KeyChain : DeviceKeychainInterface {

		public string Username {
			get {
				var iterator = AccountStore.Create().FindAccountsForService(App.AppName).GetEnumerator();
				return iterator.MoveNext() ? iterator.Current.Username : null;
			}
		}

		public string Password {
			get {
				var iterator = AccountStore.Create().FindAccountsForService(App.AppName).GetEnumerator();
				return iterator.MoveNext() ? iterator.Current.Properties["Password"] : null;
			}
		}

		public string GetPassword(string username) {
			var accounts = AccountStore.Create().FindAccountsForService(App.AppName);
			foreach(Account a in accounts) {
				Debug.WriteLine("GetPassword(): " + username);
				if(a.Username.Equals(username))
					return a.Properties["Password"];
			}
			return null;
		}

		public bool Exists(string username) {
			var accounts = AccountStore.Create().FindAccountsForService(App.AppName);
			foreach(Account a in accounts) {
				if(a.Username.Equals(username))
					return true;
			}
			return false;
		}

		public void SaveCredentials(string username, string password) {
			if(!string.IsNullOrWhiteSpace(username) && !string.IsNullOrWhiteSpace(password)) {
				var account = new Account {
					Username = username
				};
				account.Properties.Add("Password", password);
				AccountStore.Create().Save(account, App.AppName);
			}
		}

		public void DeleteAllCredentials() {
			foreach(var a in AccountStore.Create().FindAccountsForService(App.AppName)) {
				Debug.WriteLine("DeleteAllCredentials(): " + a.Username);
				AccountStore.Create().Delete(a, App.AppName);
			}
		}

		public void DeleteCredentials(string username) {
			var accounts = AccountStore.Create().FindAccountsForService(App.AppName);
			foreach(Account a in accounts) {
				if(a.Username.Equals(username)) {
					AccountStore.Create().Delete(a, App.AppName);
				}
			}
		}
	}
}
