namespace MultiTenancy.Interface
{
    public interface IMustHaveTenant
    {
        public string TenantId { get; set; }
    }
}
