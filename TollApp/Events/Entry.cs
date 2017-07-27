using System;
using System.Globalization;
using System.Text;
using Newtonsoft.Json;
using TollApp.Models;

namespace TollApp.Events
{
    public class Entry
    {
        public class EntryEvent : TollEvent
        {
            public EntryEvent(int tollId, DateTime entryTime, string licence, string state, CarModel carModel, double tollAmount, int tag)
            {
                this.TollId = tollId;
                this.EntryTime = entryTime;
                this.LicensePlate = licence;
                this.State = state;
                this.CarModel = carModel;
                this.TollAmount = tollAmount;
                this.Tag = tag;
            }

            public DateTime EntryTime { get; set; }

            public CarModel CarModel { get; set; }

            public string State { get; set; }

            public double TollAmount { get; set; }

            public long Tag { get; set; }

            public override string Format()
            {
                return FormatJson();
            }

            private string FormatJson()
            {
                return JsonConvert.SerializeObject(new
                {
                    TollId = this.TollId.ToString(CultureInfo.InvariantCulture),
                    EntryTime = this.EntryTime.ToString("o"),
                    LicensePlate = this.LicensePlate,
                    State = this.State,
                    Make = this.CarModel.Make,
                    Model = this.CarModel.Model,
                    VehicleType = this.CarModel.VehicleType.ToString(CultureInfo.InvariantCulture),
                    VehicleWeight = this.CarModel.VehicleWeight.ToString(CultureInfo.InvariantCulture),
                    Toll = this.TollAmount.ToString(CultureInfo.InvariantCulture),
                    Tag = this.Tag.ToString(CultureInfo.InvariantCulture)
                });
            }
        }
    }
}
