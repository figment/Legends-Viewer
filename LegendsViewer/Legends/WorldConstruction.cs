using LegendsViewer.Controls.HTML.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LegendsViewer.Legends
{
    public class WorldConstruction : WorldObject
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public List<Location> Locations { get; set; }
        public static List<string> Filters;
        public override List<WorldEvent> FilteredEvents
        {
            get { return Events.Where(dwarfEvent => !Filters.Contains(dwarfEvent.Type)).ToList(); }
        }

        public WorldConstruction() { Name = "Unknown"; Type = "INVALID CONSTRUCTION"; Locations = new List<Location>(); }

        public WorldConstruction(List<Property> properties, World world)
            : base(properties, world)
        {
            Name = "Unknown"; Type = "INVALID CONSTRUCTION"; Locations = new List<Location>();
            InternalMerge(properties, world);
        }
        private void InternalMerge(List<Property> properties, World world)
        {
            foreach (Property property in properties)
                switch (property.Name)
                {
                    case "name": Name = Formatting.InitCaps(property.Value); property.Known = true; break;
                    case "type": Type = string.Intern(Formatting.InitCaps(property.Value)); property.Known = true; break;
                    case "coords": Locations = property.Value.Split(new[] { "|" }, StringSplitOptions.RemoveEmptyEntries)
                     .Select(Formatting.ConvertToLocation).ToList(); property.Known = true; break;
                }
        }
        public override void Merge(List<Property> properties, World world)
        {
            base.Merge(properties, world);
            InternalMerge(properties, world);
        }

        public override string ToString() { return this.Type; }


        public override string ToLink(bool link = true, DwarfObject pov = null)
        {
            if (link)
            {
                if (pov != this)
                    return "<a href = \"construct#" + this.ID + "\">" + Name + "</a>";
                else
                    return HTMLStyleUtil.CurrentDwarfObject(Name);
            }
            else
                return Name;
        }
        
    }
}
