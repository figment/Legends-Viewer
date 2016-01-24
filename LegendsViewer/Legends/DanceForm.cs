﻿using System.Collections.Generic;
using LegendsViewer.Legends.Parser;
using LegendsViewer.Controls.HTML.Utilities;

namespace LegendsViewer.Legends
{
    public class DanceForm : ArtForm
    {
        public static string Icon = "<i class=\"fa fa-fw fa-street-view\"></i>";

        public override void Merge(List<Property> properties, World world)
        {
            base.Merge(properties, world);
            InternalMerge(properties, world);
        }
        private void InternalMerge(List<Property> properties, World world)
        {
        }

        public override string ToLink(bool link = true, DwarfObject pov = null)
        {
            if (link)
            {
                string linkedString = "";
                if (pov != this)
                {
                    string title = "Dance Form";
                    title += "&#13";
                    title += "Events: " + Events.Count;

                    linkedString = Icon + "<a title=\"" + title + "\">" + Name + "</a>";
                }
                else
                {
                    linkedString = Icon + HTMLStyleUtil.CurrentDwarfObject(Name);
                }
                return linkedString;
            }
            else
            {
                return Name;
            }
        }
    }
}
