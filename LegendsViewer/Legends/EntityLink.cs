using System;
using System.Collections.Generic;
using LegendsViewer.Legends.Enums;
using LegendsViewer.Legends.Parser;

namespace LegendsViewer.Legends
{
    public class EntityLink
    {
        public EntityLinkType Type { get; set; }
        public Entity Entity { get; set; }
        public int Strength { get; set; }
        public int PositionID { get; set; }
        public int StartYear { get; set; }
        public int EndYear { get; set; }

        public EntityLink(List<Property> properties, World world)
        {
            Strength = 0;
            StartYear = -1;
            EndYear = -1;
            foreach (Property property in properties)
            {
                switch (property.Name)
                {
                    case "target":
                    case "entity_id":
                        int id = property.ValueAsInt();
                        Entity = world.GetEntity(id);
                        break;
                    case "position_profile_id": PositionID = property.ValueAsInt(); break;
                    case "start_year": 
                        StartYear = property.ValueAsInt();
                        Type = EntityLinkType.Position;
                        break;
                    case "end_year": 
                        EndYear = property.ValueAsInt();
                        Type = EntityLinkType.FormerPosition;
                        break;
                    case "strength":
                    case "link_strength": Strength = property.ValueAsInt(); break;
                    case "link_type":
                        EntityLinkType linkType;
                        if (!Enum.TryParse(Formatting.InitCaps(property.Value), out linkType))
                        {
                            switch (property.Value)
                            {
                                case "former member": Type = EntityLinkType.FormerMember; break;
                                case "former prisoner": Type = EntityLinkType.FormerPrisoner; break;
                                case "former slave": Type = EntityLinkType.FormerSlave; break;
                                default:
                                    Type = EntityLinkType.Unknown;
                                    world.ParsingErrors.Report("Unknown Entity Link Type: " + property.Value);
                                    break;
                            }
                        }
                        else
                        {
                            Type = linkType;
                        }
                        break;
                }
            }
        }
    }
}