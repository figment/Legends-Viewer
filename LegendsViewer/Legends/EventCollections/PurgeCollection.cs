using System;
using System.Collections.Generic;
using System.Linq;
using LegendsViewer.Legends.Events;
using LegendsViewer.Legends.Parser;

namespace LegendsViewer.Legends.EventCollections
{
    public class PurgeCollection : EventCollection
    {
        public String Name;
        public string Adjective;
        public Site Site { get; set; }
        public int Ordinal;

        public List<string> Filters;
        public override List<WorldEvent> FilteredEvents
        {
            get { return AllEvents.Where(dwarfEvent => !Filters.Contains(dwarfEvent.Type)).ToList(); }
        }

        public PurgeCollection()
        {
            Ordinal = -1; 
            Adjective = "";
        }

        private void InternalMerge(List<Property> properties, World world)
        {
            foreach (Property property in properties)
                switch (property.Name)
                {
                    case "adjective": Adjective = String.Intern(property.Value); break;
                    case "site_id": Site = world.GetSite(Convert.ToInt32(property.Value)); break;
                    case "ordinal": Ordinal = Convert.ToInt32(property.Value); break;
                }
            Name = string.Format("The {0} {1} Purge in {2}", GetOrdinal(Ordinal), Adjective, Site.ToString());
        }

        public override void Merge(List<Property> properties, World world)
        {
            base.Merge(properties, world);
            InternalMerge(properties, world);
        }

        public override string ToLink(bool link = true, DwarfObject pov = null)
        {
            return Name;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
