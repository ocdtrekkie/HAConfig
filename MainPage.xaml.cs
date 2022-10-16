namespace HAConfig;
using SQLite;
using System.Net.Http;

public partial class MainPage : ContentPage
{
	SQLiteConnection conn;

	public MainPage()
	{
		InitializeComponent();
		OpenConfigTable();
	}

	private async void SaveBtn_Clicked(object sender, EventArgs e)
	{
		AddOrUpdateConfig(new Config { Key = "Sync_ServerURL", Value = Sync_ServerURL.Text });
		AddOrUpdateConfig(new Config { Key = "Sync_SandstormToken", Value = Sync_SandstormToken.Text });
		AddOrUpdateConfig(new Config { Key = "Sync_AccessKey", Value = Sync_AccessKey.Text });

		// The config tool will send a single heartbeat to confirm the Sandstorm offer template
		HttpClient heartbeatClient = new();
		HttpRequestMessage heartbeatMessage = new(HttpMethod.Post, Sync_ServerURL.Text + "?message_type=heartbeat&destination=server&access_key=" + Sync_AccessKey.Text + "&message=none&user_agent=XRFAgentConfig/1.0.0");
		string EncodedCreds = System.Convert.ToBase64String(System.Text.Encoding.GetEncoding("ISO-8859-1").GetBytes("sandstorm:" + Sync_SandstormToken.Text));
		heartbeatMessage.Headers.Add("Authorization", "Basic " + EncodedCreds);
		HttpResponseMessage heartbeatResponse = await heartbeatClient.SendAsync(heartbeatMessage);

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
		DisplayAlert("Saved", "Configuration values have been saved", "OK");
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

	}

	private void Database_FileURI_Completed(object sender, EventArgs e)
	{
		conn.Close();
		conn.Dispose();
		OpenConfigTable();
	}

	private void OpenConfigTable()
	{
        conn = new SQLiteConnection(Database_FileURI.Text);
        conn.CreateTable<Config>();
        Sync_ServerURL.Text = GetConfig("Sync_ServerURL");
        Sync_SandstormToken.Text = GetConfig("Sync_SandstormToken");
        Sync_AccessKey.Text = GetConfig("Sync_AccessKey");
    }

    [Table("CONFIG")]
	public class Config
	{
		[PrimaryKey,MaxLength(100)]
		public string Key { get; set; }

		public string Value { get; set; }
	}

	public int AddConfig(Config config)
	{
		int result = conn.Insert(config);
		return result;
	}

	public int UpdateConfig(Config config)
	{
		int result = 0;
		result = conn.Update(config);
		return result;
	}

	public int AddOrUpdateConfig(Config config)
	{
		int result = UpdateConfig(config);
		if (result == 0) {
			result = AddConfig(config);
		}
		return result;
	}

	public string GetConfig(string key)
	{
		var Value = from c in conn.Table<Config>()
					where c.Key == key
					select c.Value;
		return Value.FirstOrDefault();
	}
}

