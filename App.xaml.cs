using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace COMP609_Assessment_2_LMS_GUI_App
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        // Define a static property to store the connection string
        public static string DatabaseConnectionString { get; private set; }

        // The application's entry point
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Get the directory where your application's executable is located
            string appDirectory = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);

            // Append the database file name to the directory
            string databasePath = System.IO.Path.Combine(appDirectory, "FarmData.accdb");

            // Construct the connection string
            DatabaseConnectionString = @"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + databasePath + ";";
        }
    }

}
