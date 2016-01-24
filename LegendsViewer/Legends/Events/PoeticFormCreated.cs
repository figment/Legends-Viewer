using System.Collections.Generic;
using LegendsViewer.Legends.Enums;
using LegendsViewer.Legends.Parser;
using System;

namespace LegendsViewer.Legends.Events
{
    public class PoeticFormCreated : FormCreatedEvent
    {
        public PoeticFormCreated()
        {
            FormType = FormType.Poetic;
        }
    }
}