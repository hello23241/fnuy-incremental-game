using BreakInfinity;
using Newtonsoft.Json;

namespace WinFormsApp1.SaveSystem;

public interface IGameSaveService
{
    void Save(GameState state);
    Task<(bool ok, GameState? state)> TryLoadAsync();
}

public sealed class GameSaveService : IGameSaveService
{
    private readonly string _path;
    private readonly JsonSerializerSettings _settings;

    public GameSaveService(string path)
    {
        _path = path;
        _settings = new JsonSerializerSettings();
        _settings.Converters.Add(new BigDoubleConverter());
    }

    public void Save(GameState state)
    {
#if DEBUG
        return;
#endif
        string json = JsonConvert.SerializeObject(state, _settings);
        byte[] plain = System.Text.Encoding.UTF8.GetBytes(json);
        byte[]? cipher = ProtectBytes(plain);

        Directory.CreateDirectory(Path.GetDirectoryName(_path)!);
        if (cipher != null) File.WriteAllBytes(_path, cipher);
        else File.WriteAllText(_path, json);
    }

    // NOTE: no async/await; return Task.FromResult to satisfy async signature without warnings
    public Task<(bool ok, GameState? state)> TryLoadAsync()
    {
        if (!File.Exists(_path)) return Task.FromResult((false, (GameState?)null));

        byte[] fileBytes = File.ReadAllBytes(_path);
        byte[]? plain = UnprotectBytes(fileBytes);
        if (plain == null) return Task.FromResult((false, (GameState?)null));

        string json = System.Text.Encoding.UTF8.GetString(plain);
        var state = JsonConvert.DeserializeObject<GameState>(json, _settings);
        return Task.FromResult((state != null, state));
    }

    private static byte[]? ProtectBytes(byte[] data)
    {
        try
        {
            var entropy = System.Text.Encoding.UTF8.GetBytes("FnuyIncrementalGame_v1");
            return System.Security.Cryptography.ProtectedData
                .Protect(data, entropy, System.Security.Cryptography.DataProtectionScope.CurrentUser);
        }
        catch { return null; }
    }

    private static byte[]? UnprotectBytes(byte[] data)
    {
        try
        {
            var entropy = System.Text.Encoding.UTF8.GetBytes("FnuyIncrementalGame_v1");
            return System.Security.Cryptography.ProtectedData
                .Unprotect(data, entropy, System.Security.Cryptography.DataProtectionScope.CurrentUser);
        }
        catch { return null; }
    }
}