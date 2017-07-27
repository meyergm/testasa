using System;
using System.IO;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;
using Microsoft.ServiceBus.Messaging;
using Newtonsoft.Json.Linq;
using TollApp.Senders;
using TollApp.Events;
using System.Threading;
using TollApp.Models;

namespace TollApp
{
    // To learn more about Microsoft Azure WebJobs SDK, please see http://go.microsoft.com/fwlink/?LinkID=320976
    public class Program
    {
        #region Private variables 

        private static Timer _timer;
        private static EventHubClient _eventHubName;
        private static Registration[] _commercialVehicleRegistration;
        #endregion

        #region Public Methods

        public static void Main(string[] args)
        {
            CreateBlobs();

            try
            {
                // verify and create document db collection                     
                DocumentDbSender.CreateDocumentDb();

                // create Event Hub
                var entryEventHub = EventHubClient.CreateFromConnectionString(CloudConfiguration.EventHubConnectionString, CloudConfiguration.EntryName);
                var exitEventHub = EventHubClient.CreateFromConnectionString(CloudConfiguration.EventHubConnectionString, CloudConfiguration.ExitName);

                // generate data
                var generator = TollDataGenerator.Generator(_commercialVehicleRegistration);
                
                TollEvent data;

                var timerInterval = TimeSpan.FromSeconds(Convert.ToDouble(CloudConfiguration.TimerInterval));
              
                TimerCallback timerCallback = state =>
                {
                    var startTime = DateTime.UtcNow;
                    generator.Next(startTime, timerInterval, 5);

                    foreach (var e in generator.GetEvents(startTime))
                    {
                        if (e is Entry.EntryEvent)
                        {
                            _eventHubName = entryEventHub;
                        }
                        else
                        {
                           _eventHubName = exitEventHub;
                        }
                        data = e;

                        // Write to Event Hub
                        Senders.EventHubSender.SendData(_eventHubName, data);
                      
                        //send documentDB
                        JObject json = JObject.Parse(data.Format());
                        DocumentDbSender.SendInfo(json);
                    }
                    _timer.Change((int)timerInterval.TotalMilliseconds, Timeout.Infinite);
                };

                _timer = new Timer(timerCallback, null, Timeout.Infinite, Timeout.Infinite);
                _timer.Change(0, Timeout.Infinite);
                
                var exitEvent = new ManualResetEvent(false);
                Console.CancelKeyPress += (sender, eventArgs) =>
                {
                    Console.WriteLine("Stopping...");
                    eventArgs.Cancel = true;
                    exitEvent.Set();
                };

                exitEvent.WaitOne();              
                _timer.Change(Timeout.Infinite, Timeout.Infinite);
                Thread.Sleep(timerInterval);
                _timer.Dispose();
                entryEventHub.Close();
                exitEventHub.Close();            

            }
            catch (Exception exception)
            {
                //log to text file on blob               
               // _errorLogBlockBlob.UploadText(exception.ToString());
            }
        }

        #endregion

        #region Private Methods

        private static void CreateBlobs()
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(CloudConfiguration.StorageAccountUrl);
            CloudBlobContainer container = storageAccount.CreateCloudBlobClient().GetContainerReference(CloudConfiguration.StorageAccountContainer);
            CloudBlockBlob registrationBlockBlob = container.GetBlockBlobReference(CloudConfiguration.RegistrationFileBlob);

            //Create a new container, if it does not exist
            container.CreateIfNotExists();

            using (var fileStream = File.OpenRead(@"Data\Registration.json"))
            {
                registrationBlockBlob.UploadFromStream(fileStream);
            }

            //read Registration.json from BLOB to be used for random data generator
            using (var stream = new MemoryStream())
            {
                registrationBlockBlob.DownloadToStream(stream);
                stream.Position = 0; //resetting stream's position to 0
                var serializer = new JsonSerializer();

                using (var sr = new StreamReader(stream))
                {
                    using (var jsonTextReader = new JsonTextReader(sr))
                    {
                        var jsonStream = serializer.Deserialize(jsonTextReader);
                        _commercialVehicleRegistration = JsonConvert.DeserializeObject<Registration[]>(jsonStream.ToString());
                    }
                }
            }
        }

        #endregion

    }
}
