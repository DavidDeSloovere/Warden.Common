﻿using System;

namespace Warden.Common.Commands.Wardens
{
    public class SpawnWarden : IAuthenticatedCommand
    {
        public Guid WardenSpawnId { get; set; }
        public Request Request { get; set; }
        public string UserId { get; set;  }
        public string ConfigurationId { get; set; }
        public object Configuration { get; set; }
        public string Token { get; set; }
        public string Region { get; set; }
    }
}