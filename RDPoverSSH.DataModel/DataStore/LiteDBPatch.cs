using System.Security.AccessControl;
using System.Security.Principal;
using System.Threading;
using HarmonyLib;
using LiteDB;
using LiteDB.Engine;

namespace RDPoverSSH.DataStore
{
    [HarmonyPatch(typeof(SharedEngine), MethodType.Constructor, typeof(EngineSettings))]
    public class MySharedEngineConstructor
    {
        /// <summary>
        /// Patch the SharedEngine constructor so we can fixup the mutex
        /// </summary>
        /// <remarks>
        /// Mutex fix from here: https://github.com/mbdavid/LiteDB/blob/66607881eab08958ac9ec55e0299fbb9a3a1505c/LiteDB/Client/Shared/SharedEngine.cs#L19
        /// </remarks>
        public static void Postfix(EngineSettings settings, SharedEngine __instance)
        {
            Mutex mutexField = AccessTools.FieldRefAccess<SharedEngine, Mutex>(__instance, "_mutex");

            // Create the permissive ACL
            MutexAccessRule allowEveryoneRule = new MutexAccessRule(new SecurityIdentifier(WellKnownSidType.WorldSid, null), MutexRights.FullControl, AccessControlType.Allow);
            MutexSecurity securitySettings = new MutexSecurity();
            securitySettings.AddAccessRule(allowEveryoneRule);

            // Set the ACL on the db engine's mutex so anyone can use it.
            mutexField.SetAccessControl(securitySettings);
        }
    }
}
