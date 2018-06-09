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
                    var value = line.Substring(line.IndexOf(Constants.KVP_SEPERATOR) + 1);

                    yield return new ConfigKVP(key, new ConfigValue(value, false));
                }
                else
                {
                    var value = line.Substring(line.IndexOf(Constants.BLOCK_MARK) + 1) + Global.NL;
                    StringBuilder valueAsStr = new StringBuilder();
                    bool isBlockClosed = false;
                    while (true)
                    {
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

                    yield return new ConfigKVP(key, new ConfigValue(valueAsStr.ToString(), true));
                }

                bool isOneLineKVP()
                {
                    return !line.Contains(Constants.MULTILUNE_VALUE_MARK) || !line.Contains(Constants.BLOCK_MARK);
                }
            }
        }
    }
}
