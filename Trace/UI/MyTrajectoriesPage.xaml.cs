﻿using Xamarin.Forms;

namespace Trace {

	/// <summary>
	/// Page displaying the list of trajectories that the user produced.
	/// An image is displayed next to each item, indicating whether the trajectory was uploaded to the server or not.
	/// </summary>
	public partial class MyTrajectoriesPage : ContentPage {

		public MyTrajectoriesPage() {
			InitializeComponent();
			BindingContext = new TrajectoryVM { Trajectories = User.Instance.Trajectories };
		}


		/// <summary>
		/// Show the details of the specified trajectory on click.
		/// </summary>
		/// <param name="sender">Sender.</param>
		/// <param name="e">The trajectory in the listview that was clicked.</param>
		void OnSelection(object sender, SelectedItemChangedEventArgs e) {
			//((ListView)sender).SelectedItem = null; //uncomment line if you want to disable the visual selection state.
			Trajectory trajectory = ((Trajectory) e.SelectedItem);
			Navigation.PushAsync(new TrajectoryDetailsPage(trajectory));
		}
	}
}
