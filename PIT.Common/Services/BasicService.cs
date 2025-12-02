namespace PIT.Common.Services
{
    public class BasicService<TContext>
    {
        public BasicService(TContext context)
        {
            DBContext = context;
        }
        /// <summary>
        /// Контекст БД подключенный через EntityFramework
        /// Системная база BPMEngine
        /// </summary>
        protected readonly TContext DBContext;
    }
}
