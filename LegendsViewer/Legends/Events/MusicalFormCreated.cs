using System.Collections.Generic;
using LegendsViewer.Legends.Enums;
using LegendsViewer.Legends.Parser;
using System;

namespace LegendsViewer.Legends.Events
{
    public class MusicalFormCreated : FormCreatedEvent
    {
        public MusicalFormCreated()
        {
            FormType = FormType.Musical;
        }
    }
}