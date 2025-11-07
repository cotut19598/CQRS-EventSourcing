using CQRS.Core.Domain;
using CQRS.Core.Events;
using CQRS.Core.Exceptions;
using CQRS.Core.Infrastructure;
using CQRS.Core.Producers;
using Post.Cmd.Domain.Aggregates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Post.Cmd.Infrastructure.Stores
{
    public class EventStore : IEventStore
    {
        private readonly IEventStoreRepository _eventRepository;
        private readonly IEventProducer _eventProducer;
        public EventStore(IEventStoreRepository eventRepository, IEventProducer eventProducer)
        {
            _eventRepository = eventRepository;
            _eventProducer = eventProducer;
        }

        public async Task<List<BaseEvent>> GetEventAsync(Guid aggreagateId)
        {
            var eventStream = await _eventRepository.FindByAggregateId(aggreagateId);
            if (eventStream == null || !eventStream.Any())
                throw new AggregateNotFoundException("Incorrect post ID provided");
            return eventStream.OrderBy(x => x.Version).Select(x=>x.EventData).ToList();
        }

        public async Task SaveEventAsync(Guid aggregateId, IEnumerable<BaseEvent> events, int expectedVersion)
        {
            var eventStream = await _eventRepository.FindByAggregateId(aggregateId);
            if(expectedVersion != -1 && eventStream[^1].Version != expectedVersion)
            {
                throw new ConcurrencyException();
            }
            var version = expectedVersion;
            foreach(var @event in events)
            {
                version++;
                @event.Version = version;
                var eventType = @event.GetType().Name;
                var eventModel = new EventModel
                {
                    TimeStamp = DateTime.Now,
                    AggregateIdentifier = aggregateId,
                    AggregateType = nameof(PostAggregate),
                    Version = version,
                    EventType = eventType,
                    EventData = @event
                };

                await _eventRepository.SaveAsync(eventModel);

                var topic = Environment.GetEnvironmentVariable("KAFKA_TOPIC");
                await _eventProducer.ProduceAsync(topic, @event);
            }
        }
    }
}
