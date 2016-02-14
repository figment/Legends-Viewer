﻿using System.Collections.Generic;
using LegendsViewer.Legends.Parser;
using LegendsViewer.Controls.HTML.Utilities;

namespace LegendsViewer.Legends
{
    public class MusicalForm : ArtForm
    {
        public static string Icon = "<i class=\"fa fa-fw fa-music\"></i>";

        public override void Merge(List<Property> properties, World world)
        {
            base.Merge(properties, world);

        }

        public override string ToLink(bool link = true, DwarfObject pov = null)
        {
            if (link)
            {
                string title = "Musical Form";
                title += "&#13";
                title += "Events: " + Events.Count;

                if (pov != this)
                {
                    return Icon + "<a title=\"" + title + "\">" + Name + "</a>";
                }
                else
                {
                    return Icon + "<a title=\"" + title + "\">" + HTMLStyleUtil.CurrentDwarfObject(Name) + "</a>";
                }
            }
            else
            {
                return Name;
            }
        }
    }
}
