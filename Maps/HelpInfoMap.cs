using FluentNHibernate.Mapping;
using WinformsVibes.Models;

namespace WinformsVibes.Maps;

public class HelpInfoMap : ClassMap<HelpInfo>
{
    public HelpInfoMap()
    {
        Table("HelpInfo");
        Id(x => x.Id).Column("Id");
        Map(x => x.Category);
        Map(x => x.Topic);
        Map(x => x.Content);
    }
}
