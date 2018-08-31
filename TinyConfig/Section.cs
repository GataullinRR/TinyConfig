using System;
using System.Collections.Generic;
using System.Linq;
using Utilities.Extensions;

namespace TinyConfig
{
    class Section : ConfigEntity, IEquatable<Section>, IComparable<Section>
    {
        public static Section InvalidSection = new Section();
        public static Section RootSection = new Section(null);

        public string FullName { get; }
        public int Order { get; }
        public bool IsRoot { get; }
        public bool IsCorrect { get; }

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
            :this()
        {
            FullName = fullName;
            Order = FullName?.FindAll(Constants.SUBSECTION_SEPARATOR)?.Count() + 1 ?? 0;
            IsRoot = FullName == null;
            IsCorrect = isFullNameValid(FullName);
        }
        static bool isFullNameValid(string fullName)
        {
            return fullName == null
                || !fullName.EndsWith(Constants.SUBSECTION_SEPARATOR)
                && !fullName.StartsWith(Constants.SUBSECTION_SEPARATOR)
                && fullName.Remove(Constants.SUBSECTION_SEPARATOR).All(char.IsLetterOrDigit);
        }
        Section() : base(Types.SECTION)
        {

        }

        public bool IsInsideSection(Section section)
        {
            return IsInsideSection(section.FullName);
        }
        public bool IsInsideSection(string sectionFullName)
        {
            var passedInSection = new Section(sectionFullName);
            if (!passedInSection.IsCorrect || !IsCorrect)
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

        public string GetSubsection(Section rootSection)
        {
            if (!IsInsideSection(rootSection) || IsRoot)
            {
                throw new ArgumentException();
            }
            else
            {
                var count = rootSection.IsRoot 
                    ? 0 
                    : (rootSection.FullName.Length + Constants.SUBSECTION_SEPARATOR.Length);
                return FullName.Remove(0, count);
            }
        }

        public Section GetParent()
        {
            throwIfNotCorrect();
            if (IsRoot)
            {
                throw new InvalidOperationException("Root section never has a parent");
            }

            if (Order == 1)
            {
                return RootSection;
            }
            else
            {
                var parrentEnd = FullName.FindLast(Constants.SUBSECTION_SEPARATOR).Index;
                var parrent = FullName.Take(parrentEnd).Aggregate();

                return new Section(parrent);
            }
        }

        public override string AsText()
        {
            return Constants.SECTION_HEADER_OPEN_MARK + FullName + Constants.SECTION_HEADER_CLOSE_MARK;
        }



        public bool HasParentDirect(IEnumerable<Section> sections)
        {
            //foreach (var curr in sections)
            //{
            //    if (curr.Order == Order - 1)
            //    {
            //        if (IsInsideSection(curr))
            //        {
            //            return true;
            //        }
            //    }
            //}
            //return false;

            return FindDirectParrent(sections).Found;
        }

        public FindResult<Section> FindDirectParrent(IEnumerable<Section> sections)
        {
            var i = 0;
            foreach (var currSection in sections)
            {
                if (currSection.Order == Order - 1)
                {
                    if (IsInsideSection(currSection))
                    {
                        return new FindResult<Section>(i, currSection);
                    }
                }
                i++;
            }
            return new FindResult<Section>();
        }

        public IEnumerable<FindResult<Section>> FindDirectChildren(IEnumerable<Section> sections)
        {
            var i = 0;
            foreach (var currSection in sections)
            {
                if (currSection.Order == Order + 1)
                {
                    if (currSection.IsInsideSection(this))
                    {
                        yield return new FindResult<Section>(i, currSection);
                    }
                }
                i++;
            }
        }

        void throwIfNotCorrect()
        {
            if (!IsCorrect)
            {
                throw new InvalidOperationException("This section is not correct");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="subsections">Null subsections are ignored.</param>
        /// <returns></returns>
        public static string ConcatSubsections(params string[] subsections)
        {
            return subsections
                .Where(s => s != null && isFullNameValid(s))
                .EmptyToNull()
                ?.Aggregate((acc, val) => acc + Constants.SUBSECTION_SEPARATOR + val)
                ?.Aggregate() ?? "";
        }

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

        public override int GetHashCode()
        {
            return new { IsCorrect, FullName }.GetHashCode();
        }

        public override string ToString()
        {
            return AsText();
        }

        public int CompareTo(Section other)
        {
            var order = Order.CompareTo(other.Order);
            return order == 0
                ? (FullName?.CompareTo(other.FullName) ?? 0)
                : order;
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
