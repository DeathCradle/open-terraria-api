using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Xilium.CefGlue;
using Xilium.CefGlue.Common.Handlers;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Runtime.InteropServices;
using ImGuiNET;

//ChromiumWebBrowser browser;
Texture2D currentFrame = null;

void OnInit(On.Terraria.Main.orig_Initialize orig, Terraria.Main self)
{
    orig(self);
    OnStart();
}
void OnUpdate(On.Terraria.Main.orig_Update orig, Terraria.Main self, GameTime gametime)
{
    orig(self, gametime);
    try
    {

        if (CefRuntime.IsInitialized) // && CefRuntime.Platform != CefRuntimePlatform.Windows)
        {
            CefRuntime.DoMessageLoopWork();
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine("CEF.OnUpdate", ex);
    }
}
// void OnDraw(On.Terraria.Main.orig_Draw orig, Terraria.Main self, Microsoft.Xna.Framework.GameTime gameTime)
// {
//     orig(self, gameTime);

//     if (currentFrame is not null)
//     {
//         Terraria.Main.spriteBatch.Begin();

//         Terraria.Main.spriteBatch.Draw(currentFrame, new Vector2(0, 0), new Microsoft.Xna.Framework.Rectangle(0, 0, Terraria.Main.screenWidth / 2, Terraria.Main.screenHeight / 2), Color.White * 0.9f);

//         Terraria.Main.spriteBatch.End();
//     }
// }

bool active = true;
bool enabled = false;
void OnGUI(On.Terraria.Main.orig_OnExtGUI orig)
{
    orig();

    if (currentFrame is not null)
    {
        Terraria.Main.spriteBatch.Begin();
        Terraria.Main.spriteBatch.Draw(currentFrame, new Vector2(0, 0), new Microsoft.Xna.Framework.Rectangle(0, 0, currentFrame.Width, currentFrame.Height), Microsoft.Xna.Framework.Color.White * 0.9f);
        Terraria.Main.spriteBatch.End();
    }

    try
    {
        if (active)
        {
            ImGui.Begin("Cef Browser", ref active);
            ImGui.Checkbox("Enable browser", ref enabled);
            ImGui.End();

            if (!active || !enabled)
            {
                offscreenBrowser?.Dispose();
                offscreenBrowser = null;
                currentFrame = null;
            }
            if (enabled && offscreenBrowser is null)
            {
                if (ready)
                {
                    //browser = new ChromiumWebBrowser("https://google.com.au");
                    //browser.Paint += Browser_Paint;
                    CreateBrowser();
                }
            }
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine("CEF.OnGUI", ex);
    }
}

bool ready = false;
void OnStart()
{
    System.Console.WriteLine("Starting CefSharp");
    //System.Diagnistics.Debug.WriteLine("Starting CefSharpx");
    ready = true;
}

//void OnStop(On.Terraria.Program.orig_ShutdownOTAPI orig)
//{
//    System.Console.WriteLine("Shutting down cef");
//    //Cef.Shutdown();
//    orig();
//}

//On.Terraria.Program.ShutdownOTAPI += OnStop;
On.Terraria.Main.Initialize += OnInit;
On.Terraria.Main.Update += OnUpdate;
// On.Terraria.Main.Draw += OnDraw;
On.Terraria.Main.OnExtGUI += OnGUI;
if (Terraria.Main.instance is not null)
    OnStart();

Dispose = () =>
{
    On.Terraria.Main.Update -= OnUpdate;
    On.Terraria.Main.OnExtGUI -= OnGUI;
    On.Terraria.Main.Initialize -= OnInit;
    // On.Terraria.Main.Draw -= OnDraw;
    //On.Terraria.Program.ShutdownOTAPI -= OnStop;
    //browser?.Dispose();
    //browser = null;
    currentFrame = null;

    offscreenBrowser?.Dispose();
    offscreenBrowser = null;
    renderTarget?.Dispose();
    renderTarget = null;
};

Texture2DRenderTarget renderTarget = null;
OffscreenBrowser offscreenBrowser = null;

void CreateBrowser()
{
    currentFrame = new Texture2D(Terraria.Main.graphics.GraphicsDevice, Terraria.Main.screenWidth / 2, Terraria.Main.screenHeight / 2);
    renderTarget = new Texture2DRenderTarget(currentFrame);
    offscreenBrowser = new OffscreenBrowser(renderTarget, "https://google.com.au/");

    //while (browser?.Browser?.IsLoading != false)
    //{
    //    System.Threading.Thread.Sleep(100);
    //}

    //browser.Dispose();

    //bitmap.Save("test.png", ImageFormat.Png);
}

public class OffscreenApp : CefApp
{
    public BrowserProcessHandler? BrowserProcessHandler { get; set; } = new BackgroundBrowserProcessHandler();

    public CefSettings Settings { get; private set; }

    public OffscreenApp(CefSettings settings)
    {
        Settings = settings;
    }

    protected override void OnBeforeCommandLineProcessing(string processType, CefCommandLine commandLine)
    {
        if (Settings.WindowlessRenderingEnabled)
        {
            commandLine.AppendSwitch("disable-gpu", "1");
            commandLine.AppendSwitch("disable-gpu-compositing", "1");
            commandLine.AppendSwitch("enable-begin-frame-scheduling", "1");
            commandLine.AppendSwitch("disable-smooth-scrolling", "1");
        }
        //base.OnBeforeCommandLineProcessing(processType, commandLine);
    }

    protected override CefBrowserProcessHandler GetBrowserProcessHandler()
    {
        if (BrowserProcessHandler is null) throw new NullReferenceException(nameof(BrowserProcessHandler));
        return BrowserProcessHandler;
        //return base.GetBrowserProcessHandler();
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (disposing)
        {
            if (BrowserProcessHandler is IDisposable disposable)
            {
                disposable.Dispose();
            }
            BrowserProcessHandler = null;
        }
    }
}

public interface IOffscreenRenderTarget : IDisposable
{
    int Width { get; }
    int Height { get; }

    void Render(IntPtr buffer);
}

public abstract class OffscreenRenderTarget : IOffscreenRenderTarget
{
    public int Width { get; private set; }

    public int Height { get; private set; }

    public OffscreenRenderTarget(int width, int height)
    {
        Width = width;
        Height = height;
    }

    public abstract void Render(IntPtr buffer);
    public abstract void Dispose();
}

// windows only...gdi
//public class BitmapRenderTarget : OffscreenRenderTarget
//{
//    public Bitmap Bitmap { get; set; }

//    public BitmapRenderTarget(Bitmap bitmap) : base((int)bitmap.Size.Width, (int)bitmap.Size.Height)
//    {
//        Bitmap = bitmap;
//    }

//    //public int Width { get; set; }
//    //public int Height { get; set; }

//    public BitmapRenderTarget(int width, int height) : base(width, height)
//    {
//        Bitmap = new Bitmap(width, height);
//    }

//    public override void Render(IntPtr buffer)
//    {
//        var data = Bitmap.LockBits(new System.Drawing.Rectangle(0, 0, Bitmap.Width, Bitmap.Height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
//        var length = data.Stride * data.Height;
//        var temporaryBuffer = new byte[length];

//        Marshal.Copy(buffer, temporaryBuffer, 0, length);
//        Marshal.Copy(temporaryBuffer, 0, data.Scan0, length);

//        Bitmap.UnlockBits(data);
//    }

//    private bool disposedValue;
//    protected void Dispose(bool disposing)
//    {
//        if (!disposedValue)
//        {
//            if (disposing)
//            {
//                // TODO: dispose managed state (managed objects)
//                Bitmap?.Dispose();
//            }

//            // TODO: free unmanaged resources (unmanaged objects) and override finalizer
//            // TODO: set large fields to null
//            disposedValue = true;
//        }
//    }

//    // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
//    // ~BitmapRenderTarget()
//    // {
//    //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
//    //     Dispose(disposing: false);
//    // }

//    public override void Dispose()
//    {
//        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
//        Dispose(disposing: true);
//        GC.SuppressFinalize(this);
//    }
//}

public class Texture2DRenderTarget : OffscreenRenderTarget
{
    private Texture2D _target;

    public Texture2DRenderTarget(Texture2D target) : base(target.Width, target.Height)
    {
        _target = target;
    }

    public override void Render(IntPtr buffer)
    {
        _target.SetDataPointerEXT(
            0,
            new Microsoft.Xna.Framework.Rectangle(0, 0, _target.Width, _target.Height),
            buffer,
            _target.Width * _target.Height
        );
        //var length = _target.Width * _target.Height;
        //int[] imgData = new int[length];
        //var temporaryBuffer = new int[length];

        //Marshal.Copy(buffer, temporaryBuffer, 0, length);

        _target.SetData(temporaryBuffer);
    }

    private bool disposedValue;
    protected void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                // TODO: dispose managed state (managed objects)
                _target?.Dispose();
            }

            // TODO: free unmanaged resources (unmanaged objects) and override finalizer
            // TODO: set large fields to null
            disposedValue = true;
        }
    }

    // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
    // ~Texture2DRenderTarget()
    // {
    //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
    //     Dispose(disposing: false);
    // }

    public override void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}

