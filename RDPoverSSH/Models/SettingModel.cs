using System;
using LiteDB;

namespace RDPoverSSH.Models
{
    /// <summary>
    /// The persistent representation of a setting
    /// </summary>
    public class SettingModel : ModelBase<SettingModel>
    {
        [BsonId]
        public Guid Guid { get; set; }

        public string Value { get; set; }
    }
}
