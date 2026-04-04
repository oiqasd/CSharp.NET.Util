namespace CSharp.Net.Mvc.Filters
{
    public class PermissionIdAttribute : Attribute
    {
        public string[] PermissionIds { get; set; }
        public PermissionIdAttribute(params string[] permissionId)
        {
            PermissionIds = permissionId;
        }
    }
}
