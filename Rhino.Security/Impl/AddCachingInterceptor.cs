using Castle.Core.Interceptor;
using Rhino.Commons;

namespace Rhino.Security.Impl
{
	/// <summary>
	/// Add caching to all the called methods
	/// </summary>
	public class AddCachingInterceptor : IInterceptor
	{
		/// <summary>
		/// Intercepts the specified invocation.
		/// </summary>
		/// <param name="invocation">The invocation.</param>
		public void Intercept(IInvocation invocation)
		{
			using (With.QueryCache())
			{
				invocation.Proceed();
			}
		}
	}
}