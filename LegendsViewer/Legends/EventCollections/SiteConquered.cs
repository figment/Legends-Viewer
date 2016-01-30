﻿using System;
using System.Collections.Generic;
using System.Linq;
using LegendsViewer.Legends.Enums;
using LegendsViewer.Legends.Events;
using LegendsViewer.Legends.Parser;
using LegendsViewer.Controls.HTML.Utilities;

namespace LegendsViewer.Legends.EventCollections
{
    public class SiteConquered : EventCollection
    {
        public string Icon = "<i class=\"glyphicon fa-fw glyphicon-pawn\"></i>";

        public int Ordinal { get; set; }
        public SiteConqueredType ConquerType { get; set; }
        public Site Site { get; set; }
        public Entity Attacker { get; set; }
        public Entity Defender { get; set; }
        public Battle Battle { get; set; }
        public List<HistoricalFigure> Deaths { get { return GetSubEvents().OfType<HFDied>().Select(death => death.HistoricalFigure).ToList(); } set { } }
        public static List<string> Filters;
        public override List<WorldEvent> FilteredEvents
        {
            get { return AllEvents.Where(dwarfEvent => !Filters.Contains(dwarfEvent.Type)).ToList(); }
        }
        public SiteConquered()
        {
            Initialize();
        }
        
        private void InternalMerge(List<Property> properties, World world)
        {
            foreach (Property property in properties)
                switch (property.Name)
                {
                    case "ordinal": Ordinal = property.ValueAsInt(); break;
                    case "war_eventcol":
                        ParentCollection = world.GetEventCollection(property.ValueAsInt());
                        if (ParentCollection != null)
                        {
                            (ParentCollection as War).DeathCount += Collection.OfType<HFDied>().Count();
                            if (Attacker == (ParentCollection as War).Attacker)
                                (ParentCollection as War).AttackerVictories.Add(this);
                            else
                                (ParentCollection as War).DefenderVictories.Add(this);
                        }
                        break;
                    case "site_id": Site = world.GetSite(property.ValueAsInt()); Site.Warfare.Add(this); break;
                    case "attacking_enid": Attacker = world.GetEntity(property.ValueAsInt()); break;
                    case "defending_enid": Defender = world.GetEntity(property.ValueAsInt()); break;
                }

            if (Collection.OfType<PlunderedSite>().Any()) ConquerType = SiteConqueredType.Pillaging;
            else if (Collection.OfType<DestroyedSite>().Any()) ConquerType = SiteConqueredType.Destruction;
            else if (Collection.OfType<NewSiteLeader>().Any() || Collection.OfType<SiteTakenOver>().Any()) ConquerType = SiteConqueredType.Conquest;
            else ConquerType = SiteConqueredType.Unknown;
            if (ConquerType == SiteConqueredType.Pillaging) Notable = false;
        }

        public override void Merge(List<Property> properties, World world)
            {
            base.Merge(properties, world);
            InternalMerge(properties, world);
            }

        private void Initialize()
        {
            Ordinal = 1;
        }

        public override string ToLink(bool link = true, DwarfObject pov = null)
        {
            string name = "The " + GetOrdinal(Ordinal) + ConquerType + " of " + Site.ToLink(false);
            if (link)
            {
                string title = Type;
                title += "&#13";
                title += Attacker.PrintEntity(false) + " (Attacker)(V)";
                title += "&#13";
                title += Defender.PrintEntity(false) + " (Defender)";

                string linkedString = "";
                if (pov != this)
                {
                    linkedString = "<a href = \"collection#" + ID + "\" title=\"" + title + "\"><font color=\"6E5007\">" + name + "</font></a>";
                    if (pov != Battle) linkedString += " as a result of " + Battle.ToLink();
                }
                else
                {
                    linkedString = Icon + "<a title=\"" + title + "\">" + HTMLStyleUtil.CurrentDwarfObject(name) + "</a>";
                }
                return linkedString;
            }
            else
            {
                return name;
            }
        }
        public override string ToString()
        {
            return ToLink(false);
        }

    }
}
