using System;
using System.Collections.Generic;
using LegendsViewer.Legends.Parser;

namespace LegendsViewer.Legends.Events
{
    public class ArtifactTransformed : WorldEvent
    {
        public int UnitID { get; set; }
        public Artifact NewArtifact { get; set; }
        public Artifact OldArtifact { get; set; }
        public HistoricalFigure HistoricalFigure { get; set; }
        public Site Site { get; set; }

        
        public override void Merge(List<Property> properties, World world)
        {
            base.Merge(properties, world);

            foreach (Property property in properties)
                switch (property.Name)
                {
                    case "unit_id": UnitID = property.ValueAsInt(); break;
                    case "new_artifact_id": NewArtifact = world.GetArtifact(property.ValueAsInt()); break;
                    case "old_artifact_id": OldArtifact = world.GetArtifact(property.ValueAsInt()); break;
                    case "hist_figure_id": HistoricalFigure = world.GetHistoricalFigure(property.ValueAsInt()); break;
                    case "site_id": Site = world.GetSite(property.ValueAsInt()); break;
                }
            NewArtifact.AddEvent(this);
            OldArtifact.AddEvent(this);
            HistoricalFigure.AddEvent(this);
            Site.AddEvent(this);
        }
        public override string Print(bool link = true, DwarfObject pov = null)
        {
            string eventString = GetYearTime();
            eventString += NewArtifact.ToSafeLink(link, pov);
            eventString += ", ";
            if (!string.IsNullOrWhiteSpace(NewArtifact.Material))
            {
                eventString += NewArtifact.Material;
            }
            if (!string.IsNullOrWhiteSpace(NewArtifact.SubType))
            {
                eventString += " ";
                eventString += NewArtifact.SubType;
            }
            else
            {
                eventString += " ";
                eventString += !string.IsNullOrWhiteSpace(NewArtifact.Type) ? NewArtifact.Type.ToLower() : "UNKNOWN TYPE";
            }
            eventString += ", was made from ";
            eventString += OldArtifact.ToSafeLink(link, pov);
            eventString += ", ";
            if (!string.IsNullOrWhiteSpace(OldArtifact.Material))
            {
                eventString += OldArtifact.Material;
            }
            if (!string.IsNullOrWhiteSpace(OldArtifact.SubType))
            {
                eventString += " ";
                eventString += OldArtifact.SubType;
            }
            else
            {
                eventString += " ";
                eventString += !string.IsNullOrWhiteSpace(OldArtifact.Type) ? OldArtifact.Type.ToLower() : "UNKNOWN TYPE";
            }
            if (Site != null)
                eventString += " in " + Site.ToSafeLink(link, pov);
            eventString += " by ";
            eventString += HistoricalFigure.ToSafeLink(link, pov);
            eventString += ". ";
            eventString += PrintParentCollection(link, pov);
            return eventString;
        }
    }
}