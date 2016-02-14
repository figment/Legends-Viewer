using System;
using System.Collections.Generic;
using LegendsViewer.Legends.Parser;

namespace LegendsViewer.Legends.Events
{
    public class ArtifactCreated : WorldEvent
    {
        public int UnitID;
        public Artifact Artifact;
        public bool RecievedName;
        public HistoricalFigure HistoricalFigure;
        public Site Site;

        public override void Merge(List<Property> properties, World world)
        {
            base.Merge(properties, world);

            foreach (Property property in properties)
                switch (property.Name)
                {
                    case "unit_id": UnitID = property.ValueAsInt(); break;
                    case "artifact_id": Artifact = world.GetArtifact(property.ValueAsInt()); Artifact.AddEvent(this); break;
                    case "hfid":
                    case "hist_figure_id": HistoricalFigure = world.GetHistoricalFigure(property.ValueAsInt()); HistoricalFigure.AddEvent(this); break;
                    case "site":
                    case "site_id": Site = world.GetSite(property.ValueAsInt()); Site.AddEvent(this); break;
                    case "name_only": RecievedName = true; property.Known = true; break;
                }
            if (Artifact != null)
                Artifact.Creator = HistoricalFigure;
        }
        
        public override string Print(bool link = true, DwarfObject pov = null)
        {
            string eventString = this.GetYearTime() + Artifact.ToSafeLink(link, pov);
            if (RecievedName)
                eventString += " recieved its name";
            else
                eventString += " was created";
            if (Site != null)
                eventString += " in " + Site.ToSafeLink(link, pov);
            if (RecievedName)
                eventString += " from ";
            else
                eventString += " by ";
            eventString += HistoricalFigure.ToSafeLink(link, pov) + ". ";
            eventString += PrintParentCollection(link, pov);
            return eventString;
        }
    }
}