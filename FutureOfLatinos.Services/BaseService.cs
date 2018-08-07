using FutureOfLatinos.Data;
using FutureOfLatinos.Data.Providers;

namespace FutureOfLatinos.Services
{
    public abstract class BaseService
    {
        public IDataProvider DataProvider { get; set; }

        public BaseService(IDataProvider dataProvider)
        {
            this.DataProvider = dataProvider;
        }

        public BaseService()
        {
            string connStr = System.Configuration.ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            this.DataProvider = new SqlDataProvider(connStr);
        }
    }
}
