using System.Collections.Generic;

namespace TollApp.Models
{
    public abstract class TollEvent
    {
        public int TollId { get; set; }

        public string LicensePlate { get; set; }

        public abstract string Format();
    }
}
