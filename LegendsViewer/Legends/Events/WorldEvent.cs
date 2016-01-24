using System;
using System.Collections.Generic;
using System.Linq;
using LegendsViewer.Legends.EventCollections;
using LegendsViewer.Legends.Parser;

namespace LegendsViewer.Legends.Events
{
    public class WorldEvent : IComparable<WorldEvent>
    {
        public int ID { get; set; }
        public int Year { get; set; }
        public int Seconds72 { get; set; }
        public string Type { get; set; }
        public EventCollection ParentCollection { get; set; }
        public World World;

        public WorldEvent() { ID = -1; Year = -1; Seconds72 = -1; Type = "INVALID"; }

        private void InternalMerge(List<Property> properties, World world)
        {
            World = world;
            foreach (Property property in properties)
                switch (property.Name)
                {
                    case "id": this.ID = property.ValueAsInt(); break;
                    case "year": this.Year = property.ValueAsInt(); break;
                    case "seconds72": this.Seconds72 = property.ValueAsInt(); break;
                    case "type": this.Type = String.Intern(property.Value.Replace('_', ' ')); break;
                    default: break;
                }
        }

        public virtual void Merge(List<Property> properties, World world)
        {
            InternalMerge(properties, world);
        }

        public virtual string Print(bool link = true, DwarfObject pov = null)
        {
            string eventString = GetYearTime() + Type;
            eventString += PrintParentCollection(link, pov);
            return eventString;
        }

        public int Compare(WorldEvent worldEvent)
        {
            return this.ID.CompareTo(worldEvent.ID);
        }

        public virtual string GetYearTime()
        {
            if (this.Year == -1) return "In a time before time, ";
            string yearTime = "In " + this.Year + ", ";
            if (this.Seconds72 == -1)
                return yearTime;

            int month = this.Seconds72 % 100800;
            if (month <= 33600) yearTime += "early ";
            else if (month <= 67200) yearTime += "mid";
            else if (month <= 100800) yearTime += "late ";

            int season = this.Seconds72 % 403200;
            if (season < 100800) yearTime += "spring, ";
            else if (season < 201600) yearTime += "summer, ";
            else if (season < 302400) yearTime += "autumn, ";
            else if (season < 403200) yearTime += "winter, ";

            int monthIndex = this.Seconds72 / (28 * 1200);
            string[] monthNames = { "Granite", "Slate", "Felsite", "Hematite", "Malachite", "Galena", "Limestone", "Sandstone", "Timber", "Moonstone", "Opal", "Obsidian" };
            string monthName = monthNames[monthIndex];
            int dayIndex = 1 + (this.Seconds72 % (28 * 1200)) / 1200;

            return yearTime + " (" + Formatting.AddOrdinal(dayIndex) + " of " + monthName + ") ";
        }
        public string PrintParentCollection(bool link = true, DwarfObject pov = null)
        {
            EventCollection parent = ParentCollection;
            string collectionString = "";
            while (parent != null)
            {
                if (collectionString.Length > 0) collectionString += " as part of ";
                collectionString += parent.ToLink(link, pov);
                parent = parent.ParentCollection;
            }

            if (collectionString.Length > 0)
                return "In " + collectionString + ". ";
            else
                return collectionString;
        }

        public int CompareTo(object obj)
        {
            return this.ID.CompareTo(obj);
        }

        public int CompareTo(WorldEvent other)
        {
            return this.ID.CompareTo(other.ID);
        }
    }

    public class EntityAction
    {
        public static void DelegateUpsertEvent(List<WorldEvent> events, List<Property> properties, World world)
        {
            var action = properties.Where(x => x.Name == "action").Select(x => new int?(System.Convert.ToInt32(x.Value))).FirstOrDefault();
            if (action.HasValue && action.Value > -1)
            {
                switch (action)
                {
                    case 0: World.UpsertEvent<EntityPrimaryCriminals>(events, properties, world); break;
                    case 1: World.UpsertEvent<EntityRelocate>(events, properties, world); break;
                }
            }
        }
    }

    public class HFActOnBuilding
    {
        public static void DelegateUpsertEvent(List<WorldEvent> events, List<Property> properties, World world)
        {
            var action = properties.Where(x => x.Name == "action").Select(x => new int?(System.Convert.ToInt32(x.Value))).FirstOrDefault();
            if (action.HasValue && action.Value > -1)
            {
                switch (action)
                {
                    case 0: World.UpsertEvent<HFProfanedStructure>(events, properties, world); break;
                        //case 1: World.UpsertEvent<HFDisturbedStructure>(events, properties, world); break;
                }
            }
        }
    }

}