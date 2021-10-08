using System.Data.Common;

namespace DbExtensions
{
    public static class DbDataReaderExtension
    {
        public static int GetInt(this DbDataReader reader, string paramName)
        {
            var ordinal = reader.GetOrdinal(paramName);
            return reader.GetInt32(ordinal);
        }
    }
}
