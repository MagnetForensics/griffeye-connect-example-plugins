using System;
using System.Collections.Generic;
using FauxFace.Bindings;

namespace FauxFace.Db
{
    public interface IFauxFaceDb
    {
        bool TryAdd(byte[] imageData, Entity entity, out Entry entry);
        IEnumerable<Entry> GetResults();
    }
}