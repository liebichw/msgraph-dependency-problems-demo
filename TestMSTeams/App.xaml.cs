using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using common;

namespace TestMSTeams
{
  /// <summary>
  /// Interaction logic for App.xaml
  /// </summary>
  public partial class App
  {
    private readonly ConfigData _configData;
    private readonly string _pluginPath;
    private readonly string _pluginFactoryName;

    public App()
    {
      var tenantId=Environment.GetEnvironmentVariable("TENANT_ID") ?? throw new ArgumentException("NULL TENANT_ID");
      var clientId=Environment.GetEnvironmentVariable("CLIENT_ID") ?? throw new ArgumentException("NULL CLIENT_ID");
      var userId=Environment.GetEnvironmentVariable("USER_ID");
      var useProxy=Environment.GetEnvironmentVariable("USE_PROXY")=="true";
      var redirectUrl=Environment.GetEnvironmentVariable("REDIRECT_URL") ?? "http://localhost";

      _pluginPath = Environment.GetEnvironmentVariable("PLUGIN-PATH") ?? throw new ArgumentException("NULL PLUGIN-PATH");
      _pluginFactoryName = Environment.GetEnvironmentVariable("PLUGIN_FACTORY") ?? throw new ArgumentException("NULL PLUGIN_FACTORY");
      _configData = new ConfigData(tenantId, clientId, null, userId, useProxy, redirectUrl);
    }

    protected override void OnStartup(StartupEventArgs e)
    {
      base.OnStartup(e);
      Window window = new MainWindow(_configData, _pluginPath,_pluginFactoryName);
      window.Show();

    }
  }
}
