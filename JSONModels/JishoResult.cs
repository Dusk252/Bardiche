using System;
using System.Collections.Generic;
using System.Text;

namespace Bardiche.JSONModels
{
    public class Japanese
    {
        public string word { get; set; }
        public string reading { get; set; }
    }

    public class Sens
    {
        public List<string> english_definitions { get; set; }
        public List<string> parts_of_speech { get; set; }
        public List<string> tags { get; set; }
        public List<string> restrictions { get; set; }
        public List<string> see_also { get; set; }

        public string getparts()
        {
            StringBuilder sb = new StringBuilder();
            if (parts_of_speech.Count > 0)
            {
                sb.Append("(");
                foreach (string s in parts_of_speech)
                {
                    sb.Append(s + ", ");
                }
                sb.Remove(sb.Length - 2, 2);
                sb.Append(')');
            }
            return sb.ToString();
        }

        public string getothers()
        {
            StringBuilder sb = new StringBuilder();
            if (see_also.Count > 0)
            {
                foreach (string s in see_also)
                {
                    sb.Append(s + ", ");
                }
                sb.Remove(sb.Length - 2, 2);
            }
            return sb.ToString();
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            if (english_definitions.Count > 0)
            {
                foreach (string s in english_definitions)
                {
                    if ((parts_of_speech.Count == 0) || (!(parts_of_speech.Contains("Wikipedia definition"))))
                    {
                        sb.Append(s + "; ");
                    }
                }
                if (sb.Length > 2)
                {
                    sb.Remove(sb.Length - 2, 2);
                }
            }
            
            if (restrictions.Count > 0) {
                sb.Append(" (");
                foreach (string s in restrictions)
                {
                    sb.Append("only applies to " + s + ", ");
                }
                sb.Remove(sb.Length-2, 2);
                sb.Append(')');
            }

            if (tags.Count > 0)
            {
                if (!string.IsNullOrWhiteSpace(tags[0]))
                {
                    sb.Append(" [");
                    foreach (string s in tags)
                    {
                        sb.Append(s.ToLower() + ", ");
                    }
                    sb.Remove(sb.Length - 2, 2);
                    sb.Append(']');
                }  
            }
            return sb.ToString();
        }
    }

    public class JishoResult
    {
        public bool is_common { get; set; }
        public List<Japanese> japanese { get; set; }
        public List<Sens> senses { get; set; }

        private string words;

        private string formatsenses()
        {
            StringBuilder sb = new StringBuilder();
            foreach (Sens s in senses)
            {
                string temp = s.ToString();
                if (!String.IsNullOrWhiteSpace(temp))
                {
                    sb.Append((senses.IndexOf(s) + 1) + ". ");
                    sb.Append(temp + '\n');
                }
            }
            return sb.ToString();
        }

        private string formatforms()
        {
            StringBuilder sb = new StringBuilder();
            japanese.RemoveAt(0);
            if (japanese.Count > 0)
            {
                foreach (Japanese s in japanese)
                {
                    sb.Append(s.word + " [" + s.reading + "], ");
                }
                sb.Remove(sb.Length - 2, 2);
            }
            words = sb.ToString();
            return words;
        }

        public override string ToString() =>
            "```" +
            japanese[0].word + " 【" + japanese[0].reading + "】" +
            (is_common ? " [common word]" : "") + "\n\n" +
            senses[0].getparts() + '\n' +
            formatsenses() +
            (String.IsNullOrWhiteSpace(formatforms()) ? "" : ("\nOther forms:\n" + words)) +
            (String.IsNullOrWhiteSpace(senses[0].getothers()) ? "" : ("\n\nSee also:\n" + senses[0].getothers())) +
            "```";
    }
}
