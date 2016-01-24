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
            return hf?.ToLink(link, pov) ?? "UNKNOWN HISTORICAL FIGURE";
        }
        public static string ToSafeLink(this Entity entity, bool link = true, DwarfObject pov = null, string type = "ENTITY")
        {
            return entity?.ToLink(link, pov) ?? "UNKNOWN " + type;
        }
        public static string ToSafeLink(this Artifact entity, bool link = true, DwarfObject pov = null)
        {
            return entity?.ToLink(link, pov) ?? "UNKNOWN ARTIFACT";
        }
        public static string ToSafeLink(this WorldRegion entity, bool link = true, DwarfObject pov = null)
        {
            return entity?.ToLink(link, pov) ?? "UNKNOWN REGION";
        }
        public static string ToSafeLink(this UndergroundRegion entity, bool link = true, DwarfObject pov = null)
        {
            return entity?.ToLink(link, pov) ?? "UNKNOWN UNDERGROUND REGION";
        }
        public static string ToSafeLink(this Structure entity, bool link = true, DwarfObject pov = null)
        {
            return entity?.ToLink(link, pov) ?? "UNKNOWN STRUCTURES";
        }
        public static string ToSafeLink(this Site site, bool link = true, DwarfObject pov = null)
        {
            return site?.ToLink(link, pov) ?? "an unknown site";
        }
        public static string ToSafeLink(this EventCollection collection, bool link = true, DwarfObject pov = null)
        {
            return collection?.ToLink(link, pov) ?? "UNKNOWN";
        }
        public static string ToSafeLink(this WorldConstruction construction, bool link = true, DwarfObject pov = null)
        {
            return construction?.ToLink(link, pov) ?? "UNKNOWN CONSTRUCTION";
        }
        public static string ToSafeLink(this ArtForm art, bool link = true, DwarfObject pov = null, string type = "ART FORM")
        {
            return art?.ToLink(link, pov) ?? "UNKNOWN " + type;
        }

        public static string ToSafeLink(this WrittenContent wc, bool link = true, DwarfObject pov = null)
        {
            return wc?.ToLink(link, pov) ?? "UNKNOWN WRITTEN CONTENT";
        }

        public static int SearchWorldObject<T>(this List<T> collection, int id) where T : WorldObject
        {
            if (id == -1) return ~0;
            int min = 0;
            int max = collection.Count - 1;
            while (min <= max)
            {
                int mid = min + (max - min) / 2;
                if (id > collection[mid].ID)
                    min = mid + 1;
                else if (id < collection[mid].ID)
                    max = mid - 1;
                else
                    return mid;
            }
            return ~min;
        }

        public static T GetWorldObject<T>(this List<T> collection, int id) where T : WorldObject
        {
            if (id == -1) return null;
            int min = 0;
            int max = collection.Count - 1;
            while (min <= max)
            {
                int mid = min + (max - min) / 2;
                if (id > collection[mid].ID)
                    min = mid + 1;
                else if (id < collection[mid].ID)
                    max = mid - 1;
                else
                    return collection[mid];
            }
            return null;
        }

        public static T GetEvent<T>(this List<T> collection, int id) where T : WorldEvent
        {
            if (id == -1) return null;
            int min = 0;
            int max = collection.Count - 1;
            while (min <= max)
            {
                int mid = min + (max - min) / 2;
                if (id > collection[mid].ID)
                    min = mid + 1;
                else if (id < collection[mid].ID)
                    max = mid - 1;
                else
                    return collection[mid];
            }
            return null;
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
