using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using NHibernate;
using WinformsVibes.Models;

namespace WinformsVibes;

public static class DbConfig
{
    private static ISessionFactory? _sessionFactory;

    public static ISessionFactory SessionFactory =>
        _sessionFactory ??= Fluently.Configure()
            .Database(MsSqlConfiguration.MsSql2012
                .ConnectionString("Server=localhost;Database=winformsvibes;User Id=sa;Password=password;"))
            .Mappings(m => m.FluentMappings.AddFromAssemblyOf<ApplicationInfo>())
            .ExposeConfiguration(cfg =>
            {
                cfg.SetProperty("use_proxy_validator", "false");
                cfg.SetProperty("default_lazy", "false");
            })
            .BuildSessionFactory();

    public static ApplicationInfo? GetApplicationInfo()
    {
        using var session = SessionFactory.OpenSession();
        return session.CreateCriteria<ApplicationInfo>().UniqueResult<ApplicationInfo?>();
    }
}
