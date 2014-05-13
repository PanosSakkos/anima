﻿namespace Di.Kdd.Experiments
{
	using Di.Kdd.Experiments.Twitter;
	using Di.Kdd.WriteRightSimulator;

	using System;

	public class TimeAwareExperiment
	{
		public TimeAwareExperiment (float trainSetPercentage)
		{
			this.run (trainSetPercentage);
		}

		void run (float trainSetPercentage)
		{
			var dataSet = new DataSet ();

			Console.WriteLine ("Time Aware WriteRight");

			foreach (User user in dataSet.Users) 
			{
				var timeAwareWriteRight = new TimeAwareWriteRight(2);
				timeAwareWriteRight.LoadDB ("dummy");

				/* Train the engine */

				User trainSet = user.GetTrainSet (trainSetPercentage);

				while (trainSet.HasNext ()) 
				{
					var ch = trainSet.ConsumeNext ();
					timeAwareWriteRight.SetTime (trainSet.GetTime ());
					timeAwareWriteRight.CharacterTyped (ch);
				}

				User testSet = user.GetTestSet ();

				var totalChars = 0;
				var guessedChars = 0;

				while (testSet.HasNext ()) 
				{
					var ch = testSet.ConsumeNext ();
					timeAwareWriteRight.SetTime (testSet.GetTime ());
					timeAwareWriteRight.CharacterTyped (ch);

					var next = testSet.PeekNext ();
					timeAwareWriteRight.SetTime (testSet.PeekNextTime ());
					var predictions = timeAwareWriteRight.GetTopKPredictions ();

					if (timeAwareWriteRight.IsValidCharacter (next) == false || 
						timeAwareWriteRight.IsWordSeparator (next) || 
						timeAwareWriteRight.IsIdle ()) 
					{
						continue;
					}

					if (predictions.ContainsKey (next) == false)
					{
						timeAwareWriteRight.BadPrediction ();
						totalChars++;
					} 
					else if (timeAwareWriteRight.IsWordSeparator (next) == false) 
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
