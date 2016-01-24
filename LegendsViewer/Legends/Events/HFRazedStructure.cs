using System;
using System.Collections.Generic;
using LegendsViewer.Legends.Parser;

namespace LegendsViewer.Legends.Events
{
    public class HFRazedStructure : WorldEvent
    {
        public int StructureID { get; set; }
        public Structure Structure { get; set; }
        public HistoricalFigure HistoricalFigure;
        public Site Site;

        private void InternalMerge(List<Property> properties, World world)
        {
            foreach (Property property in properties)
                switch (property.Name)
                {
                    case "structure":
                    case "structure_id": Structure = Site?.GetStructure(StructureID = property.ValueAsInt()); Structure.AddEvent(this); break;
                    case "hist_fig_id": HistoricalFigure = world.GetHistoricalFigure(property.ValueAsInt()); HistoricalFigure.AddEvent(this); break;
                    case "site_id": Site = world.GetSite(property.ValueAsInt()); Site.AddEvent(this); break;
                }
        }
        public override void Merge(List<Property> properties, World world)
        {
            base.Merge(properties, world);
            InternalMerge(properties, world);
        }

        public override string Print(bool link = true, DwarfObject pov = null)
        {
            string eventString = this.GetYearTime() + HistoricalFigure.ToSafeLink(link, pov) + " razed a ";
            eventString += Structure != null ? Structure.Name : $"({StructureID})";
            eventString += " in " + Site.ToSafeLink(link, pov) + ". ";
            eventString += PrintParentCollection(link, pov);
            return eventString;
        }
    }
}