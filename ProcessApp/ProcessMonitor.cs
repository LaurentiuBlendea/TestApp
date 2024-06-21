using System.Diagnostics;

public class ProcessMonitor
{
    private string logPath;
    private string processName;
    private int retryNumberOfMinutes;
    private int killNumberOfMinutes;

    public ProcessMonitor(string logPath, string processName, int killNumberOfMinutes, int retryNumberOfMinutes)
    {
        this.logPath = logPath;
        this.processName = processName;
        this.retryNumberOfMinutes = retryNumberOfMinutes;
        this.killNumberOfMinutes = killNumberOfMinutes;
    }

    public static async Task Main(string[] args)
    {
        var logPath = @"C:\Temp\ProcessesLogFile.txt";
        var processName = "";
        var retryNumberOfMinutes = 0;
        var killNumberOfMinutes = 0;

        if (args.Length > 0)
            processName = args[0];
        if (args.Length > 1)
            Int32.TryParse(args[1], out killNumberOfMinutes);
        if (args.Length > 2)
            Int32.TryParse(args[2], out retryNumberOfMinutes);

        var monitor = new ProcessMonitor(logPath, processName, killNumberOfMinutes, retryNumberOfMinutes);
        await monitor.StartMonitoringAsync();
    }

    public async Task StartMonitoringAsync(bool killIfTest = false)
    {
        Console.WriteLine($"Hello, you have started monitoring process with name: '{processName}' each {retryNumberOfMinutes} minutes(s) and it will be killed after {killNumberOfMinutes} minutes(s)");

        var numberOfFinds = 0;
        var periodicTimer = new PeriodicTimer(TimeSpan.FromMinutes(retryNumberOfMinutes));
        var endTime = new DateTime();
        var wasProcessedKilled = false;
        var wasLogingDone = false;

        while (await periodicTimer.WaitForNextTickAsync() && !(Console.KeyAvailable && Console.ReadKey(true).Key == ConsoleKey.Q))
        {
            var theProcesses = GetProcessesByName(processName);

            if (theProcesses.Length > 0 && numberOfFinds == 0)
            {
                endTime = DateTime.Now.AddMinutes(killNumberOfMinutes);
                numberOfFinds++;
            }
            if (theProcesses.Length > 0 && wasProcessedKilled)
            {
                wasProcessedKilled = false;
                endTime = DateTime.Now.AddMinutes(killNumberOfMinutes);
            }

            WriteMonitorLine(theProcesses);

            if (endTime < DateTime.Now && theProcesses.Length > 0)
            {
                wasProcessedKilled = KillProcessAndLog(theProcesses);
                wasLogingDone = wasProcessedKilled;
                numberOfFinds++;
            }

            if (endTime < DateTime.Now && killIfTest)
            {
                break;
            }
        }
        if (wasLogingDone) File.AppendAllText(logPath, Environment.NewLine);
        periodicTimer.Dispose();
    }

    public virtual Process[] GetProcessesByName(string processName)
    {
        return Process.GetProcessesByName(processName);
    }

    public void WriteMonitorLine(Process[] theProcesses)
    {
        var processResponse = theProcesses.Length > 0 ? "is/are running" : "is/are not running";
        var processCount = theProcesses.Length.ToString();
        Console.WriteLine($"{processCount} process(es) {processName} {processResponse}");
    }

    public bool KillProcessAndLog(Process[] theProcesses)
    {
        foreach (var theProcess in theProcesses) theProcess.Kill();
        File.AppendAllText(logPath, $"{DateTime.Now} : All processes {processName} were killed!" + Environment.NewLine);
        return true;
    }
}
