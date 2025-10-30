﻿using CQRS.Core.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CQRS.Core.Infrastructure
{
    public interface IEventStore
    {
        Task SaveEventAsync(Guid aggregate, IEnumerable<BaseEvent> events, int expectedVersion);
        Task<List<BaseEvent>> GetEventAsync(Guid aggreagate);

    }
}
