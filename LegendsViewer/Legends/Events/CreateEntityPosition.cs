using System;
using System.Collections.Generic;
using LegendsViewer.Legends.Parser;

namespace LegendsViewer.Legends.Events
{
    public class CreateEntityPosition : WorldEvent
    {
        public HistoricalFigure HistoricalFigure { get; set; }
        public Entity Civ { get; set; }
        public Entity SiteCiv { get; set; }
        public string Position { get; set; }
        public int Reason { get; set; } // TODO // legends_plus.xml

        public override void Merge(List<Property> properties, World world)
        {
            base.Merge(properties, world);

            foreach (Property property in properties)
                switch (property.Name)
                {
                    case "civ":
                    case "civ_id": Civ = world.GetEntity(property.ValueAsInt()); Civ.AddEvent(this); break;
                    case "site_civ": SiteCiv = world.GetEntity(property.ValueAsInt()); SiteCiv.AddEvent(this); break;
                    case "histfig": HistoricalFigure = world.GetHistoricalFigure(property.ValueAsInt()); HistoricalFigure.AddEvent(this); break;
                    case "reason": Reason = property.ValueAsInt(); break;
                    case "position": Position = string.Intern(Formatting.InitCaps(property.Value)); break;
                }
        }
        
        public override string Print(bool link = true, DwarfObject pov = null)
        {
            string eventString = GetYearTime();
            switch (Reason)
            {
                case 0:
                    eventString += HistoricalFigure.ToSafeLink(link, pov);
                    eventString += " of ";
                    eventString += Civ.ToSafeLink(link, pov, "CIV");
                    eventString += " created the position of ";
                    eventString += !string.IsNullOrWhiteSpace(Position) ? Position : "UNKNOWN POSITION";
                    eventString += " through force of argument. ";
                    break;
                case 1:
                    eventString += HistoricalFigure.ToSafeLink(link, pov);
                    eventString += " of ";
                    eventString += Civ.ToSafeLink(link, pov, "CIV");
                    eventString += " compelled the creation of the position of ";
                    eventString += !string.IsNullOrWhiteSpace(Position) ? Position : "UNKNOWN POSITION";
                    eventString += " with threats of violence. ";
                    break;
                case 2:
                    eventString += SiteCiv.ToSafeLink(link, pov);
                    eventString += " collaborated to create the position of ";
                    eventString += !string.IsNullOrWhiteSpace(Position) ? Position : "UNKNOWN POSITION";
                    eventString += ". ";
                    break;
                case 3:
                    eventString += HistoricalFigure.ToSafeLink(link, pov);
                    eventString += " of ";
                    eventString += Civ.ToSafeLink(link, pov, "CIV");
                    eventString += " created the position of ";
                    eventString += !string.IsNullOrWhiteSpace(Position) ? Position : "UNKNOWN POSITION";
                    eventString += ", pushed by a wave of popular support. ";
                    break;
                case 4:
                    eventString += HistoricalFigure.ToSafeLink(link, pov);
                    eventString += " of ";
                    eventString += Civ.ToSafeLink(link, pov, "CIV");
                    eventString += " created the position of ";
                    eventString += !string.IsNullOrWhiteSpace(Position) ? Position : "UNKNOWN POSITION";
                    eventString += " as a matter of course. ";
                    break;
            }
            eventString += PrintParentCollection(link, pov);
            return eventString;
        }
    }
}