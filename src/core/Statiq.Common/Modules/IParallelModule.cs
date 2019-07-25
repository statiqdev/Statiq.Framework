namespace Statiq.Common
{
    /// <summary>
    /// A marker interface to apply to modules that
    /// inherit from <see cref="Module"/> to indicate
    /// the module can process documents in parallel
    /// (with an option to disable parallel processing).
    /// </summary>
    /// <remarks>
    /// Implementing this interface on <see cref="Module"/>
    /// will process documents in parallel by default unless
    /// <see cref="IParallelModuleExtensions.WithSequentialExecution{TModule}(TModule)"/>
    /// is called in the module constructor.
    /// </remarks>
    public interface IParallelModule
    {
    }
}