public class OffscreenLifeSpanHandlerCreatedEventArgs : EventArgs
{
    public CefBrowser Browser { get; set; }
}

public class OffscreenLifeSpanHandler : CefLifeSpanHandler
{

    public event EventHandler<OffscreenLifeSpanHandlerCreatedEventArgs> Created;

    protected override void OnAfterCreated(CefBrowser browser)
    {
        base.OnAfterCreated(browser);
        Created?.Invoke(this, new OffscreenLifeSpanHandlerCreatedEventArgs() { Browser = browser });
    }
}

public class BackgroundBrowserProcessHandler : BrowserProcessHandler
{
    private IDisposable? _current;
    private object _schedule = new object();

    protected override void OnScheduleMessagePumpWork(long delayMs)
    {

    }
}

public class OffscreenBrowser : IDisposable
{
    public int Width => RenderTarget.Width;
    public int Height => RenderTarget.Height;
    public IOffscreenRenderTarget RenderTarget { get; private set; }

    public CefBrowser? Browser { get; private set; }

    private OffscreenClient Client { get; set; }

    //public bool IsLoading { get; set; } = true;

    public OffscreenBrowser(IOffscreenRenderTarget renderTarget, string url)
    {
        RenderTarget = renderTarget;

        var wi = CefWindowInfo.Create();
        wi.SetAsChild(IntPtr.Zero, new CefRectangle(0, 0, renderTarget.Width, renderTarget.Height));
        wi.SetAsWindowless(IntPtr.Zero, true);
        Client = new OffscreenClient(this);
        var bsettings = new CefBrowserSettings();

        Client.LifeSpanHandler.Created += LifeSpanHandler_Created;

        //if (!CefRuntime.IsInitialized)
        //{
        //    var cachePath = Path.Combine(Environment.CurrentDirectory, "cef-cache");
        //    if (!Directory.Exists(cachePath))
        //        Directory.CreateDirectory(cachePath);
        //    var cefsettings = new CefSettings()
        //    {
        //        WindowlessRenderingEnabled = true,
        //        LogSeverity = CefLogSeverity.Verbose,
        //        LogFile = "cef_debug.log",
        //        RootCachePath = cachePath,
        //        MultiThreadedMessageLoop = true,
        //        SingleProcess = false,
        //    };
        //    CefLoader.Load(cefsettings);
        //}

        CefLoader.Load();

        //Browser = CefBrowserHost.CreateBrowserSync(wi, client, bsettings, "https://google.com.au/");
        CefBrowserHost.CreateBrowser(wi, Client, bsettings, url);
    }

