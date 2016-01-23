using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LegendsViewer.Legends
{
    public class EntityPopulation : WorldObject
    {
        public string Race { get; set; } // legends_plus.xml
        public int Count { get; set; } // legends_plus.xml
        public Entity Entity { get; set; } // legends_plus.xml

        public static List<string> Filters;
        public override List<WorldEvent> FilteredEvents
        {
            get { return Events.Where(dwarfEvent => !Filters.Contains(dwarfEvent.Type)).ToList(); }
        }

        private void InternalMerge(List<Property> properties, World world)
        {
            foreach (Property property in properties)
                switch (property.Name)
                {
                    case "race":
                    {
                        var vals = property.Value.Split(':');
                        if (vals.Length == 2) { Race = Formatting.InitCaps(vals[0]); Count = Convert.ToInt32(vals[1]); }
                        property.Known = true;
                    } break;
                    case "civ_id":
                        Entity = world.GetEntity(property.ValueAsInt());
                        break;
                }
            }

        public override void Merge(List<Property> properties, World world)
        {
            base.Merge(properties, world);
            InternalMerge(properties, world);
        }
    }
}
