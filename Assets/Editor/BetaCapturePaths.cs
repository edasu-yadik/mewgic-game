using System;using System.IO;
public static class BetaCapturePaths
{
 public static string NewRun(string suite){string path="C:/Dev/Mewgic/docs/Part08Tests/Run-"+suite+"-"+Guid.NewGuid().ToString("N")+"/";Directory.CreateDirectory(path);return path;}
}
