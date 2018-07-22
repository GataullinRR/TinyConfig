using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Utilities;
using Utilities.Extensions;
using Utilities.Types;
using Vectors;

namespace TinyConfig
{
    static class SectionsFinder
    {
        public class SectionInfo
        {
            public Section Section { get; }
            public IEnumerable<string> FullSection { get; }
            public IEnumerable<string> Body { get; }
            public IntInterval SectionLocation { get; }
            public IntInterval SectionBodyLocation { get; }

            public SectionInfo
                (string sectionName, IEnumerable<string> section, IEnumerable<string> body, IntInterval sectionLocation, IntInterval sectionBodyLocation)
            {
                Section = new Section(sectionName);
                FullSection = section;
                Body = body;
                SectionLocation = sectionLocation;
                SectionBodyLocation = sectionBodyLocation;
            }

            public override string ToString()
            {
                return new { Section, SectionLocation, SectionBodyLocation, Body = Body.AsMultilineString() }.ToString();
            }
        }

        public static IEnumerable<SectionInfo> GetSections(IEnumerable<string> iniFile)
        {
            var readSections = new List<string>();
            string currentSectionName = null;
            var linesInCurrentSection = 0;
            var lineEnumerator = new EnhancedEnumerator<string>(iniFile);
            while (lineEnumerator.MoveNext())
            {
                var line = lineEnumerator.Current;
                var sectionName = tryExtractSectionName();
                if (sectionName == null)
                {
                    linesInCurrentSection++;
                }
                if (sectionName != null || lineEnumerator.IsLastElement)
                {
                    if (readSections.NotContains(currentSectionName))
                    {
                        var bodyLocation = new IntInterval(lineEnumerator.Index - linesInCurrentSection, lineEnumerator.Index);
                        var sectionLoaction = bodyLocation.SetFrom(bodyLocation.From - 1);
                        if (lineEnumerator.IsLastElement)
                        {
                            sectionLoaction += 1;
                            bodyLocation += 1;
                        }
                        var body = iniFile.Skip(bodyLocation.From).Take(bodyLocation.Len);
                        var section = iniFile.Skip(sectionLoaction.From).Take(sectionLoaction.Len);
                        yield return new SectionInfo(currentSectionName, section, body, sectionLoaction, bodyLocation);
                        readSections.Add(currentSectionName);
                    }
                    if (lineEnumerator.IsLastElement && sectionName != null && readSections.NotContains(sectionName))
                    {
                        yield return new SectionInfo(sectionName, new string[0], new string[0], IntInterval.Zero, IntInterval.Zero);
                    }
                    linesInCurrentSection = 0;
                    currentSectionName = sectionName;
                }

                /////////////////////////////////

                string tryExtractSectionName()
                {
                    var containsSection = line
                        .SkipWhile(char.IsWhiteSpace).Aggregate()
                        .StartsWith(Constants.SECTION_HEADER_OPEN_MARK);
                    var name = line
                        .Between(Constants.SECTION_HEADER_OPEN_MARK, Constants.SECTION_HEADER_CLOSE_MARK, false, false);

                    return containsSection && isNameValid()
                        ? name
                        : null;

                    bool isNameValid()
                    {
                        return name.Replace(Constants.SUBSECTION_SEPARATOR, "").All(char.IsLetterOrDigit);
                    }
                }
            }
        }
    }

    static class KVPExtractor
    {

        public static IEnumerable<ConfigKVP> ExtractAll(StreamReader reader)
        {
            var iniFile = reader.ReadAllLines().ToArray();
            foreach (var section in SectionsFinder.GetSections(iniFile))
            {
                foreach (var kvp in exctractFromSection(section))
                {
                    yield return kvp;
                }
            }

            /////////////////////////////////
        
            IEnumerable<ConfigKVP> exctractFromSection(SectionsFinder.SectionInfo section)
            {
                var lineEnumerator = section.Body.GetEnumerator();
                while (lineEnumerator.MoveNext())
                {
                    var line = lineEnumerator.Current;
                    var key = extractKey();
                    yield return extractKVP();

                    //////////////////////////////////

                    string extractKey()
                    {
                        return line
                               .Split(Constants.KVP_SEPERATOR)[0]
                               .SkipFromEndWhile(c => !char.IsLetterOrDigit(c))
                               .Aggregate();
                    }

                    ConfigKVP extractKVP()
                    {
                        var kvp = isOneLineKVP()
                                  ? extractSinglelineKVP(key, line)
                                  : extractMultilineKVP(key, lineEnumerator);

                        return new ConfigKVP(section.Section, kvp.Key, kvp.Value, kvp.Commentary);

                        //////////////////////////////////

                        bool isOneLineKVP()
                        {
                            return !line.Contains(Constants.MULTILUNE_VALUE_MARK) || !line.Contains(Constants.BLOCK_MARK);
                        }
                    }
                }
            }
        }

        static ConfigKVP extractSinglelineKVP(string key, string line)
        {
            var valueStartIndex = line.IndexOf(Constants.KVP_SEPERATOR) + 1;
            var commentBlockStartIndex = line.IndexOf(Constants.COMMENT_SEPARATOR);
            var value = commentBlockStartIndex < 0
                        ? line.Substring(valueStartIndex)
                        : line.Substring(valueStartIndex, commentBlockStartIndex - valueStartIndex);
            var comment = commentBlockStartIndex < 0
                        ? null
                        : line.Substring(commentBlockStartIndex + Constants.COMMENT_SEPARATOR.Length);

            return new ConfigKVP(key, new ConfigValue(value, false), comment);
        }

        static ConfigKVP extractMultilineKVP(string key, IEnumerator<string> lineEnumerator)
        {
            var line = lineEnumerator.Current;
            var value = line.Substring(line.IndexOf(Constants.BLOCK_MARK) + 1) + Global.NL;
            string commentary = null;
            StringBuilder valueAsStr = new StringBuilder();
            bool isBlockClosed = false;
            while (true)
            {
                var valueBlockEnd = 0;
                for (int i = 1; i < value.Length; i++)
                {
                    var prev = value[i - 1];
                    var curr = value[i];
                    if (prev == Constants.BLOCK_MARK && curr == Constants.BLOCK_MARK)
                    {
                        i++;
                        valueAsStr.Append(Constants.BLOCK_MARK);
                        continue;
                    }
                    else if (prev == Constants.BLOCK_MARK && curr != Constants.BLOCK_MARK)
                    {
                        isBlockClosed = true;
                        valueBlockEnd = i - 1;
                        break;
                    }
                    else
                    {
                        valueAsStr.Append(prev);
                        if (!isBlockClosed && i == value.Length - 1)
                        {
                            valueAsStr.Append(curr);
                        }
                    }
                }


                if (isBlockClosed)
                {
                    var commentBlockStart = line
                        .FindAll(Constants.COMMENT_SEPARATOR)
                        .Where(i => i > valueBlockEnd)
                        .FirstOrDefault(-1);
                    if (commentBlockStart >= 0)
                    {
                        commentary = line.Substring(commentBlockStart + Constants.COMMENT_SEPARATOR.Length);
                    }
                    break;
                }
                else if (lineEnumerator.MoveNext())
                {
                    value = lineEnumerator.Current + Global.NL;
                }
                else // Не удалось найти закрывающую скобку, а стрим уже закончился
                {
                    break;
                }
            }

            return new ConfigKVP(key, new ConfigValue(valueAsStr.ToString(), true), commentary);
        }
    }
}
