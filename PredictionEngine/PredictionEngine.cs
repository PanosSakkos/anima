using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Di.Kdd.PredictionEngine
{

	public class PredictionEngine
	{
		private const string WordsFileName = "words.txt";

		private Trie trie = new Trie();
		private Dictionary<string, Statistics> knowledge = new Dictionary<string, Statistics>();
		private float personalizationFactor = 1.0F;

		private string currentWord = "";
		private int wordsTyped = 0;
		private Trie currentSubTrie;
		private bool unknownWord = false;

		public static char [] latinLetters = {
										'a', 'b', 'c', 'd', 'e', 'f', 'g',
										'h', 'i', 'j', 'k', 'l', 'm', 'n',
										'o', 'p', 'q', 'r', 's', 't', 'u', 
										'v', 'w', 'x', 'y', 'z'
									};

		public static char [] wordSeparators = { ' ', '.', ',', '!' };

		public PredictionEngine ()
		{
			this.trie.LoadWordsFromFile(WordsFileName);
			this.currentSubTrie = this.trie;
		}

		public Dictionary<char, float> GetSortedPredictions()
		{
			var predictions = this.GetPredictions();

			return predictions.OrderByDescending(kv => kv.Value).ToDictionary(k => k.Key, v => v.Value);
		}

		public Dictionary<char, float> GetPredictions()
		{
			int popularity, postfixesCounter;
			float evaluation, evaluationSum = 0.0F;

			if (this.currentSubTrie == null)
			{
				this.unknownWord = true;
				return new Dictionary<char, float>();
			}

			foreach (char possibleNextLetter in latinLetters)
			{
				popularity = this.currentSubTrie.GetPopularity(possibleNextLetter);
				postfixesCounter = this.currentSubTrie.GetSubtrieSize(possibleNextLetter);

				evaluation = this.Evaluate(popularity, postfixesCounter);

				evaluationSum += evaluation;
			}

			Dictionary<char, float> predictions = new Dictionary<char, float>();

			if (evaluationSum == 0)
			{
				foreach (char letter in latinLetters)
				{
					predictions.Add(letter, 0.0F);
				}
			}
			else
			{
				foreach (char possibleNextLetter in latinLetters)
				{
					popularity = this.currentSubTrie.GetPopularity(possibleNextLetter);
					postfixesCounter = this.currentSubTrie.GetSubtrieSize(possibleNextLetter);

					evaluation = this.Evaluate(popularity, postfixesCounter);

					predictions.Add(possibleNextLetter, evaluation / evaluationSum);
				}
			}

			return predictions;
		}

		public void LetterTyped(char letter)
		{
			if (Array.IndexOf(wordSeparators, letter) >= 0)
			{
				this.WordTyped();
				return;
			}

			this.currentWord += letter;

			if (this.currentSubTrie != null)
			{
				this.currentSubTrie = this.currentSubTrie.GetSubTrie(letter);
			}
			else
			{
				this.unknownWord = true;
			}
		}

		public void PredictionCancelled()
		{
			this.Reset();
		}

		public void Save(string fileName)
		{
			if (File.Exists(fileName))
			{
				File.Delete(fileName);
			}

			StreamWriter writer = new StreamWriter(File.OpenWrite(fileName));

			foreach (KeyValuePair<string, Statistics> data in this.knowledge)
			{
				writer.WriteLine("{0}±{1}", data.Key, data.Value);
			}

			writer.Close();
		}

		public void Load(string fileName)
		{
			if (File.Exists(fileName) == false)
			{
				return;
			}

			StreamReader reader = File.OpenText(fileName);

			string line;
			while((line = reader.ReadLine()) != null)
			{
				StringReader stringReader = new StringReader(line);
				int keyLength = line.IndexOf('±');
				char [] keyArray = new char[keyLength];

				stringReader.ReadBlock(keyArray, 0, keyLength);
				string key = new string(keyArray);

				stringReader.Read();

				Statistics statistics = new Statistics(stringReader.ReadToEnd());

				knowledge.Add(key, statistics);
			}

			reader.Close();
			this.GetTrained();
		}

		public static bool ValidLetter(char letter)
		{
			letter = Char.ToLower(letter);

			return (letter >= 'a' & letter <= 'z') || (Array.IndexOf(wordSeparators, letter) >= 0);
		}

		private void Reset()
		{
			this.currentWord = "";
			this.currentSubTrie = this.trie;
			this.unknownWord = false;
		}

		private void GetTrained()
		{
			foreach (KeyValuePair<string, Statistics> data in this.knowledge)
			{
				this.trie.WasTyped(data.Key, data.Value.GetPopularity());
				this.wordsTyped += data.Value.GetPopularity();
			}
		}

		private void WordTyped()
		{
			if (this.knowledge.ContainsKey(this.currentWord) == false)
			{
				Statistics statistics = new Statistics();
				statistics.WordTyped();
				this.knowledge.Add(this.currentWord, statistics);
			}
			else
			{
				this.knowledge[this.currentWord].WordTyped();
			}

			if (this.unknownWord == false)
			{
				this.trie.WasTyped(this.currentWord);
			}
			else
			{
				this.trie.Add(this.currentWord);
				this.AddCurrentWordToWordsFile();
			}

			this.wordsTyped++;

			this.Reset();
		}

		private float Evaluate(int popularity, int prefixesCounter)
		{
			float usageRatio = this.personalizationFactor * this.wordsTyped / this.trie.Size();

			if (usageRatio > 1)
			{
				usageRatio = 1;
			}

			return usageRatio * popularity + (1 - usageRatio) * prefixesCounter;
		}

		private void AddCurrentWordToWordsFile()
		{
			StreamWriter writer = File.AppendText(WordsFileName);
			writer.WriteLine(this.currentWord);
			writer.Close();
		}
	}
}

