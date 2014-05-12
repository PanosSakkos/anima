﻿namespace Di.Kdd.Experiments
{
	using Di.Kdd.Experiments.Twitter;
	using Di.Kdd.WriteRightSimulator;

	using System;

	public class Baseline
	{
		public Baseline ()
		{
			this.BaeysianWithPersonilization ();
		}
			
		public void BaeysianWithPersonilization()
		{
			var dataSet = new DataSet ();

			Console.WriteLine ("WriteRight with personalization");

			foreach (User user in dataSet.Users) 
			{
				var writeRight = new WriteRight();

				/* Train the engine */

				while (user.HasNext ()) 
				{
					var ch = user.ConsumeNext ();
					writeRight.CharacterTyped (ch);
				}

				user.Reset ();

				var totalChars = 0;
				var guessedChars = 0;

				while (user.HasNext ()) 
				{
					var ch = user.ConsumeNext ();
					writeRight.CharacterTyped (ch);

					var next = user.PeekNext ();
					var predictions = writeRight.GetTopKPredictions ();

					if (writeRight.IsValidCharacter (next) == false || 
						writeRight.IsWordSeparator (next) || 
						writeRight.IsIdle ()) 
					{
						continue;
					}

					if (predictions.ContainsKey (next) == false)
					{
						writeRight.BadPrediction ();
						totalChars++;
					} 
					else if (writeRight.IsWordSeparator (next) == false) 
					{
						guessedChars++;
						totalChars++;
					}
				}

				Console.WriteLine (user.GetId() + " [" + guessedChars + " out of " + totalChars + "] " + 
																(float) guessedChars / (float) totalChars);
			}

			dataSet.Reset ();
		}
	}
}

