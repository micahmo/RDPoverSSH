using System;
using System.Collections.Generic;
using System.IO;
using HarmonyLib;
using LiteDB;
using RDPoverSSH.Models;

namespace RDPoverSSH.DataStore
{
    /// <summary>
    /// Access to the database
    /// </summary>
    public static class DatabaseEngine
    {
        /// <summary>
        /// Static constructor so we can hook our patches before anything else happens.
        /// (Needs to be in the same assembly where the patches are defined)
        /// </summary>
        /// <remarks>
        /// Notes for memory's sake. There is an issue with LiteDB and mutexes. If a global mutex is grabbed by SYSTEM, it cannot subsequently be grabbed by a normal user.
        /// This occurs in LiteDB here: https://github.com/mbdavid/LiteDB/blob/00d28bfafe3c685ae239f759f812def495278eaf/LiteDB/Client/Shared/SharedEngine.cs#L22
        /// and happens in our Windows service.
        /// As a result, I am patching the LiteDB DLL on the fly using Harmony (see LiteDBPatch.cs) to make the mutex permissible.
        /// There are also some notes in this issue saying that the usage of mutex may be removed in LiteDB 5.1. https://github.com/mbdavid/LiteDB/issues/1546
        /// </remarks>
        static DatabaseEngine()
        {
            // Automatically perform patches based on HarmonyPatch attribute
            new Harmony("mdm.patch").PatchAll();
        }

        /// <summary>
        /// The singleton database instance.
        /// </summary>
        /// <remarks>
        /// Prefer using <see cref="GetCollection{T}"/> as opposed to accessing the database instance directly.
        /// </remarks>
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

                    if (!File.Exists(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments), ProgramDataFolderName, DbName)))
                    {
                        // Database doesn't exist, this is our chance to do first-time operations.
                        FirstDbInstance = true;
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

        /// <summary>
        /// Get a collection for the given type
        /// </summary>
        public static ILiteCollection<T> GetCollection<T>() where T : ModelBase<T>, new() => CollectionFactory<T>.Instance;

        public static void Shutdown()
        {
            foreach (Action teardownCollectionCallback in CollectionFactorySchema.TeardownCollectionCallbacks)
            {
                teardownCollectionCallback();
            }
            
            _databaseInstance?.Dispose();
            _databaseInstance = null;
        }

        private const string DbName = "RDPoverSSH.db";
        private const string ProgramDataFolderName = "RDPoverSSH";

        /// <summary>
        /// Whether we have created the database in this process instance
        /// </summary>
        internal static bool FirstDbInstance;
    }

    #region Collection factory classes

    /// <summary>
    /// Helper class that contains metadata about the various types of CollectionFactories we have "instantiated"
    /// </summary>
    internal static class CollectionFactorySchema
    {
        /// <summary>
        /// A list of callbacks to teardown each "instance" of CollectionFactory
        /// </summary>
        public static List<Action> TeardownCollectionCallbacks = new List<Action>();
    }

    /// <summary>
    /// A factory class for generating, caching, and returning database collection instances
    /// </summary>
    internal static class CollectionFactory<T> where T : ModelBase<T>, new()
    {
        /// <summary>
        /// Static constructor
        /// </summary>
        static CollectionFactory()
        {
            // Every time this generic static class is "instantiated" with a type,
            // create a callback that knows how to tear ourselves down
            CollectionFactorySchema.TeardownCollectionCallbacks.Add(() => _instance = null);
        }

        /// <summary>
        /// A database collection instance for the given type
        /// </summary>
        public static ILiteCollection<T> Instance => _instance ??=
            new Func<ILiteCollection<T>>(() =>
            {
                var collection = DatabaseEngine.DatabaseInstance.GetCollection<T>(typeof(T).Name.ToLowerInvariant());

                if (DatabaseEngine.FirstDbInstance)
                {
                    // This is a brand new database, and this collection type has not been accessed yet.
                    // Do it now.
                    new T().Initialize?.Invoke(collection);
                }

                return collection;
            })();
        private static ILiteCollection<T> _instance;
    }

    #endregion
}
