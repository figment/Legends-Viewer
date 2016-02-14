﻿using System;
using System.Collections.Generic;
using System.Linq;
using LegendsViewer.Legends.Events;
using LegendsViewer.Legends.Parser;

namespace LegendsViewer.Legends.EventCollections
{
    class Insurrection : EventCollection
    {
        public String Name { get; set; }
        public Site Site { get; set; }
        public Entity TargetEntity { get; set; }
        public int Ordinal { get; set; }
        public List<string> Filters;
        public override List<WorldEvent> FilteredEvents
        {
            get { return AllEvents.Where(dwarfEvent => !Filters.Contains(dwarfEvent.Type)).ToList(); }
        }

        public Insurrection() { Ordinal = -1; }

        public override void Merge(List<Property> properties, World world)
        {
            base.Merge(properties, world);

            Filters = new List<string>();
            foreach (Property property in properties)
            {
                switch (property.Name)
                {
                    case "site_id":
                        Site = world.GetSite(property.ValueAsInt());
                        break;
                    case "target_enid":
                        TargetEntity = world.GetEntity(property.ValueAsInt());
                        break;
                    case "ordinal":
                        Ordinal = property.ValueAsInt();
                        break;
                }
            }

            Name = "The " + GetOrdinal(Ordinal) + " Insurrection in " + Site.ToString();
            InsurrectionStarted insurrectionStart = Collection.OfType<InsurrectionStarted>().FirstOrDefault();
            if (insurrectionStart != null)
            {
                insurrectionStart.ActualStart = true;
            }
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
