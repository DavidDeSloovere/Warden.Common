﻿using System;

namespace Warden.Common.Events
{
    //Marker interface
    public interface IEvent
    {
        Guid RequestId { get; }
    }
}