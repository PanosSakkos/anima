using System;
using System.Collections.Generic;
using System.IO;

namespace Di.Kdd.TextPrediction
{
	public class Trie
	{
		private int size = 1;
		private int popularity = 0;
		private Dictionary<char, Trie> subTries;

		public Trie ()
		{
			this.subTries = new Dictionary<char, Trie>();

			foreach (var letter in PredictionEngine.latinLetters)
			{
				this.subTries.Add(letter, null);
			}
		}

		public void Add(string word)
		{
			word = word.ToLower();

			if (this.ValidWord(word) == false)
			{
				return;
			}

			this.InnerAdd(word);
		}

		public void LoadWordsFromFile(string fileName)
		{
			StreamReader reader = File.OpenText(fileName);

			var word = "";

			while ((word = reader.ReadLine()) != null)
			{
				this.Add(word);
			}

			reader.Close();
		}

		public void Clear()
		{
			subTries.Clear();
		}

		public int GetSubtrieSize(char letter)
		{
			letter = Char.ToLower(letter);

			return this.subTries[letter] != null ? this.subTries[letter].size : 0;
		}

		public int Size()
		{
			return this.size;
		}

		public void WasTyped(string word)
		{
			this.WasTyped(word, 1);
		}


		public void WasTyped(string word, int times)
		{
			if (String.IsNullOrEmpty(word))
			{
				this.popularity += times;
				return;
			}

			this.popularity += times;

			word = word.ToLower();

			var firstChar = word[0];
			var postfix = word.Substring(1);

			this.subTries[firstChar].WasTyped(postfix, times);
		}

		public Trie GetSubTrie(char letter)
		{
			letter = Char.ToLower(letter);

			return this.subTries[letter] != null ? this.subTries[letter] : null;
		}

		public int GetPopularity(char letter)
		{
			letter = Char.ToLower(letter);

			return this.subTries[letter] != null ? this.subTries[letter].GetPopularity() : 0;
		}

		public bool Search(string word)
		{
			if (String.IsNullOrEmpty(word))
			{
				return true;
			}

			var firstLetter = word[0];

			if (this.subTries[firstLetter] == null)
			{
				return false;
			}
			else
			{
				var postfix = word.Substring(1);
				return this.subTries[firstLetter].Search(postfix);
			}
		}

		private bool ValidWord(string word)
		{
			foreach (var letter in word)
			{
				if (Array.IndexOf(PredictionEngine.latinLetters, letter) < 0)
				{
					return false;
				}
			}

			return true;
		}

		private void InnerAdd(string word)
		{
			if (String.IsNullOrEmpty(word))
			{
				return;
			}

			var firstLetter = word[0];
			var postfix = word.Substring(1);

			if (this.subTries[firstLetter] == null)
			{
				this.subTries[firstLetter] = new Trie();
			}

			this.subTries[firstLetter].InnerAdd(postfix);
			this.size++;
		}

		private int GetPopularity()
		{
			return this.popularity;
		}
	}
}

