using Diffinity;
using System.Diagnostics;

internal class Program
{
    static void Main(string[] args)
    {
        var MyDbV1 = new DbServer("Corewell", Environment.GetEnvironmentVariable("connectionString"));
        var MyDbV2 = new DbServer("CMH", Environment.GetEnvironmentVariable("CmhCs"));
        string IndexPage = DbComparer.Compare(MyDbV1, MyDbV2);
        #region Optional:
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
