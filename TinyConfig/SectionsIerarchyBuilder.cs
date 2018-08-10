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
    class SharedList<T> : List<T>
    {
        override 
    }

    class SectionsTreeBuilder
    {
        public class RootSection
        {
            public Section Section;
            public IEnumerable<RootSection> Childs;
            public IEnumerable<string> Lines;
        }

        public RootSection BuildTree(SectionsFinder.SectionInfo[] allSections)
        {
            var sections = allSections
                .Where(s => s.Section.IsCorrect)
                .OrderBy(s => s.SectionLocation.From)
                .Distinct(s => s.Section.FullName)
                .Select(s => new { Section = s.Section, Lines = s.FullSection })
                .ToList();

            for (int i = 1; i < sections.Count; i++)
            {
                var prev = sections[i - 1].Section;
                var curr = sections[i].Section;
                if (curr.IsInsideSection(prev)) // All ok
                {

                }
                else if (curr.Order == prev.Order)
                {
                    getClosestParrent(curr) == getClosestParrent(prev)
                }

                Section getClosestParrent(Section s) => null;
                {
                    for (int j = i; j > 0; j--)
                    {
                        if (currSection.IsInsideSection(sections[j].Section))
                        {
                            return true;
                        }
                        else if (currSection.Order != )
                    }

                    return false;
                }
            }

            bool validate(int from, int to)
            {
                var currSection = sections[i].Section;
                for (int i = from; i < to; i++)
                {
                    for (int j = i + 1; j < sections.Count; j++)
                    {
                        if (currSection.Order < sections[j].Section.Order) // End of section
                        {
                            if (!validate(i + 1))
                            {
                                return false;
                            }
                            i = j - 1;
                        }
                        else if (!sections[j].Section.IsInsideSection(currSection))
                        {

                        }


                    }
                }
            }





            for (int i = 1; i < sections.Count; i++)
            {
                var currSection = sections[i].Section;
                var prevSection = sections[i - 1].Section;
                if (currSection.Order > prevSection.Order
                    && !currSection.IsInsideSection(prevSection))
                {
                    removeSectionWithChildren();
                }
                else if (currSection.Order > 1)
                {
                    
                }

                bool hasParrent()
                {
                    for (int j = i; j > 0; j--)
                    {
                        if (currSection.IsInsideSection(sections[j].Section))
                        {
                            return true;
                        }
                        else if (currSection.Order != )
                    }

                    return false;
                }

                void removeSectionWithChildren()
                {
                    for (int j = i + 1; j < sections.Count; j++)
                    {
                        if(sections[j].Section.Order == currSection.Order)
                        {
                            break; // currSection was closed
                        }
                        else
                        {
                            sections.RemoveAt(j--); // Wrong child
                        }
                    }
                    sections.RemoveAt(i--);
                }
            }

            RootSection getChild(SectionsFinder.SectionInfo[] sections)
            {
                if (sections.Length == 1) // Leaf
                {
                    return new RootSection()
                    {
                        Childs = Enumerable.Empty<RootSection>(),
                        Section = sections[0].Section,
                        Lines = sections[0].FullSection
                    };
                }
                else
                {
                    var lines = Enumerable.Empty<string>();
                    var childs = new List<RootSection>();
                    
                    for (int i = 0; i < sections.Length; i++)
                    {
                        var s = sections[i];

                        int end = i + 1;
                        for (int j = i + 1; j < sections.Length; j++)
                        {
                            var ss = sections[j];
                            if (ss.Section.Order == ss.Section.Order)
                            {
                                end = j;
                                break; // Further advancing is not required
                            }
                            else
                            {
                                lines.Concat(ss.)
                            }
                        }

                        var start = i + 1;
                        var sectionsInSection = sections.GetRange(start, end - start).ToArray();
                        var section = getChild(sectionsInSection);
                        childs.Add(section);

                        i = end;
                    }

                    return new RootSection()
                    {
                        Childs = childs,
                        Section = sections[0].Section,
                        Lines = ArrayUtils.ConcatAll(sections[0].FullSection, ArrayUtils.ConcatSequences(childs.Select(c => c.Lines)))
                    };
                }
            }
        }
    }
}
