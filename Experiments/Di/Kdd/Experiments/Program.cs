﻿namespace Di.Kdd.Experiments
{
	using System;

	using Di.Kdd.Experiments.Twitter;

	using Di.Kdd.WriteRightSimulator;

	class MainClass
	{
		public static void Main (string[] args)
		{
			float trainSetPercentage = 0.7f;

			new DictionaryExperiment (trainSetPercentage);
			new DictionaryWithPersonalization (trainSetPercentage);
			new TimeAwareExperiment (trainSetPercentage);
			new FaultyTimeExperiment (trainSetPercentage);
		}
	}
}
