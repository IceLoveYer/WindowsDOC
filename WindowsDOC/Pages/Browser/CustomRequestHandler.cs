using CefSharp;
using CefSharp.Handler;

namespace WindowsDOC.Pages.Browser
{
    // 定义请求处理
    public class CustomRequestHandler : RequestHandler
    {
        // 使其成为静态变量，以便所有实例共享同一个UA
        private static string customUserAgent = string.Empty;

        // 使其成为静态方法，以便可以不依赖实例来调用
        public static void SetCustomUserAgent(string userAgent)
        {
            customUserAgent = userAgent;
        }

        protected override IResourceRequestHandler GetResourceRequestHandler(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request, bool isNavigation, bool isDownload, string requestInitiator, ref bool disableDefaultHandling)
        {
            return new CustomResourceRequestHandler(customUserAgent);
        }
    }
}
