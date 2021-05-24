﻿using Azure.Core;
using Azure.Messaging.EventHubs.Consumer;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MyCentral.Client.Azure
{
    public class AzureEventClient : IEventClient
    {
        private readonly EventHubConsumerClient _events;
        private readonly IObservable<Item> _observable;

        public AzureEventClient(string eventConnectionString)
        {
            _events = new EventHubConsumerClient(EventHubConsumerClient.DefaultConsumerGroupName, eventConnectionString);
            _observable = _events.ReadEventsAsync()
                .Select(t => new Item(t.Data.PartitionKey, t.Data.SequenceNumber.ToString()))
                .ToObservable();
        }

        private static string GetEventHubName(string hostname)
            => hostname.Split('.')[0];

        public ValueTask DisposeAsync()
            => _events.DisposeAsync();

        public IDisposable Subscribe(IObserver<Item> observer)
            => _observable.Subscribe(observer);
    }
}