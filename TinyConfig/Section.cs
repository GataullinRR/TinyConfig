using System;
using System.Linq;
using Utilities.Extensions;

namespace TinyConfig
{
    class Section : IEquatable<Section>
    {
        public static Section InvalidSection = new Section();
        public static Section RootSection = new Section(null);

        public string FullName { get; }
        public int Order { get; }
        public bool IsRoot { get; }
        public bool IsCorrect { get; }

        Section()
        {

        }
        public Section(Section root, string subsection)
            :this(aggregate(root, subsection))
        {

        }
        static string aggregate(Section root, string subsection)
        {
            var gap = root.IsRoot || subsection == null 
                ? null 
                : Constants.SUBSECTION_SEPARATOR;
            var isRoot = root.IsRoot && subsection == null;
            return isRoot ? null : root.FullName + gap + subsection;
        }
        public Section(string fullName)
        {
            FullName = fullName;
            Order = FullName?.FindAll(Constants.SUBSECTION_SEPARATOR)?.Count + 1 ?? 0;
            IsRoot = FullName == null;
            IsCorrect = FullName == null
                || !FullName.EndsWith(Constants.SUBSECTION_SEPARATOR)
                && !FullName.StartsWith(Constants.SUBSECTION_SEPARATOR)
                && FullName.Remove(Constants.SUBSECTION_SEPARATOR).All(char.IsLetterOrDigit);
        }

        public bool IsInsideSection(Section section)
        {
            return IsInsideSection(section.FullName);
        }
        public bool IsInsideSection(string sectionFullName)
        {
            var passedInSection = new Section(sectionFullName);
            //if (!passedInSection.isValid())
            if (!passedInSection.IsCorrect)
            {
                throw new ArgumentException();
            }
            
            if (passedInSection.IsRoot)
            {
                return true;
            }
            else if (IsRoot)
            {
                return false;
            }
            else
            {
                return FullName.StartsWith(passedInSection.FullName);
            }
        }
        //bool isValid()
        //{
        //    return IsRoot || 
        //            (
        //                FullName.Length > 0
        //                && !FullName.StartsWith(Constants.SUBSECTION_SEPARATOR)
        //                && !FullName.EndsWith(Constants.SUBSECTION_SEPARATOR)
        //            );
        //}

        public override bool Equals(object obj)
        {
            if (obj is Section section)
            {
                return Equals(section);
            }
            else
            {
                return false;
            }
        }

        public bool Equals(Section other)
        {
            return FullName == other.FullName
                && IsCorrect == other.IsCorrect;
        }

        public override string ToString()
        {
            return Constants.SECTION_HEADER_OPEN_MARK + FullName + Constants.SECTION_HEADER_CLOSE_MARK;
        }

        ///// <summary>
        ///// Never throws an exception.
        ///// </summary>
        ///// <param name="line"></param>
        ///// <returns></returns>
        //public static Section Parse(string line)
        //{
        //    var containsSection = line
        //        .SkipWhile(char.IsWhiteSpace).Aggregate()
        //        .StartsWith(Constants.SECTION_HEADER_OPEN_MARK);
        //    var name = line
        //        .Between(Constants.SECTION_HEADER_OPEN_MARK, Constants.SECTION_HEADER_CLOSE_MARK, false, false);

        //    var isValid = containsSection && isNameValid();
        //    return isValid ? new Section(name) : InvalidSection;

        //    bool isNameValid()
        //    {
        //        return name.Replace(Constants.SUBSECTION_SEPARATOR, "").All(char.IsLetterOrDigit);
        //    }
        //}
    }
}
