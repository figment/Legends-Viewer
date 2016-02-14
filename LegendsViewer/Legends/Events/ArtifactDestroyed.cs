using System;
using System.Collections.Generic;
using LegendsViewer.Legends.Parser;

namespace LegendsViewer.Legends.Events
{
    public class ArtifactDestroyed : WorldEvent
    {
        public Artifact Artifact { get; set; }
        public Site Site { get; set; }
        public HistoricalFigure Destroyer { get; set; }

        public override void Merge(List<Property> properties, World world)
        {
            base.Merge(properties, world);

            foreach (Property property in properties)
                switch (property.Name)
                {
                    case "artifact_id": Artifact = world.GetArtifact(property.ValueAsInt()); Artifact.AddEvent(this); break;
                    case "site_id": Site = world.GetSite(property.ValueAsInt()); Site.AddEvent(this); break;
                    case "destroyer_enid": Destroyer = world.GetHistoricalFigure(property.ValueAsInt()); Destroyer.AddEvent(this); break;
                }
        }
        
        public override string Print(bool link = true, DwarfObject pov = null)
        {
            string eventString = GetYearTime();
            eventString += Artifact.ToSafeLink(link, pov);
            eventString += " was destroyed by ";
            eventString += Destroyer.ToSafeLink(link, pov);
            eventString += " in ";
            eventString += Site.ToSafeLink(link, pov);
            eventString += ".";
            return eventString;
        }
    }
}