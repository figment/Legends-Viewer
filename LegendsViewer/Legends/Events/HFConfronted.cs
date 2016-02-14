using System;
using System.Collections.Generic;
using System.Linq;
using LegendsViewer.Legends.Enums;
using LegendsViewer.Legends.Parser;

namespace LegendsViewer.Legends.Events
{
    public class HFConfronted : WorldEvent
    {
        public HistoricalFigure HistoricalFigure { get; set; }
        public ConfrontSituation Situation { get; set; }
        public List<ConfrontReason> Reasons { get; set; }
        public Site Site { get; set; }
        public WorldRegion Region { get; set; }
        public UndergroundRegion UndergroundRegion { get; set; }
        public Location Coordinates { get; set; }
        private string UnknownSituation;
        private List<string> UnknownReasons;

        public override void Merge(List<Property> properties, World world)
        {
            base.Merge(properties, world);

            Reasons = new List<ConfrontReason>();
            UnknownReasons = new List<string>();
            foreach (Property property in properties)
            {
                switch (property.Name)
                {
                    case "hfid": HistoricalFigure = world.GetHistoricalFigure(property.ValueAsInt()); break;
                    case "situation":
                        switch (property.Value)
                        {
                            case "general suspicion": Situation = ConfrontSituation.GeneralSuspicion; break;
                            default:
                                Situation = ConfrontSituation.Unknown;
                                UnknownSituation = property.Value;
                                world.ParsingErrors.Report("Unknown HF Confronted Situation: " + UnknownSituation);
                                break;
                        }
                        break;
                    case "reason":
                        switch (property.Value)
                        {
                            case "murder": Reasons.Add(ConfrontReason.Murder); break;
                            case "ageless": Reasons.Add(ConfrontReason.Ageless); break;
                            default:
                                Reasons.Add(ConfrontReason.Unknown);
                                UnknownReasons.Add(property.Value);
                                world.ParsingErrors.Report("Unknown HF Confronted Reason: " + property.Value);
                                break;
                        }
                        break;
                    case "site_id": Site = world.GetSite(property.ValueAsInt()); break;
                    case "subregion_id": Region = world.GetRegion(property.ValueAsInt()); break;
                    case "feature_layer_id": UndergroundRegion = world.GetUndergroundRegion(property.ValueAsInt()); break;
                    case "coords": Coordinates = Formatting.ConvertToLocation(property.Value); break;
                }
            }

            HistoricalFigure.AddEvent(this);
            Site.AddEvent(this);
            Region.AddEvent(this);
            UndergroundRegion.AddEvent(this);
        }

        public override string Print(bool link = true, DwarfObject pov = null)
        {
            string eventString = GetYearTime() + HistoricalFigure.ToSafeLink(link, pov);
            string situationString = "";
            switch (Situation)
            {
                case ConfrontSituation.GeneralSuspicion: situationString = "aroused general suspicion"; break;
                case ConfrontSituation.Unknown: situationString = "(" + UnknownSituation + ")"; break;
            }
            eventString += " " + situationString;

            if (Region != null)
                eventString += " in " + Region.ToSafeLink(link, pov);

            if (Site != null)
                eventString += " at " + Site.ToSafeLink(link, pov);

            string reasonString = "after ";
            int unknownReasonIndex = 0;
            foreach (ConfrontReason reason in Reasons)
            {
                switch (reason)
                {
                    case ConfrontReason.Murder: reasonString += "a murder"; break;
                    case ConfrontReason.Ageless: reasonString += "appearing not to age"; break;
                    case ConfrontReason.Unknown:
                        reasonString += "(" + UnknownReasons[unknownReasonIndex++] + ")";
                        break;
                }

                if (reason != Reasons.Last() && Reasons.Count > 2)
                    reasonString += ",";
                reasonString += " ";
                if (Reasons.Count > 1 && reason == Reasons[Reasons.Count - 2])
                    reasonString += "and ";
            }
            eventString += " " + reasonString + ". ";

            eventString += PrintParentCollection(link, pov);

            return eventString;
        }
    }
}