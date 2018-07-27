using System;
using Utilities.Extensions;

namespace TinyConfig
{
    class Section : IEquatable<Section>
    {
        public string FullName { get; }
        public int Order { get; }
        public bool IsRoot { get; }

        public Section(string fullName)
        {
            FullName = fullName;
            Order = FullName?.FindAll(Constants.SUBSECTION_SEPARATOR)?.Count ?? 0;
            IsRoot = FullName == null;
        }

        public bool IsInsideSection(string sectionFullName)
        {
            var passedInSection = new Section(sectionFullName);
            if (!passedInSection.isValid())
            {
                throw new ArgumentException();
            }
            
            if (IsRoot)
            {
                return true;
            }
            else
            {
                return FullName.StartsWith(passedInSection.FullName);
            }
        }
        bool isValid()
        {
            return IsRoot || 
                    (
                        FullName.Length > 0
                        && !FullName.StartsWith(Constants.SUBSECTION_SEPARATOR)
                        && !FullName.EndsWith(Constants.SUBSECTION_SEPARATOR)
                    );
        }

        public bool Equals(Section other)
        {
            return FullName == other.FullName
                && Order == other.Order;
        }

        public override string ToString()
        {
            return Constants.SECTION_HEADER_OPEN_MARK + FullName + Constants.SECTION_HEADER_CLOSE_MARK;
        }
    }
}
