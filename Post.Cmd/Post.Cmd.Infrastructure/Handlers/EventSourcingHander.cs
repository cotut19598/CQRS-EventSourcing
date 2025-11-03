using CQRS.Core.Domain;
using CQRS.Core.Handlers;
using CQRS.Core.Infrastructure;
using Post.Cmd.Domain.Aggregates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Post.Cmd.Infrastructure.Handlers
{
    public class EventSourcingHander : IEventSourcingHandler<PostAggregate>
    {
        private readonly IEventStore _eventStore;
        public EventSourcingHander(IEventStore eventStore)
        {
            _eventStore = eventStore;
        }

        public async Task<PostAggregate> GetByIdAsync(Guid aggregateId)
        {
            var aggregate = new PostAggregate();
            var events = await _eventStore.GetEventAsync(aggregateId);
            if (events == null && !events.Any())
            {
                return aggregate;
            }
            aggregate.ReplayEvent(events);
            aggregate.Version = events.Select(x => x.Version).Max();

            return aggregate;
        }

        public async Task SaveAsync(AggregateRoot aggregate)
        {
            await _eventStore.SaveEventAsync(aggregate.Id, aggregate.GetCommittedChanges(), aggregate.Version);
            aggregate.MarkChangesAsCommitted();
        }
    }
}
