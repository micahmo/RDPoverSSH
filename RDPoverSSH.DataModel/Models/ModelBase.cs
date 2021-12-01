using System;
using LiteDB;
using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace RDPoverSSH.Models
{
    /// <summary>
    /// Defines an interface for persistent object models
    /// </summary>
    /// <remarks>
    /// Derived types must must implement the interface with the derived type as T. For example:
    /// <code>
    /// public class <typeparamref name="T"/> : ModelBase&lt;<typeparamref name="T"/>&gt;
    /// </code>
    /// </remarks>
    public abstract class ModelBase<T> : ObservableObject
    {
        /// <summary>
        /// An initialization callback that can be invoked upon database creation to create standing data, for example.
        /// </summary>
        /// <remarks>
        /// Has an empty definition by default
        /// </remarks>
        [BsonIgnore]
        public virtual Action<ILiteCollection<T>> Initialize { get; } = _ => { };
    }
}
