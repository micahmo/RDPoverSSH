using System;
using System.IO;
using HarmonyLib;
using LiteDB;
using RDPoverSSH.Models;

namespace RDPoverSSH.DataStore
{
    /// <summary>
    /// Access to the database
    /// </summary>
    /// <remarks>
    /// Notes for memory's sake. There is an issue with LiteDB and mutexes. If a global mutex is grabbed by SYSTEM, it cannot subsequently be grabbed by a normal user.
    /// This occurs in LiteDB here: https://github.com/mbdavid/LiteDB/blob/00d28bfafe3c685ae239f759f812def495278eaf/LiteDB/Client/Shared/SharedEngine.cs#L22
    /// and happens in our Windows service.
    /// As a result, I am patching the LiteDB DLL on the fly using Harmony (see LiteDBPatch.cs) to make the mutex permissible.
    /// There are also some notes in this issue saying that the usage of mutex may be removed in LiteDB 5.1. https://github.com/mbdavid/LiteDB/issues/1546
    /// </remarks>
    public static class DatabaseEngine
    {
        /// <summary>
        /// Static constructor so we can hook our patches before anything else happens.
        /// (Needs to be in the same assembly where the patches are defined)
        /// </summary>
        static DatabaseEngine()
        {
            // Automatically perform patches based on HarmonyPatch attribute
            new Harmony("mdm.patch").PatchAll();
        }

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

                    return new LiteDatabase(new ConnectionString(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments), ProgramDataFolderName, DbName))
                    {
                        // Don't lock it from other processes
                        Connection = ConnectionType.Shared
                    });
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
