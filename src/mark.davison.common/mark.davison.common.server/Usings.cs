﻿global using mark.davison.common.CQRS;
global using mark.davison.common.Identification;
global using mark.davison.common.server.abstractions;
global using mark.davison.common.server.abstractions.Authentication;
global using mark.davison.common.server.abstractions.CQRS;
global using mark.davison.common.server.abstractions.Health;
global using mark.davison.common.server.abstractions.Identification;
global using mark.davison.common.server.abstractions.Repository;
global using mark.davison.common.server.Authentication;
global using mark.davison.common.server.CQRS;
global using mark.davison.common.server.Utilities;
global using Microsoft.AspNetCore.Authorization;
global using Microsoft.AspNetCore.Builder;
global using Microsoft.AspNetCore.Http;
global using Microsoft.AspNetCore.Mvc;
global using Microsoft.AspNetCore.Routing;
global using Microsoft.AspNetCore.WebUtilities;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Diagnostics.HealthChecks;
global using Microsoft.Extensions.Hosting;
global using Microsoft.Extensions.Options;
global using Microsoft.IdentityModel.Protocols.OpenIdConnect;
global using System.Diagnostics.CodeAnalysis;
global using System.Net;
global using System.Net.Http.Json;
global using System.Reflection;
global using System.Security.Claims;
global using System.Security.Cryptography;
global using System.Text;
global using System.Text.Json;
global using System.Web;
