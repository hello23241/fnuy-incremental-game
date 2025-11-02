using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using WinFormsApp1.Core.Persistence;
using WinFormsApp1.SaveSystem;

namespace WinFormsApp1.UI.Controllers
{
    // Orchestrates load + offline progress + visibility unlocks. No UI controls are held here.
    public sealed class GameLifecycleCoordinator
    {
        private readonly IGameSaveService _saveService;
        private readonly string _savePath;

        public GameLifecycleCoordinator(IGameSaveService saveService, string savePath)
        {
            _saveService = saveService ?? throw new ArgumentNullException(nameof(saveService));
            _savePath = savePath ?? throw new ArgumentNullException(nameof(savePath));
        }

        public async Task LoadAsync(
            Form owner,
            Action<GameRuntimeState> applyRuntime,
            Action createOrResetController,
            Func<Task<DateTime?>> getServerUtcDateAsync,
            Action<DateTime, DateTime> applyOfflineProgress,
            Action<GameState, DateTime?> applyUnlocks,
            Action recoverVisibility,
            Action saveGame,
            Action updateUi,
            Action<Exception> logCrash,
            Action resetToDefaults)
        {
            try
            {
                if (!File.Exists(_savePath))
                {
                    saveGame();
                    return;
                }

                var (ok, loaded) = await _saveService.TryLoadAsync();
                if (!ok || loaded == null)
                {
                    var backup = SaveFileCoordinator.TryBackup(_savePath);

                    if (backup.ok)
                    {
                        try
                        {
                            MessageBox.Show(
                                owner,
                                "Your old data will NOT CARRY OVER in this version.\nThere's a special bonus waiting after prestiging\n\n" +
                                $"A backup of your old save has been created at:\n{backup.backupPath}\n\n" +
                                "The game will start a fresh save now.",
                                "Haha get reset son",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Warning
                            );
                        }
                        catch { }

                        saveGame();
                        return;
                    }
                    else
                    {
                        try
                        {
                            MessageBox.Show(
                                owner,
                                "An incompatible save was found and could not be backed up. The save will be reset to defaults.",
                                "Incompatible Save",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Warning
                            );
                        }
                        catch { }

                        saveGame();
                        return;
                    }
                }

                var state = loaded;
                var rs = GamePersistence.ToRuntimeState(state);

                // Apply all runtime fields to the form
                applyRuntime(rs);

                // Recreate controller on top of latest fields
                createOrResetController();

                DateTime? serverTime = await getServerUtcDateAsync();
                if (serverTime != null)
                    applyOfflineProgress(state.LastSavedTime, serverTime.Value);

                // Apply visibility/unlocks and then recover any missing flags
                applyUnlocks(state, serverTime);
                recoverVisibility();

                // Persist final visibility safety and update UI
                saveGame();
                updateUi();
            }
            catch (Exception ex)
            {
                // Log and inform user, then reset state to defaults
                logCrash(ex);
                try
                {
                    MessageBox.Show(
                        owner,
                        "Your save file is missing or corrupted. A new save has been created.",
                        "Load Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning
                    );
                }
                catch { }

                resetToDefaults();
                saveGame();
                updateUi();
            }
        }
    }
}