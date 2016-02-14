﻿using System.Collections.Generic;
using LegendsViewer.Legends.Parser;
using LegendsViewer.Controls.HTML.Utilities;

namespace LegendsViewer.Legends
{
    public class PoeticForm : ArtForm
    {
        public static string Icon = "<i class=\"fa fa-fw fa-sticky-note-o\"></i>";

        public override void Merge(List<Property> properties, World world)
        {
            base.Merge(properties, world);

        }

        public override string ToLink(bool link = true, DwarfObject pov = null)
        {
            if (link)
            {
                string title = "Poetic Form";
                title += "&#13";
                title += "Events: " + Events.Count;

                string linkedString = "";
                if (pov != this)
                {
                    linkedString = Icon + "<a title=\"" + title + "\">" + Name + "</a>";
                }
                else
                {
                    linkedString = Icon + "<a title=\"" + title + "\">" + HTMLStyleUtil.CurrentDwarfObject(Name) + "</a>";
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
