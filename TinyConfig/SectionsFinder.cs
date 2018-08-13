using System;
using System.Collections.Generic;
using System.Linq;
using Utilities.Extensions;
using Utilities.Types;
using Vectors;

namespace TinyConfig
{
    static class SectionsFinder
    {
        public class SectionInfo : IEquatable<SectionInfo>
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

            public bool Equals(SectionInfo other)
            {
                return other == null
                    ? false
                    : Section.Equals(other.Section)
                    && FullSection.SequenceEqual(other.FullSection)
                    && Body.SequenceEqual(other.Body)
                    && SectionLocation.Equals(other.SectionLocation)
                    && SectionBodyLocation.Equals(other.SectionBodyLocation);
            }

            public override string ToString()
            {
                return new
                {
                    Section,
                    SectionLocation,
                    SectionBodyLocation,
                    FullSection = FullSection.AsMultilineString()
                }.ToString();
            }
        }

        public static IEnumerable<SectionInfo> GetSections(string[] iniFile)
        {
            return getSectionsWithoutChecking(iniFile)
                .Distinct((si1, si2) => si1.Section.Equals(si2.Section));
        }
        static IEnumerable<SectionInfo> getSectionsWithoutChecking(string[] iniFile)
        {
            var NONE = new IntInterval(-1);
            var EMPTY_ARR = Enumerable.Empty<string>();

            var currentSectionStartIndex = 0;
            string currentSection = null;
            for (int i = 0; i < iniFile.Length; i++)
            {
                var line = iniFile[i];
                var sectionName = tryExtractSectionName();
                var isLastLine = i == iniFile.Length - 1;
                if (sectionName != null || isLastLine)
                {
                    var isRootSection = currentSection == null;
                    var startOfSection = currentSectionStartIndex;
                    // Root section have not [Section] header, so we have not to add 1 in this case
                    var startOfBody = isRootSection ? currentSectionStartIndex : currentSectionStartIndex + 1;
                    // We must not lost last line when coming to an end of file
                    var endOfSection = (isLastLine && sectionName == null) ? i : (i - 1);
                    var sectionLocation = new IntInterval(startOfSection, endOfSection);
                    var bodyLocation = new IntInterval(startOfBody, endOfSection);

                    if (endOfSection == -1)
                    {
                        sectionLocation = NONE;
                        bodyLocation = NONE;
                    }
                    else if (startOfBody > endOfSection)
                    {
                        bodyLocation = currentSection == null ? sectionLocation : NONE;
                    }

                    var body = bodyLocation == NONE
                        ? EMPTY_ARR
                        : iniFile.Skip(bodyLocation.From).Take(bodyLocation.Len + 1);
                    var section = sectionLocation == NONE
                        ? EMPTY_ARR
                        : iniFile.Skip(sectionLocation.From).Take(sectionLocation.Len + 1);
                    yield return new SectionInfo(currentSection, section, body, sectionLocation, bodyLocation);

                    currentSectionStartIndex = i;
                    currentSection = sectionName;
                }
                if (isLastLine && sectionName != null)
                {
                    var section = iniFile.Skip(i).Take(1);
                    yield return new SectionInfo(sectionName, section, EMPTY_ARR, new IntInterval(i), NONE);
                }

                ////////////////////////////////

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
