#region Using Directives and Copyright Notice

// Copyright (c) 2007-2010, Computer Consultancy Pty Ltd
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:
//     * Redistributions of source code must retain the above copyright
//       notice, this list of conditions and the following disclaimer.
//     * Redistributions in binary form must reproduce the above copyright
//       notice, this list of conditions and the following disclaimer in the
//       documentation and/or other materials provided with the distribution.
//     * Neither the name of the Computer Consultancy Pty Ltd nor the
//       names of its contributors may be used to endorse or promote products
//       derived from this software without specific prior written permission.
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" 
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE 
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE 
// ARE DISCLAIMED. IN NO EVENT SHALL COMPUTER CONSULTANCY PTY LTD BE LIABLE 
// FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL 
// DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR 
// SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER 
// CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT 
// LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY 
// OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH
// DAMAGE.

#endregion

// Portions of this code were originally developed for Bit Plantation Dinero.
// (Portions Copyright © 2006 Bit Plantation)

using System;
using System.Collections.Generic;

namespace Interlace.DoubleMetaphone
{
	public class DoubleMetaphone
	{
		string _primary;
		string _secondary;

		public DoubleMetaphone()
		{
			_primary = "";
			_secondary = null;
		}

		private void Add(string both)
		{
			_primary += both;

			if (_secondary != null) _secondary += both;
		}

		private void Add(string primary, string secondary)
		{
			if (_secondary == null)
			{
				_secondary = _primary;
			}
			
			_primary += primary;
			_secondary += secondary;
		}

		private bool IsVowel(char c)
		{
			return c == 'A' || c == 'E' || c == 'I' || c == 'O' || c == 'U' || c == 'Y';
		}

		private bool IsSlavoGermanic(DoubleMetaphoneIterator word)
		{
			return word.Contains("W") || word.Contains("K") || word.Contains("CZ") || word.Contains("WITZ");
		}

		public static string[] GetPhonetics(string s)
		{
			DoubleMetaphone m = new DoubleMetaphone();
			return m.Get(s);
		}

