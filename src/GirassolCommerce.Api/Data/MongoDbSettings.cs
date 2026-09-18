namespace GirassolCommerce.Api.Data;

public class MongoDbSettings
{
    public string ConnectionString { get; set; } = string.Empty;
    public string DatabaseName { get; set; } = "girassol-commerce";
}
