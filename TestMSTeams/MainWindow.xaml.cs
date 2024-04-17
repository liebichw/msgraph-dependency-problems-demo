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

    public MainWindow(ConfigData configData, string pluginPath,string pluginFactoryName)
    {
      if (pluginFactoryName == null) throw new ArgumentNullException(nameof(pluginFactoryName));
      var assembly = Assembly.LoadFile(pluginPath) ?? throw new ArgumentException("Canot load assembly "+pluginPath);

      IPluginFactory factory = (IPluginFactory)assembly.CreateInstance(pluginFactoryName) ?? throw new ArgumentException("Cannot create "+pluginFactoryName+" instance from "+assembly);
      _plugin = factory.CreatePlugin(configData);
      InitializeComponent();
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
        var userId = user?.Id;

        TxtUser.Text = $"user: {user?.DisplayName}\nmail: {user?.Email}\nID: {user?.Id}";
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

   private async void GetPresence_OnClick(object sender, RoutedEventArgs e)
    {
      var presence = await _plugin.GetPresenceData();
      TxtPresence.Text = $"Availability: {presence?.Availability}, Activity: {presence?.Activity}";
    }
  }
}