    private void LifeSpanHandler_Created(object? sender, OffscreenLifeSpanHandlerCreatedEventArgs e)
    {
        Browser = e.Browser;
    }

    private bool disposedValue;
    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                // TODO: dispose managed state (managed objects)

                Browser?.StopLoad();

                Browser?.GetHost()?.CloseBrowser(true);
                Browser?.GetHost()?.Dispose();
                Browser?.Dispose();
                RenderTarget = null;

                Client.LifeSpanHandler.Created -= LifeSpanHandler_Created;
                Client = null;
            }

            // TODO: free unmanaged resources (unmanaged objects) and override finalizer
            // TODO: set large fields to null
            disposedValue = true;
        }
    }

    // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
    // ~OffscreenBrowser()
    // {
    //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
    //     Dispose(disposing: false);
    // }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}

public class OffscreenLoadHandler : CefLoadHandler
{
    protected override void OnLoadingStateChange(CefBrowser browser, bool isLoading, bool canGoBack, bool canGoForward)
    {
        base.OnLoadingStateChange(browser, isLoading, canGoBack, canGoForward);
    }
}

public class OffscreenClient : CefClient
{
    public OffscreenRenderHandler RenderHandler { get; private set; }
    public OffscreenLifeSpanHandler LifeSpanHandler { get; private set; }
    public OffscreenLoadHandler LoadHandler { get; private set; }

