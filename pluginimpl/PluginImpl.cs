using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using common;
using Microsoft.Graph;
using Microsoft.Identity.Client;
using Microsoft.Kiota.Abstractions.Authentication;

namespace pluginimpl
{
  public class PluginMsGraph5Factory : IPluginFactory
  {
    public IPlugin CreatePlugin(ConfigData configData)
    {
      return new PluginMsGraph5(configData);
    }
  }

  public class PluginMsGraph5 : IPlugin
  {
    private readonly string _clientId;
    private readonly string _tenantId;
    private readonly string _redirectUrl;
    private readonly bool _useProxy;
    private GraphServiceClient _gsc;
    private string _userId;

    public PluginMsGraph5(ConfigData configData)
    {
      _tenantId = configData.TenantId;
      _clientId = configData.ClientId;
      _redirectUrl = configData.RedirectUrl;
      _useProxy = configData.UseProxy;
    }

    public async Task Login(LoginMode loginMode)
    {
      var app = MakeClientApp(loginMode);
      var proxy = MakeProxy();
      var httpClient = GraphClientFactory.Create(proxy: proxy);
      _gsc = new GraphServiceClient(httpClient, MakeAuthProvider(loginMode, app));

      await SetCurrentUserInfo(_gsc);
    }

    public async Task<UserData> GetUser()
    {
      if (_gsc != null)
      {
        var user = await _gsc.Me.GetAsync();
        if (user != null)
        {
          return new UserData(user.Id,user.DisplayName,user.Mail);
        }
        else
        {
          return null;
        }
      }

      throw new Exception("Not logged in yet");
    }

    public async Task<PresenceData> GetPresenceData(string userId)
    {
      if (_gsc!=null)
      {
        var presence = await _gsc.Users[_userId].Presence.GetAsync();
        if (presence != null)
        {
          return new PresenceData(presence.Availability, presence.Activity);
        }
      }
      throw new Exception("Not logged in yet");
    }

    private async Task SetCurrentUserInfo(BaseGraphServiceClient graphClient)
    {

      if (graphClient == null)
      {
        throw new Exception("SetCurrentUserInfo: Error: graphClient is not set");
      }

      var user = await graphClient.Me.GetAsync();
      if (user != null)
      {
        _userId = user.Id ?? throw new Exception("Teams user '" + user.Mail + "' has null ID!");
      }
      else
      {
        throw new Exception("Could not obtain current user from Teams");
      }
    }


    internal class SilentTokenProvider : IAccessTokenProvider
    {
      private readonly IPublicClientApplication _app;
      private readonly LoginMode _loginMode;

      public SilentTokenProvider(IPublicClientApplication app, LoginMode loginMode)
      {
        _app = app;
        _loginMode = loginMode;
        AllowedHostsValidator = new AllowedHostsValidator();
      }

      [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
      public async Task<string> GetAuthorizationTokenAsync(Uri uri, Dictionary<string, object> additionalAuthenticationContext = null,
        CancellationToken cancellationToken = new CancellationToken())
      {
        var scopes = new[] { "User.Read" };

        // use AcquireTokenSilent according to https://aka.ms/msal-net-throttling
        // see https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/wiki/AcquireTokenSilentAsync-using-a-cached-token

        AuthenticationResult result;
        var accounts = await _app.GetAccountsAsync();


        if (accounts == null)
        {
          throw new Exception("Could not obtain any accounts from the token cache - _app.GetAccountsAsync() returned null!");
        }

        try
        {
          var firstAcc = accounts.FirstOrDefault();
          result = await _app.AcquireTokenSilent(scopes, firstAcc).ExecuteAsync();
        }
        catch (MsalUiRequiredException)
        {
          if (_loginMode == LoginMode.DIALOG)
          {
            result = await _app.AcquireTokenInteractive(scopes).ExecuteAsync();
          }
          else
          {
            result = await _app.AcquireTokenByIntegratedWindowsAuth(scopes).ExecuteAsync();
          }
        }

        if (result != null)
        {
          var accessToken = result.AccessToken;
          return accessToken;
        }
        else
        {
          throw new Exception("could not obtain a token");
        }

      }
      public AllowedHostsValidator AllowedHostsValidator { get; }
    }


    private BaseBearerTokenAuthenticationProvider MakeAuthProvider(LoginMode loginMode, IPublicClientApplication app)
    {
      return new BaseBearerTokenAuthenticationProvider(new SilentTokenProvider(app, loginMode));
    }


    private IPublicClientApplication MakeClientApp(LoginMode loginMode)
    {
      var builder = PublicClientApplicationBuilder
        .Create(_clientId)
        .WithTenantId(_tenantId);
      if (loginMode == LoginMode.DIALOG)
      {
        builder = builder.WithRedirectUri(_redirectUrl);
      }
      return builder.Build();
    }
    private IWebProxy MakeProxy()
    {
      IWebProxy proxy = null;
      if (_useProxy)
      {
        proxy = WebRequest.DefaultWebProxy;
        proxy.Credentials = CredentialCache.DefaultCredentials;
      }
      return proxy;
    }


  }
}
