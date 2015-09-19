using System.Collections.Generic;
using System.Xml.Serialization;

namespace Barsix.BarsEntity.BarsOptions
{
    public class EntityOptions
    {
        private ProjectProfileBase _profile;

        public EntityOptions()
        {
            _profile = new EmptyProfile();
        }

        public EntityOptions(ProjectProfileBase profile)
        {
            _profile = profile;
        }

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

        [XmlIgnore]
        public ProjectProfileBase Profile;
    }
}
