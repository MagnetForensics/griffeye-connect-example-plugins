using System;
using System.Collections.Generic;
using System.IO;
using FauxFace.Bindings;

namespace FauxFace.Db
{
    public struct Entry
    {
        public byte[] imageData { get; set; }
        public string id { get; set; }
        public bool Identified { get; set; }
        public string Subject { get; set; }
    }

    public class FauxFaceDb : IFauxFaceDb
    {
        readonly List<string> names = new List<string>
        {
            "Lance Glaude",
            "Dallas Alm",
            "Ramonita Giambrone",
            "Nilda Karns",
            "Lacey Furrow",
            "Jefferey Carlon",
            "Donette Dolby",
            "Iona Yurick",
            "Saul Gullett",
            "Bret Raymo",
            "Kristle Haye",
            "Claudette Farwell",
            "Brittni Sowders",
            "Verda Gruner",
            "Illa Jansen",
            "Ronna Hagins",
            "Margo Flory",
            "Elissa Hoaglin",
            "Mandy Porcaro",
            "Selma Ek"
        };

        readonly Dictionary<string, Entry> db = new Dictionary<string, Entry>();

        public bool TryAdd(byte[] imageData, Entity entity, out Entry entry)
        {
            lock (db)
            {
                var rnd = new Random();
                entry = new Entry
                {
                    Identified = rnd.Next(0, 2) > 0,
                    imageData = imageData,
                    id = entity.EntityId.ToString(),
                };

                entry.Subject = entry.Identified ? names[rnd.Next(0, names.Count)] : string.Empty;

                if (db.ContainsKey(entry.id))
                {
                    return false;
                }

                db.Add(entry.id, entry);
                Console.WriteLine($"Added db entry: {entry.id}");
                return true;
            }
        }
        public IEnumerable<Entry> GetResults() => db.Values;
    }
}