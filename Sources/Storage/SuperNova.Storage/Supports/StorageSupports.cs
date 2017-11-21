


namespace SuperNova.Storage.Supports
{
    public static class StorageSupports
    {
        public static void OptimizeTableStorageAccess()
        {
            // TODO: https://stackoverflow.com/questions/39560249/servicepointmanager-in-asp-net-core

            /*
            ServicePointManager.DefaultConnectionLimit = 200;
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.UseNagleAlgorithm = false;
            ServicePointManager
                .FindServicePoint(CloudStorageAccount
                    .Parse(Configs.StorageConfiguration.BusinessAccount).TableEndpoint)
                        .UseNagleAlgorithm = false; AppInsights.Initialize(AppInsights.Tags.EventListener);
            ServicePointManager
                .FindServicePoint(CloudStorageAccount
                    .Parse(Configs.StorageConfiguration.LogAccount).TableEndpoint)
                        .UseNagleAlgorithm = false; AppInsights.Initialize(AppInsights.Tags.EventListener);
            */
        }
    }
}
