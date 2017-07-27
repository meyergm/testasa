using System;
using System.Globalization;
using System.Text;
using Newtonsoft.Json;
using TollApp.Models;

namespace TollApp.Events
{
    public class Exit
    {
        public class ExitEvent : TollEvent
        {
            public ExitEvent(int tollId, DateTime exitTime, string licence)
            {
                this.TollId = tollId;
                this.ExitTime = exitTime;
                this.LicensePlate = licence;
            }

            public DateTime ExitTime { get; set; }

            public override string Format()
            {
                return FormatJson();
            }

            public string FormatJson()
            {
                return JsonConvert.SerializeObject(new
                {
                    TollId = this.TollId.ToString(CultureInfo.InvariantCulture),
                    ExitTime = this.ExitTime.ToString("o"),
                    LicensePlate = this.LicensePlate,
                });
            }
        }
    }
}
