using System.Collections.Generic;

namespace Barsix.BarsEntity.BarsGenerators
{
    using BarsOptions;
    using CodeGeneration;

    public class PermissionGenerator : BaseBarsGenerator
    {
        public override GeneratedFile Generate(ProjectInfo project, EntityOptions options, GeneratedFragments fragments)
        {
            var file = base.Generate(project, options, fragments);
            var map = options.Permission;
            List<string> lines = new List<string>();


            if (map.NeedNamespace)
            {
                lines.Add("Namespace(\"{0}\", \"{1}\");".F(map.Prefix, options.DisplayName));
            }
            else if (!map.SimpleCRUDMap)
            {
                lines.Add("Permission(\"{0}\", \"{1}\");".F(map.Prefix, options.DisplayName));
            }

            if (map.SimpleCRUDMap)
            {
                lines.Add("CRUDandViewPermissions(\"{0}\");".F(map.Prefix));
            }
            else
            {
                lines.Add("Permission(\"{0}.View\", \"Просмотр\");".F(map.Prefix));
                lines.Add("Permission(\"{0}.Edit\", \"Изменение записей\");".F(map.Prefix));
                lines.Add("Permission(\"{0}.Create\", \"Создание записей\");".F(map.Prefix));
                lines.Add("Permission(\"{0}.Delete\", \"Удаление записей\");".F(map.Prefix));
            }

            if (options.Signable)
                lines.Add("Permission(\"{0}.Sign\", \"Подписание документа\");".F(map.Prefix));

            fragments.AddLines("PermissionMap/PermissionMap.cs", this, lines);
            return null;
        }
    }
}