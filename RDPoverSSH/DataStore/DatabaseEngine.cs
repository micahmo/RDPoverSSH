using System;
using System.IO;
using LiteDB;
using RDPoverSSH.Models;

namespace RDPoverSSH.DataStore
{
    public static class DatabaseEngine
    {
        public static LiteDatabase DatabaseInstance
        {
            get
            {
                return _databaseInstance ??= new Func<LiteDatabase>(() =>
                {
                    if (!Directory.Exists(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments), ProgramDataFolderName)))
                    {
                        Directory.CreateDirectory(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments), ProgramDataFolderName));
                    }

                    return new LiteDatabase(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments), ProgramDataFolderName, DbName));
                })();
            }
        }
        private static LiteDatabase _databaseInstance;

        public static void Shutdown()
        {
            _collectionConnection = null;
            _databaseInstance?.Dispose();
            _databaseInstance = null;
        }

        public static ILiteCollection<ConnectionModel> ConnectionCollection => _collectionConnection ??= DatabaseInstance.GetCollection<ConnectionModel>("connectionitem");
        private static ILiteCollection<ConnectionModel> _collectionConnection;

        private const string DbName = "RDPoverSSH.db";
        private const string ProgramDataFolderName = "RDPoverSSH";
    }
}
