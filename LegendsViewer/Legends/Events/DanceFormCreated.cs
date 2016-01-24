using System.Collections.Generic;
using LegendsViewer.Legends.Enums;
using LegendsViewer.Legends.Parser;
using System;

namespace LegendsViewer.Legends.Events
{
    public class DanceFormCreated : FormCreatedEvent
    {
        public DanceFormCreated()
        {
            FormType = FormType.Dance;
        }
    }
}