    public OffscreenClient(OffscreenBrowser browser)
    {
        RenderHandler = new OffscreenRenderHandler(browser);
        LifeSpanHandler = new OffscreenLifeSpanHandler();
        LoadHandler = new OffscreenLoadHandler();
    }

    protected override CefRenderHandler GetRenderHandler() => RenderHandler;
    protected override CefLifeSpanHandler GetLifeSpanHandler() => LifeSpanHandler;
    protected override CefLoadHandler GetLoadHandler() => LoadHandler;
}

public class OffscreenAccessibilityHandler : CefAccessibilityHandler
{
    protected override void OnAccessibilityLocationChange(CefValue value)
    {

    }

    protected override void OnAccessibilityTreeChange(CefValue value)
    {

    }
}

public class OffscreenRenderHandler : CefRenderHandler
{
    private readonly OffscreenBrowser _browser;
    private readonly OffscreenAccessibilityHandler offscreenAccessibilityHandler;

    public OffscreenRenderHandler(OffscreenBrowser browser)
    {
        _browser = browser;
        offscreenAccessibilityHandler = new OffscreenAccessibilityHandler();
    }

    protected override CefAccessibilityHandler GetAccessibilityHandler() => offscreenAccessibilityHandler;

    protected override bool GetScreenInfo(CefBrowser browser, CefScreenInfo screenInfo) => false;

    protected override void GetViewRect(CefBrowser browser, out CefRectangle rect)
    {
        rect = new CefRectangle(0, 0, _browser.Width, _browser.Height);
    }

    protected override void OnAcceleratedPaint(CefBrowser browser, CefPaintElementType type, CefRectangle[] dirtyRects, IntPtr sharedHandle) { }

    protected override void OnImeCompositionRangeChanged(CefBrowser browser, CefRange selectedRange, CefRectangle[] characterBounds) { }

    protected override void OnPaint(CefBrowser browser, CefPaintElementType type, CefRectangle[] dirtyRects, IntPtr buffer, int width, int height)
    {
        _browser.RenderTarget?.Render(buffer);
    }

    protected override void OnPopupSize(CefBrowser browser, CefRectangle rect) { }

    protected override void OnScrollOffsetChanged(CefBrowser browser, double x, double y) { }

    protected override bool GetRootScreenRect(CefBrowser browser, ref CefRectangle rect)
    {
        GetViewRect(browser, out rect);
        return true;
    }

    protected override bool GetScreenPoint(CefBrowser browser, int viewX, int viewY, ref int screenX, ref int screenY)
    {
        screenX = viewX;
        screenY = viewY;
        return true;
    }
}

