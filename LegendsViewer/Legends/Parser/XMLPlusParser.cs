using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using LegendsViewer.Controls;
using LegendsViewer.Legends.Parser;

namespace LegendsViewer.Legends
{
    class XMLPlusParser : XMLParser
    {
        public XMLPlusParser(World world, string xmlFile) : base(world, xmlFile)
        {
            if (File.Exists(xmlFile))
            {
                World.Log.AppendLine("Found LEGENDS_PLUS.XML!");
                World.Log.AppendLine("Parsed additional data...\n");
            }
        }

        protected override Section GetSectionType(string sectionName)
        {
            switch (sectionName)
            {
                case "name": return Section.Name;
                case "altname": return Section.AltName;
                default:
                    return base.GetSectionType(sectionName);
            }
        }

        protected override void ParseSection()
        {
            if (CurrentSection == Section.Name || CurrentSection == Section.AltName)
            {
                AddItemToWorld(new List<Property>(new[] { new Property() { Name = XML.Name, Value = XML.ReadElementString() } }));
            }
            else
            {
                base.ParseSection();
            }
        }

        protected override void AddItemToWorld(List<Property> properties)
        {
            if (CurrentSection == Section.Name)
                World.Name = string.Intern(properties.FirstOrDefault()?.Value);
            else if (CurrentSection == Section.AltName)
                World.AltName = string.Intern(properties.FirstOrDefault()?.Value);
            else
                base.AddItemToWorld(properties);
        }
        

        protected override void ProcessXMLSection(Section section)
        {
            if (section == Section.HistoricalFigures)
                return;
            base.ProcessXMLSection(section);
        }

    }
}
