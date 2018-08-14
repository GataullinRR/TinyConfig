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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="allSections">Sections with correct hierarchy</param>
        /// <returns></returns>
        public RootSection BuildTree(SectionsFinder.SectionInfo[] allSections)
        {
            var sections = allSections
                .Select(s => new { Section = s.Section, Lines = s.FullSection })
                .ToList();

            var dipestOrder = sections.Max(s => s.Section.Order);
            return getRoot(0);

            /////////////////////////////////////////

            RootSection getRoot(int offset)
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
                            child = getRoot(i);
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
        }
    }
}
