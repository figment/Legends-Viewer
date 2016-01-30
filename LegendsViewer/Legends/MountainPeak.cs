﻿using LegendsViewer.Controls.HTML.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using LegendsViewer.Legends.Events;
using LegendsViewer.Legends.Interfaces;
using LegendsViewer.Legends.Parser;

namespace LegendsViewer.Legends
{
    public class MountainPeak : WorldObject, IHasCoordinates
    {
        public string Name { get; set; } // legends_plus.xml
        public List<Location> Coordinates { get; set; } // legends_plus.xml
        public int Height { get; set; } // legends_plus.xml

        public string Icon = "<i class=\"fa fa-fw fa-wifi fa-flip-vertical\"></i>";

        public static List<string> Filters;
        public override List<WorldEvent> FilteredEvents
        {
            get { return Events.Where(dwarfEvent => !Filters.Contains(dwarfEvent.Type)).ToList(); }
        }

        public override void Merge(List<Property> properties, World world)
        {
            base.Merge(properties, world);
            InternalMerge(properties, world);
        }
        private void InternalMerge(List<Property> properties, World world)
        {
            Name = "Untitled";
            Coordinates = new List<Location>();

            foreach (Property property in properties)
            {
                switch (property.Name)
                {
                    case "name": Name = Formatting.InitCaps(property.Value); break;
                    case "coords":
                        string[] coordinateStrings = property.Value.Split(new char[] { '|' },
                            StringSplitOptions.RemoveEmptyEntries);
                        foreach (var coordinateString in coordinateStrings)
                        {
                            string[] xYCoordinates = coordinateString.Split(',');
                            int x = Convert.ToInt32(xYCoordinates[0]);
                            int y = Convert.ToInt32(xYCoordinates[1]);
                            Coordinates.Add(new Location(x, y));
                        }
                        break;
                    case "height": Height = property.ValueAsInt(); break;
                }
            }
        }

        public override string ToString() { return Name; }

        public override string ToLink(bool link = true, DwarfObject pov = null)
        {
            if (link)
            {
                string linkedString = "";
                if (pov != this)
                {
                    string title = "";
                    title += "MountainPeak";
                    title += "&#13";
                    title += "Events: " + Events.Count;

                    linkedString = Icon + "<a href = \"mountainpeak#" + ID + "\" title=\"" + title + "\">" + Name + "</a>";
                }
                else
                    linkedString = Icon + HTMLStyleUtil.CurrentDwarfObject(Name);
                return linkedString;
            }
            else
                return Name;
        }
    }
}
