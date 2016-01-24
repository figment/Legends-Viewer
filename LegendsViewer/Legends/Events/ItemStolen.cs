using System;
using System.Collections.Generic;
using System.Linq;
using LegendsViewer.Legends.Parser;

namespace LegendsViewer.Legends.Events
{
    public class ItemStolen : WorldEvent
    {
        public int StructureID { get; set; }
        public Structure Structure { get; set; }
        public string ItemType { get; set; }
        public int ItemSubType { get; set; }
        public string Material { get; set; }
        public int MaterialTypeID { get; set; }
        public int MaterialIndex { get; set; }
        public HistoricalFigure Thief { get; set; }
        public Entity Entity { get; set; }
        public Site Site { get; set; }
        public Site ReturnSite { get; set; }

        public ItemStolen()
        {
            ItemType = "UNKNOWN ITEM";
            Material = "UNKNOWN MATERIAL";
        }
        private void InternalMerge(List<Property> properties, World world)
        {
            foreach (Property property in properties)
                switch (property.Name)
                {
                    case "item_type": ItemType = string.Intern(Formatting.InitCaps(property.Value.Replace("_", " "))); break;
                    case "mat": Material = string.Intern(Formatting.InitCaps(property.Value)); break;
                    case "histfig": Thief = world.GetHistoricalFigure(property.ValueAsInt()); Thief.AddEvent(this); break;
                    case "entity": Entity = world.GetEntity(property.ValueAsInt()); Entity.AddEvent(this); break;
                    case "site":
                    case "site_id": Site = world.GetSite(property.ValueAsInt()); Site.AddEvent(this); break;
                    case "structure":
                    case "structure_id": Structure = Site?.GetStructure(StructureID = property.ValueAsInt()); Structure.AddEvent(this); break;
                    case "item_subtype": ItemSubType = property.ValueAsInt(); break;
                    case "mattype": MaterialIndex = property.ValueAsInt(); break;
                    case "matindex": ItemSubType = property.ValueAsInt(); break;
                }
        }
        public override void Merge(List<Property> properties, World world)
        {
            base.Merge(properties, world);
            InternalMerge(properties, world);
        }
        public override string Print(bool path = true, DwarfObject pov = null)
        {
            string eventString = this.GetYearTime();
            string itemType = !string.IsNullOrEmpty(ItemType) ? (Material + " " + ItemType) : "UNKNOWN ITEM";
            string site = Site.ToSafeLink(path, pov);
            string thief = Thief.ToSafeLink(path, pov);

            eventString += $" a {itemType} was stolen from {site} by {thief}";
            if (Entity != null) eventString += " from " + Entity.ToSafeLink(path, pov);

            eventString += " a ";
            eventString += Material + " " + ItemType;
            eventString += " was stolen from ";

            if (ReturnSite != null)
            {
                eventString += " and brought to " + ReturnSite.ToSafeLink();
            }
            eventString += ". ";
            eventString += PrintParentCollection(path, pov);
            return eventString;
        }
    }
}