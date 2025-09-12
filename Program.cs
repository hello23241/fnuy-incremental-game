namespace WinFormsApp1
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);

            AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
            {
                Exception ex = (Exception)args.ExceptionObject;
                LogCrash(ex);
                MessageBox.Show("The game crashed. A log has been saved.", "Crash Detected");
            };

            Application.ThreadException += (sender, args) =>
            {
                LogCrash(args.Exception);
                MessageBox.Show("An error occurred. A log has been saved.", "Error");
            };

            ApplicationConfiguration.Initialize();
            Application.Run(new MainForm());
        }
        private static void LogCrash(Exception ex)
        {
            string crashLogPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "FnuyIncrementalGame",
                "crashlog.txt"
            );

            Directory.CreateDirectory(Path.GetDirectoryName(crashLogPath));

            string log = $"[{DateTime.Now}] {ex.GetType().Name}: {ex.Message}\n{ex.StackTrace}\n\n";
            File.AppendAllText(crashLogPath, log);
        }

    }
}