        public static bool StringMatchesPhonetics(string s, IEnumerable<string> phoenitics)
        {
            DoubleMetaphone m = new DoubleMetaphone();
            IEnumerable<string> lhsPhoenetics = m.Get(s);
            IEnumerable<string> rhsPhoenetics = phoenitics;

            foreach (string lhsPhoenetic in lhsPhoenetics)
            {
                foreach (string rhsPhoenetic in rhsPhoenetics)
                {
                    if (rhsPhoenetic.Contains(lhsPhoenetic) && 
                        rhsPhoenetic.Length - lhsPhoenetic.Length <= 2)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public static bool StringMatchesString(string lhs, string rhs)
        {
            DoubleMetaphone m = new DoubleMetaphone();
            return StringMatchesPhonetics(lhs, m.Get(rhs));
        }

		private string[] Get(string s)
		{
			DoubleMetaphoneIterator word = new DoubleMetaphoneIterator(s);

			if (word.Length == 0) return new string[] {};

			// Skip silent letters when at the start of a word:
			word.SkipOnMatch("GN", "KN", "PN", "WR", "PS");

			// Initial 'X' is pronounced 'Z' e.g. 'Xavier'
			if (word.Letter == 'X')
			{
				Add("S"); // 'Z' maps to 'S'
				word.Advance(1);
			}

			while (!word.AtEnd)
			{
				switch (word.Letter)
				{
					case 'A':
					case 'E':
					case 'I':
					case 'O':
					case 'U':
					case 'Y':
						// All initial vowels now map to 'A':
						if (word.AtStart) Add("A"); 
						word.Advance(1);
						break;
                        
					case 'B':
						//"-mb", e.g", "dumb", already skipped over...
						Add("P");
						word.Advance(1);

						word.SkipOnMatch("B");
						break;
                        
					case 'Ç':
						Add("S");
						word.Advance(1);
						break;

					case 'C':
						//various germanic
						if (word.Position > 1
							&& !word.Matches(-2, "A", "E", "I", "O", "U", "Y")
							&& word.Matches(-1, "ACH")
							&& !word.Matches(2, "I", "E")
							|| word.Matches(-2, "BACHER", "MACHER"))
						{       
							Add("K");
							word.Advance(2);
							break;
						}

						//special case 'caesar'
						if ((word.AtStart) && word.Matches("CAESAR"))
						{
							Add("S");
							word.Advance(2);
							break;
						}

						//italian 'chianti'
						if (word.Matches("CHIA"))
						{
							Add("K");
							word.Advance(2);
							break;
						}

						if (word.Matches("CH"))
						{       
							//find 'michael'
							if ((!word.AtStart) && word.Matches("CHAE"))
							{
								Add("K", "X");
								word.Advance(2);
								break;
							}

							//greek roots e.g. 'chemistry', 'chorus'
							if ((word.AtStart)
								&& (word.Matches("CHARAC", "CHARIS") 
								|| word.Matches("CHOR", "CHYM", "CHIA", "CHEM")) 
								&& !word.StartsWith("CHORE"))
							{
								Add("K");
								word.Advance(2);
								break;
							}

							//germanic, greek, or otherwise 'ch' for 'kh' sound
							if (word.StartsWith("VAN ", "VON ", "SCH")
								// 'architect but not 'arch', 'orchestra', '||chid'
								|| word.Matches(-2, "ORCHES", "ARCHIT", "ORCHID")
								|| word.Matches(2, "T", "S")
								|| (word.Matches(-1, "A", "O", "U", "E") || word.AtStart)
								//e.g., 'wachtler', 'wechsler', but not 'tichner'
								&& (word.Matches(2, "L", "R", "N", "M", "B", "H", "F", "V", "W") || word.IsBeyondEnd(2)))
							{
								Add("K");
							}
							else
							{  
								if (!word.AtStart)
								{
									if (word.StartsWith("MC"))
										//e.g., "McHugh"
										Add("K");
									else
										Add("X", "K");
								}
								else
									Add("X");
							}

							word.Advance(2);
							break;
						}
						//e.g, 'czerny'
						if (word.Matches("CZ") && !word.Matches(-2, "WICZ"))
						{
							Add("S", "X");
							word.Advance(2);
							break;
						}

						//e.g., 'focaccia'
						if (word.Matches(1, "CIA"))
						{
							Add("X");
							word.Advance(3);
							break;
						}

						//double 'C', but not if e.g. 'McClellan'
						if (word.Matches("CC") && !(word.Position == 1 && word.StartsWith("M")))
							//'bellocchio' but not 'bacchus'
							if (word.Matches(2, "I", "E", "H") && !word.Matches(2, "HU"))
							{
								//'accident', 'accede' 'succeed'
								if ((word.Position == 1 && word.StartsWith("A")) 
									|| word.Matches(1, "UCCEE", "UCCES"))
								{
									Add("KS");
									//'bacci', 'bertucci', other italian
								}
								else
								{
									Add("X");
								}

								word.Advance(3);
								break;
							}
							else
							{//Pierce's rule
								Add("K");
								word.Advance(2);
								break;
							}

						if (word.Matches("CK", "CG", "CQ"))
						{
							Add("K");
							word.Advance(2);
							break;
						}

						if (word.Matches("CI", "CE", "CY"))
						{
							//italian vs. english
							if (word.Matches("CIO", "CIE", "CIA"))
								Add("S", "X");
							else
								Add("S");
							word.Advance(2);
							break;
						}

						//else
						Add("K");
                                
						//name sent in 'mac caffrey', 'mac gregor
						if (word.Matches(1, " C", " Q", " G"))
							word.Advance(3);
						else
							if (word.Matches(1, "C", "K", "Q") 
							&& !word.Matches(1, "CE", "CI"))
							word.Advance(2);
						else
							word.Advance(1);
						break;

					case 'D':
						if (word.Matches("DG"))
							if (word.Matches(2, "I", "E", "Y"))
							{
								//e.g. 'edge'
								Add("J");
								word.Advance(3);
								break;
							}
							else
							{
								//e.g. 'edgar'
								Add("TK");
								word.Advance(2);
								break;
							}

						if (word.Matches("DT", "DD"))
						{
							Add("T");
							word.Advance(2);
							break;
						}
                                
						//else
						Add("T");
						word.Advance(1);
						break;

					case 'F':
						if (word.NextLetter == 'F')
							word.Advance(2);
						else
							word.Advance(1);
						Add("F");
						break;

					case 'G':
						if (word.NextLetter == 'H')
						{
							if ((!word.AtStart) && !IsVowel(word.LastLetter))
							{
								Add("K");
								word.Advance(2);
								break;
							}

							if (word.Position < 3)
							{
								//'ghislane', ghiradelli
								if (word.AtStart)
								{ 
									if (word.Matches(2, "I"))
										Add("J");
									else
										Add("K");
									word.Advance(2);
									break;
								}
							}
							//Parker's rule (with some further refinements) - e.g., 'hugh'
							if (word.Matches(-2, "B", "H", "D") 
								|| word.Matches(-3, "B", "H", "D")
								|| word.Matches(-4, "B", "H"))
							{
								word.Advance(2);
								break;
							}
							else
							{
								//e.g., 'laugh', 'McLaughlin', 'cough', 'gough', 'rough', 'tough'
								if (word.Position > 2 
									&& word.Matches(-1, "U")
									&& word.Matches(-3, "C", "G", "L", "R", "T"))
								{
									Add("F");
								}
								else
									if (!word.AtStart && word.Matches(-1, "I")) Add("K");

								word.Advance(2);
								break;
							}
						}

						if (word.NextLetter == 'N')
						{
							if ((word.Position == 1) && IsVowel(word.FirstLetter) && !IsSlavoGermanic(word))
							{
								Add("KN", "N");
							}
							else
								//not e.g. 'cagney'
								if (!word.Matches(2, "EY") && (word.NextLetter != 'Y') && !IsSlavoGermanic(word))
							{
								Add("N", "KN");
							}
							else
								Add("KN");
							word.Advance(2);
							break;
						}

						//'tagliaro'
						if (word.Matches(1, "LI") && !IsSlavoGermanic(word))
						{
							Add("KL", "L");
							word.Advance(2);
							break;
						}

						//-ges-,-gep-,-gel-, -gie- at beginning
						if ((word.AtStart)
							&& ((word.NextLetter == 'Y') 
							|| word.Matches(1, "ES", "EP", "EB", "EL", "EY", "IB", "IL", "IN", "IE", "EI", "ER")) )
						{
							Add("K", "J");
							word.Advance(2);
							break;
						}

						// -ger-,  -gy-
						if ((word.Matches(1, "ER") || (word.NextLetter == 'Y'))
							&& !word.StartsWith("DANGER", "RANGER", "MANGER")
							&& !word.Matches(-1, "E", "I") 
							&& !word.Matches(-1, "RGY", "OGY") )
						{
							Add("K", "J");
							word.Advance(2);
							break;
						}

						// italian e.g, 'biaggi'
						if (word.Matches(1, "E", "I", "Y") || word.Matches(-1, "AGGI", "OGGI"))
						{
							//obvious germanic
							if (word.StartsWith("VAN ", "VON ") || word.StartsWith("SCH") || word.Matches(1, "ET"))
								Add("K");
							else
								//always soft if french ending
								if (word.Matches(1, "IER ")) Add("J");
							else
								Add("J", "K");
							word.Advance(2);
							break;
						}

						if (word.NextLetter == 'G')
							word.Advance(2);
						else
							word.Advance(1);
						Add("K");
						break;

					case 'H':
						//only keep if first & before vowel or btw. 2 vowels
						if (((word.AtStart) || IsVowel(word.LastLetter)) 
							&& IsVowel(word.NextLetter))
						{
							Add("H");
							word.Advance(2);
						}
						else//also takes care of 'HH'
							word.Advance(1);
						break;

					case 'J':
						//obvious spanish, 'jose', 'san jacinto'
						if (word.Matches("JOSE") || word.StartsWith("SAN ") )
						{
							if ((word.AtStart && word.IsBeyondEnd(4)) || word.StartsWith("SAN "))
								Add("H");
							else
							{
								Add("J", "H");
							}
							word.Advance(1);
							break;
						}

						if ((word.AtStart) && !word.Matches("JOSE"))
							Add("J", "A");//Yankelovich/Jankelowicz
						else
							//spanish pron. of e.g. 'bajador'
							if (IsVowel(word.LastLetter) 
							&& !IsSlavoGermanic(word)
							&& ((word.NextLetter == 'A') || (word.NextLetter == 'O')))
							Add("J", "H");
						else
							if (word.AtLastLetter) Add("J", " ");
						else
							if (!word.Matches(1, "L", "T", "K", "S", "N", "M", "B", "Z") 
							&& !word.Matches(-1, "S", "K", "L"))
							Add("J");

						if (word.NextLetter == 'J')//it could happen!
							word.Advance(2);
						else
							word.Advance(1);
						break;

					case 'K':
						if (word.NextLetter == 'K')
							word.Advance(2);
						else
							word.Advance(1);
						Add("K");
						break;

					case 'L':
						if (word.NextLetter == 'L')
						{
							//spanish e.g. 'cabrillo', 'gallegos'
							if (((word.Position == word.Letter - 3) 
								&& word.Matches(-1, "ILLO", "ILLA", "ALLE"))
								|| (word.EndsWith("AS", "OS", "A", "O") 
								&& word.Matches(-1, "ALLE")))
							{
								Add("L", " ");
								word.Advance(2);
								break;
							}
							word.Advance(2);
						}
						else
							word.Advance(1);
						Add("L");
						break;

					case 'M':
						if ((word.Matches(-1, "UMB") 
							&& ((word.Position + 1 == word.Length - 1) || word.Matches(2, "ER")))
							//'dumb','thumb'
							|| word.NextLetter == 'M')
							word.Advance(2);
						else
							word.Advance(1);
						Add("M");
						break;

					case 'N':
						if (word.NextLetter == 'N')
							word.Advance(2);
						else
							word.Advance(1);
						Add("N");
						break;

					case 'Ñ':
						word.Advance(1);
						Add("N");
						break;

					case 'P':
						if (word.NextLetter == 'H')
						{
							Add("F");
							word.Advance(2);
							break;
						}

						//also account for "campbell", "raspberry"
						if (word.Matches(1, "P", "B"))
							word.Advance(2);
						else
							word.Advance(1);
						Add("P");
						break;

					case 'Q':
						if (word.NextLetter == 'Q')
							word.Advance(2);
						else
							word.Advance(1);
						Add("K");
						break;

					case 'R':
						//french e.g. 'rogier', but exclude 'hochmeier'
						if ((word.Position == word.Length - 1)
							&& !IsSlavoGermanic(word)
							&& word.Matches(-2, "IE") 
							&& !word.Matches(-4, "ME", "MA"))
							Add("", "R");
						else
							Add("R");

						if (word.NextLetter == 'R')
							word.Advance(2);
						else
							word.Advance(1);
						break;

					case 'S':
						//special cases 'isl&&', 'isle', 'carlisle', 'carlysle'
						if (word.Matches(-1, "ISL", "YSL"))
						{
							word.Advance(1);
							break;
						}

						//special case 'sugar-'
						if ((word.AtStart) && word.Matches("SUGAR"))
						{
							Add("X", "S");
							word.Advance(1);
							break;
						}

						if (word.Matches("SH"))
						{
							//germanic
							if (word.Matches(1, "HEIM", "HOEK", "HOLM", "HOLZ"))
								Add("S");
							else
								Add("X");
							word.Advance(2);
							break;
						}

						//italian & armenian
						if (word.Matches("SIO", "SIA") || word.Matches("SIAN"))
						{
							if (!IsSlavoGermanic(word))
								Add("S", "X");
							else
								Add("S");
							word.Advance(3);
							break;
						}

						//german & anglicisations, e.g. 'smith' match 'schmidt', 'snider' match 'schneider'
						//also, -sz- in slavic language altho in hungarian it is pronounced 's'
						if (((word.AtStart) 
							&& word.Matches(1, "M", "N", "L", "W"))
							|| word.Matches(1, "Z"))
						{
							Add("S", "X");
							if (word.Matches(1, "Z"))
								word.Advance(2);
							else
								word.Advance(1);
							break;
						}

						if (word.Matches("SC"))
						{
							//Schlesinger's rule
							if (word.Matches(2, "H"))
								//dutch origin, e.g. 'school', 'schooner'
								if (word.Matches(3, "OO", "ER", "EN", "UY", "ED", "EM"))
								{
									//'schermerhorn', 'schenker'
									if (word.Matches(3, "ER", "EN"))
									{
										Add("X", "SK");
									}
									else
										Add("SK");
									word.Advance(3);
									break;
								}
								else
								{
									if ((word.AtStart) && !word.Matches(3, "A", "E", "I", "O", "U", "Y") && (!word.Matches(3, "W")))
										Add("X", "S");
									else
										Add("X");
									word.Advance(3);
									break;
								}

							if (word.Matches(2, "I", "E", "Y"))
							{
								Add("S");
								word.Advance(3);
								break;
							}
							//else
							Add("SK");
							word.Advance(3);
							break;
						}

						//french e.g. 'resnais', 'artois'
						if (word.AtLastLetter && word.Matches(-2, "AI", "OI"))
							Add("", "S");
						else
							Add("S");

						if (word.Matches(1, "S", "Z"))
							word.Advance(2);
						else
							word.Advance(1);
						break;

					case 'T':
						if (word.Matches("TION"))
						{
							Add("X");
							word.Advance(3);
							break;
						}

						if (word.Matches("TIA", "TCH"))
						{
							Add("X");
							word.Advance(3);
							break;
						}

						if (word.Matches("TH") 
							|| word.Matches("TTH"))
						{
							//special case 'thomas', 'thames' || germanic
							if (word.Matches(2, "OM", "AM") 
								|| word.StartsWith("VAN ", "VON ") 
								|| word.StartsWith("SCH"))
							{
								Add("T");
							}
							else
							{
								Add("0", "T");
							}
							word.Advance(2);
							break;
						}

						if (word.Matches(1, "T", "D"))
							word.Advance(2);
						else
							word.Advance(1);
						Add("T");
						break;

					case 'V':
						if (word.NextLetter == 'V')
							word.Advance(2);
						else
							word.Advance(1);
						Add("F");
						break;

					case 'W':
						//can also be in middle of word
						if (word.Matches("WR"))
						{
							Add("R");
							word.Advance(2);
							break;
						}

						if ((word.AtStart) 
							&& (IsVowel(word.NextLetter) || word.Matches("WH")))
						{
							//Wasserman should match Vasserman
							if (IsVowel(word.NextLetter))
								Add("A", "F");
							else
								//need Uomo to match Womo
								Add("A");
						}

						//Arnow should match Arnoff
						if ((word.AtLastLetter && IsVowel(word.LastLetter)) 
							|| word.Matches(-1, "EWSKI", "EWSKY", "OWSKI", "OWSKY") 
							|| word.StartsWith("SCH"))
						{
							Add("", "F");
							word.Advance(1);
							break;
						}

						//polish e.g. 'filipowicz'
						if (word.Matches("WICZ", "WITZ"))
						{
							Add("TS", "FX");
							word.Advance(4);
							break;
						}

						//else skip it
						word.Advance(1);
						break;

					case 'X':
						//french e.g. breaux
						if (!(word.AtLastLetter 
							&& (word.Matches(-3, "IAU", "EAU") 
							|| word.Matches(-2, "AU", "OU"))) )
							Add("KS");

						if (word.Matches(1, "C", "X"))
							word.Advance(2);
						else
							word.Advance(1);
						break;

					case 'Z':
						//chinese pinyin e.g. 'zhao'
						if (word.NextLetter == 'H')
						{
							Add("J");
							word.Advance(2);
							break;
						}
						else
							if (word.Matches(1, "ZO", "ZI", "ZA") 
							|| (IsSlavoGermanic(word) && ((!word.AtStart) && !word.Matches(-1, "T"))))
						{
							Add("S", "TS");
						}
						else
							Add("S");

						if (word.NextLetter == 'Z')
							word.Advance(2);
						else
							word.Advance(1);
						break;

					default:
						word.Advance(1);
						break;
				}
			}

			if (_secondary == null)
			{
				return new string[] { _primary };
			}
			else
			{
				return new string[] { _primary, _secondary };
			}
		}
	}
}
