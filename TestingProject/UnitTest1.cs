namespace TestingProject
{
    public class Tests
    {        
        [Test]
        public void WriteMonitorLine_ProcessesRunning_WritesCorrectMessage()
        {
            var logPath = @"C:\Temp\TestProcessesLogFile.txt";
            var processName = "notepad";
            var processes = new Process[] { new Process() };
            var monitor = new ProcessMonitor(logPath, processName, 1, 1);
            using var sw = new StringWriter();
            Console.SetOut(sw);
            monitor.WriteMonitorLine(processes);
            var expected = $"1 process(es) {processName} is/are running{Environment.NewLine}";
            Assert.AreEqual(expected, sw.ToString());
        }

        [Test]
        public void WriteMonitorLine_NoProcesses_WritesCorrectMessage()
        {
            var logPath = @"C:\Temp\TestProcessesLogFile.txt";
            var processName = "notepad";
            var processes = new Process[] { };
            var monitor = new ProcessMonitor(logPath, processName, 1, 1);
            using var sw = new StringWriter();
            Console.SetOut(sw);
            monitor.WriteMonitorLine(processes);
            var expected = $"0 process(es) {processName} is/are not running{Environment.NewLine}";
            Assert.AreEqual(expected, sw.ToString());
        }

        [Test]
        public async Task StartMonitoringAsync_IfProcessFoundAndKilled_LogsCorrectly()
        {
            var logPath = @"C:\Temp\TestProcessesLogFile.txt";
            var processName = "notepad";
            var killNumberOfMinutes = 1;
            var retryNumberOfMinutes = 1;
            var monitor = new ProcessMonitor(logPath, processName, killNumberOfMinutes, retryNumberOfMinutes);
            var processExists = monitor.GetProcessesByName(processName).Length>0;
            await monitor.StartMonitoringAsync(true);       
            Assert.True(File.Exists(logPath)==processExists);
        }
    }
}