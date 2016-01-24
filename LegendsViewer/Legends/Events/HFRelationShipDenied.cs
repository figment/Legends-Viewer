using System;
using System.Collections.Generic;
using LegendsViewer.Legends.Parser;

namespace LegendsViewer.Legends.Events
{
    public class HFRelationShipDenied : WorldEvent
    {
        public Site Site { get; set; }
        public WorldRegion Region { get; set; }
        public UndergroundRegion UndergroundRegion { get; set; }
        public HistoricalFigure Seeker { get; set; }
        public HistoricalFigure Target { get; set; }
        public string Relationship { get; set; }
        public string Reason { get; set; }
        public HistoricalFigure ReasonHF { get; set; }

        private void InternalMerge(List<Property> properties, World world)
        {
            foreach (Property property in properties)
                switch (property.Name)
                {
                    case "site_id":
                        Site = world.GetSite(property.ValueAsInt());
                        break;
                    case "subregion_id":
                        Region = world.GetRegion(property.ValueAsInt());
                        break;
                    case "feature_layer_id":
                        UndergroundRegion = world.GetUndergroundRegion(property.ValueAsInt());
                        break;
                    case "seeker_hfid":
                        Seeker = world.GetHistoricalFigure(property.ValueAsInt());
                        break;
                    case "target_hfid":
                        Target = world.GetHistoricalFigure(property.ValueAsInt());
                        break;
                    case "relationship":
                        Relationship = property.Value;
                        break;
                    case "reason":
                        Reason = property.Value;
                        break;
                    case "reason_id":
                        ReasonHF = world.GetHistoricalFigure(property.ValueAsInt());
                        break;
                }
            Site.AddEvent(this);
            Region.AddEvent(this);
            UndergroundRegion.AddEvent(this);
            Seeker.AddEvent(this);
            Target.AddEvent(this);
            if (ReasonHF != null && !ReasonHF.Equals(Seeker) && !ReasonHF.Equals(Target))
            {
                ReasonHF.AddEvent(this);
            }
        }

        public override void Merge(List<Property> properties, World world)
        {
            base.Merge(properties, world);
            InternalMerge(properties, world);
        }
        public override string Print(bool link = true, DwarfObject pov = null)
        {
            string eventString = GetYearTime();
            eventString += Seeker.ToSafeLink(link, pov);
            eventString += " was denied ";
            switch (Relationship)
            {
                case "apprentice":
                    eventString += "an apprenticeship under";
                    break;
                default:
                    break;
            }
            eventString += Target.ToSafeLink(link, pov);
            if (Site != null)
            {
                eventString += " in ";
                eventString += Site.ToSafeLink(link, pov);
            }
            if (ReasonHF != null)
            {
                switch (Reason)
                {
                    case "jealousy":
                        eventString += " due to jealousy of " + ReasonHF.ToSafeLink(link, pov);
                        break;
                    case "prefers working alone":
                        eventString += " as " + ReasonHF.ToSafeLink(link, pov) + " prefers to work alone";
                        break;
                    default:
                        break;
                }
            }
            eventString += ".";
            return eventString;
        }
    }
}