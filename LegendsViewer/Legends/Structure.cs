using LegendsViewer.Controls.HTML.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LegendsViewer.Legends
{
    public class Structure : DwarfObject
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public string AltName { get; set; }


        public Structure()
        {
            ID = -1; Name = "INVALID STRUCTURE"; Type = "INVALID";
        }
        public Structure(List<Property> properties, World world)
        {
            InternalMerge(properties, world);
        }
        private void InternalMerge(List<Property> properties, World world)
        {
            foreach (Property property in properties)
                switch (property.Name)
                {
                    case "id": ID = Convert.ToInt32(property.Value); property.Known = true; break;
                    case "name": Name = Formatting.InitCaps(property.Value); property.Known = true; break;
                    case "name2": AltName = Formatting.InitCaps(property.Value); property.Known = true; break;
                    case "type": Type = string.Intern(Formatting.InitCaps(property.Value)); property.Known = true; break;
                }
        }
        public virtual void Merge(List<Property> properties, World world)
        {
            //base.Merge(properties, world);
            InternalMerge(properties, world);
        }

        public override string ToString() { return this.Name; }

        public override string ToLink(bool link = true, DwarfObject pov = null)
        {
            return $"{Name} ({AltName}), {Type}";
        }

    }
}
