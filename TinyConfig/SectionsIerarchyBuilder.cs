using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Utilities;
using Utilities.Extensions;

namespace TinyConfig
{
    class SectionsTreeBuilder
    {
        public class RootSection : IEquatable<RootSection>, IEqualityComparer<RootSection>
        {
            public Section Section;
            public IEnumerable<RootSection> Children;
            public IEnumerable<string> Lines;

            public IEnumerable<RootSection> AllChildren
            {
                get
                {
                    foreach (var child in getAllChildren(this))
                    {
                        yield return child;
                    }

                    /////////////////////////////

                    IEnumerable<RootSection> getAllChildren(RootSection section)
                    {
                        foreach (var directChild in section.Children)
                        {
                            yield return directChild;
                            foreach (var child in getAllChildren(directChild))
                            {
                                yield return child;
                            }
                        }
                    }
                }
            }

            public bool Equals(RootSection other)
            {
                return other == null
                    ? false
                    : Section.Equals(other.Section)
                    && Children.SequenceEqual(other.Children)
                    && Lines.SequenceEqual(other.Lines);
            }

            public bool Equals(RootSection x, RootSection y)
            {
                return x == null || y == null
                    ? false
                    : x.Equals(y);
            }

            public override string ToString()
            {
                return $"{Section} >> Children: {Children.Select(s => s.Section).AsString(" ")}";
            }

            public int GetHashCode(RootSection obj)
            {
                return new { Section, Children, Lines }.GetHashCode();
            }
        }

        public RootSection BuildTree(SectionsFinder.SectionInfo[] allSections)
        {
            var sections = allSections
                .Where(s => s.Section.IsCorrect)
                .Distinct(s => s.Section.FullName)
                //.OrderBy(s => s.SectionLocation.From)
                .Select(s => new { Section = s.Section, Lines = s.FullSection })
                .ToList();

            for (int i = 0; i < sections.Count; i++)
            {
                if (!validate(i))
                {
                    sections.RemoveAt(i--);
                }
            }

            bool validate(int index)
            {
                var header = sections[index].Section;
                for (int i = index - 1; i >= 0; i--)
                {
                    var curr = sections[i].Section;
                    if (curr.Order == header.Order - 1)
                    {
                        if (header.IsInsideSection(curr)) // have parrent
                        {
                            return true;
                        }
                    }
                }
                return header.Order > 0 || index == 0; // not root section must have a parrent (if root exists)
            }

            var dipestOrder = sections.Max(s => s.Section.Order);
            return ddd(0);

            RootSection ddd(int offset)
            {
                var header = sections[offset];
                var lines = new List<string>(header.Lines).ToEnumerable();
                var children = new List<RootSection>();
                for (int i = offset + 1; i < sections.Count; i++)
                {
                    var curr = sections[i];
                    if (curr.Section.IsInsideSection(header.Section)
                        && curr.Section.Order - 1 == header.Section.Order)
                    {
                        RootSection child;
                        if (curr.Section.Order != dipestOrder) // continue unwrapping
                        {
                            child = ddd(i);
                        }
                        else // leaf
                        {
                            child = new RootSection()
                            {
                                Children = Enumerable.Empty<RootSection>(),
                                Lines = curr.Lines,
                                Section = curr.Section
                            };
                        }
                        lines = lines.Concat(child.Lines);
                        children.Add(child);
                    }
                }

                return new RootSection()
                {
                    Children = children,
                    Lines = lines,
                    Section = header.Section
                };
            }


            //bool validate(int from, int to)
            //{
            //    var currSection = sections[i].Section;
            //    for (int i = from; i < to; i++)
            //    {
            //        for (int j = i + 1; j < sections.Count; j++)
            //        {
            //            if (currSection.Order < sections[j].Section.Order) // End of section
            //            {
            //                if (!validate(i + 1))
            //                {
            //                    return false;
            //                }
            //                i = j - 1;
            //            }
            //            else if (!sections[j].Section.IsInsideSection(currSection))
            //            {

            //            }


            //        }
            //    }
            //}





            //for (int i = 1; i < sections.Count; i++)
            //{
            //    var currSection = sections[i].Section;
            //    var prevSection = sections[i - 1].Section;
            //    if (currSection.Order > prevSection.Order
            //        && !currSection.IsInsideSection(prevSection))
            //    {
            //        removeSectionWithChildren();
            //    }
            //    else if (currSection.Order > 1)
            //    {

            //    }

            //    bool hasParrent()
            //    {
            //        for (int j = i; j > 0; j--)
            //        {
            //            if (currSection.IsInsideSection(sections[j].Section))
            //            {
            //                return true;
            //            }
            //            else if (currSection.Order != )
            //        }

            //        return false;
            //    }

            //    void removeSectionWithChildren()
            //    {
            //        for (int j = i + 1; j < sections.Count; j++)
            //        {
            //            if(sections[j].Section.Order == currSection.Order)
            //            {
            //                break; // currSection was closed
            //            }
            //            else
            //            {
            //                sections.RemoveAt(j--); // Wrong child
            //            }
            //        }
            //        sections.RemoveAt(i--);
            //    }
            //}

            //RootSection getChild(SectionsFinder.SectionInfo[] sections)
            //{
            //    if (sections.Length == 1) // Leaf
            //    {
            //        return new RootSection()
            //        {
            //            Childs = Enumerable.Empty<RootSection>(),
            //            Section = sections[0].Section,
            //            Lines = sections[0].FullSection
            //        };
            //    }
            //    else
            //    {
            //        var lines = Enumerable.Empty<string>();
            //        var childs = new List<RootSection>();

            //        for (int i = 0; i < sections.Length; i++)
            //        {
            //            var s = sections[i];

            //            int end = i + 1;
            //            for (int j = i + 1; j < sections.Length; j++)
            //            {
            //                var ss = sections[j];
            //                if (ss.Section.Order == ss.Section.Order)
            //                {
            //                    end = j;
            //                    break; // Further advancing is not required
            //                }
            //                else
            //                {
            //                    lines.Concat(ss.)
            //                }
            //            }

            //            var start = i + 1;
            //            var sectionsInSection = sections.GetRange(start, end - start).ToArray();
            //            var section = getChild(sectionsInSection);
            //            childs.Add(section);

            //            i = end;
            //        }

            //        return new RootSection()
            //        {
            //            Childs = childs,
            //            Section = sections[0].Section,
            //            Lines = ArrayUtils.ConcatAll(sections[0].FullSection, ArrayUtils.ConcatSequences(childs.Select(c => c.Lines)))
            //        };
            //    }
        }
    }
}
