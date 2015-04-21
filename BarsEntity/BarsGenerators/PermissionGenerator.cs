using System;

namespace Barsix.BarsEntity.BarsGenerators
{
    using BarsOptions;
    using CodeGeneration;

    public class PermissionGenerator : BaseBarsGenerator
    {
        public override void Generate(EnvDTE.Project project, EntityOptions options)
        {
            base.Generate(project, options);
            var map = options.Permission;

            DontForget.Add("PermissionMap/PermissionMap.cs/.ctor()");


            if (map.NeedNamespace)
            {
                DontForget.Add("Namespace(\"{0}\", \"{1}\");".F(map.Prefix, options.DisplayName));
            }
            else
            {
                DontForget.Add("Permission(\"{0}\", \"{1}\");".F(map.Prefix, options.DisplayName));
            }

            if (map.SimpleCRUDMap)
            {
                DontForget.Add("CRUDandViewPermissions(\"{0}\");".F(map.Prefix));
            }
            else
            {
                DontForget.Add("Permission(\"{0}.View\", \"Просмотр\");".F(map.Prefix));
                DontForget.Add("Permission(\"{0}.Edit\", \"Изменение записей\");".F(map.Prefix));
                DontForget.Add("Permission(\"{0}.Create\", \"Создание записей\");".F(map.Prefix));
                DontForget.Add("Permission(\"{0}.Delete\", \"Удаление записей\");".F(map.Prefix));
            }

            if (options.Signable)
                DontForget.Add("Permission(\"{0}.Sign\", \"Подписание документа\");".F(map.Prefix));
        }
    }
}