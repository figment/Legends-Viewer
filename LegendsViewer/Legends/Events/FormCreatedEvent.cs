using System;
using System.Collections.Generic;
using LegendsViewer.Legends.Enums;
using LegendsViewer.Legends.Parser;

namespace LegendsViewer.Legends.Events
{
    public class FormCreatedEvent : WorldEvent
    {
        public Site Site { get; set; }
        public WorldRegion Region { get; set; }
        public HistoricalFigure HistoricalFigure { get; set; }
        public int FormId { get; set; }
        public string Reason { get; set; }
        public int ReasonId { get; set; }
        public HistoricalFigure GlorifiedHF { get; set; }
        public HistoricalFigure PrayToHF { get; set; }
        public string Circumstance { get; set; }
        public int CircumstanceId { get; set; }
        public FormType FormType { get; set; }
        public ArtForm ArtForm { get; set; }

        public override void Merge(List<Property> properties, World world)
        {
            base.Merge(properties, world);

            foreach (Property property in properties)
                switch (property.Name)
                {
                    case "hist_figure_id":
                        HistoricalFigure = world.GetHistoricalFigure(property.ValueAsInt()); HistoricalFigure.AddEvent(this);
                        break;
                    case "site_id":
                        Site = world.GetSite(property.ValueAsInt()); Site.AddEvent(this);
                        break;
                    case "form_id":
                        ArtForm = world.GetOrCreateArtForm(FormId = property.ValueAsInt(), FormType);
                        ArtForm.AddEvent(this);
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
            if (Circumstance == "pray to hf")
            {
                PrayToHF = world.GetHistoricalFigure(CircumstanceId);
                PrayToHF.AddEvent(this);
            }
        }
        
        public override string Print(bool link = true, DwarfObject pov = null)
        {
            string eventString = this.GetYearTime();
            eventString += ArtForm.ToSafeLink(link, pov, FormType.ToString().ToUpper() + " FORM");
            eventString += " was created by ";
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
                if (PrayToHF != null)
                {
                    eventString += " after praying to " + PrayToHF.ToSafeLink(link, pov);
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