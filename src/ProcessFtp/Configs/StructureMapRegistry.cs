
namespace ProcessFtp.Configs
{
 
    using StructureMap.Configuration.DSL;

    public class StructureMapRegistry : Registry
    {
        public StructureMapRegistry()
        {
            ProcEnvironment.SetUp(this);
            Scan(z =>
            {
                z.TheCallingAssembly();
                z.WithDefaultConventions();
            });
        }

 
    }
}