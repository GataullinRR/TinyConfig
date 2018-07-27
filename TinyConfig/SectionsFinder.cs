using System.Collections.Generic;
using System.Linq;
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

        public static IEnumerable<SectionInfo> GetSections(string[] iniFile)
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
                        var sectionLoaction = bodyLocation
                            .SetFrom(sectionName == null ? bodyLocation.From : bodyLocation.From - 1);
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
}
