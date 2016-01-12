using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LegendsViewer.Legends
{
    public class Journey : EventCollection
    {
        public int Ordinal;
        public List<string> Filters;
        public override List<WorldEvent> FilteredEvents
        {
            get { return AllEvents.Where(dwarfEvent => !Filters.Contains(dwarfEvent.Type)).ToList(); }
        }

        public Journey() { Ordinal = -1; }
        public Journey(List<Property> properties, World world)
            : base(properties, world)
        {
            InternalMerge(properties, world);
        }


        private void InternalMerge(List<Property> properties, World world)
        {
            foreach (Property property in properties)
                switch (property.Name)
                {
                    case "ordinal": Ordinal = Convert.ToInt32(property.Value); break;
                }
        }

        public override void Merge(List<Property> properties, World world)
        {
            base.Merge(properties, world);
            InternalMerge(properties, world);
        }

        public override string ToLink(bool link = true, DwarfObject pov = null)
        {
            return "a journey";
        }
    }
}
