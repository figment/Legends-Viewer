using System;
using System.Collections.Generic;
using LegendsViewer.Legends.Parser;

namespace LegendsViewer.Legends.Events
{
    public class WrittenContentComposed : WorldEvent
    {
        public Entity Civ { get; set; }
        public Site Site { get; set; }
        public WorldRegion Region { get; set; }
        public WrittenContent WrittenContent { get; set; }
        public HistoricalFigure HistoricalFigure { get; set; }
        public string Reason { get; set; }
        public int ReasonId { get; set; }
        public HistoricalFigure GlorifiedHF { get; set; }
        public HistoricalFigure CircumstanceHF { get; set; }
        public string Circumstance { get; set; }
        public int CircumstanceId { get; set; }

        public override void Merge(List<Property> properties, World world)
        {
            base.Merge(properties, world);

            foreach (Property property in properties)
                switch (property.Name)
                {
                    case "civ_id":
                        Civ = world.GetEntity(property.ValueAsInt()); Civ.AddEvent(this);
                        break;
                    case "site_id":
                        Site = world.GetSite(property.ValueAsInt()); Site.AddEvent(this);
                        break;
                    case "hist_figure_id":
                        HistoricalFigure = world.GetHistoricalFigure(property.ValueAsInt()); HistoricalFigure.AddEvent(this);
                        break;
                    case "wc_id":
                        WrittenContent = World.GetOrCreateWorldObject(world.WrittenContents, property.ValueAsInt());
                        break;
                    case "reason":
                        Reason = property.Value;
                        break;
                    case "reason_id":
                        ReasonId = property.ValueAsInt();
                        break;
                    case "circumstance":
                        Circumstance = property.Value;
                        break;
                    case "circumstance_id":
                        CircumstanceId = property.ValueAsInt();
                        break;
                    case "subregion_id":
                        Region = world.GetRegion(property.ValueAsInt()); Region.AddEvent(this);
                        break;
                }
           
            if (Reason == "glorify hf")
            {
                GlorifiedHF = world.GetHistoricalFigure(ReasonId);
                GlorifiedHF.AddEvent(this);
            }
            if (Circumstance == "pray to hf" || Circumstance == "dream about hf")
            {
                CircumstanceHF = world.GetHistoricalFigure(CircumstanceId);
                CircumstanceHF.AddEvent(this);
            }
        }
        public override string Print(bool link = true, DwarfObject pov = null)
        {
            string eventString = this.GetYearTime();
            eventString += WrittenContent.ToSafeLink(link, pov);
            eventString += " was authored by ";
            eventString += HistoricalFigure.ToSafeLink(link, pov);
            if (Site != null)
            {
                eventString += " in ";
                eventString += Site.ToSafeLink(link, pov);
            }
            if (GlorifiedHF != null)
            {
                eventString += " in order to glorify " + GlorifiedHF.ToSafeLink(link, pov);
            }
            if (!string.IsNullOrWhiteSpace(Circumstance))
            {
                if (CircumstanceHF != null)
                {
                    switch (Circumstance)
                    {
                        case "pray to hf":
                            eventString += " after praying to " + CircumstanceHF.ToSafeLink(link, pov);
                            break;
                        case "dream about hf":
                            eventString += " after dreaming of " + CircumstanceHF.ToSafeLink(link, pov);
                            break;
                    }
                }
                else
                {
                    eventString += " after a " + Circumstance;
                }
            }
            eventString += ".";
            return eventString;
        }
    }
}