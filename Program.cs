using System.Diagnostics;

namespace bunnyprompt;

class bunnyPrompt {
  private enum Display {
    GitPush,
    GitPull,
    GitStage,
    GitCommit,
    GitBranch
  }

  private static string user = Environment.UserName;
  private static string hostname = System.Net.Dns.GetHostName();
  private static List<Display> displayTable = new List<Display>();
  private static bool trueColor = false;
  private static string workDir = Directory.GetCurrentDirectory();
  private static string branch = "";
  private static int unstagedChanges = 0;
  private static int stagedChanges = 0;

  private const string gitIcon = "\ue65d ";
  private const string gitBranchIcon = "\uf418 ";
  private const string gitUnstagedIcon = "\uf040 ";
  private const string gitStagedIcon = "\uebbc ";

  private static bool isGit = false;

  private static string ps1 = "\r\n";

  public static void Main() {
    string colorTerm = Environment.GetEnvironmentVariable("COLORTERM");
    if (colorTerm == null) {colorTerm = "";}
    if ((colorTerm.ToLower() == "truecolor") || (colorTerm.ToLower() == "24bit")) {trueColor = true;}

    if (trueColor == false) {ps1 = ps1 + @"\[\033[1m\]";}

    ProcessStartInfo gitStartInfo1 = new ProcessStartInfo("bash", "-c \"command -v git\"");
    gitStartInfo1.RedirectStandardOutput = true;

    Process gitProcess1 = Process.Start(gitStartInfo1);
    gitProcess1.WaitForExit();

    int exitCode = gitProcess1.ExitCode;

    if (exitCode == 0)
    {
      ProcessStartInfo gitStartInfo = new ProcessStartInfo("git");

      gitStartInfo.UseShellExecute = false;
      gitStartInfo.WorkingDirectory = workDir;
      gitStartInfo.RedirectStandardInput = true;
      gitStartInfo.RedirectStandardOutput = true;
      gitStartInfo.Arguments = "rev-parse --is-inside-work-tree";

      Process gitProcess = new Process();
      gitProcess.StartInfo = gitStartInfo;
      gitProcess.Start();

      Boolean.TryParse(gitProcess.StandardOutput.ReadLine(), out isGit);

      gitProcess.Dispose(); 
    }

    if (isGit) {
      displayTable.Add(Display.GitBranch);

      ProcessStartInfo startInfo = new ProcessStartInfo("git");

      startInfo.UseShellExecute = false;
      startInfo.WorkingDirectory = workDir;
      startInfo.RedirectStandardInput = true;
      startInfo.RedirectStandardOutput = true;
      startInfo.Arguments = "diff --name-status";

      Process process = new Process();
      process.StartInfo = startInfo;
      process.Start();

      // Wait for the process to finish
      process.WaitForExit();

      // Read the output and count the lines
      string output = process.StandardOutput.ReadToEnd();
      unstagedChanges = output.Split(Environment.NewLine).Length - 1;
      
      process.Dispose();

      startInfo = new ProcessStartInfo("git");

      startInfo.UseShellExecute = false;
      startInfo.WorkingDirectory = workDir;
      startInfo.RedirectStandardInput = true;
      startInfo.RedirectStandardOutput = true;
      startInfo.Arguments = "diff --cached --name-status";

      process = new Process();
      process.StartInfo = startInfo;
      process.Start();

      // Wait for the process to finish
      process.WaitForExit();

      // Read the output and count the lines
      output = process.StandardOutput.ReadToEnd();
      stagedChanges = output.Split(Environment.NewLine).Length - 1;

      process.Dispose();
    }

    if (trueColor) {
      ps1 = ps1 + @$"{RgbToANSI(138, 173, 244)}\u{RgbToANSI(202, 211, 245)}@{RgbToANSI(138, 173, 244)}\h ";
    }else {
      ps1 = ps1 + @$"\[\033[34m\]\u\[\033[37m\]@\[\033[34m\]\h ";
    }

    string[] splitWorkDir = workDir.Split("/");
    if (splitWorkDir.Length > 2) {
      if (splitWorkDir[1] == "home") {
        List<string> newSplit = new List<string>();
        newSplit.Add("~");
        for (int i = 0; i < splitWorkDir.Length; i++) {
          if (i <= 2) {
            continue;
          }
          newSplit.Add(splitWorkDir[i]);
        }
        splitWorkDir = newSplit.ToArray();
      }
    }
    if (trueColor) {
      ps1 = ps1 + @$"{RgbToANSI(198, 160, 246)}\w\r\n";
    }else {
      ps1 = ps1 + @$"\[\033[35m\]\w\r\n";
    }

    if (displayTable.Contains(Display.GitBranch)) {
      ProcessStartInfo startInfo = new ProcessStartInfo("git");

      startInfo.UseShellExecute = false;
      startInfo.WorkingDirectory = workDir;
      startInfo.RedirectStandardInput = true;
      startInfo.RedirectStandardOutput = true;
      startInfo.Arguments = "branch --show-current";

      Process process = new Process();
      process.StartInfo = startInfo;
      process.Start();

      branch = process.StandardOutput.ReadLine();

      process.Dispose();
      if (trueColor) {
        ps1 = ps1 + RgbToANSI(245, 169, 127);
      }else {
        ps1 = ps1 + @"\[\033[33m\]";
      }
      if (!(branch == null)) {
        ps1 = ps1 + gitIcon + ": " + gitBranchIcon + branch;
      }
    }

    if (unstagedChanges > 0) {
      ps1 = ps1 + " : " + gitUnstagedIcon + unstagedChanges;
    }
    if (stagedChanges > 0) {
      if (trueColor) {
        ps1 = ps1 + " : " + RgbToANSI(166, 218, 149) + gitStagedIcon + stagedChanges + RgbToANSI(245, 169, 127);
      }else {
        ps1 = ps1 + " : " + @"\[\033[32m\]" + gitStagedIcon + stagedChanges + @"\[\033[33m\]";
      }
    }

    if (isGit) {
      ps1 = ps1 + "\r\n";
    }

    //ps1 = ps1 + @$"{RgbToANSI(198, 160, 246)}");
    ps1 = ps1 + @$"{RgbToANSI(202, 211, 245)}> ";
    Console.Write(ps1);
  }

  private static string RgbToANSI(byte r, byte g, byte b) {
    return @$"\[\033[38;2;{r};{g};{b}m\]";
  }
}
