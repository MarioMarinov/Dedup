namespace Services.Extensions
{
    using System.Data.Common;

    public static class SqliteDataReaderExtensions
    {
        private static readonly object _lock = new object();

        public static T? GetSafeFieldValue<T>(this DbDataReader reader, int ordinal)
        {
            lock (_lock)
            {
                return reader.IsDBNull(ordinal) ? default : reader.GetFieldValue<T>(ordinal);
            }
        }
    }

}
