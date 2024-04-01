using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;

namespace CSharp.Net.Util.CsHttp
{
    internal abstract class HttpMessageHandlerBuilder
    {
        //
        // 摘要:
        //     Gets or sets the name of the System.Net.Http.HttpClient being created.
        //
        // 言论：
        //     The Microsoft.Extensions.Http.HttpMessageHandlerBuilder.Name is set by the System.Net.Http.IHttpClientFactory
        //     infrastructure and is public for unit testing purposes only. Setting the Microsoft.Extensions.Http.HttpMessageHandlerBuilder.Name
        //     outside of testing scenarios may have unpredictable results.
        public abstract string Name { get; set; }

        //
        // 摘要:
        //     Gets or sets the primary System.Net.Http.HttpMessageHandler.
        public abstract HttpMessageHandler PrimaryHandler { get; set; }

        //
        // 摘要:
        //     Gets a list of additional System.Net.Http.DelegatingHandler instances used to
        //     configure an System.Net.Http.HttpClient pipeline.
        public abstract IList<DelegatingHandler> AdditionalHandlers { get; }

        //
        // 摘要:
        //     Gets an System.IServiceProvider which can be used to resolve services from the
        //     dependency injection container.
        //
        // 言论：
        //     This property is sensitive to the value of Microsoft.Extensions.Http.HttpClientFactoryOptions.SuppressHandlerScope.
        //     If true this property will be a reference to the application's root service provider.
        //     If false (default) this will be a reference to a scoped service provider that
        //     has the same lifetime as the handler being created.
        public virtual IServiceProvider Services { get; }

        //
        // 摘要:
        //     Creates an System.Net.Http.HttpMessageHandler.
        //
        // 返回结果:
        //     An System.Net.Http.HttpMessageHandler built from the Microsoft.Extensions.Http.HttpMessageHandlerBuilder.PrimaryHandler
        //     and Microsoft.Extensions.Http.HttpMessageHandlerBuilder.AdditionalHandlers.
        public abstract HttpMessageHandler Build();

        protected internal static HttpMessageHandler CreateHandlerPipeline(HttpMessageHandler primaryHandler, IEnumerable<DelegatingHandler> additionalHandlers)
        {
            if (primaryHandler == null)
            {
                throw new ArgumentNullException("primaryHandler");
            }

            if (additionalHandlers == null)
            {
                throw new ArgumentNullException("additionalHandlers");
            }

            IReadOnlyList<DelegatingHandler> readOnlyList = (additionalHandlers as IReadOnlyList<DelegatingHandler>) ?? additionalHandlers.ToArray();
            HttpMessageHandler httpMessageHandler = primaryHandler;
            for (int num = readOnlyList.Count - 1; num >= 0; num--)
            {
                DelegatingHandler delegatingHandler = readOnlyList[num];
                if (delegatingHandler == null)
                {
                    string message = "HttpMessageHandlerBuilder_AdditionalHandlerIsNull";
                    throw new InvalidOperationException(message);
                }

                if (delegatingHandler.InnerHandler != null)
                {
                    string message2 = "HttpMessageHandlerBuilder_AdditionHandlerIsInvalid";
                    throw new InvalidOperationException(message2);
                }

                delegatingHandler.InnerHandler = httpMessageHandler;
                httpMessageHandler = delegatingHandler;
            }

            return httpMessageHandler;
        }
    }
}
