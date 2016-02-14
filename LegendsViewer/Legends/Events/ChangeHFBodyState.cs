using System;
using System.Collections.Generic;
using System.Linq;
using LegendsViewer.Legends.Enums;
using LegendsViewer.Legends.Parser;

namespace LegendsViewer.Legends.Events
{
    public class ChangeHFBodyState : WorldEvent
    {
        public HistoricalFigure HistoricalFigure { get; set; }
        public BodyState BodyState { get; set; }
        public Site Site { get; set; }
        public int StructureID { get; set; }
        public Structure Structure { get; set; }
        public WorldRegion Region { get; set; }
        public UndergroundRegion UndergroundRegion { get; set; }
        public Location Coordinates { get; set; }
        private string UnknownBodyState;

        public override void Merge(List<Property> properties, World world)
        {
            base.Merge(properties, world);

            foreach (Property property in properties)
            {
                switch (property.Name)
                {
                    case "hfid": HistoricalFigure = world.GetHistoricalFigure(property.ValueAsInt()); HistoricalFigure.AddEvent(this); break;
                    case "body_state":
                        switch (property.Value)
                        {
                            case "entombed at site": BodyState = BodyState.EntombedAtSite; break;
                            default:
                                BodyState = BodyState.Unknown;
                                UnknownBodyState = property.Value;
                                world.ParsingErrors.Report("Unknown HF Body State: " + UnknownBodyState);
                                break;
                        }
                        break;
                    case "site_id": Site = world.GetSite(property.ValueAsInt()); Site.AddEvent(this); break;
                    case "building_id": Structure = Site?.GetStructure(property.ValueAsInt()); Structure.AddEvent(this); break;
                    case "subregion_id": Region = world.GetRegion(property.ValueAsInt()); Region.AddEvent(this); break;
                    case "feature_layer_id": UndergroundRegion = world.GetUndergroundRegion(property.ValueAsInt()); UndergroundRegion.AddEvent(this); break;
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
            string eventString = GetYearTime() + HistoricalFigure.ToSafeLink(link, pov) + " ";
            string stateString = "";
            switch (BodyState)
            {
                case BodyState.EntombedAtSite: stateString = "was entombed"; break;
                case BodyState.Unknown: stateString = "(" + UnknownBodyState + ")"; break;
            }
            eventString += stateString;
            if (Region != null)
                eventString += " in " + Region.ToSafeLink(link, pov);
            if (Site != null)
                eventString += " at " + Site.ToSafeLink(link, pov);
            eventString += " within ";
            eventString += Structure.ToSafeLink(link, pov);
            eventString += ". ";
            eventString += PrintParentCollection(link, pov);
            return eventString;
        }
    }
}