using System;
using System.Collections.Generic;
using LegendsViewer.Legends.Parser;

namespace LegendsViewer.Legends.Events
{
    public class MasterpieceFood : WorldEvent
    {
        private int SkillAtTime { get; set; }
        public HistoricalFigure Maker { get; set; }
        public Entity MakerEntity { get; set; }
        public Site Site { get; set; }
        public int ItemID { get; set; }
        public string ItemType { get; set; }
        public string ItemSubType { get; set; }

        public override void Merge(List<Property> properties, World world)
        {
            base.Merge(properties, world);

            foreach (Property property in properties)
            {
                switch (property.Name)
                {
                    case "skill_at_time": SkillAtTime = property.ValueAsInt(); break;
                    case "maker":
                    case "hfid": Maker = world.GetHistoricalFigure(property.ValueAsInt()); Maker.AddEvent(this); break;
                    case "maker_entity":
                    case "entity_id": MakerEntity = world.GetEntity(property.ValueAsInt()); MakerEntity.AddEvent(this); break;
                    case "site":
                    case "site_id": Site = world.GetSite(property.ValueAsInt()); Site.AddEvent(this); break;
                    case "item_type": ItemType = property.Value; break;
                    case "item_subtype": ItemSubType = property.Value; break;
                    case "item_id": ItemID = property.ValueAsInt(); break;
                }
            }
        }

        public override string Print(bool link = true, DwarfObject pov = null)
        {
            string eventString = GetYearTime();
            eventString += Maker.ToSafeLink(link, pov);
            eventString += " prepared a masterful ";
            switch (ItemSubType)
            {
                case "0": eventString += "biscuits"; break;
                case "1": eventString += "stew"; break;
                case "2": eventString += "roasts"; break;
                default: eventString += "meal"; break;
            }
            eventString += " for ";
            eventString += MakerEntity.ToSafeLink(link, pov);
            eventString += " in ";
            eventString += Site.ToSafeLink(link, pov);
            eventString += ". ";
            eventString += PrintParentCollection(link, pov);
            return eventString;
        }
    }
}