using Diffinity;
using System.Diagnostics;

internal class Program
{
    static void Main(string[] args)
    {
        var DEV002   = new DbServer("DEV002", Environment.GetEnvironmentVariable("dev2cs"));
        var Corewell = new DbServer("Corewell", Environment.GetEnvironmentVariable("connectionString"));
        var CMH      = new DbServer("CMH", Environment.GetEnvironmentVariable("cmhCs"));
        
        string IndexPage = DbComparer.Compare(DEV002,CMH);
        #region Optional
        // You can optionally pass any of the following parameters:
        // logger: your custom ILogger instance
        // outputFolder: path to save the results (string)
        // makeChange: whether to apply changes (ComparerAction.ApplyChanges,ComparerAction.DoNotApplyChanges)
        // filter: filter rules (DbObjectFilter.ShowUnchanged,DbObjectFilter.HideUnchanged)
        // run: execute comparison on specific dbObject(Run.Proc,Run.View,Run.Table,Run.ProcView,Run.ProcTable,Run.ViewTable,Run.All)
        //
        // Example:
        // string IndexPage = DbComparer.Compare(MyDbV1, MyDbV2, logger: myLogger, outputFolder: "customPath", makeChange: true);
        #endregion
        var psi = new ProcessStartInfo
        {
            FileName = IndexPage,
            UseShellExecute = true
        };
        Process.Start(psi);
    }
}
