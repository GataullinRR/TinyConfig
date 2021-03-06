﻿namespace TinyConfig
{
    public static class Constants
    {
        public const char MULTILINE_VALUE_MARK = '#';
        public const char BLOCK_MARK = '\'';
        public const string KVP_SEPERATOR = "=";
        public const string COMMENT_SEPARATOR = @"\\";
        public const string NULL_VALUE = "NULL";
        public const char NULL_VALUE_ESCAPE_PERFIX = '$';

        public const string SECTION_HEADER_OPEN_MARK = "[";
        public const string SECTION_HEADER_CLOSE_MARK = "]";
        public const string SUBSECTION_SEPARATOR = ".";
    }
}
