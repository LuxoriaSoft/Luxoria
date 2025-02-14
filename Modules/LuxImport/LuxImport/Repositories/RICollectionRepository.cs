using LuxImport.Interfaces;
using Microsoft.Data.Sqlite;

namespace LuxImport.Repositories
{
    /// <summary>
    /// Recent Imported Collection Repository
    /// </summary>
    public class RICollectionRepository : IRICollectionRepository
    {
        private readonly string _dbPath;

        /// <summary>
        /// Constructor for the RICollectionRepository
        /// </summary>
        public RICollectionRepository()
        {
            _dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Luxoria", "LuxImport", "RIC.db");
            InitializeDatabase();
        }

        /// <summary>
        /// Initializes the database.
        /// </summary>
        private void InitializeDatabase()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_dbPath)!);
            using var connection = new SqliteConnection($"Data Source={_dbPath}");
            connection.Open();
            string sql = @"CREATE TABLE IF NOT EXISTS Collections (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name TEXT NOT NULL UNIQUE,
                    Path TEXT NOT NULL,
                    ImportedAt DATETIME DEFAULT CURRENT_TIMESTAMP
                  );";
            using var command = new SqliteCommand(sql, connection);
            command.ExecuteNonQuery();
        }


        /// <summary>
        /// Updates an existing collection if it exists; otherwise, creates a new one.
        /// </summary>
        public void UpdateOrCreate(string collectionName, string collectionPath)
        {
            using var connection = new SqliteConnection($"Data Source={_dbPath}");
            connection.Open();

            string sql = @"INSERT INTO Collections (Name, Path, ImportedAt)
                            VALUES (@name, @path, CURRENT_TIMESTAMP)
                            ON CONFLICT(Name) DO UPDATE SET Path = @path, ImportedAt = CURRENT_TIMESTAMP;";
            using var command = new SqliteCommand(sql, connection);
            command.Parameters.AddWithValue("@name", collectionName);
            command.Parameters.AddWithValue("@path", collectionPath);
            command.ExecuteNonQuery();
        }

        /// <summary>
        /// Retrieves the Nth latest imported collection.
        /// </summary>
        public (string Name, string Path)? GetNthLatestImportedCollection(int n)
        {
            using var connection = new SqliteConnection($"Data Source={_dbPath}");
            connection.Open();

            string sql = @"SELECT Name, Path FROM Collections
                            ORDER BY ImportedAt DESC
                            LIMIT 1 OFFSET @offset;";
            using var command = new SqliteCommand(sql, connection);
            command.Parameters.AddWithValue("@offset", n - 1);
            using var reader = command.ExecuteReader();

            return reader.Read() ? (reader.GetString(0), reader.GetString(1)) : null;
        }

        /// <summary>
        /// Retrieves the X latest imported collections.
        /// </summary>
        /// <param name="x">X elements to be returned</param>
        /// <returns>Returns a collection of Name and Path</returns>
        public ICollection<(string Name, string Path)> GetXLatestImportedCollections(int x)
        {
            using var connection = new SqliteConnection($"Data Source={_dbPath}");
            connection.Open();
            string sql = @"SELECT Name, Path FROM Collections
                            ORDER BY ImportedAt DESC
                            LIMIT @limit;";
            using var command = new SqliteCommand(sql, connection);
            command.Parameters.AddWithValue("@limit", x);
            using var reader = command.ExecuteReader();
            var collections = new List<(string Name, string Path)>();
            while (reader.Read())
            {
                collections.Add((reader.GetString(0), reader.GetString(1)));
            }
            return collections;
        }

        /// <summary>
        /// Flushes all history from the database.
        /// </summary>
        public void FlushHistory()
        {
            using var connection = new SqliteConnection($"Data Source={_dbPath}");
            connection.Open();
            using var command = new SqliteCommand("DELETE FROM Collections;", connection);
            command.ExecuteNonQuery();
        }
    }
}
