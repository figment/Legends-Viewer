﻿using System;
using System.Collections.Generic;
using System.Linq;
using LegendsViewer.Legends.EventCollections;
using LegendsViewer.Legends.Events;
using LegendsViewer.Legends.Parser;

namespace LegendsViewer.Legends
{
    public class Era : WorldObject
    {
        public static List<string> Filters;
        public List<War> Wars { get; set; }
        public override List<WorldEvent> FilteredEvents
        {
            get { return Events.Where(dwarfEvent => !Filters.Contains(dwarfEvent.Type)).ToList(); }
        }
        public int StartYear, EndYear;
        public string Name;

        public Era(List<Property> properties, World world)
            : base(properties, world)
        {
            this.ID = world.Eras.Count;
            InternalMerge(properties, world);
        }

        public Era(int startYear, int endYear, World world)
        {
            world.TempEras.Add(this);
            this.ID = world.Eras.Count - 1 + world.TempEras.Count;
            StartYear = startYear; EndYear = endYear; Name = "";
            Events = world.Events.Where(ev => ev.Year >= StartYear && ev.Year <= EndYear).OrderBy(events => events.Year).ToList();
            Wars = world.EventCollections.OfType<War>().Where(war => (war.StartYear >= StartYear && war.EndYear <= EndYear && war.EndYear != -1) //entire war between
                                                                                                    || (war.StartYear >= StartYear && war.StartYear <= EndYear) //war started before & ended
                                                                                                    || (war.EndYear >= StartYear && war.EndYear <= EndYear && war.EndYear != -1) //war started during
                                                                                                    || (war.StartYear <= StartYear && war.EndYear >= EndYear) //war started before & ended after
                                                                                                    || (war.StartYear <= StartYear && war.EndYear == -1)).ToList();
        }
        private void InternalMerge(List<Property> properties, World world)
        {
            foreach (Property property in properties)
                switch (property.Name)
                {
                    case "start_year": StartYear = property.ValueAsInt(); break;
                    case "name": Name = property.Value; break;
                }
        }
        public override void Merge(List<Property> properties, World world)
        {
            base.Merge(properties, world);
            InternalMerge(properties, world);
        }

        public override string ToLink(bool link = true, DwarfObject pov = null)
        {
            if (Name != "") return Name;
            else return "(" + StartYear + " - " + EndYear + ")";
        }
        public override string ToString() { return Name; }
    }
}
