using System;
using System.Collections.Generic;
using LegendsViewer.Legends.Events;
using LegendsViewer.Legends.Parser;
using LegendsViewer.Controls;

namespace LegendsViewer.Legends
{
    public abstract class WorldObject : DwarfObject
    {
        public List<WorldEvent> Events { get; set; }
        public int EventCount { get { return Events.Count; } set { } }
        public int ID { get; set; }

        public WorldObject() { 
            ID = -1; 
            Events = new List<WorldEvent>(); 
        }

        public virtual void Merge(List<Property> properties, World world)
        {
            foreach (Property property in properties)
                switch (property.Name)
                {
                    case "id": ID = property.ValueAsInt(); property.Known = true; break;
                    default: break;
                }
        }

        internal static IComparer<T> GetDefaultComparer<T>() where T : WorldObject
        {
            return new LambaComparer<T>((x, y) => Comparer<int>.Default.Compare(x.ID, y.ID));
        }

        public override string ToLink(bool link = true, DwarfObject pov = null)
        {
            return "";
        }
        public abstract List<WorldEvent> FilteredEvents { get; }
        public override string ToString()
        {
            return ToLink(false);
        }
    }
}
