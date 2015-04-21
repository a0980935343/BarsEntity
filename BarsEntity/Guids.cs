using System;

namespace Barsix.BarsEntity
{
    static class GuidList
    {
        public const string guidAddEntityPkgString = "27dd9dea-6dd2-403e-929d-3ff20d896c5e";
        public const string guidAddEntityCmdSetString = "32af8a17-bbbc-4c56-877e-fc6c6575a8cf";
        public const string guidAddMigrationCmdSetString = "32af8a17-bbbc-4c56-877e-fc6c6575a8ce";

        public static readonly Guid guidAddEntityCmdSet = new Guid(guidAddEntityCmdSetString);
        public static readonly Guid guidAddMigrationCmdSet = new Guid(guidAddMigrationCmdSetString);
    }

    static class PkgCmdIDList
    {
        public const uint cmdidAddEntity = 0x100;
        public const uint cmdidAddMigration = 0x104;
    }
}