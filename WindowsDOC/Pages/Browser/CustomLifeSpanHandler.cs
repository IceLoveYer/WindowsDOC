using CefSharp;
using System;

namespace WindowsDOC.Pages.Browser
{
    // 定义新窗口弹出方式，拦截弹出窗口的请求
    public class CustomLifeSpanHandler : ILifeSpanHandler
    {
        public event Action<string>? PopupRequested;

        // 实现 ILifeSpanHandler 接口中的 DoClose 方法
        public bool DoClose(IWebBrowser chromiumWebBrowser, IBrowser browser)
        {
            // 在这里添加关闭浏览器窗口前的逻辑
            // 如果返回 true，则阻止窗口关闭
            return false; // 通常返回 false
        }

        // 实现 ILifeSpanHandler 接口中的 OnAfterCreated 方法
        public void OnAfterCreated(IWebBrowser chromiumWebBrowser, IBrowser browser)
        {
            // 在这里添加浏览器窗口创建后的逻辑
        }

        // 实现 ILifeSpanHandler 接口中的 OnBeforeClose 方法
        public void OnBeforeClose(IWebBrowser chromiumWebBrowser, IBrowser browser)
        {
            // 在这里添加浏览器窗口关闭前的逻辑
        }

        // 实现 ILifeSpanHandler 接口中的 OnBeforePopup 方法
        public bool OnBeforePopup(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, string targetUrl, string targetFrameName, WindowOpenDisposition targetDisposition, bool userGesture, IPopupFeatures popupFeatures, IWindowInfo windowInfo, IBrowserSettings browserSettings, ref bool noJavascriptAccess, out IWebBrowser? newBrowser)
        {
            PopupRequested?.Invoke(targetUrl);
            newBrowser = null;
            return true; // 通过返回 true，取消默认的弹出窗口行为
        }
    }
}
