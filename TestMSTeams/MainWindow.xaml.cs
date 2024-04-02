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
      var tenantId = RequireEnvVar("TENANT_ID");
      var clientId = RequireEnvVar("CLIENT_ID");
      var userId = Environment.GetEnvironmentVariable("USER_ID");
      var useProxy = Environment.GetEnvironmentVariable("USE_PROXY") == "true";
      var redirectUrl = Environment.GetEnvironmentVariable("REDIRECT_URL") ?? "http://localhost";
      var configData = new ConfigData(tenantId, clientId, userId, useProxy, redirectUrl);

      var pluginPath = RequireEnvVar("PLUGIN_PATH");
      var pluginFactoryName = RequireEnvVar("PLUGIN_FACTORY");


      if (pluginFactoryName == null) throw new ArgumentNullException(nameof(pluginFactoryName));
      var assembly = Assembly.LoadFile(pluginPath) ?? throw new ArgumentException("Canot load assembly "+pluginPath);

      IPluginFactory factory = (IPluginFactory)assembly.CreateInstance(pluginFactoryName) ?? throw new ArgumentException("Cannot create "+pluginFactoryName+" instance from "+assembly);
      _plugin = factory.CreatePlugin(configData);
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
        MessageBox.Show(ex.ToString());
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
