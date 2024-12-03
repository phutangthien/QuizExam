using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Identity.API.Configuration;
using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Entities;
using IdentityServer4.EntityFramework.Mappers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Identity.API.Database
{
    public class ConfigurationDbContextSeed
    {
        public async Task SeedAsync(ConfigurationDbContext context, IConfiguration configuration)
        {

            //callbacks urls from config:
            var clientUrls = new Dictionary<string, string>
            {
                {"ExamWebApp", configuration.GetValue<string>("ExamWebAppClient")},
                {"ExamWebAdmin", configuration.GetValue<string>("ExamWebAdminClient")},
                {"ExamApi", configuration.GetValue<string>("ExamWebApiClient")}
            };


            if (!context.Clients.Any())
            {
                foreach (var client in Config.GetClients(clientUrls))
                {
                    var entity = new IdentityServer4.EntityFramework.Entities.Client
                    {
                        ClientId = client.ClientId,
                        ClientName = client.ClientName,
                        ClientSecrets = client.ClientSecrets.Select(s => new IdentityServer4.EntityFramework.Entities.ClientSecret
                        {
                            Value = s.Value
                        }).ToList(),
                        RedirectUris = client.RedirectUris.Select(uri => new IdentityServer4.EntityFramework.Entities.ClientRedirectUri
                        {
                            RedirectUri = uri
                        }).ToList(),
                        PostLogoutRedirectUris = client.PostLogoutRedirectUris.Select(uri => new IdentityServer4.EntityFramework.Entities.ClientPostLogoutRedirectUri
                        {
                            PostLogoutRedirectUri = uri
                        }).ToList(),
                        AllowedScopes = client.AllowedScopes.Select(scope => new IdentityServer4.EntityFramework.Entities.ClientScope
                        {
                            Scope = scope
                        }).ToList(),
                        AllowedGrantTypes = client.AllowedGrantTypes.Select(grantType => new IdentityServer4.EntityFramework.Entities.ClientGrantType
                        {
                            GrantType = grantType
                        }).ToList(),
                        AllowedCorsOrigins = client.AllowedCorsOrigins.Select(origin => new IdentityServer4.EntityFramework.Entities.ClientCorsOrigin
                        {
                            Origin = origin
                        }).ToList(),
                        AccessTokenLifetime = client.AccessTokenLifetime,
                        IdentityTokenLifetime = client.IdentityTokenLifetime,
                        RequireClientSecret = client.RequireClientSecret,
                        RequireConsent = client.RequireConsent,
                        AllowOfflineAccess = client.AllowOfflineAccess,
                        AlwaysIncludeUserClaimsInIdToken = client.AlwaysIncludeUserClaimsInIdToken,
                        AllowAccessTokensViaBrowser = client.AllowAccessTokensViaBrowser,
                    };

                    context.Clients.Add(entity);

                }
                await context.SaveChangesAsync();
            }
            else
            {
                List<ClientRedirectUri> oldRedirects = (await context.Clients.Include(c => c.RedirectUris).ToListAsync())
                    .SelectMany(c => c.RedirectUris)
                    .Where(ru => ru.RedirectUri.EndsWith("/o2c.html"))
                    .ToList();

                if (oldRedirects.Any())
                {
                    foreach (var ru in oldRedirects)
                    {
                        ru.RedirectUri = ru.RedirectUri.Replace("/o2c.html", "/oauth2-redirect.html");
                        context.Update(ru.Client);
                    }
                    await context.SaveChangesAsync();
                }
            }
            
            if (!context.IdentityResources.Any())
            {
                foreach (var resource in Config.GetIdentityResources())
                {
                    try
                    {
                        var entity = new IdentityServer4.EntityFramework.Entities.IdentityResource
                        {
                            Name = resource.Name,
                            DisplayName = resource.DisplayName,
                            Description = resource.Description,
                            Emphasize = resource.Emphasize,
                            Enabled = resource.Enabled,
                            Required = resource.Required,
                            ShowInDiscoveryDocument = resource.ShowInDiscoveryDocument
                        };
                        context.IdentityResources.Add(entity);
                    }catch(Exception ex)
                    {
                        string exm = ex.Message;
                    }
                }
                await context.SaveChangesAsync();
            }

            if (!context.ApiResources.Any())
            {
                foreach (var api in Config.GetApis())
                {
                    var entity = new IdentityServer4.EntityFramework.Entities.ApiResource
                    {
                        AllowedAccessTokenSigningAlgorithms = api.AllowedAccessTokenSigningAlgorithms.FirstOrDefault(),
                        DisplayName = api.DisplayName,
                        Enabled = api.Enabled,
                        Name = api.Name,
                        ShowInDiscoveryDocument = api.ShowInDiscoveryDocument,
                    };
                    context.ApiResources.Add(entity);
                }

                await context.SaveChangesAsync();
            }

            if (!context.ApiScopes.Any())
            {
                foreach (var resource in Config.GetApiScopes())
                {
                    var entity = new IdentityServer4.EntityFramework.Entities.ApiScope
                    {
                        DisplayName = resource.DisplayName,
                        Description = resource.Description,
                        Emphasize = resource.Emphasize,
                        Enabled = resource.Enabled,
                        Name = resource.Name,
                        Required = resource.Required,
                        ShowInDiscoveryDocument = resource.ShowInDiscoveryDocument
                    };
                    context.ApiScopes.Add(entity);
                }
                await context.SaveChangesAsync();
            }
        }
    }
}