using System.Threading.Tasks;

namespace common
{
  public sealed class ConfigData
  {
    public string TenantId { get; }
    public string ClientId { get; }
    public bool UseProxy { get; }
    public string RedirectUrl { get; }

    public ConfigData(string tenantId, string clientId, bool useProxy, string redirectUrl)
    {
      TenantId = tenantId;
      ClientId = clientId;
      UseProxy = useProxy;
      RedirectUrl = redirectUrl;
    }

    public override string ToString()
    {
      return
        string.Format("ConfigData: {0}: {1}, {2}: {3}, {4}: {5}, {6}: {7}", nameof(TenantId), TenantId,
          nameof(ClientId), ClientId, nameof(UseProxy), UseProxy, nameof(RedirectUrl), RedirectUrl);
    }
  }

  public enum LoginMode
  {
    DIALOG,
    INTEGRATED_WINDOWS
  }

  public sealed class UserData
  {
    public UserData(string id, string displayName, string email)
    {
      Id = id;
      DisplayName = displayName;
      Email = email;
    }

    public string Id { get; }
    public string DisplayName { get; }

    public string Email { get; }
  }

  public sealed class PresenceData
  {
    public PresenceData(string availability, string activity)
    {
      Availability = availability;
      Activity = activity;
    }

    public string Availability { get; }
    public string Activity { get; }
  }

  public interface IPlugin
  {
    Task Login(LoginMode loginMode);
    Task<UserData> GetUser();
    Task<PresenceData> GetPresenceData(string userId);
  }

  public interface IPluginFactory
  {
    IPlugin CreatePlugin(ConfigData configData);
  }
}