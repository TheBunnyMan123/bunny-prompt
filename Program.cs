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

  private const string gitIcon = "\ue65d ";
  private const string gitBranchIcon = "\uf418 ";

  private static string ps1 = "\r\n";

  public static void Main() {
    string colorTerm = Environment.GetEnvironmentVariable("COLORTERM");
    if (colorTerm == null) {colorTerm = "";}
    if ((colorTerm.ToLower() == "truecolor") || (colorTerm.ToLower() == "24bit")) {trueColor = true;}

    if (trueColor == false) {ps1 = ps1 + @"\[\x1b[1m\]";}

    ProcessStartInfo gitStartInfo = new ProcessStartInfo("git");

    gitStartInfo.UseShellExecute = false;
    gitStartInfo.WorkingDirectory = workDir;
    gitStartInfo.RedirectStandardInput = true;
    gitStartInfo.RedirectStandardOutput = true;
    gitStartInfo.Arguments = "rev-parse --is-inside-work-tree";

    Process gitProcess = new Process();
    gitProcess.StartInfo = gitStartInfo;
    gitProcess.Start();

    bool isGit = false;
    Boolean.TryParse(gitProcess.StandardOutput.ReadLine(), out isGit);

    gitProcess.Dispose(); 

    if (isGit) {
      displayTable.Add(Display.GitBranch);
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
      ps1 = ps1 + @$"{RgbToANSI(166, 218, 149)}\w\r\n";
    }else {
      ps1 = ps1 + @$"\[\033[32m\]\w\r\n";
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
        ps1 = ps1 + @"\[\x1b[33m\]";
      }
      if (!(branch == null)) {
        ps1 = ps1 + gitIcon + ": " + gitBranchIcon + branch;
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
