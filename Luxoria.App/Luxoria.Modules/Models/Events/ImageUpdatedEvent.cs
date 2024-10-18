using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luxoria.Modules.Models.Events
{
    public class ImageUpdatedEvent
    {
        public string ImagePath { get; }

        public ImageUpdatedEvent(string imagePath)
        {
            ImagePath = imagePath;
        }
    }
}
