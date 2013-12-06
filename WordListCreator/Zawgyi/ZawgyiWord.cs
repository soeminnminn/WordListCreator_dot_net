﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Unicode
{
    /**
     * This class exists to help resolve a series of issues with Zawgyi:
     *  1) Many zero-width characters in a row (1039+1039) might be illegal and difficult to detect visually.
     *  2) When a sequence of zero-width characters IS valid, a strict ordering is imposed to prevent
     *     having one sequence treated as two distinct ones.
     *  3) Zawgyi illegally disunifies several characters. Even getting past that, Burmese with no dialects
     *     disunifies short/long "ar". This class can detect such collisions, and deal with them
     *     when necessary (e.g., for sorting).
     * @author Seth N. Hetu
     */
    public class ZawgyiWord : IComparable<ZawgyiWord>
    {
        #region Variables
        //Regexes
        //public static Pattern consonantRegex = Pattern.compile("([\\u1000-\\u1021[\\u108F[\\u104C-\\u104F]]])[^\\u1039]");
        //public static Pattern medialRegex = Pattern.compile("((\\-\\u103A)|(\\u103B\\-)|(\\u107E\\-)|(\\-\\u103C)|(\\-\\u103D)|(\\-\\u103C\\u103A)|(\\-\\u103D\\u103A)|(\\u1081\\-\\u103D)|(\\u1082\\-\\u103D)|(\\-\\u108A)|(\\u1081\\-\\u108A)|(\\u1082\\-\\u108A))");

        //The actual string the user entered
        private string m_rawText = null;

        //Sorting elements
        private int m_sortNumeral = 0;
        private int m_sortConsonant = 0;
        private int m_sortMedial = 0;
        private int m_sortFinal = 0;
        private int m_sortVowel = 0;
        private int m_sortTone = 0;
        private string m_unknownBit = "";

        //private static int count = 0;

        private string m_data = "";
        #endregion

        #region Constructor/Destructor
        public ZawgyiWord(string zawgyiText)
        {
            //Before anything, re-order the string where necessary.
            ZawgyiWord.ProperlyOrder(zawgyiText);

            //Even invalid sequences still have representable text.
            this.m_rawText = zawgyiText;

            //Errors?
            if (ZawgyiWord.IsInError(zawgyiText))
                return;

            //Handle independent vowels and special contractions
            string[] independentVowels = new string[] { "\u1023", "\u1024", "\u1025", "\u1026", "\u1025\u102E", "\u1027", "\u1029", "\u107E\u101E", "\u102A", "\u1031\u107E\u101E\u102C\u1039" };
            string[] collationForms = new string[] { "\u1021\u102D", "\u1021\u102E", "\u1021\u102F", "\u1021\u1030", "\u1021\u1030", "\u1021\u1031", "\u1031\u1021\u102C", "\u1031\u1021\u102C", "\u1031\u1021\u102C\u1039", "\u1031\u1021\u102C\u1039" };

            for (int i = 0; i < independentVowels.Length; i++)
            {
                if (this.m_rawText.Contains(independentVowels[i]) && (!independentVowels[i].Equals("\u1025") || !this.m_rawText.Contains("\u1025\u1039")))
                {
                    this.SegmentWord(this.m_rawText.Replace(independentVowels[i], collationForms[i]));
                    break;
                }
                else if (i == independentVowels.Length - 1)
                {
                    if (this.m_rawText.Equals("\u1031\u101A\u102C\u1000\u1039\u103A\u102C\u1038"))
                        this.SegmentWord("\u101A\u102C\u1000\u1039\u1000\u103A\u102C\u1038");
                    else if (this.m_rawText.Equals("\u1000\u103C\u103A\u108F\u102F\u1039\u1015\u1039"))
                        this.SegmentWord("\u1000\u103C\u103A\u108F\u102F\u1014\u102F\u1015\u1039");
                    else
                        this.SegmentWord(this.m_rawText);
                }
            }
        }
        #endregion

        #region Override Methods
        /**
         * Returns the raw string, as typed by the user.
         */
        public override string ToString()
        {
            return this.m_rawText;
        }
        #endregion

        #region Private Methods
        /**
	     * Populates consonant/medial/vowel etc. fields
	     * @param text
	     */
        private void SegmentWord(string text)
        {
            //System.out.println(count++ + " " + printMM(text));

            //First, merge all characters to one representation
            text = this.UnifyText(text);

            //Special case for WZ: allow "-" as a consonant
            text = text.Replace("\\-", "");

            //Figure out any numbers...? a bit hackish...
            text = this.ExtractNumeral(text);

            //Figure out the consonant, replace with a "-"
            text = this.ExtractConsonant(text);

            //Segment the medial next
            text = this.ExtractMedial(text);

            //Segment vowell
            text = this.ExtractVowel(text);

            //Segment tone BEFORE final to prevent weird glitches
            text = this.ExtractTone(text);

            //Segment final
            text = this.ExtractFinal(text);

            //Finally
            if (text.Length > 0)
            {
                this.m_unknownBit = text;
            }
        }

        private string ExtractNumeral(string text)
        {
            StringBuilder sb = new StringBuilder();
            this.m_sortNumeral = -1;
            for (int i = 0; i < text.Length; i++)
            {
                if (text[i] >= '\u1040' && text[i] <= '\u1049')
                {
                    if (m_sortNumeral != -1)
                        throw new RuntimeException("Word can contain no more than one numeral: " + PrintMM(text));
                    this.m_sortNumeral = text[i] - '\u1040';
                }
                else
                    sb.Append(text[i]);
            }

            return sb.ToString();
        }

        private string ExtractTone(string text)
        {
            StringBuilder sb = new StringBuilder();
            bool foundDotBelow = false;
            bool foundVisarga = false;

            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];
                if (c == '\u1037')
                {
                    if (foundDotBelow)
                        throw new RuntimeException("Already found (" + ((int)c).ToString("X") + ") in " + PrintMM(text));
                    else
                        foundDotBelow = true;
                }
                else if (c == '\u1038')
                {
                    if (foundVisarga)
                        throw new RuntimeException("Already found (" + ((int)c).ToString("X") + ") in " + PrintMM(text));
                    else
                        foundVisarga = true;
                }
                else
                    sb.Append(c);
            }

            if (foundDotBelow && foundVisarga)
                this.m_sortTone = 3;
            else if (foundDotBelow)
                this.m_sortTone = 1;
            else if (foundVisarga)
                this.m_sortTone = 2;

            return sb.ToString();
        }

        private string ExtractFinal(string text)
        {
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];

                //Special case for kinzi
                if (c == '\u1064')
                {
                    if (this.m_sortFinal != 0)
                    {
                        throw new RuntimeException("Mixed final found on " + PrintMM(text));
                    }
                    else
                        this.m_sortFinal = 6;
                }

                //One exception, then proceed as normal, looking for killed characters
                int finalVal = 0;
                if (c == '\u1025' && (i < text.Length - 1 && text[i + 1] == '\u1039'))
                    c = '\u1009';
                if ((c >= '\u1000' && c <= '\u1020') && (i < text.Length - 1 && text[i + 1] == '\u1039'))
                {
                    finalVal = c - '\u1000' + 1;
                    i++; //Skip asat
                }
                else
                    sb.Append(c);

                //Set it
                if (finalVal != 0 && m_sortFinal != 0)
                    throw new RuntimeException("Final already set on : " + PrintMM(text));
                this.m_sortFinal = finalVal;
            }

            return sb.ToString();
        }

        private string ExtractVowel(string text)
        {
            StringBuilder sb = new StringBuilder();
            bool foundAa = false;
            bool foundI = false;
            bool foundIi = false;
            bool foundU = false;
            bool foundUu = false;
            bool foundE = false;
            bool foundAi = false;
            bool foundKilledAa = false;
            bool foundAnusvara = false;

            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];
                if (c == '\u102C')
                {
                    if (i < text.Length - 1 && text[i + 1] == '\u1039')
                    {
                        if (foundKilledAa)
                            throw new RuntimeException("Already found (" + ((int)c).ToString("X") + ") in " + PrintMM(text));
                        else
                            foundKilledAa = true;
                        i++; //Skip asat
                    }
                    else
                    {
                        if (foundAa)
                            throw new RuntimeException("Already found (" + ((int)c).ToString("X") + ") in " + PrintMM(text));
                        else
                            foundAa = true;
                    }
                }
                else if (c == '\u102D')
                {
                    if (foundI)
                        throw new RuntimeException("Already found (" + ((int)c).ToString("X") + ") in " + PrintMM(text));
                    else
                        foundI = true;
                }
                else if (c == '\u102F')
                {
                    if (foundU)
                        throw new RuntimeException("Already found (" + ((int)c).ToString("X") + ") in " + PrintMM(text));
                    else
                        foundU = true;
                }
                else if (c == '\u1030')
                {
                    if (foundUu)
                        throw new RuntimeException("Already found (" + ((int)c).ToString("X") + ") in " + PrintMM(text));
                    else
                        foundUu = true;
                }
                else if (c == '\u1031')
                {
                    if (foundE)
                        throw new RuntimeException("Already found (" + ((int)c).ToString("X") + ") in " + PrintMM(text));
                    else
                        foundE = true;
                }
                else if (c == '\u1036')
                {
                    if (foundAnusvara)
                        throw new RuntimeException("Already found (" + ((int)c).ToString("X") + ") in " + PrintMM(text));
                    else
                        foundAnusvara = true;
                }
                else if (c == '\u102E')
                {
                    if (foundIi)
                        throw new RuntimeException("Already found (" + ((int)c).ToString("X") + ") in " + PrintMM(text));
                    else
                        foundIi = true;
                }
                else if (c == '\u1032')
                {
                    if (foundAi)
                        throw new RuntimeException("Already found (" + ((int)c).ToString("X") + ") in " + PrintMM(text));
                    else
                        foundAi = true;
                }
                else
                {
                    sb.Append(c);
                }
            }


            //Done as bitflags to help find new combinations
            int flagAa = 1;
            int flagI = 2;
            int flagIi = 4;
            int flagU = 8;
            int flagUu = 16;
            int flagE = 32;
            int flagAi = 64;
            int flagKilledAa = 128;
            int flagAnusvara = 256;
            int acumulator = 0;
            if (foundAa)
                acumulator |= flagAa;
            if (foundI)
                acumulator |= flagI;
            if (foundIi)
                acumulator |= flagIi;
            if (foundU)
                acumulator |= flagU;
            if (foundUu)
                acumulator |= flagUu;
            if (foundE)
                acumulator |= flagE;
            if (foundAi)
                acumulator |= flagAi;
            if (foundKilledAa)
                acumulator |= flagKilledAa;
            if (foundAnusvara)
                acumulator |= flagAnusvara;

            int[] vowelIds = new int[]{0, flagAa, flagI, flagIi, flagU, flagUu, flagE, flagAi,
									    flagAa|flagE, flagKilledAa|flagE, flagAnusvara,
									    flagI|flagU, flagI|flagUu, flagAnusvara|flagU,
									    flagU|flagUu|flagI};

            //Super-cool use of xor
            this.m_sortVowel = -1;
            for (int i = 0; i < vowelIds.Length; i++)
            {
                if ((acumulator ^ vowelIds[i]) == 0)
                {
                    if (this.m_sortVowel != -1)
                        throw new RuntimeException("Double vowel found on " + PrintMM(text));
                    this.m_sortVowel = i;
                }
            }

            if (this.m_sortVowel == -1)
                throw new RuntimeException("New Vowel Combination: " + PrintMM(text) + " (" + acumulator + ")");

            return sb.ToString();
        }

        private string ExtractMedial(string text)
        {
            StringBuilder sb = new StringBuilder();
            bool foundYa = false;
            bool foundYeye = false;
            bool foundWa = false;
            bool foundHa = false;

            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];
                if (c == '\u103A')
                {
                    if (foundYa)
                        throw new RuntimeException("Already found (" + (int)c + ") in " + PrintMM(text));
                    else
                        foundYa = true;
                }
                else if (c == '\u103B')
                {
                    if (foundYeye)
                        throw new RuntimeException("Already found (" + ((int)c).ToString("X") + ") in " + PrintMM(text));
                    else
                        foundYeye = true;
                }
                else if (c == '\u103C')
                {
                    if (foundWa)
                        throw new RuntimeException("Already found (" + ((int)c).ToString("X") + ") in " + PrintMM(text));
                    else
                        foundWa = true;
                }
                else if (c == '\u103D')
                {
                    if (foundHa)
                        throw new RuntimeException("Already found (" + ((int)c).ToString("X") + ") in " + PrintMM(text));
                    else
                        foundHa = true;
                }
                else
                    sb.Append(c);
            }


            //Set using bitflags
            int flagYa = 1;
            int flagYeye = 2;
            int flagWa = 4;
            int flagHa = 8;
            int acumulator = 0;
            if (foundYa)
                acumulator |= flagYa;
            if (foundYeye)
                acumulator |= flagYeye;
            if (foundWa)
                acumulator |= flagWa;
            if (foundHa)
                acumulator |= flagHa;

            int[] medialIds = new int[]{0, flagYa, flagYeye, flagWa, flagHa,
					    flagWa|flagYa, flagHa|flagYa,
					    flagYeye|flagWa, flagYeye|flagHa, flagHa|flagWa, flagWa|flagHa|flagYa,
					    flagYeye|flagWa|flagHa};


            //Super-cool use of xor
            this.m_sortMedial = -1;
            for (int i = 0; i < medialIds.Length; i++)
            {
                if ((acumulator ^ medialIds[i]) == 0)
                {
                    if (this.m_sortMedial != -1)
                        throw new RuntimeException("Double medial found on " + PrintMM(text));
                    this.m_sortMedial = i;
                }
            }

            if (this.m_sortMedial == -1)
                throw new RuntimeException("New Medial Combination: " + PrintMM(text) + " (" + acumulator + ")");


            return sb.ToString();
        }

        private string ExtractConsonant(string text)
        {
            //Consonants are somewhat tricky (since they can also appear in finals).
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];

                //Basic consonants first. Test for "asat" in all cases, even though it's probably only necessary in the first.
                int newConsonant = 0;
                if (c >= '\u1000' && c <= '\u1021' && (i == text.Length - 1 || text[i + 1] != '\u1039'))
                {
                    newConsonant = ((int)c) - 0x1000 + 1;
                }
                else if (c == '\u103F' && (i == text.Length - 1 || text[i + 1] != '\u1039'))
                {
                    newConsonant = 35;
                }
                else if (c >= '\u104C' && c <= '\u104F' && (i == text.Length - 1 || text[i + 1] != '\u1039'))
                {
                    newConsonant = ((int)c) - 0x104C + 36;
                }
                else
                {
                    sb.Append(c);
                }

                //Set it?
                if (newConsonant != 0)
                {
                    if (this.m_sortConsonant != 0)
                    {
                        throw new RuntimeException("Double consonant (" + ((int)c).ToString("X") + ") in " + PrintMM(text));
                    }
                    else
                    {
                        this.m_sortConsonant = newConsonant;
                    }
                }
            }

            return sb.ToString();
        }
        #endregion

        #region Public Methods
        public string UnifyText(string text)
        {
            //First, some quick substitutions
            text = text.Replace("\u105A", "\u102C\u1039").Replace("\u1088", "\u103D\u102F").Replace("\u1089", "\u103D\u1030").Replace("\u108A", "\u103D\u103C");

            char[] src = text.ToCharArray();
            char[] res = new char[src.Length];
            for (int i = 0; i < src.Length; i++)
            {
                //Handle error cases:
                char c = src[i];
                if ((c >= '\u1023' && c <= '\u1027' && c != '\u1025')
                    || (c >= '\u1029' && c <= '\u102A')
                    //|| (c>='\u1040' && c<='\u1049')
                    || (c >= '\u104A' && c <= '\u104B')
                    || (c >= '\u1050' && c <= '\u1059')
                    || (c >= '\u1060' && c <= '\u1063')
                    || (c >= '\u1065' && c <= '\u1068')
                    || (c == '\u1069')
                    || (c == '\u106C')
                    || (c == '\u106D')
                    || (c >= '\u106E' && c <= '\u106F')
                    || (c >= '\u1070' && c <= '\u107C')
                    || (c == '\u1085')
                    || (c >= '\u108B' && c <= '\u108E')
                    || (c >= '\u1091' && c <= '\u1092')
                    || (c >= '\u1096' && c <= '\u1097')
                    || (c == '\u109F'))
                {

                    //Error
                    throw new RuntimeException("Bad character (" + ((int)c).ToString("X") + ") range in: " + PrintMM(text));
                }
                else
                {
                    switch (c)
                    {
                        case '\u1000':
                        case '\u1001':
                        case '\u1002':
                        case '\u1003':
                        case '\u1004':
                        case '\u1005':
                        case '\u1006':
                        case '\u1007':
                        case '\u1008':
                        case '\u100B':
                        case '\u100C':
                        case '\u100D':
                        case '\u100E':
                        case '\u100F':
                        case '\u1010':
                        case '\u1011':
                        case '\u1012':
                        case '\u1013':
                        case '\u1015':
                        case '\u1016':
                        case '\u1017':
                        case '\u1018':
                        case '\u1019':
                        case '\u101A':
                        case '\u101C':
                        case '\u101D':
                        case '\u101E':
                        case '\u101F':
                        case '\u1020':
                        case '\u1021':
                        case '\u102D':
                        case '\u102E':
                        case '\u1031':
                        case '\u1032':
                        case '\u1036':
                        case '\u1038':
                        case '\u1039':
                        case '\u103C':
                        case '\u1040':
                        case '\u1041':
                        case '\u1042':
                        case '\u1043':
                        case '\u1044':
                        case '\u1045':
                        case '\u1046':
                        case '\u1047':
                        case '\u1048':
                        case '\u1049':
                        case '\u104C':
                        case '\u104D':
                        case '\u104E':
                        case '\u104F':
                        case '\u1064':
                        case '\u1086':
                        case '-': //eek
                        case '\u1025': //double-eek
                        case ' ':
                            res[i] = c;
                            break;
                        case '\u1009':
                        case '\u106A':
                            res[i] = '\u1009';
                            break;
                        case '\u100A':
                        case '\u106B':
                            res[i] = '\u100A';
                            break;
                        case '\u1014':
                        case '\u108F':
                            res[i] = '\u1014';
                            break;
                        case '\u101B':
                        case '\u1090':
                            res[i] = '\u101B';
                            break;
                        case '\u102C':
                        case '\u102B':
                            res[i] = '\u102C';
                            break;
                        case '\u102F':
                        case '\u1033':
                            res[i] = '\u102F';
                            break;
                        case '\u1030':
                        case '\u1034':
                            res[i] = '\u1030';
                            break;
                        case '\u1037':
                        case '\u1094':
                        case '\u1095':
                            res[i] = '\u1037';
                            break;
                        case '\u103A':
                        case '\u107D':
                            res[i] = '\u103A';
                            break;
                        case '\u103B':
                        case '\u107E':
                        case '\u107F':
                        case '\u1080':
                        case '\u1081':
                        case '\u1082':
                        case '\u1083':
                        case '\u1084':
                            res[i] = '\u103B';
                            break;
                        case '\u103D':
                        case '\u1087':
                            res[i] = '\u103D';
                            break;

                        default:
                            //Unknown
                            throw new RuntimeException("Unknown character (" + ((int)c).ToString("X") + ") in range: " + PrintMM(text));
                    }
                }
            }

            return new String(res);
        }

        /**
         * Returns the string used for sorting.
         */
        public string ToCanonString()
        {
            return "N" + this.m_sortNumeral + " C" + this.m_sortConsonant + " M" + this.m_sortMedial + " F" + this.m_sortFinal + " V" + this.m_sortVowel + " T" + this.m_sortTone;
        }

        public int CompareTo(ZawgyiWord other)
        {
            return ZawgyiWord.Compare(this, other);
        }
        #endregion

        #region Static Methods
        public static string PrintMM(string txt)
        {
            StringBuilder sb = new StringBuilder();
            foreach (char c in txt.ToCharArray())
            {
                if (c < '\u1000' || c > '\u109F')
                    sb.Append(c);
                else
                    sb.Append("[").Append(((int)c).ToString("X")).Append("]");
            }
            return sb.ToString();
        }

        //Useful static functions
        public static void ProperlyOrder(string txt)
        {
            //Re-order where necessary.
            char[] chars = txt.Trim().ToCharArray();

            //Sort
            for (int chIn = 0; chIn < chars.Length; chIn++)
            {
                //Properly order: 102F 102D
                if (chIn > 0 && chars[chIn - 1] == (char)0x102D && chars[chIn] == (char)0x102F)
                {
                    chars[chIn - 1] = (char)0x102F;
                    chars[chIn] = (char)0x102D;
                }
                //Properly order: 103A 102D
                if (chIn > 0 && chars[chIn - 1] == (char)0x102D && chars[chIn] == (char)0x103A)
                {
                    chars[chIn - 1] = (char)0x103A;
                    chars[chIn] = (char)0x102D;
                }
                //Properly order: 103D 102D
                if (chIn > 0 && chars[chIn - 1] == (char)0x102D && chars[chIn] == (char)0x103D)
                {
                    chars[chIn - 1] = (char)0x103D;
                    chars[chIn] = (char)0x102D;
                }
                //Properly order: 1075 102D
                if (chIn > 0 && chars[chIn - 1] == (char)0x102D && chars[chIn] == (char)0x1075)
                {
                    chars[chIn - 1] = (char)0x1075;
                    chars[chIn] = (char)0x102D;
                }
                //Properly order: 102D 1087
                if (chIn > 0 && chars[chIn - 1] == (char)0x1087 && chars[chIn] == (char)0x102D)
                {
                    chars[chIn - 1] = (char)0x102D;
                    chars[chIn] = (char)0x1087;
                }
                //Properly order: 103D 102E
                if (chIn > 0 && chars[chIn - 1] == (char)0x102E && chars[chIn] == (char)0x103D)
                {
                    chars[chIn - 1] = (char)0x103D;
                    chars[chIn] = (char)0x102E;
                }
                //Properly order: 103D 103A
                if (chIn > 0 && chars[chIn - 1] == (char)0x103A && chars[chIn] == (char)0x103D)
                {
                    chars[chIn - 1] = (char)0x103D;
                    chars[chIn] = (char)0x103A;
                }
                //Properly order: 1039 103A -Note that this won't actually merge this fix!
                // Possibly set merged = true... ?
                if (chIn > 0 && chars[chIn - 1] == (char)0x103A && chars[chIn] == (char)0x1039)
                {
                    chars[chIn - 1] = (char)0x1039;
                    chars[chIn] = (char)0x103A;
                }
                //Properly order: 1030 102D
                if (chIn > 0 && chars[chIn - 1] == (char)0x102D && chars[chIn] == (char)0x1030)
                {
                    chars[chIn - 1] = (char)0x1030;
                    chars[chIn] = (char)0x102D;
                }
                //Properly order: 1037 1039
                if (chIn > 0 && chars[chIn - 1] == (char)0x1039 && chars[chIn] == (char)0x1037)
                {
                    chars[chIn - 1] = (char)0x1037;
                    chars[chIn] = (char)0x1039;
                }
                //Properly order: 1032 1037
                if (chIn > 0 && chars[chIn - 1] == (char)0x1037 && chars[chIn] == (char)0x1032)
                {
                    chars[chIn - 1] = (char)0x1032;
                    chars[chIn] = (char)0x1037;
                }
                //Properly order: 1032 1094
                if (chIn > 0 && chars[chIn - 1] == (char)0x1094 && chars[chIn] == (char)0x1032)
                {
                    chars[chIn - 1] = (char)0x1032;
                    chars[chIn] = (char)0x1094;
                }
                //Properly order: 1064 1094
                if (chIn > 0 && chars[chIn - 1] == (char)0x1094 && chars[chIn] == (char)0x1064)
                {
                    chars[chIn - 1] = (char)0x1064;
                    chars[chIn] = (char)0x1094;
                }
                //Properly order: 102D 1094
                if (chIn > 0 && chars[chIn - 1] == (char)0x1094 && chars[chIn] == (char)0x102D)
                {
                    chars[chIn - 1] = (char)0x102D;
                    chars[chIn] = (char)0x1094;
                }
                //Properly order: 102D 1071
                if (chIn > 0 && chars[chIn - 1] == (char)0x1071 && chars[chIn] == (char)0x102D)
                {
                    chars[chIn - 1] = (char)0x102D;
                    chars[chIn] = (char)0x1071;
                }
                //Properly order: 1036 1037
                if (chIn > 0 && chars[chIn - 1] == (char)0x1037 && chars[chIn] == (char)0x1036)
                {
                    chars[chIn - 1] = (char)0x1036;
                    chars[chIn] = (char)0x1037;
                }
                //Properly order: 1036 1088
                if (chIn > 0 && chars[chIn - 1] == (char)0x1088 && chars[chIn] == (char)0x1036)
                {
                    chars[chIn - 1] = (char)0x1036;
                    chars[chIn] = (char)0x1088;
                }
                //Properly order: 1039 1037
                // ###NOTE: I don't know how [XXXX][1037][1039] can parse correctly...
                if (chIn > 0 && chars[chIn - 1] == (char)0x1037 && chars[chIn] == (char)0x1039)
                {
                    chars[chIn - 1] = (char)0x1039;
                    chars[chIn] = (char)0x1037;
                }
                //Properly order: 102D 1033
                //NOTE that this is later reversed for "103A 1033 102D" below
                // Also for 103C 1033 102D, what a mess...
                if (chIn > 0 && chars[chIn - 1] == (char)0x1033 && chars[chIn] == (char)0x102D)
                {
                    chars[chIn - 1] = (char)0x102D;
                    chars[chIn] = (char)0x1033;
                }
                //Properly order: 103C 1032
                if (chIn > 0 && chars[chIn - 1] == (char)0x1032 && chars[chIn] == (char)0x103C)
                {
                    chars[chIn - 1] = (char)0x103C;
                    chars[chIn] = (char)0x1032;
                }
                //Properly order: 103C 102D
                if (chIn > 0 && chars[chIn - 1] == (char)0x102D && chars[chIn] == (char)0x103C)
                {
                    chars[chIn - 1] = (char)0x103C;
                    chars[chIn] = (char)0x102D;
                }
                //Properly order: 103C 102E
                if (chIn > 0 && chars[chIn - 1] == (char)0x102E && chars[chIn] == (char)0x103C)
                {
                    chars[chIn - 1] = (char)0x103C;
                    chars[chIn] = (char)0x102E;
                }
                //Properly order: 1036 102F
                if (chIn > 0 && chars[chIn - 1] == (char)0x102F && chars[chIn] == (char)0x1036)
                {
                    chars[chIn - 1] = (char)0x1036;
                    chars[chIn] = (char)0x102F;
                }
                //Properly order: 1036 1088
                if (chIn > 0 && chars[chIn - 1] == (char)0x1088 && chars[chIn] == (char)0x1036)
                {
                    chars[chIn - 1] = (char)0x1036;
                    chars[chIn] = (char)0x1088;
                }
                //Properly order: 1036 103D
                if (chIn > 0 && chars[chIn - 1] == (char)0x103D && chars[chIn] == (char)0x1036)
                {
                    chars[chIn - 1] = (char)0x1036;
                    chars[chIn] = (char)0x103D;
                }
                //Properly order: 1036 103C
                if (chIn > 0 && chars[chIn - 1] == (char)0x103C && chars[chIn] == (char)0x1036)
                {
                    chars[chIn - 1] = (char)0x1036;
                    chars[chIn] = (char)0x103C;
                }
                //Properly order: 107D 103C
                if (chIn > 0 && chars[chIn - 1] == (char)0x103C && chars[chIn] == (char)0x107D)
                {
                    chars[chIn - 1] = (char)0x107D;
                    chars[chIn] = (char)0x103C;
                }
                //Properly order: 1088 102D
                if (chIn > 0 && chars[chIn - 1] == (char)0x102D && chars[chIn] == (char)0x1088)
                {
                    chars[chIn - 1] = (char)0x1088;
                    chars[chIn] = (char)0x102D;
                }
                //Properly order: 1019 107B 102C
                //ASSUME: 1019 is stationary
                if (chIn > 1 && chars[chIn - 2] == (char)0x1019 && chars[chIn - 1] == (char)0x102C && chars[chIn] == (char)0x107B)
                {
                    chars[chIn - 1] = (char)0x107B;
                    chars[chIn] = (char)0x102C;
                }
                //Properly order: 103A 1033 102D
                //NOTE that this directly overrides "102D 1033" as entered above
                //ASSUME: 103A is stationary
                if (chIn > 1 && chars[chIn - 2] == (char)0x103A && chars[chIn - 1] == 0x102D && chars[chIn] == (char)0x1033)
                {
                    chars[chIn - 1] = (char)0x1033;
                    chars[chIn] = (char)0x102D;
                }
                //Properly order: 103C 102D 1033
                if (chIn > 1 && chars[chIn - 2] == (char)0x103C && chars[chIn - 1] == (char)0x1033 && chars[chIn] == (char)0x102D)
                {
                    chars[chIn - 1] = (char)0x102D;
                    chars[chIn] = (char)0x1033;
                }
                //Properly order: 1019 107B 102C 1037
                if (chIn > 2 && chars[chIn - 3] == (char)0x1019 &&
                        ((chars[chIn - 2] == (char)0x107B && chars[chIn - 1] == (char)0x1037 && chars[chIn] == (char)0x102C)
                          || (chars[chIn - 2] == (char)0x102C && chars[chIn - 1] == (char)0x107B && chars[chIn] == (char)0x1037)
                          || (chars[chIn - 2] == (char)0x102C && chars[chIn - 1] == (char)0x1037 && chars[chIn] == (char)0x107B)
                          || (chars[chIn - 2] == (char)0x1037 && chars[chIn - 1] == (char)0x107B && chars[chIn] == (char)0x102C)
                        ))
                {
                    chars[chIn - 2] = (char)0x107B;
                    chars[chIn - 1] = (char)0x102C;
                    chars[chIn] = (char)0x1037;
                }

                //Properly order: 107E XXXX 1036 1033
                if (chIn > 2 && chars[chIn - 3] == (char)0x107E && chars[chIn - 1] == (char)0x1033 && chars[chIn] == (char)0x1036)
                {
                    chars[chIn - 1] = (char)0x1036;
                    chars[chIn] = (char)0x1033;
                }

                //FIX: [103B-->1081 XXXX 103C] and [107E-->1082 XXXX 103C]
                if (chIn > 1 && chars[chIn] == (char)0x103C)
                {
                    if (chars[chIn - 2] == (char)0x103B)
                        chars[chIn - 2] = (char)0x1081;
                    else if (chars[chIn - 2] == (char)0x107E)
                        chars[chIn - 2] = (char)0x1082;
                }
                //FIX: [100A-->106B  108A]
                if (chIn > 0 && chars[chIn] == (char)0x108A && chars[chIn - 1] == (char)0x100A)
                {
                    chars[chIn - 1] = (char)0x106B;
                }

                //Small fix 1072's a bit ugly at times
                if (chIn > 0 && chars[chIn - 1] == (char)0x1010 && chars[chIn] == (char)0x1072)
                {
                    chars[chIn] = (char)0x1071;
                }
            }

            txt = new string(chars);
        }

        public static bool IsInError(string txt)
        {
            //Check for bad sequences, etc.
            return false;
        }

        public static int Compare(ZawgyiWord word1, ZawgyiWord word2)
        {
            //zeroth order: numerals
            if (word1.m_sortNumeral < word2.m_sortNumeral)
                return -1;
            else if (word1.m_sortNumeral > word2.m_sortNumeral)
                return 1;

            //first order: consonants
            if (word1.m_sortConsonant < word2.m_sortConsonant)
                return -1;
            else if (word1.m_sortConsonant > word2.m_sortConsonant)
                return 1;

            //second order: medials
            if (word1.m_sortMedial < word2.m_sortMedial)
                return -1;
            else if (word1.m_sortMedial > word2.m_sortMedial)
                return 1;

            //third order: finals
            if (word1.m_sortFinal < word2.m_sortFinal)
                return -1;
            else if (word1.m_sortFinal > word2.m_sortFinal)
                return 1;

            //fourth order: vowels
            if (word1.m_sortVowel < word2.m_sortVowel)
                return -1;
            else if (word1.m_sortVowel > word2.m_sortVowel)
                return 1;

            //fifth order: tones
            if (word1.m_sortTone < word2.m_sortTone)
                return -1;
            else if (word1.m_sortTone > word2.m_sortTone)
                return 1;

            //That's it.... unknown stuff should all sort into the "0" basket
            return 0;
        }
        #endregion

        #region Properties
        public string Data
        {
            get { return this.m_data; }
            set { this.m_data = value; }
        }

        public string Unknown
        {
            get { return this.m_unknownBit; }
        }
        #endregion

        #region Nested Types
        public class RuntimeException : Exception
        {
            public RuntimeException(string message)
                : base(message)
            {
            }
        }

        /*public enum CHARACTER_WIDTH {
           NOT_MYANMAR, UNKNOWN, ZERO_WIDTH, SOME_WIDTH
       };

       public static CHARACTER_WIDTH getWidthClassifier(char mmChar) {
           if (mmChar<'\u1000' || mmChar>'\u109F')
               return CHARACTER_WIDTH.NOT_MYANMAR;

       }*/
        #endregion
    }
}
