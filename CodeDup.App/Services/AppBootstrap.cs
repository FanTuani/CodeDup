using System.IO;
using CodeDup.Core.Storage;

namespace CodeDup.App.Services;

public static class AppBootstrap {
    public static IProjectStore CreateStore() {
        var root = Path.Combine(AppContext.BaseDirectory, "DataProjects");
        return new FileProjectStore(root);
    }
}