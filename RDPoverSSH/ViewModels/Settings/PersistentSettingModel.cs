using System;
using LiteDB;
using RDPoverSSH.DataStore;
using RDPoverSSH.Models;

namespace RDPoverSSH.ViewModels.Settings
{
    /// <summary>
    /// The persistent representation of a setting
    /// </summary>
    public class PersistentSettingModel : ModelBase<PersistentSettingModel>, ISettingModel
    {
        [BsonId]
        public Guid Guid { get; set; }

        public string Value { get; set; }

        public void Save()
        {
            DatabaseEngine.GetCollection<PersistentSettingModel>().Upsert(this);
        }
    }
}
