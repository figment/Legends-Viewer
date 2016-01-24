using System;
using System.Collections.Generic;
using System.Linq;
using LegendsViewer.Legends.Enums;
using LegendsViewer.Legends.Parser;

namespace LegendsViewer.Legends.Events
{
    public class Competition : OccasionEvent
    {
        HistoricalFigure Winner { get; set; }
        List<HistoricalFigure> Competitors { get; set; }

        private void InternalMerge(List<Property> properties, World world)
        {
            OccasionType = OccasionType.Competition;
            Competitors = new List<HistoricalFigure>();
            foreach (Property property in properties)
                switch (property.Name)
                {
                    case "winner_hfid":
                        Winner = world.GetHistoricalFigure(property.ValueAsInt());
                        break;
                    case "competitor_hfid":
                        Competitors.Add(world.GetHistoricalFigure(property.ValueAsInt()));
                        break;
                }
            Winner.AddEvent(this);
            Competitors.ForEach(competitor => competitor.AddEvent(this));
        }

        public override void Merge(List<Property> properties, World world)
        {
            base.Merge(properties, world);
            InternalMerge(properties, world);
        }
        public override string Print(bool link = true, DwarfObject pov = null)
        {
            string eventString = base.Print(link, pov);
            if (Competitors.Any())
            {
                eventString += "</br>";
                eventString += "Competing were ";
                for (int i = 0; i < Competitors.Count; i++)
                {
                    HistoricalFigure competitor = Competitors.ElementAt(i);
                    if (i == 0)
                    {
                        eventString += competitor.ToSafeLink(link, pov);
                    }
                    else if (i == Competitors.Count - 1)
                    {
                        eventString += " and " + competitor.ToSafeLink(link, pov);
                    }
                    else
                    {
                        eventString += ", " + competitor.ToSafeLink(link, pov);
                    }
                }
                eventString += ". ";
            }
            if (Winner != null)
            {
                eventString += "The winner was ";
                eventString += Winner.ToSafeLink(link, pov);
                eventString += ".";
            }
            return eventString;
        }
    }
}