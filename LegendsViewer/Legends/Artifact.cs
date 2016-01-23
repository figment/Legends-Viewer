using LegendsViewer.Controls.HTML.Utilities;
using System.Collections.Generic;
using System.Linq;

namespace LegendsViewer.Legends
{
    public class Artifact : WorldObject
    {
        public string Name { get; set; }
        public string Item { get; set; }
        public HistoricalFigure Creator { get; set; }
        public string Type { get; set; } // legends_plus.xml
        public string SubType { get; set; } // legends_plus.xml
        public string Description { get; set; } // legends_plus.xml
        public string Material { get; set; } // legends_plus.xml
        public static List<string> Filters;
        public override List<WorldEvent> FilteredEvents
        {
            get { return Events.Where(dwarfEvent => !Filters.Contains(dwarfEvent.Type)).ToList(); }
        }

        public Artifact()
        {
            Name = "Untitled";
        }
        public Artifact(List<Property> properties, World world)
            : base(properties, world)
        {
            Name = "Untitled";
            InternalMerge(properties, world);
        }
        private void InternalMerge(List<Property> properties, World world)
        {
            foreach(Property property in properties)
            {
                switch (property.Name)
                {
                    case "name": Name = Formatting.InitCaps(property.Value); break;
                    case "item": Item = string.Intern(Formatting.InitCaps(property.Value)); break;
                    case "item_type": Type = string.Intern(Formatting.InitCaps(property.Value)); break;
                    case "item_subtype": SubType = string.Intern(Formatting.InitCaps(property.Value)); break;
                    case "item_description": Description = Formatting.InitCaps(property.Value); break;
                    case "mat": Material = string.Intern(Formatting.InitCaps(property.Value)); break;

                }
            }
        }
        public override void Merge(List<Property> properties, World world)
        {
            base.Merge(properties, world);
            InternalMerge(properties, world);
        }

        public override string ToString() { return Name; }

        public override string ToLink(bool link = true, DwarfObject pov = null)
        {
            if (link)
            {
                string linkedString = "";
                if (pov != this)
                {
                    string title = "Events: " + this.Events.Count;

                    linkedString = "<a href = \"artifact#" + this.ID + "\" title=\"" + title + "\">" + Name + "</a>";
                }
                else
                    linkedString = HTMLStyleUtil.CurrentDwarfObject(Name);
                return linkedString;
            }
            else
                return Name;
        }
    }
}
