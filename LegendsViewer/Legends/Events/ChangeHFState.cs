using System;
using System.Collections.Generic;
using System.Linq;
using LegendsViewer.Legends.Enums;
using LegendsViewer.Legends.Parser;

namespace LegendsViewer.Legends.Events
{
    public class ChangeHFState : WorldEvent
    {
        public HistoricalFigure HistoricalFigure;
        public Site Site;
        public WorldRegion Region;
        public UndergroundRegion UndergroundRegion;
        public Location Coordinates;
        public HFState State;
        //public HFSubState SubState;
        public int SubState;
        public string UnknownState;

        public override void Merge(List<Property> properties, World world)
        {
            base.Merge(properties, world);

            foreach (Property property in properties)
                switch (property.Name)
                {
                    case "state":
                        switch (property.Value)
                        {
                            case "0": case "wandering": State = HFState.Wandering; break;
                            case "1": case "settled": State = HFState.Settled; break;
                            case "2": case "refugee": State = HFState.Refugee; break;
                            case "5": case "visiting": State = HFState.Visiting; break;
                            case "scouting": State = HFState.Scouting; break;
                            case "snatcher": State = HFState.Snatcher; break;
                            case "thief": State = HFState.Thief; break;
                            case "hunting": State = HFState.Hunting; break;
                            default: State = HFState.Unknown; UnknownState = property.Value; world.ParsingErrors.Report("Unknown HF State: " + property.Value); break;
                        }
                        break;
                    case "substate":
                        SubState = property.ValueAsInt(); // 45,46, 47
                        break;
                    case "coords": Coordinates = Formatting.ConvertToLocation(property.Value); break;
                    case "hfid":
                        HistoricalFigure = world.GetHistoricalFigure(property.ValueAsInt());
                        if (HistoricalFigure != null && HistoricalFigure.AddEvent(this))
                            HistoricalFigure.States.Add(new HistoricalFigure.State(State, Year));
                        break;
                    case "site":
                    case "site_id": Site = world.GetSite(property.ValueAsInt()); Site.AddEvent(this); break;
                    case "subregion_id": Region = world.GetRegion(property.ValueAsInt()); Region.AddEvent(this); break;
                    case "feature_layer_id": UndergroundRegion = world.GetUndergroundRegion(property.ValueAsInt()); UndergroundRegion.AddEvent(this); break;
                }
            if (HistoricalFigure != null)
            {
                HistoricalFigure.State lastState = HistoricalFigure.States.LastOrDefault();
                if (lastState != null) lastState.EndYear = Year;
                HistoricalFigure.CurrentState = State;
            }
        }
        
        public override string Print(bool link = true, DwarfObject pov = null)
        {
            string eventString = GetYearTime() + HistoricalFigure.ToSafeLink(link, pov);
            if (State == HFState.Settled)
            {
                switch (SubState)
                {
                    case 45:
                        eventString += " fled to ";
                        break;
                    case 46:
                    case 47:
                        eventString += " moved to study in ";
                        break;
                    default:
                        eventString += " settled in ";
                        break;
                }
            }
            else if (State == HFState.Refugee || State == HFState.Snatcher || State == HFState.Thief) eventString += " became a " + State.ToString().ToLower() + " in ";
            else if (State == HFState.Wandering) eventString += " began wandering ";
            else if (State == HFState.Scouting) eventString += " began scouting the area around ";
            else if (State == HFState.Hunting) eventString += " began hunting great beasts in ";
            else if (State == HFState.Visiting) eventString += " visited ";
            else
            {
                eventString += " " + UnknownState + " in ";
            }
            if (Site != null) eventString += Site.ToSafeLink(link, pov);
            else if (Region != null) eventString += Region.ToSafeLink(link, pov);
            else if (UndergroundRegion != null) eventString += UndergroundRegion.ToSafeLink(link, pov);
            else eventString += "the wilds";
            eventString += ". ";
            eventString += PrintParentCollection(link, pov);
            return eventString;
        }
    }
}