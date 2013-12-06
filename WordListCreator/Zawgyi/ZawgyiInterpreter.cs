using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace Unicode
{
    public class ZawgyiInterpreter
    {
        #region Constance Variables
        private const int NULL_CHAR = 0x00;
        #endregion

        #region Variables
        private int[] m_input = null;
        private List<int> m_result = null;
        private string m_text = null;
        #endregion

        #region Constructor/Destructor
        public ZawgyiInterpreter()
        {
        }

        public ZawgyiInterpreter(int[] input)
        {
            if (input == null) return;
            this.Interpret(input);
        }
        #endregion

        #region Override Methods
        public override string ToString()
        {
            if (string.IsNullOrEmpty(this.m_text))
                return base.ToString();

            return this.ToHex();
        }
        #endregion

        #region Private Methods
        private void Interpret()
        {
            if (this.m_input == null) return;

            Word word = new Word();

            for (int i = 0; i < this.m_input.Length; i++)
            {
                int code = this.m_input[i];

                //System.Diagnostics.Debug.Write(string.Format("0x{0:X} ", code));
                //System.Diagnostics.Debug.WriteLine(code.ToString() + " =   " + ((char)code).ToString());

                CharIndex charIndex = this.GetCharIndex(code);
                bool firstChar = ((charIndex == CharIndex.ThaWaiHtoe)
                    || (charIndex == CharIndex.RaRitt)
                    || (charIndex == CharIndex.Consonant)
                    || (charIndex == CharIndex.DoubleConsonant)
                    || (charIndex == CharIndex.Other));

                if (word.HasConsonant() && firstChar)
                {
                    word.Fix();
                    this.AppendResult(word);
                    word.Reset();
                    //System.Diagnostics.Debug.WriteLine("--------------------------------------");
                }

                if (charIndex == CharIndex.Other)
                    this.AppendResult(code);
                else
                    this.AppendChar(code, charIndex, ref word);
            }

            word.Fix();
            this.AppendResult(word);
            word.Reset();
            //System.Diagnostics.Debug.WriteLine("--------------------------------------");

            this.m_text = this.ToText(this.m_result.ToArray());
        }

        private void AppendResult(int code)
        {
            this.m_result.Add(code);
        }

        private void AppendResult(Word word)
        {
            this.m_result.AddRange(word.Result);
        }

        private void AppendChar(int code, CharIndex charIndex, ref Word word)
        {
            switch (code)
            {
                case 0x106E: // ၮ
                    word[(int)CharIndex.Consonant] = 0x100D;
                    word[(int)CharIndex.LowerConsonant] = code;
                    break;

                case 0x106F: // ၯ
                    word[(int)CharIndex.Consonant] = 0x100E;
                    word[(int)CharIndex.LowerConsonant] = code;
                    break;

                case 0x1097: // ႗
                    word[(int)CharIndex.Consonant] = 0x100B;
                    word[(int)CharIndex.LowerConsonant] = code;
                    break;

                case 0x102A: // ဪ
                    word[(int)CharIndex.ThaWaiHtoe] = 0x1031;
                    word[(int)CharIndex.RaRitt] = 0x107E;
                    word[(int)CharIndex.DoubleConsonant] = 0x101E;
                    word[(int)CharIndex.YeeKhya] = 0x102C;
                    word[(int)CharIndex.AThart] = 0x1039;
                    break;

                case 0x1029: // ဩ
                    word[(int)CharIndex.RaRitt] = 0x107E;
                    word[(int)CharIndex.DoubleConsonant] = 0x101E;
                    break;

                case 0x1026: // ဦ
                    word[(int)CharIndex.LoneGyiTin] = 0x102E;
                    word[(int)CharIndex.Consonant] = 0x1025;
                    break;

                // TODO Need to fix up
                case 0x104E: // ၎
                    word[(int)CharIndex.Consonant] = 0x1044;
                    word[(int)CharIndex.LowerConsonant] = 0x1004;
                    word[(int)CharIndex.AThart] = 0x1039;
                    word[(int)CharIndex.WittSaPout] = 0x1038;
                    break;

                case 0x105A: // xၚ
                    word[(int)CharIndex.YeeKhya] = 0x102C;
                    word[(int)CharIndex.AThart] = 0x1039;
                    break;

                case 0x1088: // ‍×ႈ
                    word[(int)CharIndex.HaHtoe] = 0x103D;
                    word[(int)CharIndex.ChangNyin] = 0x102F;
                    break;

                case 0x1089: // ×ႉ
                    word[(int)CharIndex.HaHtoe] = 0x103D;
                    word[(int)CharIndex.ChangNyin] = 0x1030;
                    break;

                case 0x108A: // ×‍ႊ
                    word[(int)CharIndex.WaSwal] = 0x103C;
                    word[(int)CharIndex.HaHtoe] = 0x103D;
                    break;

                case 0x108B: // ‍×ႋ
                    word[(int)CharIndex.AThart] = 0x1064;
                    word[(int)CharIndex.LoneGyiTin] = 0x102D;
                    break;

                case 0x108C: // ‍×ႌ
                    word[(int)CharIndex.AThart] = 0x1064;
                    word[(int)CharIndex.LoneGyiTin] = 0x102E;
                    break;

                case 0x108D: // ×ႍ
                    word[(int)CharIndex.AThart] = 0x1064;
                    word[(int)CharIndex.TeeTeeTin] = 0x1036;
                    break;

                case 0x108E: // ×ႎ
                    word[(int)CharIndex.LoneGyiTin] = 0x102D;
                    word[(int)CharIndex.TeeTeeTin] = 0x1036;
                    break;

                default:
                    word[(int)charIndex] = code;
                    break;
            }
        }

        private string ToText(int[] value)
        {
            if ((value == null) || (value.Length < 1)) return string.Empty;
            StringBuilder stringBuilder = new StringBuilder();
            for (int i = 0; i < value.Length; i++)
            {
                stringBuilder.Append((char)value[i]);
            }

            return stringBuilder.ToString();
        }

        private CharIndex GetCharIndex(int code)
        {
            CharIndex result = CharIndex.Other;
            switch (code)
            {
                // Consonant
                case 0x1001:
                case 0x1002:
                case 0x1004:
                case 0x1005:
                case 0x1007:
                case 0x1008:
                case 0x100B:
                case 0x100C:
                case 0x100D:
                case 0x100E:
                case 0x1012:
                case 0x1013:
                case 0x1014:
                case 0x1015:
                case 0x1016:
                case 0x1017:
                case 0x1019:
                case 0x101B:
                case 0x101D:
                case 0x1020:
                // SymbolConsonant
                case 0x1025:
                case 0x1026:
                case 0x1027:
                case 0x104C:
                case 0x104D:
                case 0x104F:
                case 0x106A:
                case 0x106E:
                case 0x106F:
                case 0x108F:
                case 0x1090:
                case 0x1092:
                case 0x1097:
                // Digit
                case 0x1040:
                case 0x1041:
                case 0x1042:
                case 0x1043:
                case 0x1044:
                case 0x1045:
                case 0x1046:
                case 0x1047:
                case 0x1048:
                case 0x1049:
                // Sign
                case 0x104A:
                case 0x104B:
                    result = CharIndex.Consonant;
                    break;

                // DoubleConsonant
                case 0x1000:
                case 0x1003:
                case 0x1006:
                case 0x100A:
                case 0x100F:
                case 0x1010:
                case 0x1011:
                case 0x1018:
                case 0x101A:
                case 0x101C:
                case 0x101E:
                case 0x101F:
                case 0x1021:
                // SymbolConsonant
                case 0x1009:
                case 0x1023:
                case 0x1024:
                case 0x1029:
                case 0x102A:
                case 0x103F:
                case 0x104E:
                case 0x106B:
                case 0x1086:
                case 0x1091:
                    result = CharIndex.DoubleConsonant;
                    break;

                // LowerConsonant
                case 0x1060:
                case 0x1061:
                case 0x1062:
                case 0x1063:
                case 0x1065:
                case 0x1066:
                case 0x1067:
                case 0x1068:
                case 0x1069:
                case 0x106C:
                case 0x106D:
                case 0x1070:
                case 0x1071:
                case 0x1072:
                case 0x1073:
                case 0x1074:
                case 0x1075:
                case 0x1076:
                case 0x1077:
                case 0x1078:
                case 0x1079:
                case 0x107A:
                case 0x107B:
                case 0x107C:
                case 0x1085:
                case 0x1093:
                case 0x1096:
                    result = CharIndex.LowerConsonant;
                    break;

                case 0x1031:
                    result = CharIndex.ThaWaiHtoe;
                    break;

                case 0x103B:
                case 0x107E:
                case 0x107F:
                case 0x1080:
                case 0x1081:
                case 0x1082:
                case 0x1083:
                case 0x1084:
                    result = CharIndex.RaRitt;
                    break;

                case 0x102D:
                case 0x102E:
                case 0x108B:
                case 0x108C:
                case 0x108E:
                    result = CharIndex.LoneGyiTin;
                    break;

                case 0x103C:
                case 0x108A:
                    result = CharIndex.WaSwal;
                    break;

                case 0x103D:
                case 0x1087:
                    result = CharIndex.HaHtoe;
                    break;

                case 0x103A:
                case 0x107D:
                    result = CharIndex.YaPint;
                    break;

                case 0x102F:
                case 0x1030:
                case 0x1033:
                case 0x1034:
                case 0x1088:
                case 0x1089:
                    result = CharIndex.ChangNyin;
                    break;

                case 0x1036:
                    result = CharIndex.TeeTeeTin;
                    break;

                case 0x1037:
                case 0x1094:
                case 0x1095:
                    result = CharIndex.OutNyint;
                    break;

                case 0x1039:
                case 0x1064:
                case 0x108D:
                    result = CharIndex.AThart;
                    break;

                case 0x1032:
                    result = CharIndex.NoutPyit;
                    break;

                case 0x102B:
                case 0x102C:
                case 0x105A:
                    result = CharIndex.YeeKhya;
                    break;

                case 0x1038:
                    result = CharIndex.WittSaPout;
                    break;

                default:
                    result = CharIndex.Other;
                    break;
            }

            return result;
        }
        #endregion

        #region Public Methods
        public void Interpret(string input)
        {
            if (string.IsNullOrEmpty(input)) return;

            char[] chrs = input.ToCharArray();
            int[] iInput = new int[chrs.Length];

            for (int i = 0; i < chrs.Length; i++)
            {
                iInput[i] = (int)chrs[i];
            }

            this.Interpret(iInput);
        }

        public void Interpret(int[] input)
        {
            if (input == null) return;

            this.m_input = input;
            this.m_result = new List<int>();
            this.m_text = string.Empty;

            this.Interpret();
        }

        public string ToHex()
        {
            if (this.m_result == null)
                return string.Empty;

            System.Text.StringBuilder builder = new System.Text.StringBuilder();
            for (int i = 0; i < this.m_result.Count; i++)
            {
                if (i > 0) builder.Append(", ");
                builder.AppendFormat("0x{0:X}", this.m_result[i]);
            }

            return builder.ToString();
        }
        #endregion

        #region Properties
        public string Text
        {
            get { return this.m_text; }
        }
        #endregion

        #region Nested Types
        private enum CharIndex : int
        {
            ThaWaiHtoe = 0,
            RaRitt = 1,
            Consonant = 2,
            DoubleConsonant = 3,
            LowerConsonant = 4,
            LoneGyiTin = 5,
            HaHtoe = 6,
            WaSwal = 7,
            YaPint = 8,
            TeeTeeTin = 9,
            ChangNyin = 10,
            NoutPyit = 11,
            YeeKhya = 12,
            AThart = 13,
            OutNyint = 14,
            WittSaPout = 15,
            Other = 16
        }

        private class Word
        {
            #region Variables
            private const int WORD_LENGTH = 17;

            private int[] mWord = null;
            #endregion

            #region Constructor
            public Word()
            {
                this.mWord = new int[WORD_LENGTH];
            }
            #endregion

            #region Properties
            public int this[int index]
            {
                get { return this.mWord[index]; }
                set { this.mWord[index] = value; }
            }

            public int[] Result
            {
                get
                {
                    bool fixAThart = ((this.mWord[(int)CharIndex.ThaWaiHtoe] == NULL_CHAR)
                        && (this.mWord[(int)CharIndex.YaPint] != NULL_CHAR)
                        && (this.mWord[(int)CharIndex.AThart] != NULL_CHAR));

                    if ((this.mWord[(int)CharIndex.AThart] == 0x1064)
                        || (this.mWord[(int)CharIndex.AThart] == 0x108D))
                    {
                        fixAThart = true;
                    }

                    bool fixLoneGyiTin = (this.mWord[(int)CharIndex.YaPint] != NULL_CHAR);
                    List<int> list = new List<int>();

                    this.AddToList(this.mWord[(int)CharIndex.ThaWaiHtoe], ref list);
                    this.AddToList(this.mWord[(int)CharIndex.RaRitt], ref list);
                    this.AddToList(this.mWord[(int)CharIndex.Consonant], ref list);
                    this.AddToList(this.mWord[(int)CharIndex.DoubleConsonant], ref list);
                    this.AddToList(this.mWord[(int)CharIndex.LowerConsonant], ref list);
                    if (!fixLoneGyiTin) this.AddToList(this.mWord[(int)CharIndex.LoneGyiTin], ref list);
                    this.AddToList(this.mWord[(int)CharIndex.HaHtoe], ref list);
                    this.AddToList(this.mWord[(int)CharIndex.WaSwal], ref list);
                    this.AddToList(this.mWord[(int)CharIndex.YaPint], ref list);
                    if (fixAThart) this.AddToList(this.mWord[(int)CharIndex.AThart], ref list);
                    this.AddToList(this.mWord[(int)CharIndex.TeeTeeTin], ref list);
                    this.AddToList(this.mWord[(int)CharIndex.ChangNyin], ref list);
                    if (fixLoneGyiTin) this.AddToList(this.mWord[(int)CharIndex.LoneGyiTin], ref list);
                    this.AddToList(this.mWord[(int)CharIndex.NoutPyit], ref list);
                    this.AddToList(this.mWord[(int)CharIndex.YeeKhya], ref list);
                    if (!fixAThart) this.AddToList(this.mWord[(int)CharIndex.AThart], ref list);
                    this.AddToList(this.mWord[(int)CharIndex.OutNyint], ref list);
                    this.AddToList(this.mWord[(int)CharIndex.WittSaPout], ref list);
                    this.AddToList(this.mWord[(int)CharIndex.Other], ref list);

                    //for (int i = 0; i < WORD_LENGTH; i++)
                    //{
                    //    if (this.m_word[i] > 0)
                    //        list.Add(this.m_word[i]);
                    //}

                    return list.ToArray();
                }
            }
            #endregion

            #region Private Methods
            private void AddToList(int code, ref List<int> list)
            {
                if (code != NULL_CHAR) list.Add(code);
            }

            private void FixThaWaiHtoe()
            {
                if (this.mWord[(int)CharIndex.ThaWaiHtoe] == NULL_CHAR) return;
                // No Change
            }

            private void FixRaYitt() // Change
            {
                if (this.mWord[(int)CharIndex.RaRitt] == NULL_CHAR) return;

                bool upper = ((this.mWord[(int)CharIndex.LoneGyiTin] != NULL_CHAR) 
                                || (this.mWord[(int)CharIndex.NoutPyit] != NULL_CHAR)
                                || (this.mWord[(int)CharIndex.TeeTeeTin] != NULL_CHAR)
                                || (this.mWord[(int)CharIndex.AThart] == 0x1064)
                                || (this.mWord[(int)CharIndex.AThart] == 0x108D));

                bool lower = ((this.mWord[(int)CharIndex.WaSwal] != NULL_CHAR) 
                                || (this.mWord[(int)CharIndex.LowerConsonant] != NULL_CHAR));

                if (this.mWord[(int)CharIndex.Consonant] != NULL_CHAR)
                {
                    if (upper && lower)
                    {
                        this.mWord[(int)CharIndex.RaRitt] = 0x1083;
                    }
                    else if (upper)
                    {
                        this.mWord[(int)CharIndex.RaRitt] = 0x107F;
                    }
                    else if (lower)
                    {
                        this.mWord[(int)CharIndex.RaRitt] = 0x1081;
                    }
                    else
                    {
                        this.mWord[(int)CharIndex.RaRitt] = 0x103B;
                    }
                }
                else
                {
                    if (upper && lower)
                    {
                        this.mWord[(int)CharIndex.RaRitt] = 0x1084;
                    }
                    else if (upper)
                    {
                        this.mWord[(int)CharIndex.RaRitt] = 0x1080;
                    }
                    else if (lower)
                    {
                        this.mWord[(int)CharIndex.RaRitt] = 0x1082;
                    }
                    else
                    {
                        this.mWord[(int)CharIndex.RaRitt] = 0x107E;
                    }
                }
            }

            private void FixConsonant()
            {
                if ((this.mWord[(int)CharIndex.Consonant] == NULL_CHAR) && (this.mWord[(int)CharIndex.DoubleConsonant] == NULL_CHAR)) return;

                if ((this.mWord[(int)CharIndex.DoubleConsonant] == 0x100A)
                    || (this.mWord[(int)CharIndex.DoubleConsonant] == 0x106B)) // ည
                {
                    if (this.mWord[(int)CharIndex.WaSwal] != NULL_CHAR)
                    {
                        this.mWord[(int)CharIndex.DoubleConsonant] = 0x106B;
                    }
                    else
                    {
                        this.mWord[(int)CharIndex.DoubleConsonant] = 0x100A;
                    }
                }

                if ((this.mWord[(int)CharIndex.Consonant] == 0x1014)
                    || (this.mWord[(int)CharIndex.Consonant] == 0x108F)) // န
                {
                    if ((this.mWord[(int)CharIndex.RaRitt] != NULL_CHAR)
                       || (this.mWord[(int)CharIndex.WaSwal] != NULL_CHAR)
                       || (this.mWord[(int)CharIndex.HaHtoe] != NULL_CHAR)
                       || (this.mWord[(int)CharIndex.YaPint] != NULL_CHAR)
                       || (this.mWord[(int)CharIndex.ChangNyin] != NULL_CHAR)
                        || (this.mWord[(int)CharIndex.LowerConsonant] != NULL_CHAR)
                       )
                    {
                        this.mWord[(int)CharIndex.Consonant] = 0x108F;
                    }
                    else
                    {
                        this.mWord[(int)CharIndex.Consonant] = 0x1014;
                    }
                }
                if ((this.mWord[(int)CharIndex.Consonant] == 0x101B)
                    || (this.mWord[(int)CharIndex.Consonant] == 0x1090)) // ရ
                {
                    if (this.mWord[(int)CharIndex.ChangNyin] != NULL_CHAR)
                    {
                        this.mWord[(int)CharIndex.Consonant] = 0x1090;
                    }
                    else
                    {
                        this.mWord[(int)CharIndex.Consonant] = 0x101B;
                    }
                }
                if ((this.mWord[(int)CharIndex.Consonant] == 0x1025)
                    || (this.mWord[(int)CharIndex.Consonant] == 0x106A)) // ဥ
                {
                    if (this.mWord[(int)CharIndex.WaSwal] != NULL_CHAR)
                    {
                        this.mWord[(int)CharIndex.Consonant] = 0x106A;
                    }
                    else
                    {
                        this.mWord[(int)CharIndex.Consonant] = 0x1025;
                    }
                }
            }

            private void FixLowerConsonant()
            {
                if (this.mWord[(int)CharIndex.LowerConsonant] == NULL_CHAR) return;

                if ((this.mWord[(int)CharIndex.LowerConsonant] == 0x1066)
                    || (this.mWord[(int)CharIndex.LowerConsonant] == 0x1067))
                {
                    if (this.mWord[(int)CharIndex.DoubleConsonant] != NULL_CHAR)
                    {
                        this.mWord[(int)CharIndex.LowerConsonant] = 0x1066;
                    }
                    else
                    {
                        this.mWord[(int)CharIndex.LowerConsonant] = 0x1067;
                    }
                }

                if ((this.mWord[(int)CharIndex.LowerConsonant] == 0x1071)
                    || (this.mWord[(int)CharIndex.LowerConsonant] == 0x1072))
                {
                    if (this.mWord[(int)CharIndex.DoubleConsonant] != NULL_CHAR)
                    {
                        this.mWord[(int)CharIndex.LowerConsonant] = 0x1071;
                    }
                    else
                    {
                        this.mWord[(int)CharIndex.LowerConsonant] = 0x1072;
                    }
                }

                if ((this.mWord[(int)CharIndex.LowerConsonant] == 0x1073)
                    || (this.mWord[(int)CharIndex.LowerConsonant] == 0x1074))
                {
                    if (this.mWord[(int)CharIndex.DoubleConsonant] != NULL_CHAR)
                    {
                        this.mWord[(int)CharIndex.LowerConsonant] = 0x1073;
                    }
                    else
                    {
                        this.mWord[(int)CharIndex.LowerConsonant] = 0x1074;
                    }
                }

                if ((this.mWord[(int)CharIndex.LowerConsonant] == 0x107B)
                    || (this.mWord[(int)CharIndex.LowerConsonant] == 0x1093))
                {
                    if (this.mWord[(int)CharIndex.DoubleConsonant] != NULL_CHAR)
                    {
                        this.mWord[(int)CharIndex.LowerConsonant] = 0x107B;
                    }
                    else
                    {
                        this.mWord[(int)CharIndex.LowerConsonant] = 0x1093;
                    }
                }
            }

            private void FixLoneGyiTin()
            {
                if (this.mWord[(int)CharIndex.LoneGyiTin] == NULL_CHAR) return;

                if (this.mWord[(int)CharIndex.AThart] == 0x1064)
                {
                    if (this.mWord[(int)CharIndex.LoneGyiTin] == 0x102D)
                    {
                        this.mWord[(int)CharIndex.LoneGyiTin] = 0x108B;
                    }
                    else
                    {
                        this.mWord[(int)CharIndex.LoneGyiTin] = 0x108C;
                    }
                }
                else if (this.mWord[(int)CharIndex.TeeTeeTin] != NULL_CHAR)
                {
                    this.mWord[(int)CharIndex.LoneGyiTin] = 0x108E;
                }
                else if ((this.mWord[(int)CharIndex.LoneGyiTin] == 0x102D)
                         || (this.mWord[(int)CharIndex.LoneGyiTin] == 0x108B)
                         || (this.mWord[(int)CharIndex.LoneGyiTin] == 0x108E))
                {
                    this.mWord[(int)CharIndex.LoneGyiTin] = 0x102D;
                }
                else if ((this.mWord[(int)CharIndex.LoneGyiTin] == 0x102E)
                         || (this.mWord[(int)CharIndex.LoneGyiTin] == 0x108C))
                {
                    this.mWord[(int)CharIndex.LoneGyiTin] = 0x102E;
                }
            }

            private void FixHaHtoe()
            {
                if (this.mWord[(int)CharIndex.HaHtoe] == NULL_CHAR) return;

                if ((this.mWord[(int)CharIndex.RaRitt] != NULL_CHAR) // Change
                    || (this.mWord[(int)CharIndex.DoubleConsonant] == 0x100A)
                    || (this.mWord[(int)CharIndex.DoubleConsonant] == 0x1009)
                    || (this.mWord[(int)CharIndex.DoubleConsonant] == 0x106B)
                    || (this.mWord[(int)CharIndex.Consonant] == 0x100C)
                    || (this.mWord[(int)CharIndex.DoubleConsonant] == 0x1029)
                    || (this.mWord[(int)CharIndex.DoubleConsonant] == 0x102A)
                    )
                {
                    this.mWord[(int)CharIndex.HaHtoe] = 0x1087;
                }
                else
                {
                    this.mWord[(int)CharIndex.HaHtoe] = 0x103D;
                }
            }

            private void FixWaSwal()
            {
                if (this.mWord[(int)CharIndex.WaSwal] == NULL_CHAR) return;

                if (this.mWord[(int)CharIndex.HaHtoe] != NULL_CHAR)
                {
                    this.mWord[(int)CharIndex.WaSwal] = 0x108A;
                    this.mWord[(int)CharIndex.HaHtoe] = NULL_CHAR;
                }
                else
                {
                    this.mWord[(int)CharIndex.WaSwal] = 0x103C;
                }
            }

            private void FixYaPint()
            {
                if (this.mWord[(int)CharIndex.YaPint] == NULL_CHAR) return;

                if (this.mWord[(int)CharIndex.WaSwal] != NULL_CHAR)
                {
                    this.mWord[(int)CharIndex.YaPint] = 0x107D;
                }
                else
                {
                    this.mWord[(int)CharIndex.YaPint] = 0x103A;
                }
            }

            private void FixChangNyin()
            {
                if (this.mWord[(int)CharIndex.ChangNyin] == NULL_CHAR) return;
                bool isLong = ((this.mWord[(int)CharIndex.RaRitt] != NULL_CHAR)
                        || (this.mWord[(int)CharIndex.YaPint] != NULL_CHAR)
                        || (this.mWord[(int)CharIndex.WaSwal] != NULL_CHAR)
                        || (this.mWord[(int)CharIndex.LowerConsonant] != NULL_CHAR)

                        || (this.mWord[(int)CharIndex.Consonant] == 0x1008) // ဈ
                        || (this.mWord[(int)CharIndex.Consonant] == 0x100B) // ဋ
                        || (this.mWord[(int)CharIndex.Consonant] == 0x100C) // ဌ
                        || (this.mWord[(int)CharIndex.Consonant] == 0x100D) // ဍ
                        || (this.mWord[(int)CharIndex.Consonant] == 0x1020) // ဠ
                        || (this.mWord[(int)CharIndex.Consonant] == 0x1025) // ဥ
                        || (this.mWord[(int)CharIndex.Consonant] == 0x1026) // ဦ
                        || (this.mWord[(int)CharIndex.Consonant] == 0x106A) // ၪ
                        || (this.mWord[(int)CharIndex.Consonant] == 0x104C) // ၌
                        || (this.mWord[(int)CharIndex.Consonant] == 0x104D) // ၍
                        || (this.mWord[(int)CharIndex.Consonant] == 0x1092) // ႒
                        || (this.mWord[(int)CharIndex.Consonant] == 0x106E) // ၮ
                        || (this.mWord[(int)CharIndex.Consonant] == 0x106F) // ၯ
                        || (this.mWord[(int)CharIndex.Consonant] == 0x1097) // ႗

                        || (this.mWord[(int)CharIndex.Consonant] == 0x1042) // ၂
                        || (this.mWord[(int)CharIndex.Consonant] == 0x1043) // ၃
                        || (this.mWord[(int)CharIndex.Consonant] == 0x1044) // ၄
                        || (this.mWord[(int)CharIndex.Consonant] == 0x1045) // ၅
                        || (this.mWord[(int)CharIndex.Consonant] == 0x1046) // ၆
                        || (this.mWord[(int)CharIndex.Consonant] == 0x1047) // ၇
                        || (this.mWord[(int)CharIndex.Consonant] == 0x1049) // ၉

                        || (this.mWord[(int)CharIndex.DoubleConsonant] == 0x100A) // ည
                        || (this.mWord[(int)CharIndex.DoubleConsonant] == 0x1009) // ဉ
                        || (this.mWord[(int)CharIndex.DoubleConsonant] == 0x106B) // ၫ
                        || (this.mWord[(int)CharIndex.DoubleConsonant] == 0x1023) // ဣ
                        || (this.mWord[(int)CharIndex.DoubleConsonant] == 0x1024) // ဤ
                        || (this.mWord[(int)CharIndex.DoubleConsonant] == 0x1029) // ဩ
                        || (this.mWord[(int)CharIndex.DoubleConsonant] == 0x102A) // ဪ
                        || (this.mWord[(int)CharIndex.DoubleConsonant] == 0x104E) // ၎
                        || (this.mWord[(int)CharIndex.DoubleConsonant] == 0x1091) // ႑
                        );

                if ((this.mWord[(int)CharIndex.ChangNyin] == 0x102F)
                    || (this.mWord[(int)CharIndex.ChangNyin] == 0x1033))
                {
                    if (isLong)
                    {
                        this.mWord[(int)CharIndex.ChangNyin] = 0x1033;
                    }
                    else if (this.mWord[(int)CharIndex.HaHtoe] != NULL_CHAR)
                    {
                        this.mWord[(int)CharIndex.ChangNyin] = 0x1088;
                        this.mWord[(int)CharIndex.HaHtoe] = NULL_CHAR;
                    }
                    else
                    {
                        this.mWord[(int)CharIndex.ChangNyin] = 0x102F;
                    }
                }
                else // 0x1030 | 0x1034
                {
                    if (isLong)
                    {
                        this.mWord[(int)CharIndex.ChangNyin] = 0x1034;
                    }
                    else if (this.mWord[(int)CharIndex.HaHtoe] != NULL_CHAR)
                    {
                        this.mWord[(int)CharIndex.ChangNyin] = 0x1089;
                        this.mWord[(int)CharIndex.HaHtoe] = NULL_CHAR;
                    }
                    else
                    {
                        this.mWord[(int)CharIndex.ChangNyin] = 0x1030;
                    }
                }
            }

            private void FixTeeTeeTin()
            {
                if (this.mWord[(int)CharIndex.TeeTeeTin] == NULL_CHAR) return;
                // No Change
            }

            private void FixNoutPyit()
            {
                if (this.mWord[(int)CharIndex.NoutPyit] == NULL_CHAR) return;
                // No Change
            }

            private void FixYeeKhya()
            {
                if (this.mWord[(int)CharIndex.YeeKhya] == NULL_CHAR) return;

                if (((this.mWord[(int)CharIndex.Consonant] == 0x1001)  // ခ
                    || (this.mWord[(int)CharIndex.Consonant] == 0x1002)  // ဂ
                    || (this.mWord[(int)CharIndex.Consonant] == 0x1004)  // င
                    || (this.mWord[(int)CharIndex.Consonant] == 0x1012)  // ဒ
                    || (this.mWord[(int)CharIndex.Consonant] == 0x1015)  // ပ
                    || (this.mWord[(int)CharIndex.Consonant] == 0x101D)) // ဝ
                    &&
                    ((this.mWord[(int)CharIndex.RaRitt] == NULL_CHAR)
                    && (this.mWord[(int)CharIndex.LowerConsonant] == NULL_CHAR)
                    && (this.mWord[(int)CharIndex.LoneGyiTin] == NULL_CHAR)
                    && (this.mWord[(int)CharIndex.HaHtoe] == NULL_CHAR)
                    && (this.mWord[(int)CharIndex.WaSwal] == NULL_CHAR)
                    && (this.mWord[(int)CharIndex.YaPint] == NULL_CHAR)
                    && (this.mWord[(int)CharIndex.ChangNyin] == NULL_CHAR)
                    ))
                {
                    if (this.mWord[(int)CharIndex.AThart] != NULL_CHAR)
                    {
                        this.mWord[(int)CharIndex.YeeKhya] = 0x105A;
                        this.mWord[(int)CharIndex.AThart] = NULL_CHAR;
                    }
                    else
                    {
                        this.mWord[(int)CharIndex.YeeKhya] = 0x102B;
                    }
                }
                else
                {
                    this.mWord[(int)CharIndex.YeeKhya] = 0x102C;
                }
            }

            private void FixAThart()
            {
                if (this.mWord[(int)CharIndex.AThart] == NULL_CHAR) return;
                if (this.mWord[(int)CharIndex.AThart] == 0x1064)
                {
                    if (this.mWord[(int)CharIndex.TeeTeeTin] != NULL_CHAR)
                    {
                        this.mWord[(int)CharIndex.AThart] = 0x108D;
                    }
                    else
                    {
                        this.mWord[(int)CharIndex.AThart] = 0x1064;
                    }
                }
            }

            private void FixOutNyint()
            {
                if (this.mWord[(int)CharIndex.OutNyint] == NULL_CHAR) return;
                if ((this.mWord[(int)CharIndex.YeeKhya] == NULL_CHAR) && (
                    (this.mWord[(int)CharIndex.RaRitt] != NULL_CHAR)
                    || (this.mWord[(int)CharIndex.WaSwal] != NULL_CHAR)
                    || (this.mWord[(int)CharIndex.YaPint] != NULL_CHAR)
                    || ((this.mWord[(int)CharIndex.ChangNyin] != NULL_CHAR) && (this.mWord[(int)CharIndex.ChangNyin] != 0x102F))
                    || (this.mWord[(int)CharIndex.LowerConsonant] != NULL_CHAR)

                    || (this.mWord[(int)CharIndex.Consonant] == 0x1008) // ဈ
                    || (this.mWord[(int)CharIndex.Consonant] == 0x100B)  // ဋ
                    || (this.mWord[(int)CharIndex.Consonant] == 0x100C)  // ဌ
                    || (this.mWord[(int)CharIndex.Consonant] == 0x100D)  // ဍ
                    || (this.mWord[(int)CharIndex.Consonant] == 0x1020)  // ဠ
                    || (this.mWord[(int)CharIndex.Consonant] == 0x101B)  // ရ
                    || (this.mWord[(int)CharIndex.Consonant] == 0x1090) // ႐
                    || (this.mWord[(int)CharIndex.Consonant] == 0x104C) // ၌
                    || (this.mWord[(int)CharIndex.Consonant] == 0x104D) // ၍
                    || (this.mWord[(int)CharIndex.Consonant] == 0x1092) // ႒
                    || (this.mWord[(int)CharIndex.Consonant] == 0x106E) // ၮ
                    || (this.mWord[(int)CharIndex.Consonant] == 0x106F) // ၯ
                    || (this.mWord[(int)CharIndex.Consonant] == 0x1097) // ႗

                    || (this.mWord[(int)CharIndex.Consonant] == 0x1042) // ၂
                    || (this.mWord[(int)CharIndex.Consonant] == 0x1043) // ၃
                    || (this.mWord[(int)CharIndex.Consonant] == 0x1044) // ၄
                    || (this.mWord[(int)CharIndex.Consonant] == 0x1045) // ၅
                    || (this.mWord[(int)CharIndex.Consonant] == 0x1046) // ၆
                    || (this.mWord[(int)CharIndex.Consonant] == 0x1047) // ၇
                    || (this.mWord[(int)CharIndex.Consonant] == 0x1049) // ၉

                    || (this.mWord[(int)CharIndex.DoubleConsonant] == 0x1023) // ဣ
                    || (this.mWord[(int)CharIndex.DoubleConsonant] == 0x1024) // ဤ
                    || (this.mWord[(int)CharIndex.DoubleConsonant] == 0x1029) // ဩ
                    || (this.mWord[(int)CharIndex.DoubleConsonant] == 0x102A) // ဪ
                    || (this.mWord[(int)CharIndex.DoubleConsonant] == 0x104E) // ၎
                    || (this.mWord[(int)CharIndex.DoubleConsonant] == 0x1091) // ႑
                    ))
                {
                    this.mWord[(int)CharIndex.OutNyint] = 0x1095;
                }
                else if ((this.mWord[(int)CharIndex.YeeKhya] == NULL_CHAR) && ( // Change
                    (this.mWord[(int)CharIndex.ChangNyin] == 0x102F)
                    || (this.mWord[(int)CharIndex.HaHtoe] != NULL_CHAR)
                    || (this.mWord[(int)CharIndex.Consonant] == 0x1014)) // န 
                    )
                {
                    this.mWord[(int)CharIndex.OutNyint] = 0x1094;
                }
                else
                {
                    this.mWord[(int)CharIndex.OutNyint] = 0x1037;
                }
            }

            private void FixWittSaPout()
            {
                if (this.mWord[(int)CharIndex.WittSaPout] == NULL_CHAR) return;
                // No Change
            }

            private void FixMixChar()
            {
                if ((this.mWord[(int)CharIndex.Consonant] == 0x100D)
                    && (this.mWord[(int)CharIndex.LowerConsonant] == 0x106E))
                {
                    this.mWord[(int)CharIndex.Consonant] = 0x106E;
                    this.mWord[(int)CharIndex.LowerConsonant] = NULL_CHAR;
                }

                if ((this.mWord[(int)CharIndex.Consonant] == 0x100E)
                    && (this.mWord[(int)CharIndex.LowerConsonant] == 0x106F))
                {
                    this.mWord[(int)CharIndex.Consonant] = 0x106F;
                    this.mWord[(int)CharIndex.LowerConsonant] = NULL_CHAR;
                }

                if ((this.mWord[(int)CharIndex.Consonant] == 0x100B)
                    && (this.mWord[(int)CharIndex.LowerConsonant] == 0x1097))
                {
                    this.mWord[(int)CharIndex.Consonant] = 0x1097;
                    this.mWord[(int)CharIndex.LowerConsonant] = NULL_CHAR;
                }

                if ((this.mWord[(int)CharIndex.ThaWaiHtoe] == 0x1031)
                    && (this.mWord[(int)CharIndex.RaRitt] == 0x107E)
                    && (this.mWord[(int)CharIndex.DoubleConsonant] == 0x101E)
                    && (this.mWord[(int)CharIndex.YeeKhya] == 0x102C)
                    && (this.mWord[(int)CharIndex.AThart] == 0x1039))
                {
                    this.mWord[(int)CharIndex.ThaWaiHtoe] = NULL_CHAR;
                    this.mWord[(int)CharIndex.RaRitt] = NULL_CHAR;
                    this.mWord[(int)CharIndex.DoubleConsonant] = 0x102A;
                    this.mWord[(int)CharIndex.YeeKhya] = NULL_CHAR;
                    this.mWord[(int)CharIndex.AThart] = NULL_CHAR;
                }

                if ((this.mWord[(int)CharIndex.RaRitt] == 0x107E)
                    && (this.mWord[(int)CharIndex.DoubleConsonant] == 0x101E))
                {
                    this.mWord[(int)CharIndex.RaRitt] = NULL_CHAR;
                    this.mWord[(int)CharIndex.DoubleConsonant] = 0x1029;
                }

                if ((this.mWord[(int)CharIndex.LoneGyiTin] == 0x102E)
                    && (this.mWord[(int)CharIndex.Consonant] == 0x1025))
                {
                    this.mWord[(int)CharIndex.LoneGyiTin] = NULL_CHAR;
                    this.mWord[(int)CharIndex.Consonant] = 0x1026;
                }

                if ((this.mWord[(int)CharIndex.Consonant] == 0x1044)
                    && (this.mWord[(int)CharIndex.LowerConsonant] == 0x1004)
                    && (this.mWord[(int)CharIndex.AThart] == 0x1039)
                    && (this.mWord[(int)CharIndex.WittSaPout] == 0x1038))
                {
                    this.mWord[(int)CharIndex.Consonant] = 0x104E;
                    this.mWord[(int)CharIndex.LowerConsonant] = NULL_CHAR;
                    this.mWord[(int)CharIndex.AThart] = NULL_CHAR;
                    this.mWord[(int)CharIndex.WittSaPout] = NULL_CHAR;
                }
            }
            #endregion

            #region Public Methods
            public void Fix()
            {
                if (this.mWord == null) return;
                if ((this.mWord[(int)CharIndex.Consonant] == NULL_CHAR) && (this.mWord[(int)CharIndex.DoubleConsonant] == NULL_CHAR)) return;

                this.FixThaWaiHtoe();
                this.FixRaYitt();
                this.FixConsonant();
                this.FixLowerConsonant();
                this.FixLoneGyiTin();
                this.FixHaHtoe();
                this.FixWaSwal();
                this.FixYaPint();
                this.FixChangNyin();
                this.FixTeeTeeTin();
                this.FixNoutPyit();
                this.FixYeeKhya();
                this.FixAThart();
                this.FixOutNyint();
                this.FixWittSaPout();
                this.FixMixChar();
            }

            public void Reset()
            {
                this.mWord = new int[WORD_LENGTH];
            }

            public bool HasConsonant()
            {
                return (this.mWord[(int)CharIndex.Consonant] != NULL_CHAR) || (this.mWord[(int)CharIndex.DoubleConsonant] != NULL_CHAR);
            }
            #endregion
        }
        #endregion
    }
}

