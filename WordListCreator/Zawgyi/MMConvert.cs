using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Unicode
{
    public class MMConvert
    {
        #region Variables
        private string[] mZg2Uni = null;
        private string[] mNZg2Zg = null;
        #endregion

        #region Constructor
        public MMConvert()
        {
            this.mZg2Uni = new string[] { "\u106A", "\u1009"
                                                        , "\u1025(?=[\u1039\u102C])", "\u1009"
                                                        , "\u1025\u102E", "\u1026"
                                                        , "\u106B", "\u100A"
                                                        , "\u1090", "\u101B"
                                                        , "\u1040", "\u1040"
                                                        , "\u108F", "\u1014"
                                                        , "\u1012", "\u1012"
                                                        , "\u1013", "\u1013"
                                                        , "[\u103D\u1087]", "\u103E"
                                                        , "\u103C", "\u103D"
                                                        , "[\u103B\u107E\u107F\u1080\u1081\u1082\u1083\u1084]", "\u103C"
                                                        , "[\u103A\u107D]", "\u103B"
                                                        , "\u103E\u103B", "\u103B\u103E"
                                                        , "\u108A", "\u103D\u103E"
                                                        , "\u103E\u103D", "\u103D\u103E"
                                                        , "(\u1031)?(\u103C)?([\u1000-\u1021])\u1064", "\u1064$1$2$3"
                                                        , "(\u1031)?(\u103C)?([\u1000-\u1021])\u108B", "\u1064$1$2$3\u102D"
                                                        , "(\u1031)?(\u103C)?([\u1000-\u1021])\u108C", "\u1064$1$2$3\u102E"
                                                        , "(\u1031)?(\u103C)?([\u1000-\u1021])\u108D", "\u1064$1$2$3\u1036"
                                                        , "\u105A", "\u102B\u103A"
                                                        , "\u108E", "\u102D\u1036"
                                                        , "\u1033", "\u102F"
                                                        , "\u1034", "\u1030"
                                                        , "\u1088", "\u103E\u102F"
                                                        , "\u1089", "\u103E\u1030"
                                                        , "\u1039", "\u103A"
                                                        , "[\u1094\u1095]", "\u1037"
                                                        , "([\u1000-\u1021])([\u102C\u102D\u102E\u1032\u1036]){1,2}([\u1060\u1061\u1062\u1063\u1065\u1066\u1067\u1068\u1069\u1070\u1071\u1072\u1073\u1074\u1075\u1076\u1077\u1078\u1079\u107A\u107B\u107C\u1085])", "$1$3$2"
                                                        , "\u1064", "\u1004\u103A\u1039"
                                                        , "\u104E", "\u104E\u1004\u103A\u1038"
                                                        , "\u1086", "\u103F"
                                                        , "\u1060", "\u1039\u1000"
                                                        , "\u1061", "\u1039\u1001"
                                                        , "\u1062", "\u1039\u1002"
                                                        , "\u1063", "\u1039\u1003"
                                                        , "\u1065", "\u1039\u1005"
                                                        , "[\u1066\u1067]", "\u1039\u1006"
                                                        , "\u1068", "\u1039\u1007"
                                                        , "\u1069", "\u1039\u1008"
                                                        , "\u106C", "\u1039\u100B"
                                                        , "\u1070", "\u1039\u100F"
                                                        , "[\u1071\u1072]", "\u1039\u1010"
                                                        , "[\u1073\u1074]", "\u1039\u1011"
                                                        , "\u1075", "\u1039\u1012"
                                                        , "\u1076", "\u1039\u1013"
                                                        , "\u1077", "\u1039\u1014"
                                                        , "\u1078", "\u1039\u1015"
                                                        , "\u1079", "\u1039\u1016"
                                                        , "\u107A", "\u1039\u1017"
                                                        , "\u107B", "\u1039\u1018"
                                                        , "\u107C", "\u1039\u1019"
                                                        , "\u1085", "\u1039\u101C"
                                                        , "\u106D", "\u1039\u100C"
                                                        , "\u1091", "\u100F\u1039\u100D"
                                                        , "\u1092", "\u100B\u1039\u100C"
                                                        , "\u1097", "\u100B\u1039\u100B"
                                                        , "\u106F", "\u100E\u1039\u100D"
                                                        , "\u106E", "\u100D\u1039\u100D"
                                                        , "(\u103C)([\u1000-\u1021])(\u1039[\u1000-\u1021])?", "$2$3$1"
                                                        , "(\u103E)(\u103D)([\u103B\u103C])", "$3$2$1"
                                                        , "(([\u1000-\u101C\u101E-\u102A\u102C\u102E-\u103F\u104C-\u109F]))(\u1040)", "$1\u101D"
                                                        , "([\u1000-\u101C\u101E-\u102A\u102C\u102E-\u103F\u104C-\u109F ])(\u1047)", "$1\u101B"
                                                        , "(\u1047)([\u1000-\u101C\u101E-\u102A\u102C\u102E-\u103F\u104C-\u109F ])", "\u101B$2"
                                                        , "(\u103E)([\u103B\u103C])", "$2$1"
                                                        , "(\u103D)([\u103B\u103C])", "$2$1"
                                                        , "(\u1047)( ? = [\u1000 - \u101C\u101E - \u102A\u102C\u102E - \u103F\u104C - \u109F ])", "\u101B"
                                                        , "(\u1031)?([\u1000-\u1021])(\u1039[\u1000-\u1021])?([\u102D\u102E\u1032])?([\u1036\u1037\u1038]{0,2})([\u103B-\u103E]{0,3})([\u102F\u1030])?([\u1036\u1037\u1038]{0,2})([\u102D\u102E\u1032])?", "$2$3$6$1$4$9$7$5$8"
                                                        , "\u1036\u102F", "\u102F\u1036"
                                                        , "(\u103A)(\u1037)", "$2$1"
                                                        , "\u1005\u103B", "\u1008"
                                                        , "\u101E\u103C", "\u1029"
                                                        , "\u101E\u103C\u1031\u102C\u103A", "\u102A" };

            this.mNZg2Zg = new string[] { "\u102B\u1039", "\u105A"
                                                        , "\u103F\u1000", "\u1060"
                                                        , "\u103F\u1001", "\u1061"
                                                        , "\u103F\u1002", "\u1062"
                                                        , "\u103F\u1003", "\u1063"
                                                        , "\u103F\u1004", "\u1064"
                                                        , "\u103F\u1005", "\u1065"
                                                        , "\u103F\u1006", "\u1066"
                                                        , "\u103F\u1006", "\u1067"
                                                        , "\u103F\u1007", "\u1068"
                                                        , "\u103F\u1008", "\u1069"
                                                        , "\u103F\u1025", "\u106A"
                                                        , "\u103F\u100A", "\u106B"
                                                        , "\u103F\u100B", "\u106C"
                                                        , "\u103F\u100C", "\u106D"
                                                        , "\u100D\u103F\u100D", "\u106E"
                                                        , "\u100D\u103F\u100E", "\u106F"
                                                        , "\u103F\u100F", "\u1070"
                                                        , "\u103F\u1010", "\u1071"
                                                        , "\u103F\u1010", "\u1072"
                                                        , "\u103F\u1011", "\u1073"
                                                        , "\u103F\u1011", "\u1074"
                                                        , "\u103F\u1012", "\u1075"
                                                        , "\u103F\u1013", "\u1076"
                                                        , "\u103F\u1014", "\u1077"
                                                        , "\u103F\u1015", "\u1078"
                                                        , "\u103F\u1016", "\u1079"
                                                        , "\u103F\u1017", "\u107A"
                                                        , "\u103F\u1018", "\u107B"
                                                        , "\u103F\u1019", "\u107C"
                                                        , "\u103F\u101C", "\u1085"
                                                        , "\u101E\u103F\u101E", "\u1086"
                                                        , "\u103D\u102F", "\u1088"
                                                        , "\u103D\u1030", "\u1089"
                                                        , "\u103C\u103D", "\u108A"
                                                        , "\u103F\u1004\u102D", "\u108B"
                                                        , "\u103F\u1004\u102E", "\u108C"
                                                        , "\u103F\u1004\u1036", "\u108D"
                                                        , "\u102D\u1036", "\u108E"
                                                        , "\u100F\u103F\u100D", "\u1091"
                                                        , "\u100B\u103F\u100C", "\u1092"
                                                        , "\u103F\u1018", "\u1093"
                                                        , "\u103F\u1010\u103C", "\u1096"
                                                        , "\u100B\u103F\u100B", "\u1097"
                                                        , "\u102C\u103E", "\u102C\u1039"
                                                        , "\u102B\u103E", "\u102B\u1039"};
        }
        #endregion

        #region Private Methods
        private string Decode(string str, char unknownCh)
        {
            StringBuilder sb = new StringBuilder();
            int i1 = 0;
            int i2 = 0;

            while (i2 < str.Length)
            {
                i1 = str.IndexOf("&#", i2);
                if (i1 == -1)
                {
                    sb.Append(str.Substring(i2));
                    break;
                }
                sb.Append(str.Substring(i2, i1));
                i2 = str.IndexOf(";", i1);
                if (i2 == -1)
                {
                    sb.Append(str.Substring(i1));
                    break;
                }

                string tok = str.Substring(i1 + 2, i2);
                try
                {
                    int radix = 10;
                    if ((tok[0] == 'x') || (tok[0] == 'X'))
                    {
                        radix = 16;
                        tok = tok.Substring(1);
                    }
                    sb.Append((char)Convert.ToInt32(tok, radix));
                }
                catch (Exception exp)
                {
                    System.Diagnostics.Debug.WriteLine(exp.ToString());
                    sb.Append(unknownCh);
                }
                i2++;
            }
            return sb.ToString();
        }

        private string Encode(string str)
        {
            char[] ch = str.ToCharArray();
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < ch.Length; i++)
            {
                //if ((ch[i] < ' ') || (ch[i] > ''))
                if (((int)ch[i] < 0x0020) || ((int)ch[i] > 0x007F))
                    sb.Append("&#").Append(((int)ch[i]).ToString("X4")).Append(";");
                else
                    sb.Append(ch[i]);
            }
            return sb.ToString();
        }
        #endregion

        #region Public Methods
        public void Test()
        {
            string input = "ါှ";
            string output = ToUnicodeValue(input);
            System.Diagnostics.Trace.WriteLine(output);
        }

        public string ZawgyiToUnicode(string input)
        {
            string[] strPatterns = this.mZg2Uni;
            return Replace(input, strPatterns);
        }

        public string NewZawGyiToZawGyi(string input)
        {
            string[] strPatterns = this.mNZg2Zg;
            return Replace(input, strPatterns);
        }
        #endregion

        #region Static Methods
        public static string ToXmlUnicode(string input)
        {
            StringBuilder builder = new StringBuilder();
            foreach (char ch in input)
            {
                if (((int)ch >= 0x1000) && ((int)ch <= 0x1200))
                //if (((int)ch < 0x0020) || ((int)ch > 0x007F))
                {
                    builder.AppendFormat(@"&#{0};", (int)ch);
                }
                else
                    builder.Append(ch);
            }

            return builder.ToString();
        }

        public static string ToUnicodeValue(string input)
        {
            StringBuilder builder = new StringBuilder();
            foreach (char ch in input)
            {
                if (((int)ch >= 0x1000) && ((int)ch <= 0x1200))
                //if (((int)ch < 0x0020) || ((int)ch > 0x007F))
                {
                    builder.AppendFormat(@"\u{0:X4}", (int)ch);
                }
                else
                    builder.Append(ch);
            }

            return builder.ToString();
        }

        private static string Replace(string value, string[] parttens)
        {
            string output = value;
            int patCount = parttens.Length / 2;

            for (int i = 0; i < patCount; i++)
            {
                int idx = i * 2;
                output = Regex.Replace(output, parttens[idx], parttens[(idx + 1)]);
            }

            return output;
        }

        private static string Replace(string value, string partten, string replacement)
        {
            return Regex.Replace(value, partten, replacement);
        }

        private delegate string ReplaceCallback(string g0, string g1);
        private static string Replace(string value, string partten, ReplaceCallback replacement)
        {
            ReplaceCallback callback = replacement;
            return Regex.Replace(value, partten, (Match match) => {
                if (match.Success)
                {
                    if (match.Groups.Count > 1)
                    {
                        return callback(match.Groups[0].Value, match.Groups[1].Value);
                    }
                    else if (match.Groups.Count == 1)
                    {
                        return callback(match.Groups[0].Value, string.Empty);
                    }
                }

                return string.Empty; 
            });
        }

        public static string Uni_Z1(string input)
        {
            string output = input;

            output = Replace(output, @"\u104E\u1004\u103A\u1038", "\u104E");
            output = Replace(output, @"\u102B\u103A", "\u105A");
            output = Replace(output, @"\u102D\u1036", "\u108E");
            output = Replace(output, @"\u103F", "\u1086");

            output = Replace(output, @"(\u102F[\u1036]?)\u1037", (string g0, string g1) => {return !string.IsNullOrEmpty(g1) ? g1 + "\u1094" : g0 + g1;});
            output = Replace(output, @"(\u1030[\u1036]?)\u1037", (string g0, string g1) => {return !string.IsNullOrEmpty(g1) ? g1 + "\u1094" : g0 + g1;});
            output = Replace(output, @"(\u1014[\u103A\u1032]?)\u1037", (string g0, string g1) => {return !string.IsNullOrEmpty(g1) ? g1 + "\u1094" : g0 + g1;});
            output = Replace(output, @"(\u103B[\u1032\u1036]?)\u1037", (string g0, string g1) => {return !string.IsNullOrEmpty(g1) ? g1 + "\u1095" : g0 + g1;});

            output = Replace(output, @"(\u103D[\u1032]?)\u1037",  (string g0, string g1) => {return !string.IsNullOrEmpty(g1) ? g1 + "\u1095" : g0 + g1;});
            output = Replace(output, @"([\u103B\u103C\u103D][\u102D\u1036]?)\u102F", (string g0, string g1) => {return !string.IsNullOrEmpty(g1) ? g1 + "\u1033" : g0 + g1;});
            output = Replace(output, @"((\u1039[\u1000-\u1021])[\u102D\u1036]?)\u102F",  (string g0, string g1) => {return !string.IsNullOrEmpty(g1) ? g1 + "\u1033" : g0 + g1;});
            output = Replace(output, @"([\u100A\u100C\u1020\u1025\u1029][\u102D\u1036]?)\u102F", (string g0, string g1) => {return !string.IsNullOrEmpty(g1) ? g1 + "\u1033" : g0 + g1;});
            output = Replace(output, @"([\u103B\u103C][\u103D]?[\u103E]?[\u102D\u1036]?)\u1030", (string g0, string g1) => {return !string.IsNullOrEmpty(g1) ? g1 + "\u1034" : g0 + g1;});
            // uu - 2
            output = Replace(output, @"((\u1039[\u1000-\u1021])[\u102D\u1036]?)\u1030", (string g0, string g1) => {return !string.IsNullOrEmpty(g1) ? g1 + "\u1034" : g0 + g1;});
            // uu - 2
            output = Replace(output, @"([\u100A\u100C\u1020\u1025\u1029][\u102D\u1036]?)\u1030", (string g0, string g1) => {return !string.IsNullOrEmpty(g1) ? g1 + "\u1034" : g0 + g1;});
            // uu - 2

            output = Replace(output, @"(\u103C)\u103E", (string g0, string g1) => {return !string.IsNullOrEmpty(g1) ? g1 + "\u1087" : g0 + g1;});
            // ha - 2


            output = Replace(output, @"\u1009(?=[\u103A])", "\u1025");
            output = Replace(output, @"\u1009(?=\u1039[\u1000-\u1021])", "\u1025");

            // E render
            output = Replace(output, @"([\u1000-\u1021\u1029])(\u1039[\u1000-\u1021])?([\u103B-\u103E\u1087]*)?\u1031", "\u1031$1$2$3");

            // Ra render
            output = Replace(output, @"([\u1000-\u1021\u1029])(\u1039[\u1000-\u1021\u1000-\u1021])?(\u103C)", "$3$1$2");

            // Kinzi
            output = Replace(output, @"\u1004\u103A\u1039", "\u1064");
            // kinzi
            output = Replace(output, @"(\u1064)([\u1031]?)([\u103C]?)([\u1000-\u1021])\u102D", "$2$3$4\u108B");
            // reordering kinzi lgt
            output = Replace(output, @"(\u1064)(\u1031)?(\u103C)?([\u1000-\u1021])\u102E", "$2$3$4\u108C");
            // reordering kinzi lgtsk
            output = Replace(output, @"(\u1064)(\u1031)?(\u103C)?([\u1000-\u1021])\u1036", "$2$3$4\u108D");
            // reordering kinzi ttt
            output = Replace(output, @"(\u1064)(\u1031)?(\u103C)?([\u1000-\u1021])", "$2$3$4\u1064");
            // reordering kinzi

            // Consonant

            output = Replace(output, @"\u100A(?=[\u1039\u102F\u1030])", "\u106B");
            // nnya - 2
            output = Replace(output, @"\u100A", "\u100A");
            // nnya

            output = Replace(output, @"\u101B(?=[\u102F\u1030])", "\u1090");
            // ra - 2
            output = Replace(output, @"\u101B", "\u101B");
            // ra

            output = Replace(output, @"\u1014(?=[\u1039\u103D\u103E\u102F\u1030])", "\u108F");
            // na - 2
            output = Replace(output, @"\u1014", "\u1014");
            // na

            // Stacked consonants
            output = Replace(output, @"\u1039\u1000", "\u1060");
            output = Replace(output, @"\u1039\u1001", "\u1061");
            output = Replace(output, @"\u1039\u1002", "\u1062");
            output = Replace(output, @"\u1039\u1003", "\u1063");
            output = Replace(output, @"\u1039\u1005", "\u1065");
            output = Replace(output, @"\u1039\u1006", "\u1066");
            // 1067
            output = Replace(output, @"([\u1001\u1002\u1004\u1005\u1007\u1012\u1013\u108F\u1015\u1016\u1017\u1019\u101D])\u1066", (string g0, string g1) => {return !string.IsNullOrEmpty(g1) ? g1 + "\u1067" : g0 + g1;});
            // 1067
            output = Replace(output, @"\u1039\u1007", "\u1068");
            output = Replace(output, @"\u1039\u1008", "\u1069");

            output = Replace(output, @"\u1039\u100F", "\u1070");
            output = Replace(output, @"\u1039\u1010", "\u1071");
            // 1072 omit (little shift to right)
            output = Replace(output, @"([\u1001\u1002\u1004\u1005\u1007\u1012\u1013\u108F\u1015\u1016\u1017\u1019\u101D])\u1071", (string g0, string g1) => {return !string.IsNullOrEmpty(g1) ? g1 + "\u1072" : g0 + g1;});
            // 1067
            output = Replace(output, @"\u1039\u1011", "\u1073");
            // \u1074 omit(little shift to right)
            output = Replace(output, @"([\u1001\u1002\u1004\u1005\u1007\u1012\u1013\u108F\u1015\u1016\u1017\u1019\u101D])\u1073", (string g0, string g1) => {return !string.IsNullOrEmpty(g1) ? g1 + "\u1074" : g0 + g1;});
            // 1067
            output = Replace(output, @"\u1039\u1012", "\u1075");
            output = Replace(output, @"\u1039\u1013", "\u1076");
            output = Replace(output, @"\u1039\u1014", "\u1077");
            output = Replace(output, @"\u1039\u1015", "\u1078");
            output = Replace(output, @"\u1039\u1016", "\u1079");
            output = Replace(output, @"\u1039\u1017", "\u107A");
            output = Replace(output, @"\u1039\u1018", "\u107B");
            output = Replace(output, @"\u1039\u1019", "\u107C");
            output = Replace(output, @"\u1039\u101C", "\u1085");


            output = Replace(output, @"\u100F\u1039\u100D", "\u1091");
            output = Replace(output, @"\u100B\u1039\u100C", "\u1092");
            output = Replace(output, @"\u1039\u100C", "\u106D");
            output = Replace(output, @"\u100B\u1039\u100B", "\u1097");
            output = Replace(output, @"\u1039\u100B", "\u106C");
            output = Replace(output, @"\u100E\u1039\u100D", "\u106F");
            output = Replace(output, @"\u100D\u1039\u100D", "\u106E");

            output = Replace(output, @"\u1009(?=\u103A)", "\u1025");
            // u
            output = Replace(output, @"\u1025(?=[\u1039\u102F\u1030])", "\u106A");
            // u - 2
            output = Replace(output, @"\u1025", "\u1025");
            // u
            /////////////////////////////////////

            output = Replace(output, @"\u103A", "\u1039");
            // asat

            output = Replace(output, @"\u103B\u103D\u103E", "\u107D\u108A");
            // ya wa ha
            output = Replace(output, @"\u103D\u103E", "\u108A");
            // wa ha

            output = Replace(output, @"\u103B", "\u103A");
            // ya
            output = Replace(output, @"\u103C", "\u103B");
            // ra
            output = Replace(output, @"\u103D", "\u103C");
            // wa
            output = Replace(output, @"\u103E", "\u103D");
            // ha
            output = Replace(output, @"\u103A(?=[\u103C\u103D\u108A])", "\u107D");
            // ya - 2

            output = Replace(output, @"(\u100A(?:[\u102D\u102E\u1036\u108B\u108C\u108D\u108E])?)\u103D", (string g0, string g1) => {return !string.IsNullOrEmpty(g1) ? g1 + "\u1087" : g0 ;});
            // ha - 2

            output = Replace(output, @"\u103B(?=[\u1000\u1003\u1006\u100F\u1010\u1011\u1018\u101A\u101C\u101E\u101F\u1021])", "\u107E");
            // great Ra with wide consonants
            output = Replace(output, @"\u107E([\u1000-\u1021\u108F])(?=[\u102D\u102E\u1036\u108B\u108C\u108D\u108E])", "\u1080$1");
            // great Ra with upper sign
            output = Replace(output, @"\u107E([\u1000-\u1021\u108F])(?=[\u103C\u108A])", "\u1082$1");
            // great Ra with under signs

            output = Replace(output, @"\u103B([\u1000-\u1021\u108F])(?=[\u102D \u102E \u1036 \u108B \u108C \u108D \u108E])", "\u107F$1");
            // little Ra with upper sign

            output = Replace(output, @"\u103B([\u1000-\u1021\u108F])(?=[\u103C\u108A])", "\u1081$1");
            // little Ra with under signs

            output = Replace(output, @"(\u1014[\u103A\u1032]?)\u1037", (string g0, string g1) => {return !string.IsNullOrEmpty(g1) ? g1 + "\u1094" : g0 + g1;});
            // aukmyint
            output = Replace(output, @"(\u1033[\u1036]?)\u1094", (string g0, string g1) => {return !string.IsNullOrEmpty(g1) ? g1 + "\u1095" : g0 + g1;});
            // aukmyint
            output = Replace(output, @"(\u1034[\u1036]?)\u1094", (string g0, string g1) => {return !string.IsNullOrEmpty(g1) ? g1 + "\u1095" : g0 + g1;});
            // aukmyint
            output = Replace(output, @"([\u103C\u103D\u108A][\u1032]?)\u1037", (string g0, string g1) => {return !string.IsNullOrEmpty(g1) ? g1 + "\u1095" : g0 + g1;});

            return output;
        }

        public static string Z1_Uni(string input)
        {
            string output = input;

            output = Replace(output, @"\u106A", " \u1009");
            output = Replace(output, @"\u106B", "\u100A");
            output = Replace(output, @"\u1090", "\u101B");
            output = Replace(output, @"\u1040", "\u1040");

            output = Replace(output, @"\u108F", "\u1014");
            output = Replace(output, @"\u1012", "\u1012");
            output = Replace(output, @"\u1013", "\u1013");
            /////////////


            output = Replace(output, @"[\u103D\u1087]", "\u103E");
            // ha
            output = Replace(output, @"\u103C", "\u103D");
            // wa
            output = Replace(output, @"[\u103B\u107E\u107F\u1080\u1081\u1082\u1083\u1084]", "\u103C");
            // ya yint(ra)
            output = Replace(output, @"[\u103A\u107D]", "\u103B");
            // ya

            output = Replace(output, "\u103E\u103B", "\u103B\u103E");
            // reorder

            output = Replace(output, @"\u108A", "\u103D\u103E");
            // wa ha

            ////////////////////// Reordering

            output = Replace(output, @"(\u1031)?(\u103C)?([\u1000-\u1021])\u1064", "\u1064$1$2$3");
            // reordering kinzi
            output = Replace(output, @"(\u1031)?(\u103C)?([\u1000-\u1021])\u108B", "\u1064$1$2$3\u102D");
            // reordering kinzi lgt
            output = Replace(output, @"(\u1031)?(\u103C)?([\u1000-\u1021])\u108C", "\u1064$1$2$3\u102E");
            // reordering kinzi lgtsk
            output = Replace(output, @"(\u1031)?(\u103C)?([\u1000-\u1021])\u108D", "\u1064$1$2$3\u1036");
            // reordering kinzi ttt

            ////////////////////////////////////////

            output = Replace(output, @"\u105A", "\u102B\u103A");
            output = Replace(output, @"\u108E", "\u102D\u1036");
            // lgt ttt
            output = Replace(output, @"\u1033", "\u102F");
            output = Replace(output, @"\u1034", "\u1030");
            output = Replace(output, @"\u1088", "\u103E\u102F");
            // ha  u
            output = Replace(output, @"\u1089", "\u103E\u1030");
            // ha  uu

            ///////////////////////////////////////

            output = Replace(output, @"\u1039", "\u103A");
            output = Replace(output, @"[\u1094\u1095]", "\u1037");
            // aukmyint

            ///////////////////////////////////////


            output = Replace(output, @"\u1064", "\u1004\u103A\u1039");

            output = Replace(output, @"\u104E", "\u104E\u1004\u103A\u1038");

            output = Replace(output, @"\u1086", "\u103F");

            output = Replace(output, @"\u1060", "\u1039\u1000");
            output = Replace(output, @"\u1061", "\u1039\u1001");
            output = Replace(output, @"\u1062", "\u1039\u1002");
            output = Replace(output, @"\u1063", "\u1039\u1003");
            output = Replace(output, @"\u1065", "\u1039\u1005");
            output = Replace(output, @"[\u1066\u1067]", "\u1039\u1006");
            output = Replace(output, @"\u1068", "\u1039\u1007");
            output = Replace(output, @"\u1069", "\u1039\u1008");
            output = Replace(output, @"\u106C", "\u1039\u100B");
            output = Replace(output, @"\u1070", "\u1039\u100F");
            output = Replace(output, @"[\u1071\u1072]", "\u1039\u1010");
            output = Replace(output, @"[\u1073\u1074]", "\u1039\u1011");
            output = Replace(output, @"\u1075", "\u1039\u1012");
            output = Replace(output, @"\u1076", "\u1039\u1013");
            output = Replace(output, @"\u1077", "\u1039\u1014");
            output = Replace(output, @"\u1078", "\u1039\u1015");
            output = Replace(output, @"\u1079", "\u1039\u1016");
            output = Replace(output, @"\u107A", "\u1039\u1017");
            output = Replace(output, @"\u107B", "\u1039\u1018");
            output = Replace(output, @"\u107C", "\u1039\u1019");
            output = Replace(output, @"\u1085", "\u1039\u101C");
            output = Replace(output, @"\u106D", "\u1039\u100C");

            output = Replace(output, @"\u1091", "\u100F\u1039\u100D");
            output = Replace(output, @"\u1092", "\u100B\u1039\u100C");
            output = Replace(output, @"\u1097", "\u100B\u1039\u100B");
            output = Replace(output, @"\u106F", "\u100E\u1039\u100D");
            output = Replace(output, @"\u106E", "\u100D\u1039\u100D");

            /////////////////////////////////////////////////////////

            output = Replace(output, @"(\u103C)([\u1000-\u1021])(\u1039[\u1000-\u1021])?", "$2$3$1");
            // reordering ra

            //output = Replace(output, @"(([\u1000-\u101C\u101E-\u102A\u102C\u102E-\u103F\u104C-\u109F\u0020]))(\u1040)", (string g0, string g1) => {return !string.IsNullOrEmpty(g1) ? g1 + "\u101D" : g0 + g1;});
            output = Replace(output, @"(([\u1000-\u101C\u101E-\u102A\u102C\u102E-\u103F\u104C-\u109F\s]))(\u1040)", (string g0, string g1) => { return !string.IsNullOrEmpty(g1) ? g1 + "\u101D" : g0 + g1; });
            // zero and wa

            //output = Replace(output, @"(\u1040)(?=[\u1000-\u101C\u101E-\u102A\u102C\u102E-\u103F\u104C-\u109F\u0020])", "\u101D");
            output = Replace(output, @"(\u1040)(?=[\u1000-\u101C\u101E-\u102A\u102C\u102E-\u103F\u104C-\u109F\s])", "\u101D");
            // zero and wa

            //output = Replace(output, @"(([\u1000-\u101C\u101E-\u102A\u102C\u102E-\u103F\u104C-\u109F\u0020]))(\u1047)", (string g0, string g1) => {return !string.IsNullOrEmpty(g1) ? g1 + "\u101B" : g0 + g1;});
            output = Replace(output, @"(([\u1000-\u101C\u101E-\u102A\u102C\u102E-\u103F\u104C-\u109F\s]))(\u1047)", (string g0, string g1) => { return !string.IsNullOrEmpty(g1) ? g1 + "\u101B" : g0 + g1; });
            // seven and ra

            //output = Replace(output, @"(\u1047)( ? = [\u1000 - \u101C\u101E - \u102A\u102C\u102E - \u103F\u104C - \u109F\u0020])", "\u101B");
            output = Replace(output, @"(\u1047)( ? = [\u1000 - \u101C\u101E - \u102A\u102C\u102E - \u103F\u104C - \u109F\s])", "\u101B");
            // seven and ra

            output = Replace(output, @"(\u1031)?([\u1000-\u1021])(\u1039[\u1000-\u1021])?([\u102D\u102E\u1032])?([\u1036\u1037\u1038]){0,2}([\u103B-\u103E])*([\u102F\u1030])?([\u102D\u102E\u1032])?", "$2$3$6$1$4$8$7$5");
            // reordering storage order

            return output;
        }
        #endregion
    }
}
