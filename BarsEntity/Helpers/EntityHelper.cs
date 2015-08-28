namespace Barsix.BarsEntity
{
    public static class EntityHelper
    {
        public static string TableNameByEntityName(string entityName, string @namespace = "")
        {
            var tableName = entityName.CamelToSnake();

            if (!string.IsNullOrWhiteSpace(@namespace) && !entityName.EndsWith("View"))
            {
                var parts = @namespace.Split('.');
                tableName = (parts.Length > 1 ? parts[1].ToUpper() + "_" : "") + tableName;
            }

            return tableName;
        }
    }
}
