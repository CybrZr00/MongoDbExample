namespace Mongotest.Utilities
{
    public static class Database
    {
        private static string connectionString = "mongodb://localhost:27017/";
        private static string databaseName = "mongotest_v1";
        private static string collectionNamePeople = "people";

        public static string ConnectionString { get => connectionString;}
        public static string DatabaseName { get => databaseName; }

        public static string CollectionNamePeople { get => collectionNamePeople; }


    }
}
