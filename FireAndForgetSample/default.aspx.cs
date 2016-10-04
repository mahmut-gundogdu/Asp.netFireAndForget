using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace FireAndForgetSample
{
    public partial class _default : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void Button1_Click(object sender, EventArgs e)
        {

            Response.Write("Task Calisti");

            var path = Server.MapPath("~/App_Data/.txt");
            HostingEnvironment.QueueBackgroundWorkItem(cancellationToken =>
            new Processor(path).StartProcessing(cancellationToken));

            Response.Write("Task Bitti");
        }
    }
}
//Source: http://codingcanvas.com/using-hostingenvironment-queuebackgroundworkitem-to-run-background-tasks-in-asp-net/
public class Processor
{
    public Processor(string path)
    {
        filePath = path;
    }

    string filePath ;

    /// <summary>
    /// An operation which generates a random number in 3 seconds.
    /// </summary>
    /// <returns>Random number</returns>
    private int LongRunningOperation()
    {
        Thread.Sleep(3000);
        var rand = new Random();
        return rand.Next();
    }

    /// <summary>
    /// Method loops 10 times calling LongRunningOperation method to get a random number
    /// </summary>
    /// <param name="cancellationToken"></param>
    public void StartProcessing(CancellationToken cancellationToken = default(CancellationToken))
    {
        var result = new List<int>();
        try
        {
            for (int index = 1; index <= 10; index++)
            {

                cancellationToken.ThrowIfCancellationRequested();
                result.Add(LongRunningOperation());
            }
            WriteResultsToFile(result);
        }
        catch (Exception ex)
        {
            ProcessCancellation(result);
            File.AppendAllLines(filePath, new List<string>() { ex.GetType().ToString() + " : " + ex.Message });
        }

    }
    /// <summary>
    /// Writes result to a file
    /// </summary>
    /// <param name="numbers"></param>
    /// <param name="completed"></param>
    private void WriteResultsToFile(List<int> numbers, bool completed = true)
    {
        var content = new List<String>();
        string seed = string.Empty;

        content.Add("Results:" + numbers.Aggregate(seed, (s, n) => s + " " + n));
        if (!completed)
        {
            content.Add("Operation cancelled!!!");
        }
        File.Delete(filePath);
        File.AppendAllLines(filePath, content);
    }
    /// <summary>
    /// Method to hand the cancellation.
    /// </summary>
    /// <param name="numbers"></param>
    private void ProcessCancellation(List<int> numbers)
    {
        Thread.Sleep(10000);
        WriteResultsToFile(numbers, false);
    }
}