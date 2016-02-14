using System;
using System.Collections.Generic;
using System.Linq;
using LegendsViewer.Legends.Parser;

namespace LegendsViewer.Legends.Events
{
    public class CreatedStructure : WorldEvent
    {
        public int StructureID { get; set; }
        public Structure Structure { get; set; }
        public Entity Civ { get; set; }
        public Entity SiteEntity { get; set; }
        public Site Site { get; set; }
        public HistoricalFigure Builder { get; set; }

        public override void Merge(List<Property> properties, World world)
        {
            base.Merge(properties, world);

            foreach (Property property in properties)
                switch (property.Name)
                {
                    case "structure":
                    case "structure_id": Structure = Site?.GetStructure(StructureID = property.ValueAsInt()); Structure.AddEvent(this); break;
                    case "civ":
                    case "civ_id": Civ = world.GetEntity(property.ValueAsInt()); Civ.AddEvent(this); break;
                    case "site_civ":
                    case "site_civ_id": SiteEntity = world.GetEntity(property.ValueAsInt()); SiteEntity.AddEvent(this); break;
                    case "site":
                    case "site_id": Site = world.GetSite(property.ValueAsInt()); Site.AddEvent(this); break;
                    case "builder_hf":
                    case "builder_hfid": Builder = world.GetHistoricalFigure(property.ValueAsInt()); Builder.AddEvent(this); break;
                }

            if (Site != null)
            {
                Structure = Site.Structures.FirstOrDefault(structure => structure.ID == StructureID);
            }
        }
        
        public override string Print(bool link = true, DwarfObject pov = null)
        {
            string eventString = GetYearTime();
            if (Builder != null)
            {
                eventString += Builder.ToSafeLink(link, pov);
                eventString += ", thrust a spire of slade up from the underworld, naming it ";
                eventString += Structure.ToSafeLink(link, pov);
                eventString += ", and established a gateway between worlds in ";
                eventString += Site.ToSafeLink(link, pov);
                eventString += ". ";
            }
            else
            {
                eventString += SiteEntity.ToSafeLink(link, pov) + " of ";
                eventString += Civ.ToSafeLink(link, pov, "CIV");
                eventString += " constructed ";
                eventString += Structure.ToSafeLink(link, pov);
                eventString += " in ";
                eventString += Site.ToSafeLink(link, pov);
                eventString += ". ";
            }
            eventString += PrintParentCollection(link, pov);
            return eventString;
        }
    }
}