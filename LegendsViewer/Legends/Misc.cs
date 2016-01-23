using System;
using System.Collections.Generic;
using System.ComponentModel;

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


    public class Location : IEqualityComparer<Location>
    {
        public int X { get; set; }
        public int Y { get; set; }
        public Location(int x, int y)
        {
            X = x;
            Y = y;
        }

        public System.Drawing.Point ToPoint()
        {
            return new System.Drawing.Point(X, Y);
        }

        public static bool operator ==(Location a, Location b)
        {
            if ((object)a == null || (object)b == null)
                return false;
            return a.X == b.X && a.Y == b.Y;
        }

        public static bool operator !=(Location a, Location b)
        {
            return !(a == b);
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            Location b = obj as Location;
            if ((object)b == null)
            {
                return false;
            }

            return X == b.X && Y == b.Y;
        }

        public override int  GetHashCode()
        {
 	        return X ^ Y;
        }

        public bool Equals(Location b)
        {
            return X == b.X && Y == b.Y;
        }

        public bool Equals(Location a, Location b)
        {
            return a == b;
        }

        public int GetHashCode(Location location)
        {
            return location.X ^ location.Y;
        }
    }

    public enum DeathCause
    {
        None,
        Struck,
        [Description("Old Age")]
        OldAge,
        Thirst,
        Suffocated,
        Bled,
        Cold,
        [Description("Crushed by a Bridge")]
        CrushedByABridge,
        Drowned,
        Starved,
        [Description("In a Cage")]
        InACage,
        Infection,
        [Description("Collided With an Obstacle")]
        CollidedWithAnObstacle,
        [Description("Put to Rest")]
        PutToRest,
        [Description("Starved on Quit")]
        StarvedQuit,
        Trap,
        [Description("Dragon's Fire")]
        DragonsFire,
        Burned,
        Murdered,
        Shot,
        [Description("Cave In")]
        CaveIn,
        [Description("Frozen in Water")]
        FrozenInWater,
        [Description("Executed - Fed To Beasts")]
        ExecutedFedToBeasts,
        [Description("Executed - Burned Alive")]
        ExecutedBurnedAlive,
        [Description("Executed - Crucified")]
        ExecutedCrucified,
        [Description("Executed - Drowned")]
        ExecutedDrowned,
        [Description("Executed - Hacked To Pieces")]
        ExecutedHackedToPieces,
        [Description("Executed - Buried Alive")]
        ExecutedBuriedAlive,
        [Description("Executed - Beheaded")]
        ExecutedBeheaded,
        [Description("Drained of blood")]
        DrainedBlood,
        Collapsed,
        [Description("Scared to death")]
        ScaredToDeath,
        Scuttled,
        [Description("Killed by flying object")]
        FlyingObject,
        Slaughtered,
        Melted,
        Unknown
    }
}
