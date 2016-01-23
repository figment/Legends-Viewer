﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
        public Insurrection(List<Property> properties, World world)
            : base(properties, world)
        {
            InternalMerge(properties, world);
        }

        private void InternalMerge(List<Property> properties, World world)
        {
            Filters = new List<string>();
            foreach (Property property in properties)
            {
                switch (property.Name)
                {
                    case "site_id":
                        Site = world.GetSite(Convert.ToInt32(property.Value));
                        break;
                    case "target_enid":
                        TargetEntity = world.GetEntity(Convert.ToInt32(property.Value));
                        break;
                    case "ordinal":
                        Ordinal = Convert.ToInt32(property.Value);
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
