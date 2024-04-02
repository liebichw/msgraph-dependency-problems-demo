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
      var tenantId=RequireEnvVar("TENANT_ID");
      var clientId=RequireEnvVar("CLIENT_ID");
      var userId=Environment.GetEnvironmentVariable("USER_ID");
      var useProxy=Environment.GetEnvironmentVariable("USE_PROXY")=="true";
      var redirectUrl=Environment.GetEnvironmentVariable("REDIRECT_URL") ?? "http://localhost";

      _pluginPath = RequireEnvVar("PLUGIN-PATH");
      _pluginFactoryName = RequireEnvVar("PLUGIN_FACTORY");
      _configData = new ConfigData(tenantId, clientId, userId, useProxy, redirectUrl);
    }

    private static string RequireEnvVar(string tenantIdVar)
    {
      return Environment.GetEnvironmentVariable(tenantIdVar) ?? throw new ArgumentException("NULL " + tenantIdVar);
    }

    protected override void OnStartup(StartupEventArgs e)
    {
      base.OnStartup(e);
      Window window = new MainWindow(_configData, _pluginPath,_pluginFactoryName);
      window.Show();

    }
  }
}
