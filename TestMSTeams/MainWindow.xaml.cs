using System;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using common;


namespace TestMSTeams
{
  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow
  {
    private readonly IPlugin _plugin;
    private string? _userId;

    public MainWindow()
    {
      ConfigData configData;
      try
      {
        var tenantId = RequireEnvVar("TENANT_ID");
        var clientId = RequireEnvVar("CLIENT_ID");
        var userId = Environment.GetEnvironmentVariable("USER_ID");
        var useProxy = Environment.GetEnvironmentVariable("USE_PROXY") == "true";
        var redirectUrl = Environment.GetEnvironmentVariable("REDIRECT_URL") ?? "http://localhost";
        configData = new ConfigData(tenantId, clientId, userId, useProxy, redirectUrl);
      }
      catch (Exception e)
      {
        MessageBox.Show("Exception reading data: " + e.Message);
        throw;
      }

      var progress = "";
      try
      {
        var pluginPath = RequireEnvVar("PLUGIN_PATH");
        progress += $"Did set pluginPath={pluginPath}:";

        var pluginFactoryName = RequireEnvVar("PLUGIN_FACTORY");
        progress += $"Will use pluginFactory={pluginFactoryName}:";

        var assembly = Assembly.LoadFile(pluginPath) ?? throw new ArgumentException("Cannot load assembly " + pluginPath);
        progress += $"Did load assembly {assembly}";

        var rawInstance = assembly.CreateInstance(pluginFactoryName);
        progress += $"created rawInstance={rawInstance}:";
        if (rawInstance == null)
        {
          throw new ArgumentException("Could not create raw plugin factory instance of type " + pluginFactoryName);
        }

        var factory = (IPluginFactory)rawInstance ?? throw new ArgumentException("Cannot cast  " + rawInstance+" to " + pluginFactoryName + " loaded from " + assembly);
        _plugin = factory.CreatePlugin(configData);
        if (_plugin == null) throw new Exception("Factory " + factory + " did not create a plugin");
      }
      catch (Exception e)
      {
        MessageBox.Show("Exception loading plugin: progress="+progress+": Exception=" + e.Message);
        throw;
      }


      InitializeComponent();
    }

    private static string RequireEnvVar(string tenantIdVar)
    {
      return Environment.GetEnvironmentVariable(tenantIdVar) ?? throw new ArgumentException("NULL " + tenantIdVar);
    }

    private async void IntegratedWindowsProvider_OnClick(object sender, RoutedEventArgs e)
    {
      await DoTestLogin(LoginMode.INTEGRATED_WINDOWS);
    }

    private async Task DoTestLogin(LoginMode loginMode)
    {
      try
      {
        await _plugin.Login(loginMode);
        await TestGraphClient();
      }
      catch (Exception ex)
      {
        string messageBoxText;
        try
        {
          messageBoxText = ex.ToString();
        }
        catch (Exception e2)
        {
          messageBoxText = "ex.ToString throw 2nd exception: " + e2;
        }
        MessageBox.Show(messageBoxText);
      }
    }

    /**
     * Does Interactive provider
     * https://learn.microsoft.com/en-us/graph/sdks/choose-authentication-providers?tabs=CS#interactive-provider
     */
    private async void InteractiveProvider_OnClick(object sender, RoutedEventArgs e)
    {

      await DoTestLogin(LoginMode.DIALOG);
    }

    /**
     * Show demo user's data
     */
    private async Task TestGraphClient()
    {
      try
      {
        var user = await _plugin.GetUser();
        _userId = user?.Id;

        TxtUser.Text = $"user: {user?.DisplayName}\nmail: {user?.Email}\nID: {user?.Id}";
        if (_userId != null)
        {
          TxtUserId.Text = _userId;
        }
      }
      catch (Exception ex)
      {
        MessageBox.Show(ex.ToString());
      }
    }

   private async void GetPresence_OnClick(object sender, RoutedEventArgs e)
    {
      PresenceData presence;
      if (_userId!=null)
      {
        presence = await _plugin.GetPresenceData(_userId); 
      }
      else
      {
        presence = null;
      }
      TxtPresence.Text = $"Availability: {presence?.Availability}, Activity: {presence?.Activity}";
    }
  }
}
