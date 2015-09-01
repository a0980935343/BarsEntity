﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Barsix.BarsEntity.BarsOptions
{
    public class EntityOptions
    {
        public string ClassName;

        public string ClassFullName;
        
        public string BaseClass;

        public string TableName;

        public InheritanceType InheritanceType;

        public string DiscriminatorValue;

        public string MigrationVersion;

        public string DisplayName;

        public string Subfolder;

        public bool IsDictionary;

        public bool Stateful;

        public bool Signable;

        public bool AcceptFiles;

        public List<FieldOptions> Fields = new List<FieldOptions>();

        public ControllerOptions Controller;

        public PermissionOptions Permission;

        public ViewOptions View;

        public NavigationOptions Navigation;

        public InterceptorOptions Interceptor;

        public DomainServiceOptions DomainService;

        public bool AuditLogMap;
    }
}
