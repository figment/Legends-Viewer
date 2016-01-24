using System;
using System.Collections.Generic;
using System.ComponentModel;
using LegendsViewer.Legends.EventCollections;
using LegendsViewer.Legends.Events;

namespace LegendsViewer.Legends
{
    internal static class Misc
    {
        public static bool AddEvent(this WorldObject worldObject, WorldEvent worldEvent)
        {
            if (worldObject != null)
            {
                int idx = worldObject.Events.BinarySearch(worldEvent);
                if (idx < 0)
                {
                    worldObject.Events.Insert(~idx, worldEvent);
                    return true;
                }
            }
            return false;
        }

        public static string ToSafeLink(this HistoricalFigure hf, bool link = true, DwarfObject pov = null)
        {
            if (hf != null) return hf.ToLink(link, pov);
            return "UNKNOWN HISTORICAL FIGURE";
        }
        public static string ToSafeLink(this Entity entity, bool link = true, DwarfObject pov = null, string type = "ENTITY")
        {
            if (entity != null) return entity.ToLink(link, pov);
            return "UNKNOWN " + type;
        }
        public static string ToSafeLink(this Artifact entity, bool link = true, DwarfObject pov = null)
        {
            if (entity != null) return entity.ToLink(link, pov);
            return "UNKNOWN ARTIFACT";
        }
        public static string ToSafeLink(this WorldRegion entity, bool link = true, DwarfObject pov = null)
        {
            if (entity != null) return entity.ToLink(link, pov);
            return "UNKNOWN REGION";
        }
        public static string ToSafeLink(this UndergroundRegion entity, bool link = true, DwarfObject pov = null)
        {
            if (entity != null) return entity.ToLink(link, pov);
            return "UNKNOWN UNDERGROUND REGION";
        }
        public static string ToSafeLink(this Structure entity, bool link = true, DwarfObject pov = null)
        {
            if (entity != null) return entity.ToLink(link, pov);
            return "UNKNOWN STRUCTURES";
        }
        public static string ToSafeLink(this Site site, bool link = true, DwarfObject pov = null)
        {
            return site != null ? site.ToLink(link, pov) : "an unknown site";
        }
        public static string ToSafeLink(this EventCollection collection, bool link = true, DwarfObject pov = null)
        {
            return collection != null ? collection.ToLink(link, pov) : "UNKNOWN";
        }
        public static string ToSafeLink(this WorldConstruction construction, bool link = true, DwarfObject pov = null)
        {
            return construction != null ? construction.ToLink(link, pov) : "UNKNOWN CONSTRUCTION";
        }
    }

    class LambaComparer<T> : IComparer<T>
    {
        readonly Func<T, T, int> _comparerFunc;

        public LambaComparer(Func<T, T, int> comparerFunc)
        {
            _comparerFunc = comparerFunc;
        }

        public int Compare(T x, T y)
        {
            return _comparerFunc(x, y);
        }
    }
}
