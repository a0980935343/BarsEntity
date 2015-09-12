using System.Collections.Generic;

namespace Barsix.BarsEntity.BarsGenerators
{
    using BarsOptions;
    using CodeGeneration.CSharp;

    public class PermissionGenerator : BaseBarsGenerator
    {
        public override List<GeneratedFile> Generate(ProjectInfo project, EntityOptions options, GeneratedFragments fragments)
        {
            base.Generate(project, options, fragments);
            var map = options.Permission;
            List<string> lines = new List<string>();


            var permission = new GeneratedFragment
            {
                FileName = "PermissionMap.cs",
                FilePath = "PermissionMap",
                InsertToFile = true,
                InsertClass = "public class PermissionMap",
                InsertMethod = "public PermissionMap()",
                Generator = this
            };

            lines.Add("Namespace(\"{0}\", \"{1}\");".R(map.Prefix, options.DisplayName));
            
            if (map.SimpleCRUDMap)
            {
                lines.Add("CRUDandViewPermissions(\"{0}\");".R(map.Prefix));
            }
            else
            {
                lines.Add("Permission(\"{0}.View\", \"Просмотр\");".R(map.Prefix));
                lines.Add("Permission(\"{0}.Edit\", \"Изменение записей\");".R(map.Prefix));
                lines.Add("Permission(\"{0}.Create\", \"Создание записей\");".R(map.Prefix));
                lines.Add("Permission(\"{0}.Delete\", \"Удаление записей\");".R(map.Prefix));
            }

            if (options.Signable)
                lines.Add("Permission(\"{0}.Sign\", \"Подписание документа\");".R(map.Prefix));

            permission.Lines = lines;

            fragments.Add("PermissionMap/PermissionMap.cs", permission);
            return null;
        }
    }
}