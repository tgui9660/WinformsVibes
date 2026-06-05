using System;
using System.Windows.Forms;
using NHibernate;
using NHibernate.Cfg;

namespace WinformsVibes
{
    internal static class Program
    {
        private static ISessionFactory? _sessionFactory;

        public static ISessionFactory SessionFactory => _sessionFactory
            ?? throw new InvalidOperationException("NHibernate SessionFactory not initialized. Check connection settings and ensure the database is accessible.");

        private static void InitNHibernate()
        {
            var config = new Configuration();

            config.Properties["connection.provider"] = "NHibernate.Connection.DriverConnectionProvider";
            config.Properties["connection.driver_class"] = "NHibernate.Driver.SqlClientDriver";
            config.Properties["connection.connection_string"] =
                "Server=localhost;Database=winformsvibes;User Id=sa;Password=password;";
            config.Properties["dialect"] = "NHibernate.Dialect.MsSql2019Dialect";
            config.Properties["show_sql"] = "false";
            config.Properties["hbm2ddl.auto"] = "update";
            config.Properties["command_timeout"] = "30";

            _sessionFactory = config.BuildSessionFactory();
        }

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            try
            {
                InitNHibernate();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Warning: NHibernate initialization failed: {ex.Message}");
            }

            Application.Run(new MainForm());
        }
    }
}
