﻿using System;
using System.Collections.Generic;
using System.Text;
using TollApp.Models;

namespace TollApp.Events
{
    public class TollDataGenerator
    {
        #region Private Variables

        private const int MaxTollId = 100;

        // One of the toll stations will intentionally be missing exit events to demonstrate use of outer left join
        private const int TollIdWithFailedExitSensor = 5;

        private readonly Random _random;
        private readonly EventBuffer _eventBuffer;
        private readonly Registration[] _commercialVehicleRegistration;

        #endregion


        #region Public Methods

        public static TollDataGenerator Generator(Registration[] commercialVehicleRegistration)
        {
            return new TollDataGenerator(new Random(), commercialVehicleRegistration);
        }

        public void Next(DateTime startTime, TimeSpan interval, int n)
        {
            for (int i = 0; i < n; i++)
            {
                var carModel = Constants.CarModels[_random.Next(Constants.CarModels.Length)];
                var entryTime = startTime + TimeSpan.FromMilliseconds(_random.Next((int) interval.TotalMilliseconds));
                var exitTime = entryTime + TimeSpan.FromSeconds(_random.Next(60, 160));
                var tollId = _random.Next(MaxTollId);
                var state = Constants.States[_random.Next(Constants.States.Length)];
                var tollAmount = GetTollAmount(carModel);
                var tag = _random.Next(100000000, 999999999);

                string licence;

                // For commercial vehicle pick license number from the reference data. Use random value for others.
                if (carModel.VehicleType == 2)
                {
                    licence =
                        _commercialVehicleRegistration[_random.Next(_commercialVehicleRegistration.Length)].LicensePlate;
                }
                else
                {
                    licence = GetLicenceNumber();
                }
                _eventBuffer.Add(entryTime,
                    new Entry.EntryEvent(tollId, entryTime, licence, state, carModel, tollAmount, tag));

                if (tollId != TollIdWithFailedExitSensor)
                {
                    _eventBuffer.Add(exitTime, new Exit.ExitEvent(tollId, exitTime, licence));
                }
            }
        }

        public IEnumerable<TollEvent> GetEvents(DateTime startTime)
        {
            return _eventBuffer.GetEvents(startTime);
        }

        #endregion

        #region Private Methods

        private TollDataGenerator(Random r, Registration[] commercialVehicleRegistration)
        {
            _random = r;
            _eventBuffer = new EventBuffer();

            _commercialVehicleRegistration = commercialVehicleRegistration;
        }

        private double GetTollAmount(CarModel model)
        {
            if (model.VehicleType == 1)
            {
                return 4 + _random.Next(3);
            }
            else
            {
                return 15 + _random.Next(20);
            }
        }

        private string GetLicenceNumber()
        {
            var builder = new StringBuilder();
            for (int i = 0; i < 3; i++)
            {
                var ch = Convert.ToChar(Convert.ToInt32(Math.Floor((26*_random.NextDouble()) + 65)));
                builder.Append(ch);
            }

            builder.AppendFormat(" {0}", _random.Next(1000, 10000));
            return builder.ToString();
        }

        #endregion
    }
}
