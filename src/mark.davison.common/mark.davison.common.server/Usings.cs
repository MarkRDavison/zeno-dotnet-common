﻿global using mark.davison.common.Changeset;
global using mark.davison.common.CQRS;
global using mark.davison.common.Identification;
global using mark.davison.common.Instrumentation;
global using mark.davison.common.persistence;
global using mark.davison.common.persistence.Configuration;
global using mark.davison.common.persistence.EntityDefaulter;
global using mark.davison.common.Repository;
global using mark.davison.common.server.abstractions;
global using mark.davison.common.server.abstractions.Authentication;
global using mark.davison.common.server.abstractions.Configuration;
global using mark.davison.common.server.abstractions.CQRS;
global using mark.davison.common.server.abstractions.EventDriven;
global using mark.davison.common.server.abstractions.Health;
global using mark.davison.common.server.abstractions.Identification;
global using mark.davison.common.server.abstractions.Notifications;
global using mark.davison.common.server.abstractions.Notifications.Console;
global using mark.davison.common.server.abstractions.Notifications.Matrix;
global using mark.davison.common.server.abstractions.Repository;
global using mark.davison.common.server.abstractions.Utilities;
global using mark.davison.common.server.Authentication;
global using mark.davison.common.server.CQRS.Processors;
global using mark.davison.common.server.CQRS.Validators;
global using mark.davison.common.server.Cron;
global using mark.davison.common.server.Health;
global using mark.davison.common.server.Health.Checks;
global using mark.davison.common.server.Notifications;
global using mark.davison.common.server.Notifications.Console;
global using mark.davison.common.server.Notifications.Matrix;
global using mark.davison.common.server.Notifications.Matrix.Client;
global using mark.davison.common.server.Utilities;
global using mark.davison.common.Services;
global using Microsoft.AspNetCore.Builder;
global using Microsoft.AspNetCore.DataProtection;
global using Microsoft.AspNetCore.Diagnostics.HealthChecks;
global using Microsoft.AspNetCore.Http;
global using Microsoft.AspNetCore.Http.HttpResults;
global using Microsoft.AspNetCore.Mvc;
global using Microsoft.AspNetCore.Routing;
global using Microsoft.AspNetCore.WebUtilities;
global using Microsoft.EntityFrameworkCore;
global using Microsoft.Extensions.Configuration;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Diagnostics.HealthChecks;
global using Microsoft.Extensions.Hosting;
global using Microsoft.Extensions.Logging;
global using Microsoft.Extensions.Options;
global using Microsoft.Extensions.Primitives;
global using Microsoft.IdentityModel.Protocols.OpenIdConnect;
global using Remote.Linq;
global using Remote.Linq.Text.Json;
global using StackExchange.Redis;
global using System.Diagnostics.CodeAnalysis;
global using System.Linq.Expressions;
global using System.Net;
global using System.Net.Http.Headers;
global using System.Net.Http.Json;
global using System.Reflection;
global using System.Security.Claims;
global using System.Security.Cryptography;
global using System.Text;
global using System.Text.Json;
global using System.Text.Json.Nodes;
global using System.Text.Json.Serialization;
global using System.Web;
