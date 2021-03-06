﻿using System;
using System.Collections.Generic;
using LegendsViewer.Legends.Parser;

namespace LegendsViewer.Legends
{
    public enum EntityEntityLinkType // legends_plus.xml
    {
        Child,
        Parent,
        Unknown
    }

    public class EntityEntityLink // legends_plus.xml
    {
        public EntityEntityLink(List<Property> properties, World world)
        {
            Type = EntityEntityLinkType.Unknown;
            foreach (Property property in properties)
            {
                switch (property.Name)
                {
                    case "type":
                        switch (property.Value)
                        {
                            case "CHILD":
                                Type = EntityEntityLinkType.Child;
                                break;
                            case "PARENT":
                                Type = EntityEntityLinkType.Parent;
                                break;
                            default:
                                world.ParsingErrors.Report("Unknown Entity Entity Link Type: " + property.Value);
                                break;
                        }
                        break;
                    case "target":
                        Target = world.GetEntity(property.ValueAsInt());
                        break;
                    case "strength":
                        Strength = property.ValueAsInt();
                        break;
                }
            }
        }

        public EntityEntityLinkType Type { get; set; }
        public Entity Target { get; set; }
        public int Strength { get; set; }
    }
}