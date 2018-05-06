namespace TinyConfig
{
    public static class Constants
    {
        public const char MULTILUNE_VALUE_MARK = '#';
        public const char BLOCK_MARK = '\'';
        public const string KVP_SEPERATOR = "=";
    }

    //public abstract class MultilineTypeMarshaller<T> : TypeMarshaller
    //{
    //    //public override sealed Type ValueType { get; } = typeof(T);

    //    //public override bool TryPack(object value, StreamWriter result)
    //    //{
    //    //    var isPacked = TryPack((T)value, out string packedValue);
    //    //    if (isPacked)
    //    //    {
    //    //        var shieldedValue = packedValue
    //    //            .Replace(Constants.BLOCK_MARK.ToString(), string.Concat(Constants.BLOCK_MARK, Constants.BLOCK_MARK));

    //    //        result.Write(Constants.MULTILUNE_VALUE_MARK);
    //    //        result.Write(Constants.BLOCK_MARK);
    //    //        result.Write(shieldedValue);
    //    //        result.Write(Constants.BLOCK_MARK);
    //    //        result.Write(Global.NL);
    //    //    }

    //    //    return isPacked;
    //    //}
    //    //public override bool TryUnpack(StreamReader packed, out object result)
    //    //{
    //    //    var value = packed.ReadAllText()
    //    //        .SkipWhile(c => c != Constants.MULTILUNE_VALUE_MARK).Skip(1)
    //    //        .SkipWhile(c => c != Constants.BLOCK_MARK).Skip(1);
    //    //    char prev = value.Take(1).Single();
    //    //    StringBuilder valueAsStr = new StringBuilder();
    //    //    foreach (var curr in value.Skip(1))
    //    //    {
    //    //        if (prev == Constants.BLOCK_MARK && curr != Constants.BLOCK_MARK)
    //    //        {
    //    //            break;
    //    //        }
    //    //        valueAsStr.Append(prev);
    //    //        prev = curr;
    //    //    }

    //    //    var isOk = TryUnpack(valueAsStr.ToString(), out T specificResult);
    //    //    result = specificResult;

    //    //    return isOk;
    //    //}

    //    public abstract bool TryPack(T value, out string result);
    //    public abstract bool TryUnpack(string packed, out T result);
    //}

}
