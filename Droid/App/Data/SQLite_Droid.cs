﻿using System;
using System.IO;
using Trace.Droid;
using Xamarin.Forms;

[assembly: Dependency(typeof(SQLite_Droid))]
namespace Trace.Droid {

	public class SQLite_Droid : SQLiteInterface {

		public SQLite.SQLiteConnection GetConnection() {
			var sqliteFilename = "TraceSQLite.db3";
			string documentsPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal); // Documents folder
			var path = Path.Combine(documentsPath, sqliteFilename);

			// Create the connection
			var conn = new SQLite.SQLiteConnection(path);

			// Return the database connection
			return conn;
		}
	}
}