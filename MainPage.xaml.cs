namespace HAConfig;
using SQLite;

public partial class MainPage : ContentPage
{
	SQLiteConnection conn;

	public MainPage()
	{
		InitializeComponent();
		OpenConfigTable();
	}

	private void SaveBtn_Clicked(object sender, EventArgs e)
	{
		AddOrUpdateConfig(new Config { Key = "Sync_ServerURL", Value = Sync_ServerURL.Text });
		AddOrUpdateConfig(new Config { Key = "Sync_SandstormToken", Value = Sync_SandstormToken.Text });
		AddOrUpdateConfig(new Config { Key = "Sync_AccessKey", Value = Sync_AccessKey.Text });
		DisplayAlert("Saved", "Configuration values have been saved", "OK");

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

