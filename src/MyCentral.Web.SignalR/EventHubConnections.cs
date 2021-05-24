using System;
using System.Reactive.Subjects;

namespace MyCentral.Web.Hubs
{
    public class EventHubConnections : IObservable<EventNotification>, IDisposable
    {
        private readonly Subject<EventNotification> _subject = new();

        public void AddConnection(string id, string hostname, string eventConnectionString)
        {
            _subject.OnNext(new EventNotification(EventState.Connected, id)
            {
                Host = hostname,
                EventConnectionString = eventConnectionString
            });
        }

        public void Dispose()
        {
            _subject.Dispose();
        }

        public void RemoveConnection(string id)
        {
            _subject.OnNext(new EventNotification(EventState.Disconnected, id));
        }

        IDisposable IObservable<EventNotification>.Subscribe(IObserver<EventNotification> observer)
            => _subject.Subscribe(observer);
    }
}
