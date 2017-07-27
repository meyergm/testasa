using System.Globalization;
using System.Text;
using Microsoft.Azure.WebJobs;
using Microsoft.ServiceBus.Messaging;
using TollApp.Models;


namespace TollApp.Senders
{
    public class EventHubSender
    {
        
        public static void SendData(EventHubClient eventHubName, TollEvent data)
        {
            eventHubName.Send(
                            new EventData(Encoding.UTF8.GetBytes(data.Format()))
                            {
                                PartitionKey = data.TollId.ToString(CultureInfo.InvariantCulture)
                            });
        }
    }
}