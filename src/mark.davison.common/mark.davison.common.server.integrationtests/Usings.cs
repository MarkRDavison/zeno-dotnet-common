global using mark.davison.common.Changeset;
global using mark.davison.common.client.web.abstractions.Authentication;
global using mark.davison.common.client.web.Authentication;
global using mark.davison.common.persistence;
global using mark.davison.common.persistence.EntityDefaulter;
global using mark.davison.common.Repository;
global using mark.davison.common.server.abstractions.EventDriven;
global using mark.davison.common.server.abstractions.Identification;
global using mark.davison.common.server.abstractions.Repository;
global using mark.davison.common.server.integrationtests.Tests.Defaulters;
global using mark.davison.common.server.sample.api;
global using mark.davison.common.server.sample.api.Entities;
global using mark.davison.common.server.sample.api.Setup;
global using mark.davison.common.server.sample.cqrs.Models.Commands;
global using mark.davison.common.server.sample.cqrs.Models.Queries;
global using mark.davison.common.server.test.Framework;
global using Microsoft.AspNetCore.Hosting;
global using Microsoft.AspNetCore.Mvc.Testing;
global using Microsoft.AspNetCore.TestHost;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Logging;
global using Microsoft.VisualStudio.TestTools.UnitTesting;
global using System.Linq.Expressions;
global using System.Net;
global using System.Text.Json;
