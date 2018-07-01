using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Utilities;
using Utilities.Extensions;

namespace TinyConfig
{
    internal static class KVPExtractor
    {
        public static IEnumerable<ConfigKVP> ExtractAll(StreamReader reader)
        {
            var lineEnumerator = reader.ReadAllLines().GetEnumerator();
            while (lineEnumerator.MoveNext())
            {
                var line = lineEnumerator.Current;
                var key = line
                          .Split(Constants.KVP_SEPERATOR)[0]
                          .SkipFromEndWhile(c => !char.IsLetterOrDigit(c))
                          .Aggregate();
                if (isOneLineKVP())
                {
                    var valueStartIndex = line.IndexOf(Constants.KVP_SEPERATOR) + 1;
                    var commentBlockStartIndex = line.IndexOf(Constants.COMMENT_SEPARATOR);
                    var value = commentBlockStartIndex < 0 
                                ? line.Substring(valueStartIndex)
                                : line.Substring(valueStartIndex, commentBlockStartIndex - valueStartIndex);
                    var comment = commentBlockStartIndex < 0
                                ? null
                                : line.Substring(commentBlockStartIndex + Constants.COMMENT_SEPARATOR.Length);

                    yield return new ConfigKVP(key, new ConfigValue(value, false), comment);
                }
                else
                {
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

                    yield return new ConfigKVP(key, new ConfigValue(valueAsStr.ToString(), true), commentary);
                }

                bool isOneLineKVP()
                {
                    return !line.Contains(Constants.MULTILUNE_VALUE_MARK) || !line.Contains(Constants.BLOCK_MARK);
                }
            }
        }
    }
}
