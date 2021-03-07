using System;
using System.IO;
using SensorFeedback.Models;
using SQLite;

using Tizen.Applications;

namespace SensorFeedback.Services
{
    /// <summary>
    /// Provides a connection for SQLite database.
    /// </summary>
    /// <remarks>
    /// This DatabaseService uses the SQLite-net and the SQLitePCLRaw packages.
    /// For more details see https://github.com/praeclarum/sqlite-net and https://github.com/ericsink/SQLitePCL.raw.
    /// </remarks>
    public class DatabaseService : IDisposable
    {
        private const string DefaultDBName = "appdata";
        private const string DBFileExtension = ".db3";
        private static bool _initialized = false;
        private SQLiteConnection _db;
        private static DatabaseService instance = null;
        public static DatabaseService GetInstance
        {
            get
            {
                if (instance == null)
                    instance = new DatabaseService();
                return instance;
            }
        }

        public DatabaseService()
        {
            NewConnection();
        }

        /// <summary>
        /// Creates a new SQLite connection and opens a SQLite database specified by given name.
        /// </summary>
        /// <param name="dbname">Specifies the name to the database file. Default name is 'appdata'.</param>
        /// <returns>The connection object</returns>
        public void NewConnection(string dbname = DefaultDBName)
        {
            EnsureInitProvider();
            // Database file will be located in the application data directory.
            string dbfile = Path.Combine(Application.Current.DirectoryInfo.Data, dbname) + DBFileExtension;
            _db = new SQLiteConnection(dbfile);
        }

        private void EnsureInitProvider()
        {
            if (!_initialized)
            {
                // The provider 'SQLite3Provider_sqlite3' uses libsqlite3.so installed in target system.
                SQLitePCL.raw.SetProvider(new SQLitePCL.SQLite3Provider_sqlite3());
                SQLitePCL.raw.FreezeProvider(true);
                _initialized = true;
            }
        }

        internal void UpdateUserSettings(UserSettings us)
        {
            _db.RunInTransaction(() =>
            {
                _db.Update(us);
            });
        }

        internal void InsertUserSettings(UserSettings us)
        {
            _db.RunInTransaction(() =>
            {
                _db.Insert(us);
            });
        }

        internal UserSettings LoadUserSettings()
        {
            UserSettings us = null;

            _db.RunInTransaction(() =>
            {
                // Create table if not exists
                _db.CreateTable<UserSettings>();
                // Try to get existing user settings
                us = _db.Table<UserSettings>().FirstOrDefault();

                if (us == null)
                {
                    // If no settings found, insert
                    us = new UserSettings();
                    _db.Insert(us);
                }

            });
            return us;
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _db = null;
            }
        }
    }
}
