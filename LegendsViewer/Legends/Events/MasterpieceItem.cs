using System;
using System.Collections.Generic;
using LegendsViewer.Legends.Parser;

namespace LegendsViewer.Legends.Events
{
    public class MasterpieceItem : WorldEvent
    {
        private int SkillAtTime { get; set; }
        public HistoricalFigure Maker { get; set; }
        public Entity MakerEntity { get; set; }
        public Site Site { get; set; }
        public int ItemID { get; set; }
        public string ItemType { get; set; }
        public string ItemSubType { get; set; }
        public string Material { get; set; }
        public int MaterialType { get; set; }
        public int MaterialIndex { get; set; }

        private void InternalMerge(List<Property> properties, World world)
        {
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
                    case "skill_used": SkillAtTime = property.ValueAsInt(); break;
                    case "item_type": ItemType = property.Value.Replace("_", " "); break;
                    case "item_subtype": ItemSubType = property.Value.Replace("_", " "); break;
                    case "mat": Material = property.Value.Replace("_", " "); break;
                    case "item_id": ItemID = property.ValueAsInt(); break;
                    case "mat_type": MaterialType = property.ValueAsInt(); break;
                    case "mat_index": MaterialIndex = property.ValueAsInt(); break;
                }
            }
        }
        public override void Merge(List<Property> properties, World world)
        {
            base.Merge(properties, world);
            InternalMerge(properties, world);
        }
        public override string Print(bool link = true, DwarfObject pov = null)
        {
            string eventString = GetYearTime();
            eventString += Maker.ToSafeLink(link, pov);
            eventString += " created a masterful ";
            eventString += !string.IsNullOrWhiteSpace(Material) ? Material + " " : "";
            if (!string.IsNullOrWhiteSpace(ItemSubType) && ItemSubType != "-1")
            {
                eventString += ItemSubType;
            }
            else
            {
                eventString += !string.IsNullOrWhiteSpace(ItemType) ? ItemType : "UNKNOWN ITEM";
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