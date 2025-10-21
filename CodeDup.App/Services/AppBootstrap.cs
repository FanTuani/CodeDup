using CodeDup.Core.Storage;

namespace CodeDup.App.Services
{
    public static class AppBootstrap
    {
        public static IProjectStore CreateStore()
        {
            var root = System.IO.Path.Combine(System.AppContext.BaseDirectory, "DataProjects");
            return new FileProjectStore(root);
        }
    }
}


