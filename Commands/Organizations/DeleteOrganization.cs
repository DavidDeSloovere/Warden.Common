﻿using System;

namespace Warden.Common.Commands.Organizations
{
    public class DeleteOrganization : IAuthenticatedCommand
    {
        public Request Request { get; set; }
        public string UserId { get; set; }
        public Guid Id { get; set; }
    }
}