public static class CefLoader
{
    public static List<string> CefGlueBrowserProcessPaths { get; set; } = new List<string>()
    {
        "CefGlueBrowserProcess"
    };
    public static string CefGlueBrowserProcessFileName { get; set; } = CefRuntime.Platform switch
    {
        CefRuntimePlatform.Windows => "Xilium.CefGlue.BrowserProcess.exe",
        _ => "Xilium.CefGlue.BrowserProcess",
    };

    static string DiscoverSubProcess()
    {
        foreach (var path in CefGlueBrowserProcessPaths)
        {
            var subprocessPath = Path.Combine(path, CefGlueBrowserProcessFileName);
            if (File.Exists(subprocessPath))
                return Path.Combine(Environment.CurrentDirectory, subprocessPath);
        }

        throw new FileNotFoundException($"Unable to find \"{CefGlueBrowserProcessFileName}\"");
    }

    public static OffscreenApp? Instance { get; private set; }

    public static OffscreenApp Load(CefSettings? settings = null, OffscreenApp? app = null)
    {
        CefRuntime.Load();

        if (settings == null)
            settings = new CefSettings();

        settings.BrowserSubprocessPath = DiscoverSubProcess();
        settings.UncaughtExceptionStackSize = 10;

        settings.WindowlessRenderingEnabled = true;
        settings.LogSeverity = CefLogSeverity.Verbose;
        settings.LogFile = "cef_debug.log";

        var cachePath = Path.Combine(Environment.CurrentDirectory, "cef-cache");
        if (!Directory.Exists(cachePath))
            Directory.CreateDirectory(cachePath);
        settings.RootCachePath = cachePath;

        switch (CefRuntime.Platform)
        {
            case CefRuntimePlatform.Windows:
                settings.MultiThreadedMessageLoop = true;
                break;

            case CefRuntimePlatform.Linux:
                settings.ExternalMessagePump = true;
                settings.NoSandbox = true;
                settings.MultiThreadedMessageLoop = false;

                settings.MainBundlePath = Environment.CurrentDirectory;
                settings.FrameworkDirPath = Environment.CurrentDirectory;
                settings.ResourcesDirPath = Environment.CurrentDirectory;
                break;

            case CefRuntimePlatform.MacOS:
                settings.ExternalMessagePump = true;
                settings.NoSandbox = true;
                settings.MultiThreadedMessageLoop = false;

                settings.FrameworkDirPath = Environment.CurrentDirectory;

                var resourcesPath = Path.Combine(Environment.CurrentDirectory, "Resources");
                if (!Directory.Exists(resourcesPath))
                    throw new DirectoryNotFoundException($"Unable to find Resources folder");

                settings.ResourcesDirPath = resourcesPath;
                settings.MainBundlePath = resourcesPath;

                var libswiftshader_libGLESv2 = "libswiftshader_libGLESv2.dylib";
                var dst_libswiftshader_libGLESv2 = Path.Combine(Path.GetDirectoryName(settings.BrowserSubprocessPath), libswiftshader_libGLESv2);
                if (!File.Exists(dst_libswiftshader_libGLESv2)) File.Copy(libswiftshader_libGLESv2, dst_libswiftshader_libGLESv2);

                var libswiftshader_libEGL = "libswiftshader_libEGL.dylib";
                var dst_libswiftshader_libEGL = Path.Combine(Path.GetDirectoryName(settings.BrowserSubprocessPath), libswiftshader_libEGL);
                if (!File.Exists(dst_libswiftshader_libEGL)) File.Copy(libswiftshader_libEGL, dst_libswiftshader_libEGL);
                break;
        }

        AppDomain.CurrentDomain.ProcessExit += delegate { CefRuntime.Shutdown(); };

        if (app == null)
            app = new OffscreenApp(settings);

        var args = new CefMainArgs(new string[] { });
        CefRuntime.Initialize(args, settings, app, IntPtr.Zero);

        Instance = app;

        return app;
    }
}