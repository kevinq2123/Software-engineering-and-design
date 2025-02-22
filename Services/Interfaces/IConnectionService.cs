namespace RochaBlogs.Services.Interfaces
{
    public interface IConnectionService
    {
        string GetConnectionString(IConfiguration configuration);
    }
}
