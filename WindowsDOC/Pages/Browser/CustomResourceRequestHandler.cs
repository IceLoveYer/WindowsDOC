using CefSharp;
using CefSharp.Handler;

namespace WindowsDOC.Pages.Browser
{
    // 定义资源请求处理
    public class CustomResourceRequestHandler : ResourceRequestHandler
    {
        private readonly string customUserAgent;

        public CustomResourceRequestHandler(string customUserAgent)
        {
            this.customUserAgent = customUserAgent;
        }

        protected override CefReturnValue OnBeforeResourceLoad(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request, IRequestCallback callback)
        {
            if (!string.IsNullOrEmpty(customUserAgent))
            {
                request.SetHeaderByName("User-Agent", customUserAgent, overwrite: true);
            }

            return CefReturnValue.Continue;
        }
    }
}
