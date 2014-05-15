﻿using System.IO;

namespace Di.Kdd.Experiments.Twitter
{
	using System;
	using System.Collections.Generic;
	using System.Resources;

	public class DataSet
	{
		private const string DataFolder = "../../Data/";

		public List<User> Users = new List<User>();

		public DataSet ()
		{
			string[] userFiles = Directory.GetFiles(DataSet.DataFolder, "*.txt", SearchOption.TopDirectoryOnly);

			foreach (var userFile in userFiles) {
				if (userFile.EndsWith ("words.txt")) 
				{
					continue;
				}

				this.Users.Add(new User(userFile));
			}
		}

		public void Reset()
		{
			foreach (var user in this.Users) 
			{
				user.Reset ();
			}
		}
	}
}

