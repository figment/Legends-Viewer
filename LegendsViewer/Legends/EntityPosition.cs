﻿using LegendsViewer.Legends.Parser;
using System;
using System.Collections.Generic;

namespace LegendsViewer.Legends
{
    public class EntityPosition
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string NameFemale { get; set; }
        public string NameMale { get; set; }
        public string Spouse { get; set; }
        public string SpouseFemale { get; set; }
        public string SpouseMale { get; set; }

        public EntityPosition(List<Property> properties, World world)
        {
            ID = -1;
            foreach (Property property in properties)
            {
                switch (property.Name)
                {
                    case "id": ID = property.ValueAsInt(); break;
                    case "name": Name = Formatting.InitCaps(property.Value); break;
                    case "name_male": NameMale = Formatting.InitCaps(property.Value); break;
                    case "name_female": NameFemale = Formatting.InitCaps(property.Value); break;
                    case "spouse": Spouse = Formatting.InitCaps(property.Value); break;
                    case "spouse_male": SpouseMale = Formatting.InitCaps(property.Value); break;
                    case "spouse_female": SpouseFemale = Formatting.InitCaps(property.Value); break;
                }
            }

        }
    }
}
