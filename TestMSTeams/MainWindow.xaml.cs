﻿using Microsoft.Graph;
using Microsoft.Identity.Client;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;


namespace TestMSTeams
{
  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow : Window
  {
    // TODO: get from env?
    private static string tenantId = ""; // tenant ID
    private static string clientId = ""; // client ID
    private static string clientSecret = "";  // client secret
    private static string userId = "";  // user ID
    private static string sessionId = clientId;  // used to setPresence - 'sessionId' is the Application (client) ID
    private static bool useProxy = true;

    
    private GraphServiceClient _graphClient = null!;

    public MainWindow()
    {
      InitializeComponent();
    }

    /**
     * Does Integrated Windows provider access
     * https://learn.microsoft.com/en-us/graph/sdks/choose-authentication-providers?tabs=CS#integrated-windows-provider
     */
    private async void IntegratedWindowsProvider_OnClick(object sender, RoutedEventArgs e)
    {

      var scopes = new[] { "User.Read" };

      try
      {
        var app = PublicClientApplicationBuilder
          .Create(clientId)
          .WithTenantId(tenantId)
          .Build();

        var authProvider = new DelegateAuthenticationProvider(async (request) => {
          // Use Microsoft.Identity.Client to retrieve token
          // use AcquireTokenSilent according to https://aka.ms/msal-net-throttling
          // see https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/wiki/AcquireTokenSilentAsync-using-a-cached-token

          AuthenticationResult result = null;
          var accounts = await app.GetAccountsAsync();
          try
          {
            result = await app.AcquireTokenSilent(scopes, accounts.FirstOrDefault()).ExecuteAsync();
          }
          catch (MsalUiRequiredException ex)
          {
            result = await app.AcquireTokenByIntegratedWindowsAuth(scopes).ExecuteAsync();
          }
          catch (Exception ex)
          {
            // Error Acquiring Token Silently
            ShowError(ex);
          }

          if (result != null)
          {
            string accessToken = result.AccessToken;
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
          }
        });

        // set proxy
        IWebProxy proxy = null;
        if (useProxy)
        {
          proxy = WebRequest.DefaultWebProxy;
          proxy.Credentials = CredentialCache.DefaultCredentials;
        }

        var httpClient = GraphClientFactory.Create(authProvider, proxy: proxy);
        _graphClient = new GraphServiceClient(httpClient);

        await TestGraphClient(_graphClient);
      }
      catch (Exception ex)
      {
        MessageBox.Show(ex.ToString());
      }
      
    }

    /**
     * Does Interactive provider
     * https://learn.microsoft.com/en-us/graph/sdks/choose-authentication-providers?tabs=CS#interactive-provider
     */
    private async void InteractiveProvider_OnClick(object sender, RoutedEventArgs e)
    {

      var scopes = new[] { "User.Read" };

      try
      {
        var app = PublicClientApplicationBuilder
          .Create(clientId)
          .WithTenantId(tenantId)
          .WithRedirectUri("http://localhost")
          .Build();

        var authProvider = new DelegateAuthenticationProvider(async (request) => {
          // Use Microsoft.Identity.Client to retrieve token
          // use AcquireTokenSilent according to https://aka.ms/msal-net-throttling
          // see https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/wiki/AcquireTokenSilentAsync-using-a-cached-token

          AuthenticationResult result = null;
          var accounts = await app.GetAccountsAsync();
          try
          {
            result = await app.AcquireTokenSilent(scopes, accounts.FirstOrDefault()).ExecuteAsync();
          }
          catch (MsalUiRequiredException ex)
          {
            result = await app.AcquireTokenInteractive(scopes).ExecuteAsync();
          }
          catch (Exception ex)
          {
            // Error Acquiring Token Silently
            ShowError(ex);
          }

          if (result != null)
          {
            var accessToken = result.AccessToken;
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
          }
        });

        // set proxy

        IWebProxy proxy = null;
        if (useProxy)
        {
          proxy = WebRequest.DefaultWebProxy;
          proxy.Credentials = CredentialCache.DefaultCredentials;
        }

        var httpClient = GraphClientFactory.Create(authProvider, proxy: proxy);
        _graphClient = new GraphServiceClient(httpClient);

        await TestGraphClient(_graphClient);
      }
      catch (Exception ex)
      {
        MessageBox.Show(ex.ToString());
      }
     
    }

    /**
     * Show demo user's data
     */
    private async Task TestGraphClient(GraphServiceClient graphClient, bool userAccess = true)
    {
      try
      {
        User? user;
        if (userAccess)
        {
          user = await graphClient.Me.Request().GetAsync();
          userId = user?.Id;
        }
        else
        {
          user = await graphClient.Users[userId].Request().GetAsync();
        }

        TxtUser.Text = $"user: {user?.DisplayName}\nmail: {user?.Mail}\nID: {user?.Id}";
        if (userId != null)
        {
          TxtUserId.Text = userId;
        }
      }
      catch (Exception ex)
      {
        MessageBox.Show(ex.ToString());
      }
    }

    private async Task<Presence?> GetUserPresence()
    {
      try
      {
        var presence = await _graphClient.Users[userId].Presence.Request().GetAsync();
        return presence;
      }
      catch (Exception ex)
      {
        ShowError(ex);
      }

      return null;
    }

    private void ShowError(Exception exception)
    {
      MessageBox.Show($"ERROR: \n\nexception:" + exception);
    }


   private async void GetPresence_OnClick(object sender, RoutedEventArgs e)
    {
      var presence = await GetUserPresence();
      TxtPresence.Text = $"Availability: {presence?.Availability}, Activity: {presence?.Activity}";
    }


    private void TxtUserId_OnTextChanged(object sender, TextChangedEventArgs e)
    {
      if (!string.IsNullOrEmpty(TxtUserId.Text))
      {
        userId = TxtUserId.Text;
      }
    }


   
  }
}
