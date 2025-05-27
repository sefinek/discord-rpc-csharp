using DiscordRPC.Logging;
using Microsoft.Win32;

namespace DiscordRPC.Registry;

internal class WindowsUriSchemeCreator : IUriSchemeCreator
{
	private readonly ILogger _logger;

	public WindowsUriSchemeCreator(ILogger logger)
	{
		_logger = logger;
	}

	public bool RegisterUriScheme(UriSchemeRegister register)
	{
		if (Environment.OSVersion.Platform == PlatformID.Unix || Environment.OSVersion.Platform == PlatformID.MacOSX) throw new PlatformNotSupportedException("URI schemes can only be registered on Windows");

		//Prepare our location
		string location = register.ExecutablePath;
		if (location == null)
		{
			_logger.Error("Failed to register application because the location was null.");
			return false;
		}

		//Prepare the Scheme, Friendly name, default icon and default command
		string scheme = $"discord-{register.ApplicationID}";
		string friendlyName = $"Run game {register.ApplicationID} protocol";
		string defaultIcon = location;
		string command = location;

		//We have a steam ID, so attempt to replce the command with a steam command
		if (register.UsingSteamApp)
		{
			//Try to get the steam location. If found, set the command to a run steam instead.
			string steam = GetSteamLocation();
			if (steam != null)
				command = $"\"{steam}\" steam://rungameid/{register.SteamAppID}";
		}

		//Okay, now actually register it
		CreateUriScheme(scheme, friendlyName, defaultIcon, command);
		return true;
	}

	/// <summary>
	///     Creates the actual scheme
	/// </summary>
	/// <param name="scheme"></param>
	/// <param name="friendlyName"></param>
	/// <param name="defaultIcon"></param>
	/// <param name="command"></param>
	private void CreateUriScheme(string scheme, string friendlyName, string defaultIcon, string command)
	{
		using (RegistryKey key = Microsoft.Win32.Registry.CurrentUser.CreateSubKey($"SOFTWARE\\Classes\\{scheme}"))
		{
			key.SetValue("", $"URL:{friendlyName}");
			key.SetValue("URL Protocol", "");

			using (RegistryKey iconKey = key.CreateSubKey("DefaultIcon"))
			{
				iconKey.SetValue("", defaultIcon);
			}

			using (RegistryKey commandKey = key.CreateSubKey("shell\\open\\command"))
			{
				commandKey.SetValue("", command);
			}
		}

		_logger.Trace("Registered {0}, {1}, {2}", scheme, friendlyName, command);
	}

	/// <summary>
	///     Gets the current location of the steam client
	/// </summary>
	/// <returns></returns>
	private string GetSteamLocation()
	{
		using RegistryKey key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("Software\\Valve\\Steam");
		if (key == null) return null;
		return key.GetValue("SteamExe") as string;
	}
}
