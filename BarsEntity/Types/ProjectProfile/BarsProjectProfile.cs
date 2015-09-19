namespace Barsix.BarsEntity
{
    public abstract class ProjectProfileBase
    {
        public abstract string Name { get; }
        public abstract string IconPath { get; }
        public virtual ViewType ViewType { get { return ViewType.ViewModel; } }
        public virtual string HrefPrefix { get { return ""; } }
    }
}
