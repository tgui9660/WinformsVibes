using FluentNHibernate.Mapping;
using WinformsVibes.Database;
using WinformsVibes.Models;

namespace WinformsVibes.Maps;

public class ApplicationInfoMap : ClassMap<ApplicationInfo>
{
    public ApplicationInfoMap()
    {
        Table("ApplicationInfo");
        Id(x => x.Id).Column("Id");
        Map(x => x.ApplicationName);
        Map(x => x.Author);
        Map(x => x.Version);
        Map(x => x.Description);
        Map(x => x.Framework);
        Map(x => x.Dependencies).CustomSqlType(DbConfig.LongStringSqlType);
        Map(x => x.CreatedAt);
        Map(x => x.UpdatedAt);
    }
}
