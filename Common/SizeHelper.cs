namespace KableNet.Common
{
    /// <summary>
    /// This class stores common byte sizes that will be used with KableNet.
    /// I am 100% sure theres methods directly in C# that have these values,
    /// but for now ill hard code them. Also stores the Buffer size KableNet uses.
    /// </summary>
    public class SizeHelper
    {
        public const int Normal = 4;
        public const int Large = 8;
        public const int Small = 2;
        public const int Bool = 1;
        public const int Buffer = 1024;
    }
}
