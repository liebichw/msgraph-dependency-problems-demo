using System;
using System.Runtime.InteropServices;

namespace common
{
  public sealed class ConfigData
  {
    public string TenantId { get; }
    public string ClientId { get; }
    public string ClientSecret { get; }
    public string UserId { get; }
    public bool UseProxy { get; }
    public string RedirectUrl { get; }

    public string SessionId => ClientId;

    public ConfigData(string tenantId, string clientId, string clientSecret, string userId, bool useProxy, string redirectUrl)
    {
      TenantId = tenantId;
      ClientId = clientId;
      ClientSecret = clientSecret;
      UserId = userId;
      UseProxy = useProxy;
      RedirectUrl = redirectUrl;
    }

    public override string ToString()
    {
      return string.Format("{0}: {1}, {2}: {3}, {4}: {5}, {6}: {7}, {8}: {9}, {10}: {11}", nameof(TenantId), TenantId,
        nameof(ClientId), ClientId, nameof(ClientSecret), ClientSecret, nameof(UserId), UserId, nameof(UseProxy),
        UseProxy, nameof(RedirectUrl), RedirectUrl);
    }
  }

  public enum LoginMode
  {
    DIALOG,
    INTEGRATED_WINDOWS
  }

  public interface IPlugin
  {
    
  }

  public interface IPluginFactory
  {
    IPlugin CreatePlugin(ConfigData configData);
  }
}