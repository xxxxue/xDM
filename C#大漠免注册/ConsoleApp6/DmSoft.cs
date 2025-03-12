using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Security;
using System.Runtime.CompilerServices;
using System.IO;
using System.ComponentModel;



/// <summary>
/// 免注册大漠插件类
/// https://blog.csdn.net/black_bad1993/article/details/53906335
/// </summary>
public class DmSoft : IDisposable
{
    bool _disposed = false;
    URCOMLoader _urCom;
    Idmsoft _dm;
    string _dmPath;

    /// <summary>
    /// 创建免注册表大漠实例
    /// </summary>
    /// <param name="dmpath"></param>
    public DmSoft(string dmpath)
    {
        _dmPath = dmpath;
        _urCom = new URCOMLoader();
        //这里的guid用oleview查看coclass 父节点 uuid(26037A0E-7CBD-4FFF-9C63-56F2D0770214)
        _dm = _urCom.CreateObjectFromPath(dmpath, Guid.Parse("26037A0E-7CBD-4FFF-9C63-56F2D0770214"), false) as Idmsoft;
    }

    public Idmsoft DM
    {
        get { return _dm; }
    }

    /// <summary>
    /// 大漠插件地址
    /// </summary>
    public string DmPath
    {
        get { return _dmPath; }
    }

    /// <summary>
    /// 获取大漠插件版本
    /// </summary>
    public string VER
    {
        get { return _dm.Ver(); }
    }

    #region IDisposable Members

    protected void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
            }

            _urCom.Dispose();

            _disposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    #endregion
}

#region 免注册调用COM类

//OLE Automation : {00020430-0000-0000-C000-000000000046} 大漠查看最外层节点。
[ComVisible(true), ComImport(),
 Guid("00000001-0000-0000-C000-000000000046"),
 InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
public interface IClassFactory
{
    [return: MarshalAs(UnmanagedType.I4)]
    [PreserveSig]
    int CreateInstance(
        [In, MarshalAs(UnmanagedType.IUnknown)]
            object pUnkOuter,
        [MarshalAs(UnmanagedType.LPStruct)] Guid riid,
        [Out, MarshalAs(UnmanagedType.Interface)]
            out object obj);

    [return: MarshalAs(UnmanagedType.I4)]
    [PreserveSig]
    int LockServer(
        [MarshalAs(UnmanagedType.Bool), In] bool fLock);
}


internal sealed class NativeMethods
{
    private NativeMethods()
    {
    }

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool SetDllDirectory(string lpPathName);

    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    public static extern IntPtr LoadLibrary(string lpFileName);

    [DllImport("Kernel32.dll", SetLastError = true)]
    public static extern Int32 GetLastError();

    [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
    public static extern bool FreeLibrary(IntPtr hModule);

    [DllImport("kernel32", CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true)]
    public static extern IntPtr GetProcAddress(IntPtr hModule, string procName);
}

/// <summary>
/// 免注册调用COM类
/// </summary>
public class URCOMLoader : IDisposable
{
    delegate int DllGETCLASSOBJECTInvoker([MarshalAs(UnmanagedType.LPStruct)] Guid clsid, [MarshalAs(UnmanagedType.LPStruct)] Guid iid, [MarshalAs(UnmanagedType.IUnknown)] out object ppv);

    //OLE Automation : {00020430-0000-0000-C000-000000000046} oleview查看
    static Guid IID_IUnknown = new Guid("00000000-0000-0000-C000-000000000046");

    bool _disposed = false;
    IntPtr _lib = IntPtr.Zero;
    bool _preferURObjects = true;

    public string SearchPath { get; private set; }

    public URCOMLoader()
    {
        //this should be called on app load, but this will make sure it gets done.
        SearchPath = "";
        _preferURObjects = true;
    }

    public object CreateObjectFromPath(string dllPath, Guid clsid, bool comFallback)
    {
        return CreateObjectFromPath(dllPath, clsid, false, comFallback);
    }

    public object CreateObjectFromPath(string dllPath, Guid clsid, bool setSearchPath, bool comFallback)
    {
        object createdObject = null;
        //IntPtr lib = IntPtr.Zero;
        string fullDllPath = Path.Combine(SearchPath, dllPath);

        if (File.Exists(fullDllPath) && (_preferURObjects || !comFallback))
        {
            if (setSearchPath)
            {
                NativeMethods.SetDllDirectory(Path.GetDirectoryName(fullDllPath));
            }

            _lib = NativeMethods.LoadLibrary(fullDllPath);

            if (setSearchPath)
            {
                NativeMethods.SetDllDirectory(null);
            }

            if (_lib != IntPtr.Zero)
            {
                //we need to cache the handle so the COM object will work and we can clean up later

                IntPtr fnP = NativeMethods.GetProcAddress(_lib, "DllGetClassObject");
                if (fnP != IntPtr.Zero)
                {
                    DllGETCLASSOBJECTInvoker fn = Marshal.GetDelegateForFunctionPointer(fnP, typeof(DllGETCLASSOBJECTInvoker)) as DllGETCLASSOBJECTInvoker;

                    object pUnk = null;
                    int hr = fn(clsid, IID_IUnknown, out pUnk);
                    if (hr >= 0)
                    {
                        IClassFactory pCF = pUnk as IClassFactory;
                        if (pCF != null)
                        {
                            hr = pCF.CreateInstance(null, IID_IUnknown, out createdObject);
                        }
                    }
                }
                else
                {
                    throw new Win32Exception();
                }
            }
            else if (comFallback)
            {
                Type type = Type.GetTypeFromCLSID(clsid);
                return Activator.CreateInstance(type);
            }
            else
            {
                throw new Win32Exception();
            }
        }
        else if (comFallback)
        {
            Type type = Type.GetTypeFromCLSID(clsid);
            return Activator.CreateInstance(type);
        }

        return createdObject;
    }


    #region IDisposable Members

    protected void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
            }

            NativeMethods.FreeLibrary(_lib);

            _disposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    #endregion
}

#endregion

#region 大漠插件接口

//这里的guid用oleview查看coclass子节点  是uuid(F3F54BC2-D6D1-4A85-B943-16287ECEA64C),
[Guid("F3F54BC2-D6D1-4A85-B943-16287ECEA64C"), TypeLibType(4160)]
[ComImport]
public interface Idmsoft
{
    [DispId(1)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    [return: MarshalAs(UnmanagedType.BStr)]
    string Ver();

    [DispId(2)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    int SetPath([MarshalAs(UnmanagedType.BStr)][In] string path);

    [DispId(3)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    [return: MarshalAs(UnmanagedType.BStr)]
    string Ocr([In] int x1, [In] int y1, [In] int x2, [In] int y2, [MarshalAs(UnmanagedType.BStr)][In] string color, [In] double sim);

    [DispId(4)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    int FindStr([In] int x1, [In] int y1, [In] int x2, [In] int y2, [MarshalAs(UnmanagedType.BStr)][In] string str, [MarshalAs(UnmanagedType.BStr)][In] string color, [In] double sim,
        [MarshalAs(UnmanagedType.Struct)] out object x, [MarshalAs(UnmanagedType.Struct)] out object y);

    [DispId(5)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    int GetResultCount([MarshalAs(UnmanagedType.BStr)][In] string str);

    [DispId(6)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    int GetResultPos([MarshalAs(UnmanagedType.BStr)][In] string str, [In] int index, [MarshalAs(UnmanagedType.Struct)] out object x, [MarshalAs(UnmanagedType.Struct)] out object y);

    [DispId(7)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    int StrStr([MarshalAs(UnmanagedType.BStr)][In] string s, [MarshalAs(UnmanagedType.BStr)][In] string str);

    [DispId(8)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    int SendCommand([MarshalAs(UnmanagedType.BStr)][In] string cmd);

    [DispId(9)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    int UseDict([In] int index);

    [DispId(10)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    [return: MarshalAs(UnmanagedType.BStr)]
    string GetBasePath();

    [DispId(11)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    int SetDictPwd([MarshalAs(UnmanagedType.BStr)][In] string pwd);

    [DispId(12)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    [return: MarshalAs(UnmanagedType.BStr)]
    string OcrInFile([In] int x1, [In] int y1, [In] int x2, [In] int y2, [MarshalAs(UnmanagedType.BStr)][In] string pic_name, [MarshalAs(UnmanagedType.BStr)][In] string color, [In] double sim);

    [DispId(13)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    int Capture([In] int x1, [In] int y1, [In] int x2, [In] int y2, [MarshalAs(UnmanagedType.BStr)][In] string file);

    [DispId(14)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    int KeyPress([In] int vk);

    [DispId(15)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    int KeyDown([In] int vk);

    [DispId(16)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    int KeyUp([In] int vk);

    [DispId(17)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    int LeftClick();

    [DispId(18)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    int RightClick();

    [DispId(19)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    int MiddleClick();

    [DispId(20)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    int LeftDoubleClick();

    [DispId(21)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    int LeftDown();

    [DispId(22)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    int LeftUp();

    [DispId(23)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    int RightDown();

    [DispId(24)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    int RightUp();

    [DispId(25)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    int MoveTo([In] int x, [In] int y);

    [DispId(26)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    int MoveR([In] int rx, [In] int ry);

    [DispId(27)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    [return: MarshalAs(UnmanagedType.BStr)]
    string GetColor([In] int x, [In] int y);

    [DispId(28)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    [return: MarshalAs(UnmanagedType.BStr)]
    string GetColorBGR([In] int x, [In] int y);

    [DispId(29)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    [return: MarshalAs(UnmanagedType.BStr)]
    string RGB2BGR([MarshalAs(UnmanagedType.BStr)][In] string rgb_color);

    [DispId(30)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    [return: MarshalAs(UnmanagedType.BStr)]
    string BGR2RGB([MarshalAs(UnmanagedType.BStr)][In] string bgr_color);

    [DispId(31)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    int UnBindWindow();

    [DispId(32)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    int CmpColor([In] int x, [In] int y, [MarshalAs(UnmanagedType.BStr)][In] string color, [In] double sim);

    [DispId(33)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    int ClientToScreen([In] int hwnd, [MarshalAs(UnmanagedType.Struct)][In][Out] ref object x, [MarshalAs(UnmanagedType.Struct)][In][Out] ref object y);

    [DispId(34)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    int ScreenToClient([In] int hwnd, [MarshalAs(UnmanagedType.Struct)][In][Out] ref object x, [MarshalAs(UnmanagedType.Struct)][In][Out] ref object y);

    [DispId(35)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    int ShowScrMsg([In] int x1, [In] int y1, [In] int x2, [In] int y2, [MarshalAs(UnmanagedType.BStr)][In] string msg, [MarshalAs(UnmanagedType.BStr)][In] string color);

    [DispId(36)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    int SetMinRowGap([In] int row_gap);

    [DispId(37)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    int SetMinColGap([In] int col_gap);

    [DispId(38)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    int FindColor([In] int x1, [In] int y1, [In] int x2, [In] int y2, [MarshalAs(UnmanagedType.BStr)][In] string color, [In] double sim, [In] int dir,
        [MarshalAs(UnmanagedType.Struct)] out object x, [MarshalAs(UnmanagedType.Struct)] out object y);

    [DispId(39)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    [return: MarshalAs(UnmanagedType.BStr)]
    string FindColorEx([In] int x1, [In] int y1, [In] int x2, [In] int y2, [MarshalAs(UnmanagedType.BStr)][In] string color, [In] double sim, [In] int dir);

    [DispId(40)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    int SetWordLineHeight([In] int line_height);

    [DispId(41)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    int SetWordGap([In] int word_gap);

    [DispId(42)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    int SetRowGapNoDict([In] int row_gap);

    [DispId(43)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    int SetColGapNoDict([In] int col_gap);

    [DispId(44)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    int SetWordLineHeightNoDict([In] int line_height);

    [DispId(45)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    int SetWordGapNoDict([In] int word_gap);

    [DispId(46)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    int GetWordResultCount([MarshalAs(UnmanagedType.BStr)][In] string str);

    [DispId(47)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    int GetWordResultPos([MarshalAs(UnmanagedType.BStr)][In] string str, [In] int index, [MarshalAs(UnmanagedType.Struct)] out object x, [MarshalAs(UnmanagedType.Struct)] out object y);

    [DispId(48)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    [return: MarshalAs(UnmanagedType.BStr)]
    string GetWordResultStr([MarshalAs(UnmanagedType.BStr)][In] string str, [In] int index);

    [DispId(49)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    [return: MarshalAs(UnmanagedType.BStr)]
    string GetWords([In] int x1, [In] int y1, [In] int x2, [In] int y2, [MarshalAs(UnmanagedType.BStr)][In] string color, [In] double sim);

    [DispId(50)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    [return: MarshalAs(UnmanagedType.BStr)]
    string GetWordsNoDict([In] int x1, [In] int y1, [In] int x2, [In] int y2, [MarshalAs(UnmanagedType.BStr)][In] string color);

    [DispId(51)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    int SetShowErrorMsg([In] int show);

    [DispId(52)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    int GetClientSize([In] int hwnd, [MarshalAs(UnmanagedType.Struct)] out object width, [MarshalAs(UnmanagedType.Struct)] out object height);

    [DispId(53)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    int MoveWindow([In] int hwnd, [In] int x, [In] int y);

    [DispId(54)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    [return: MarshalAs(UnmanagedType.BStr)]
    string GetColorHSV([In] int x, [In] int y);

    [DispId(55)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    [return: MarshalAs(UnmanagedType.BStr)]
    string GetAveRGB([In] int x1, [In] int y1, [In] int x2, [In] int y2);

    [DispId(56)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    [return: MarshalAs(UnmanagedType.BStr)]
    string GetAveHSV([In] int x1, [In] int y1, [In] int x2, [In] int y2);

    [DispId(57)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    int GetForegroundWindow();

    [DispId(58)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    int GetForegroundFocus();

    [DispId(59)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    int GetMousePointWindow();

    [DispId(60)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    int GetPointWindow([In] int x, [In] int y);

    [DispId(61)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    [return: MarshalAs(UnmanagedType.BStr)]
    string EnumWindow([In] int parent, [MarshalAs(UnmanagedType.BStr)][In] string title, [MarshalAs(UnmanagedType.BStr)][In] string class_name, [In] int filter);

    [DispId(62)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    int GetWindowState([In] int hwnd, [In] int flag);

    [DispId(63)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    int GetWindow([In] int hwnd, [In] int flag);

    [DispId(64)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    int GetSpecialWindow([In] int flag);

    [DispId(65)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    int SetWindowText([In] int hwnd, [MarshalAs(UnmanagedType.BStr)][In] string text);

    [DispId(66)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    int SetWindowSize([In] int hwnd, [In] int width, [In] int height);

    [DispId(67)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    int GetWindowRect([In] int hwnd, [MarshalAs(UnmanagedType.Struct)] out object x1, [MarshalAs(UnmanagedType.Struct)] out object y1, [MarshalAs(UnmanagedType.Struct)] out object x2,
        [MarshalAs(UnmanagedType.Struct)] out object y2);

    [DispId(68)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    [return: MarshalAs(UnmanagedType.BStr)]
    string GetWindowTitle([In] int hwnd);

    [DispId(69)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    [return: MarshalAs(UnmanagedType.BStr)]
    string GetWindowClass([In] int hwnd);

    [DispId(70)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    int SetWindowState([In] int hwnd, [In] int flag);

    [DispId(71)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    int CreateFoobarRect([In] int hwnd, [In] int x, [In] int y, [In] int w, [In] int h);

    [DispId(72)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    int CreateFoobarRoundRect([In] int hwnd, [In] int x, [In] int y, [In] int w, [In] int h, [In] int rw, [In] int rh);

    [DispId(73)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    int CreateFoobarEllipse([In] int hwnd, [In] int x, [In] int y, [In] int w, [In] int h);

    [DispId(74)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    int CreateFoobarCustom([In] int hwnd, [In] int x, [In] int y, [MarshalAs(UnmanagedType.BStr)][In] string pic, [MarshalAs(UnmanagedType.BStr)][In] string trans_color, [In] double sim);

    [DispId(75)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    int FoobarFillRect([In] int hwnd, [In] int x1, [In] int y1, [In] int x2, [In] int y2, [MarshalAs(UnmanagedType.BStr)][In] string color);

    [DispId(76)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    int FoobarDrawText([In] int hwnd, [In] int x, [In] int y, [In] int w, [In] int h, [MarshalAs(UnmanagedType.BStr)][In] string text, [MarshalAs(UnmanagedType.BStr)][In] string color,
        [In] int align);

    [DispId(77)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    int FoobarDrawPic([In] int hwnd, [In] int x, [In] int y, [MarshalAs(UnmanagedType.BStr)][In] string pic, [MarshalAs(UnmanagedType.BStr)][In] string trans_color);

    [DispId(78)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    int FoobarUpdate([In] int hwnd);

    [DispId(79)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    int FoobarLock([In] int hwnd);

    [DispId(80)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    int FoobarUnlock([In] int hwnd);

    [DispId(81)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    int FoobarSetFont([In] int hwnd, [MarshalAs(UnmanagedType.BStr)][In] string font_name, [In] int size, [In] int flag);

    [DispId(82)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    int FoobarTextRect([In] int hwnd, [In] int x, [In] int y, [In] int w, [In] int h);

    [DispId(83)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    int FoobarPrintText([In] int hwnd, [MarshalAs(UnmanagedType.BStr)][In] string text, [MarshalAs(UnmanagedType.BStr)][In] string color);

    [DispId(84)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    int FoobarClearText([In] int hwnd);

    [DispId(85)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    int FoobarTextLineGap([In] int hwnd, [In] int gap);

    [DispId(86)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    int Play([MarshalAs(UnmanagedType.BStr)][In] string file);

    [DispId(87)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    int FaqCapture([In] int x1, [In] int y1, [In] int x2, [In] int y2, [In] int quality, [In] int delay, [In] int time);

    [DispId(88)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    int FaqRelease([In] int handle);

    [DispId(89)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    [return: MarshalAs(UnmanagedType.BStr)]
    string FaqSend([MarshalAs(UnmanagedType.BStr)][In] string server, [In] int handle, [In] int request_type, [In] int time_out);

    [DispId(90)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    int Beep([In] int fre, [In] int delay);

    [DispId(91)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    int FoobarClose([In] int hwnd);

    [DispId(92)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    int MoveDD([In] int dx, [In] int dy);

    [DispId(93)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    int FaqGetSize([In] int handle);

    [DispId(94)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    int LoadPic([MarshalAs(UnmanagedType.BStr)][In] string pic_name);

    [DispId(95)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    int FreePic([MarshalAs(UnmanagedType.BStr)][In] string pic_name);

    [DispId(96)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    int GetScreenData([In] int x1, [In] int y1, [In] int x2, [In] int y2);

    [DispId(97)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    int FreeScreenData([In] int handle);

    [DispId(98)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    int WheelUp();

    [DispId(99)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    int WheelDown();

    [DispId(100)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    int SetMouseDelay([MarshalAs(UnmanagedType.BStr)][In] string type, [In] int delay);

    [DispId(101)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    int SetKeypadDelay([MarshalAs(UnmanagedType.BStr)][In] string type, [In] int delay);

    [DispId(102)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    [return: MarshalAs(UnmanagedType.BStr)]
    string GetEnv([In] int index, [MarshalAs(UnmanagedType.BStr)][In] string name);

    [DispId(103)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    int SetEnv([In] int index, [MarshalAs(UnmanagedType.BStr)][In] string name, [MarshalAs(UnmanagedType.BStr)][In] string value);

    [DispId(104)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    int SendString([In] int hwnd, [MarshalAs(UnmanagedType.BStr)][In] string str);

    [DispId(105)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    int DelEnv([In] int index, [MarshalAs(UnmanagedType.BStr)][In] string name);

    [DispId(106)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    [return: MarshalAs(UnmanagedType.BStr)]
    string GetPath();

    [DispId(107)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    int SetDict([In] int index, [MarshalAs(UnmanagedType.BStr)][In] string dict_name);

    [DispId(108)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    int FindPic([In] int x1, [In] int y1, [In] int x2, [In] int y2, [MarshalAs(UnmanagedType.BStr)][In] string pic_name, [MarshalAs(UnmanagedType.BStr)][In] string delta_color, [In] double sim,
        [In] int dir, [MarshalAs(UnmanagedType.Struct)] out object x, [MarshalAs(UnmanagedType.Struct)] out object y);

    [DispId(109)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    [return: MarshalAs(UnmanagedType.BStr)]
    string FindPicEx([In] int x1, [In] int y1, [In] int x2, [In] int y2, [MarshalAs(UnmanagedType.BStr)][In] string pic_name, [MarshalAs(UnmanagedType.BStr)][In] string delta_color,
        [In] double sim, [In] int dir);

    [DispId(110)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    int SetClientSize([In] int hwnd, [In] int width, [In] int height);

    [DispId(111)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    int ReadInt([In] int hwnd, [MarshalAs(UnmanagedType.BStr)][In] string addr, [In] int type);

    [DispId(112)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    float ReadFloat([In] int hwnd, [MarshalAs(UnmanagedType.BStr)][In] string addr);

    [DispId(113)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    double ReadDouble([In] int hwnd, [MarshalAs(UnmanagedType.BStr)][In] string addr);

    [DispId(114)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    [return: MarshalAs(UnmanagedType.BStr)]
    string FindInt([In] int hwnd, [MarshalAs(UnmanagedType.BStr)][In] string addr_range, [In] int int_value_min, [In] int int_value_max, [In] int type);

    [DispId(115)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    [return: MarshalAs(UnmanagedType.BStr)]
    string FindFloat([In] int hwnd, [MarshalAs(UnmanagedType.BStr)][In] string addr_range, [In] float float_value_min, [In] float float_value_max);

    [DispId(116)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    [return: MarshalAs(UnmanagedType.BStr)]
    string FindDouble([In] int hwnd, [MarshalAs(UnmanagedType.BStr)][In] string addr_range, [In] double double_value_min, [In] double double_value_max);

    [DispId(117)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    [return: MarshalAs(UnmanagedType.BStr)]
    string FindString([In] int hwnd, [MarshalAs(UnmanagedType.BStr)][In] string addr_range, [MarshalAs(UnmanagedType.BStr)][In] string string_value, [In] int type);

    [DispId(118)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    int GetModuleBaseAddr([In] int hwnd, [MarshalAs(UnmanagedType.BStr)][In] string module_name);

    [DispId(119)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    [return: MarshalAs(UnmanagedType.BStr)]
    string MoveToEx([In] int x, [In] int y, [In] int w, [In] int h);

    [DispId(120)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    [return: MarshalAs(UnmanagedType.BStr)]
    string MatchPicName([MarshalAs(UnmanagedType.BStr)][In] string pic_name);

    [DispId(121)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    int AddDict([In] int index, [MarshalAs(UnmanagedType.BStr)][In] string dict_info);

    [DispId(122)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    int EnterCri();

    [DispId(123)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    int LeaveCri();

    [DispId(124)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    int WriteInt([In] int hwnd, [MarshalAs(UnmanagedType.BStr)][In] string addr, [In] int type, [In] int v);

    [DispId(125)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    int WriteFloat([In] int hwnd, [MarshalAs(UnmanagedType.BStr)][In] string addr, [In] float v);

    [DispId(126)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    int WriteDouble([In] int hwnd, [MarshalAs(UnmanagedType.BStr)][In] string addr, [In] double v);

    [DispId(127)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    int WriteString([In] int hwnd, [MarshalAs(UnmanagedType.BStr)][In] string addr, [In] int type, [MarshalAs(UnmanagedType.BStr)][In] string v);

    [DispId(128)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    int AsmAdd([MarshalAs(UnmanagedType.BStr)][In] string asm_ins);

    [DispId(129)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    int AsmClear();

    [DispId(130)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    int AsmCall([In] int hwnd, [In] int mode);

    [DispId(131)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    int FindMultiColor([In] int x1, [In] int y1, [In] int x2, [In] int y2, [MarshalAs(UnmanagedType.BStr)][In] string first_color, [MarshalAs(UnmanagedType.BStr)][In] string offset_color,
        [In] double sim, [In] int dir, [MarshalAs(UnmanagedType.Struct)] out object x, [MarshalAs(UnmanagedType.Struct)] out object y);

    [DispId(132)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    [return: MarshalAs(UnmanagedType.BStr)]
    string FindMultiColorEx([In] int x1, [In] int y1, [In] int x2, [In] int y2, [MarshalAs(UnmanagedType.BStr)][In] string first_color, [MarshalAs(UnmanagedType.BStr)][In] string offset_color,
        [In] double sim, [In] int dir);

    [DispId(133)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    [return: MarshalAs(UnmanagedType.BStr)]
    string AsmCode([In] int base_addr);

    [DispId(134)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    [return: MarshalAs(UnmanagedType.BStr)]
    string Assemble([MarshalAs(UnmanagedType.BStr)][In] string asm_code, [In] int base_addr, [In] int is_upper);

    [DispId(135)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    int SetWindowTransparent([In] int hwnd, [In] int v);

    [DispId(136)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    [return: MarshalAs(UnmanagedType.BStr)]
    string ReadData([In] int hwnd, [MarshalAs(UnmanagedType.BStr)][In] string addr, [In] int len);

    [DispId(137)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    int WriteData([In] int hwnd, [MarshalAs(UnmanagedType.BStr)][In] string addr, [MarshalAs(UnmanagedType.BStr)][In] string data);

    [DispId(138)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    [return: MarshalAs(UnmanagedType.BStr)]
    string FindData([In] int hwnd, [MarshalAs(UnmanagedType.BStr)][In] string addr_range, [MarshalAs(UnmanagedType.BStr)][In] string data);

    [DispId(139)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    int SetPicPwd([MarshalAs(UnmanagedType.BStr)][In] string pwd);

    [DispId(140)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    int Log([MarshalAs(UnmanagedType.BStr)][In] string info);

    [DispId(141)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    [return: MarshalAs(UnmanagedType.BStr)]
    string FindStrE([In] int x1, [In] int y1, [In] int x2, [In] int y2, [MarshalAs(UnmanagedType.BStr)][In] string str, [MarshalAs(UnmanagedType.BStr)][In] string color, [In] double sim);

    [DispId(142)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    [return: MarshalAs(UnmanagedType.BStr)]
    string FindColorE([In] int x1, [In] int y1, [In] int x2, [In] int y2, [MarshalAs(UnmanagedType.BStr)][In] string color, [In] double sim, [In] int dir);

    [DispId(143)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    [return: MarshalAs(UnmanagedType.BStr)]
    string FindPicE([In] int x1, [In] int y1, [In] int x2, [In] int y2, [MarshalAs(UnmanagedType.BStr)][In] string pic_name, [MarshalAs(UnmanagedType.BStr)][In] string delta_color,
        [In] double sim, [In] int dir);

    [DispId(144)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    [return: MarshalAs(UnmanagedType.BStr)]
    string FindMultiColorE([In] int x1, [In] int y1, [In] int x2, [In] int y2, [MarshalAs(UnmanagedType.BStr)][In] string first_color, [MarshalAs(UnmanagedType.BStr)][In] string offset_color,
        [In] double sim, [In] int dir);

    [DispId(145)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    int SetExactOcr([In] int exact_ocr);

    [DispId(146)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    [return: MarshalAs(UnmanagedType.BStr)]
    string ReadString([In] int hwnd, [MarshalAs(UnmanagedType.BStr)][In] string addr, [In] int type, [In] int len);

    [DispId(147)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    int FoobarTextPrintDir([In] int hwnd, [In] int dir);

    [DispId(148)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    [return: MarshalAs(UnmanagedType.BStr)]
    string OcrEx([In] int x1, [In] int y1, [In] int x2, [In] int y2, [MarshalAs(UnmanagedType.BStr)][In] string color, [In] double sim);

    [DispId(149)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    int SetDisplayInput([MarshalAs(UnmanagedType.BStr)][In] string mode);

    [DispId(150)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    int GetTime();

    [DispId(151)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    int GetScreenWidth();

    [DispId(152)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    int GetScreenHeight();

    [DispId(153)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    int BindWindowEx([In] int hwnd, [MarshalAs(UnmanagedType.BStr)][In] string display, [MarshalAs(UnmanagedType.BStr)][In] string mouse, [MarshalAs(UnmanagedType.BStr)][In] string keypad,
        [MarshalAs(UnmanagedType.BStr)][In] string public_desc, [In] int mode);

    [DispId(154)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    [return: MarshalAs(UnmanagedType.BStr)]
    string GetDiskSerial();

    [DispId(155)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    [return: MarshalAs(UnmanagedType.BStr)]
    string Md5([MarshalAs(UnmanagedType.BStr)][In] string str);

    [DispId(156)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    [return: MarshalAs(UnmanagedType.BStr)]
    string GetMac();

    [DispId(157)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    int ActiveInputMethod([In] int hwnd, [MarshalAs(UnmanagedType.BStr)][In] string id);

    [DispId(158)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    int CheckInputMethod([In] int hwnd, [MarshalAs(UnmanagedType.BStr)][In] string id);

    [DispId(159)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    int FindInputMethod([MarshalAs(UnmanagedType.BStr)][In] string id);

    [DispId(160)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    int GetCursorPos([MarshalAs(UnmanagedType.Struct)] out object x, [MarshalAs(UnmanagedType.Struct)] out object y);

    [DispId(161)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    int BindWindow([In] int hwnd, [MarshalAs(UnmanagedType.BStr)][In] string display, [MarshalAs(UnmanagedType.BStr)][In] string mouse, [MarshalAs(UnmanagedType.BStr)][In] string keypad,
        [In] int mode);

    [DispId(162)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    int FindWindow([MarshalAs(UnmanagedType.BStr)][In] string class_name, [MarshalAs(UnmanagedType.BStr)][In] string title_name);

    [DispId(163)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    int GetScreenDepth();

    [DispId(164)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    int SetScreen([In] int width, [In] int height, [In] int depth);

    [DispId(165)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    int ExitOs([In] int type);

    [DispId(166)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    [return: MarshalAs(UnmanagedType.BStr)]
    string GetDir([In] int type);

    [DispId(167)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    int GetOsType();

    [DispId(168)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    int FindWindowEx([In] int parent, [MarshalAs(UnmanagedType.BStr)][In] string class_name, [MarshalAs(UnmanagedType.BStr)][In] string title_name);

    [DispId(169)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    int SetExportDict([In] int index, [MarshalAs(UnmanagedType.BStr)][In] string dict_name);

    [DispId(170)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    [return: MarshalAs(UnmanagedType.BStr)]
    string GetCursorShape();

    [DispId(171)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    int DownCpu([In] int rate);

    [DispId(172)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    [return: MarshalAs(UnmanagedType.BStr)]
    string GetCursorSpot();

    [DispId(173)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    int SendString2([In] int hwnd, [MarshalAs(UnmanagedType.BStr)][In] string str);

    [DispId(174)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    int FaqPost([MarshalAs(UnmanagedType.BStr)][In] string server, [In] int handle, [In] int request_type, [In] int time_out);

    [DispId(175)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    [return: MarshalAs(UnmanagedType.BStr)]
    string FaqFetch();

    [DispId(176)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    [return: MarshalAs(UnmanagedType.BStr)]
    string FetchWord([In] int x1, [In] int y1, [In] int x2, [In] int y2, [MarshalAs(UnmanagedType.BStr)][In] string color, [MarshalAs(UnmanagedType.BStr)][In] string word);

    [DispId(177)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    int CaptureJpg([In] int x1, [In] int y1, [In] int x2, [In] int y2, [MarshalAs(UnmanagedType.BStr)][In] string file, [In] int quality);

    [DispId(178)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    int FindStrWithFont([In] int x1, [In] int y1, [In] int x2, [In] int y2, [MarshalAs(UnmanagedType.BStr)][In] string str, [MarshalAs(UnmanagedType.BStr)][In] string color, [In] double sim,
        [MarshalAs(UnmanagedType.BStr)][In] string font_name, [In] int font_size, [In] int flag, [MarshalAs(UnmanagedType.Struct)] out object x, [MarshalAs(UnmanagedType.Struct)] out object y);

    [DispId(179)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    [return: MarshalAs(UnmanagedType.BStr)]
    string FindStrWithFontE([In] int x1, [In] int y1, [In] int x2, [In] int y2, [MarshalAs(UnmanagedType.BStr)][In] string str, [MarshalAs(UnmanagedType.BStr)][In] string color, [In] double sim,
        [MarshalAs(UnmanagedType.BStr)][In] string font_name, [In] int font_size, [In] int flag);

    [DispId(180)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    [return: MarshalAs(UnmanagedType.BStr)]
    string FindStrWithFontEx([In] int x1, [In] int y1, [In] int x2, [In] int y2, [MarshalAs(UnmanagedType.BStr)][In] string str, [MarshalAs(UnmanagedType.BStr)][In] string color,
        [In] double sim, [MarshalAs(UnmanagedType.BStr)][In] string font_name, [In] int font_size, [In] int flag);

    [DispId(181)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    [return: MarshalAs(UnmanagedType.BStr)]
    string GetDictInfo([MarshalAs(UnmanagedType.BStr)][In] string str, [MarshalAs(UnmanagedType.BStr)][In] string font_name, [In] int font_size, [In] int flag);

    [DispId(182)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    int SaveDict([In] int index, [MarshalAs(UnmanagedType.BStr)][In] string file);

    [DispId(183)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    int GetWindowProcessId([In] int hwnd);

    [DispId(184)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    [return: MarshalAs(UnmanagedType.BStr)]
    string GetWindowProcessPath([In] int hwnd);

    [DispId(185)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    int LockInput([In] int @lock);

    [DispId(186)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    [return: MarshalAs(UnmanagedType.BStr)]
    string GetPicSize([MarshalAs(UnmanagedType.BStr)][In] string pic_name);

    [DispId(187)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    int GetID();

    [DispId(188)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    int CapturePng([In] int x1, [In] int y1, [In] int x2, [In] int y2, [MarshalAs(UnmanagedType.BStr)][In] string file);

    [DispId(189)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    int CaptureGif([In] int x1, [In] int y1, [In] int x2, [In] int y2, [MarshalAs(UnmanagedType.BStr)][In] string file, [In] int delay, [In] int time);

    [DispId(190)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    int ImageToBmp([MarshalAs(UnmanagedType.BStr)][In] string pic_name, [MarshalAs(UnmanagedType.BStr)][In] string bmp_name);

    [DispId(191)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    int FindStrFast([In] int x1, [In] int y1, [In] int x2, [In] int y2, [MarshalAs(UnmanagedType.BStr)][In] string str, [MarshalAs(UnmanagedType.BStr)][In] string color, [In] double sim,
        [MarshalAs(UnmanagedType.Struct)] out object x, [MarshalAs(UnmanagedType.Struct)] out object y);

    [DispId(192)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    [return: MarshalAs(UnmanagedType.BStr)]
    string FindStrFastEx([In] int x1, [In] int y1, [In] int x2, [In] int y2, [MarshalAs(UnmanagedType.BStr)][In] string str, [MarshalAs(UnmanagedType.BStr)][In] string color, [In] double sim);

    [DispId(193)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    [return: MarshalAs(UnmanagedType.BStr)]
    string FindStrFastE([In] int x1, [In] int y1, [In] int x2, [In] int y2, [MarshalAs(UnmanagedType.BStr)][In] string str, [MarshalAs(UnmanagedType.BStr)][In] string color, [In] double sim);

    [DispId(194)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    int EnableDisplayDebug([In] int enable_debug);

    [DispId(195)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    int CapturePre([MarshalAs(UnmanagedType.BStr)][In] string file);

    [DispId(196)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    int RegEx([MarshalAs(UnmanagedType.BStr)][In] string code, [MarshalAs(UnmanagedType.BStr)][In] string Ver, [MarshalAs(UnmanagedType.BStr)][In] string ip);

    [DispId(197)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    [return: MarshalAs(UnmanagedType.BStr)]
    string GetMachineCode();

    [DispId(198)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    int SetClipboard([MarshalAs(UnmanagedType.BStr)][In] string data);

    [DispId(199)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    [return: MarshalAs(UnmanagedType.BStr)]
    string GetClipboard();

    [DispId(200)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    int GetNowDict();

    [DispId(201)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    int Is64Bit();

    [DispId(202)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    int GetColorNum([In] int x1, [In] int y1, [In] int x2, [In] int y2, [MarshalAs(UnmanagedType.BStr)][In] string color, [In] double sim);

    [DispId(203)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    [return: MarshalAs(UnmanagedType.BStr)]
    string EnumWindowByProcess([MarshalAs(UnmanagedType.BStr)][In] string process_name, [MarshalAs(UnmanagedType.BStr)][In] string title, [MarshalAs(UnmanagedType.BStr)][In] string class_name,
        [In] int filter);

    [DispId(204)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    int GetDictCount([In] int index);

    [DispId(205)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    int GetLastError();

    [DispId(206)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    [return: MarshalAs(UnmanagedType.BStr)]
    string GetNetTime();

    [DispId(207)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    int EnableGetColorByCapture([In] int en);

    [DispId(208)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    int CheckUAC();

    [DispId(209)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    int SetUAC([In] int uac);

    [DispId(210)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    int DisableFontSmooth();

    [DispId(211)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    int CheckFontSmooth();

    [DispId(212)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    int SetDisplayAcceler([In] int level);

    [DispId(213)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    int FindWindowByProcess([MarshalAs(UnmanagedType.BStr)][In] string process_name, [MarshalAs(UnmanagedType.BStr)][In] string class_name,
        [MarshalAs(UnmanagedType.BStr)][In] string title_name);

    [DispId(214)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    int FindWindowByProcessId([In] int process_id, [MarshalAs(UnmanagedType.BStr)][In] string class_name, [MarshalAs(UnmanagedType.BStr)][In] string title_name);

    [DispId(215)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    [return: MarshalAs(UnmanagedType.BStr)]
    string ReadIni([MarshalAs(UnmanagedType.BStr)][In] string section, [MarshalAs(UnmanagedType.BStr)][In] string key, [MarshalAs(UnmanagedType.BStr)][In] string file);

    [DispId(216)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    int WriteIni([MarshalAs(UnmanagedType.BStr)][In] string section, [MarshalAs(UnmanagedType.BStr)][In] string key, [MarshalAs(UnmanagedType.BStr)][In] string v,
        [MarshalAs(UnmanagedType.BStr)][In] string file);

    [DispId(217)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    int RunApp([MarshalAs(UnmanagedType.BStr)][In] string path, [In] int mode);

    [DispId(218)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    int delay([In] int mis);

    [DispId(219)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    int FindWindowSuper([MarshalAs(UnmanagedType.BStr)][In] string spec1, [In] int flag1, [In] int type1, [MarshalAs(UnmanagedType.BStr)][In] string spec2, [In] int flag2, [In] int type2);

    [DispId(220)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    [return: MarshalAs(UnmanagedType.BStr)]
    string ExcludePos([MarshalAs(UnmanagedType.BStr)][In] string all_pos, [In] int type, [In] int x1, [In] int y1, [In] int x2, [In] int y2);

    [DispId(221)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    [return: MarshalAs(UnmanagedType.BStr)]
    string FindNearestPos([MarshalAs(UnmanagedType.BStr)][In] string all_pos, [In] int type, [In] int x, [In] int y);

    [DispId(222)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    [return: MarshalAs(UnmanagedType.BStr)]
    string SortPosDistance([MarshalAs(UnmanagedType.BStr)][In] string all_pos, [In] int type, [In] int x, [In] int y);

    [DispId(223)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    int FindPicMem([In] int x1, [In] int y1, [In] int x2, [In] int y2, [MarshalAs(UnmanagedType.BStr)][In] string pic_info, [MarshalAs(UnmanagedType.BStr)][In] string delta_color,
        [In] double sim, [In] int dir, [MarshalAs(UnmanagedType.Struct)] out object x, [MarshalAs(UnmanagedType.Struct)] out object y);

    [DispId(224)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    [return: MarshalAs(UnmanagedType.BStr)]
    string FindPicMemEx([In] int x1, [In] int y1, [In] int x2, [In] int y2, [MarshalAs(UnmanagedType.BStr)][In] string pic_info, [MarshalAs(UnmanagedType.BStr)][In] string delta_color,
        [In] double sim, [In] int dir);

    [DispId(225)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    [return: MarshalAs(UnmanagedType.BStr)]
    string FindPicMemE([In] int x1, [In] int y1, [In] int x2, [In] int y2, [MarshalAs(UnmanagedType.BStr)][In] string pic_info, [MarshalAs(UnmanagedType.BStr)][In] string delta_color,
        [In] double sim, [In] int dir);

    [DispId(226)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    [return: MarshalAs(UnmanagedType.BStr)]
    string AppendPicAddr([MarshalAs(UnmanagedType.BStr)][In] string pic_info, [In] int addr, [In] int size);

    [DispId(227)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    int WriteFile([MarshalAs(UnmanagedType.BStr)][In] string file, [MarshalAs(UnmanagedType.BStr)][In] string content);

    [DispId(228)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    int Stop([In] int id);

    [DispId(229)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    int SetDictMem([In] int index, [In] int addr, [In] int size);

    [DispId(230)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    [return: MarshalAs(UnmanagedType.BStr)]
    string GetNetTimeSafe();

    [DispId(231)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    int ForceUnBindWindow([In] int hwnd);

    [DispId(232)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    [return: MarshalAs(UnmanagedType.BStr)]
    string ReadIniPwd([MarshalAs(UnmanagedType.BStr)][In] string section, [MarshalAs(UnmanagedType.BStr)][In] string key, [MarshalAs(UnmanagedType.BStr)][In] string file,
        [MarshalAs(UnmanagedType.BStr)][In] string pwd);

    [DispId(233)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    int WriteIniPwd([MarshalAs(UnmanagedType.BStr)][In] string section, [MarshalAs(UnmanagedType.BStr)][In] string key, [MarshalAs(UnmanagedType.BStr)][In] string v,
        [MarshalAs(UnmanagedType.BStr)][In] string file, [MarshalAs(UnmanagedType.BStr)][In] string pwd);

    [DispId(234)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    int DecodeFile([MarshalAs(UnmanagedType.BStr)][In] string file, [MarshalAs(UnmanagedType.BStr)][In] string pwd);

    [DispId(235)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    int KeyDownChar([MarshalAs(UnmanagedType.BStr)][In] string key_str);

    [DispId(236)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    int KeyUpChar([MarshalAs(UnmanagedType.BStr)][In] string key_str);

    [DispId(237)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    int KeyPressChar([MarshalAs(UnmanagedType.BStr)][In] string key_str);

    [DispId(238)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    int KeyPressStr([MarshalAs(UnmanagedType.BStr)][In] string key_str, [In] int delay);

    [DispId(239)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    int EnableKeypadPatch([In] int en);

    [DispId(240)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    int EnableKeypadSync([In] int en, [In] int time_out);

    [DispId(241)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    int EnableMouseSync([In] int en, [In] int time_out);

    [DispId(242)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    int DmGuard([In] int en, [MarshalAs(UnmanagedType.BStr)][In] string type);

    [DispId(243)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    int FaqCaptureFromFile([In] int x1, [In] int y1, [In] int x2, [In] int y2, [MarshalAs(UnmanagedType.BStr)][In] string file, [In] int quality);

    [DispId(244)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    [return: MarshalAs(UnmanagedType.BStr)]
    string FindIntEx([In] int hwnd, [MarshalAs(UnmanagedType.BStr)][In] string addr_range, [In] int int_value_min, [In] int int_value_max, [In] int type, [In] int step, [In] int multi_thread,
        [In] int mode);

    [DispId(245)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    [return: MarshalAs(UnmanagedType.BStr)]
    string FindFloatEx([In] int hwnd, [MarshalAs(UnmanagedType.BStr)][In] string addr_range, [In] float float_value_min, [In] float float_value_max, [In] int step, [In] int multi_thread,
        [In] int mode);

    [DispId(246)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    [return: MarshalAs(UnmanagedType.BStr)]
    string FindDoubleEx([In] int hwnd, [MarshalAs(UnmanagedType.BStr)][In] string addr_range, [In] double double_value_min, [In] double double_value_max, [In] int step, [In] int multi_thread,
        [In] int mode);

    [DispId(247)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    [return: MarshalAs(UnmanagedType.BStr)]
    string FindStringEx([In] int hwnd, [MarshalAs(UnmanagedType.BStr)][In] string addr_range, [MarshalAs(UnmanagedType.BStr)][In] string string_value, [In] int type, [In] int step,
        [In] int multi_thread, [In] int mode);

    [DispId(248)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    [return: MarshalAs(UnmanagedType.BStr)]
    string FindDataEx([In] int hwnd, [MarshalAs(UnmanagedType.BStr)][In] string addr_range, [MarshalAs(UnmanagedType.BStr)][In] string data, [In] int step, [In] int multi_thread, [In] int mode);

    [DispId(249)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    int EnableRealMouse([In] int en, [In] int mousedelay, [In] int mousestep);

    [DispId(250)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    int EnableRealKeypad([In] int en);

    [DispId(251)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    int SendStringIme([MarshalAs(UnmanagedType.BStr)][In] string str);

    [DispId(252)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    int FoobarDrawLine([In] int hwnd, [In] int x1, [In] int y1, [In] int x2, [In] int y2, [MarshalAs(UnmanagedType.BStr)][In] string color, [In] int style, [In] int width);

    [DispId(253)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    [return: MarshalAs(UnmanagedType.BStr)]
    string FindStrEx([In] int x1, [In] int y1, [In] int x2, [In] int y2, [MarshalAs(UnmanagedType.BStr)][In] string str, [MarshalAs(UnmanagedType.BStr)][In] string color, [In] double sim);

    [DispId(254)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    int IsBind([In] int hwnd);

    [DispId(255)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    int SetDisplayDelay([In] int t);

    [DispId(256)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    int GetDmCount();

    [DispId(257)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    int DisableScreenSave();

    [DispId(258)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    int DisablePowerSave();

    [DispId(259)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    int SetMemoryHwndAsProcessId([In] int en);

    [DispId(260)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    int FindShape([In] int x1, [In] int y1, [In] int x2, [In] int y2, [MarshalAs(UnmanagedType.BStr)][In] string offset_color, [In] double sim, [In] int dir,
        [MarshalAs(UnmanagedType.Struct)] out object x, [MarshalAs(UnmanagedType.Struct)] out object y);

    [DispId(261)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    [return: MarshalAs(UnmanagedType.BStr)]
    string FindShapeE([In] int x1, [In] int y1, [In] int x2, [In] int y2, [MarshalAs(UnmanagedType.BStr)][In] string offset_color, [In] double sim, [In] int dir);

    [DispId(262)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    [return: MarshalAs(UnmanagedType.BStr)]
    string FindShapeEx([In] int x1, [In] int y1, [In] int x2, [In] int y2, [MarshalAs(UnmanagedType.BStr)][In] string offset_color, [In] double sim, [In] int dir);

    [DispId(263)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    [return: MarshalAs(UnmanagedType.BStr)]
    string FindStrS([In] int x1, [In] int y1, [In] int x2, [In] int y2, [MarshalAs(UnmanagedType.BStr)][In] string str, [MarshalAs(UnmanagedType.BStr)][In] string color, [In] double sim,
        [MarshalAs(UnmanagedType.Struct)] out object x, [MarshalAs(UnmanagedType.Struct)] out object y);

    [DispId(264)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    [return: MarshalAs(UnmanagedType.BStr)]
    string FindStrExS([In] int x1, [In] int y1, [In] int x2, [In] int y2, [MarshalAs(UnmanagedType.BStr)][In] string str, [MarshalAs(UnmanagedType.BStr)][In] string color, [In] double sim);

    [DispId(265)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    [return: MarshalAs(UnmanagedType.BStr)]
    string FindStrFastS([In] int x1, [In] int y1, [In] int x2, [In] int y2, [MarshalAs(UnmanagedType.BStr)][In] string str, [MarshalAs(UnmanagedType.BStr)][In] string color, [In] double sim,
        [MarshalAs(UnmanagedType.Struct)] out object x, [MarshalAs(UnmanagedType.Struct)] out object y);

    [DispId(266)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    [return: MarshalAs(UnmanagedType.BStr)]
    string FindStrFastExS([In] int x1, [In] int y1, [In] int x2, [In] int y2, [MarshalAs(UnmanagedType.BStr)][In] string str, [MarshalAs(UnmanagedType.BStr)][In] string color, [In] double sim);

    [DispId(267)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    [return: MarshalAs(UnmanagedType.BStr)]
    string FindPicS([In] int x1, [In] int y1, [In] int x2, [In] int y2, [MarshalAs(UnmanagedType.BStr)][In] string pic_name, [MarshalAs(UnmanagedType.BStr)][In] string delta_color,
        [In] double sim, [In] int dir, [MarshalAs(UnmanagedType.Struct)] out object x, [MarshalAs(UnmanagedType.Struct)] out object y);

    [DispId(268)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    [return: MarshalAs(UnmanagedType.BStr)]
    string FindPicExS([In] int x1, [In] int y1, [In] int x2, [In] int y2, [MarshalAs(UnmanagedType.BStr)][In] string pic_name, [MarshalAs(UnmanagedType.BStr)][In] string delta_color,
        [In] double sim, [In] int dir);

    [DispId(269)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    int ClearDict([In] int index);

    [DispId(270)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    [return: MarshalAs(UnmanagedType.BStr)]
    string GetMachineCodeNoMac();

    [DispId(271)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    int GetClientRect([In] int hwnd, [MarshalAs(UnmanagedType.Struct)] out object x1, [MarshalAs(UnmanagedType.Struct)] out object y1, [MarshalAs(UnmanagedType.Struct)] out object x2,
        [MarshalAs(UnmanagedType.Struct)] out object y2);

    [DispId(272)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    int EnableFakeActive([In] int en);

    [DispId(273)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    int GetScreenDataBmp([In] int x1, [In] int y1, [In] int x2, [In] int y2, [MarshalAs(UnmanagedType.Struct)] out object data, [MarshalAs(UnmanagedType.Struct)] out object size);

    [DispId(274)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    int EncodeFile([MarshalAs(UnmanagedType.BStr)][In] string file, [MarshalAs(UnmanagedType.BStr)][In] string pwd);

    [DispId(275)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    [return: MarshalAs(UnmanagedType.BStr)]
    string GetCursorShapeEx([In] int type);

    [DispId(276)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    int FaqCancel();

    [DispId(277)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    [return: MarshalAs(UnmanagedType.BStr)]
    string IntToData([In] int int_value, [In] int type);

    [DispId(278)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    [return: MarshalAs(UnmanagedType.BStr)]
    string FloatToData([In] float float_value);

    [DispId(279)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    [return: MarshalAs(UnmanagedType.BStr)]
    string DoubleToData([In] double double_value);

    [DispId(280)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    [return: MarshalAs(UnmanagedType.BStr)]
    string StringToData([MarshalAs(UnmanagedType.BStr)][In] string string_value, [In] int type);

    [DispId(281)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    int SetMemoryFindResultToFile([MarshalAs(UnmanagedType.BStr)][In] string file);

    [DispId(282)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    int EnableBind([In] int en);

    [DispId(283)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    int SetSimMode([In] int mode);

    [DispId(284)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    int LockMouseRect([In] int x1, [In] int y1, [In] int x2, [In] int y2);

    [DispId(285)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    int SendPaste([In] int hwnd);

    [DispId(286)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    int IsDisplayDead([In] int x1, [In] int y1, [In] int x2, [In] int y2, [In] int t);

    [DispId(287)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    int GetKeyState([In] int vk);

    [DispId(288)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    int CopyFile([MarshalAs(UnmanagedType.BStr)][In] string src_file, [MarshalAs(UnmanagedType.BStr)][In] string dst_file, [In] int over);

    [DispId(289)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    int IsFileExist([MarshalAs(UnmanagedType.BStr)][In] string file);

    [DispId(290)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    int DeleteFile([MarshalAs(UnmanagedType.BStr)][In] string file);

    [DispId(291)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    int MoveFile([MarshalAs(UnmanagedType.BStr)][In] string src_file, [MarshalAs(UnmanagedType.BStr)][In] string dst_file);

    [DispId(292)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    int CreateFolder([MarshalAs(UnmanagedType.BStr)][In] string folder_name);

    [DispId(293)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    int DeleteFolder([MarshalAs(UnmanagedType.BStr)][In] string folder_name);

    [DispId(294)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    int GetFileLength([MarshalAs(UnmanagedType.BStr)][In] string file);

    [DispId(295)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    [return: MarshalAs(UnmanagedType.BStr)]
    string ReadFile([MarshalAs(UnmanagedType.BStr)][In] string file);

    [DispId(296)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    int WaitKey([In] int key_code, [In] int time_out);

    [DispId(297)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    int DeleteIni([MarshalAs(UnmanagedType.BStr)][In] string section, [MarshalAs(UnmanagedType.BStr)][In] string key, [MarshalAs(UnmanagedType.BStr)][In] string file);

    [DispId(298)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    int DeleteIniPwd([MarshalAs(UnmanagedType.BStr)][In] string section, [MarshalAs(UnmanagedType.BStr)][In] string key, [MarshalAs(UnmanagedType.BStr)][In] string file,
        [MarshalAs(UnmanagedType.BStr)][In] string pwd);

    [DispId(299)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    int EnableSpeedDx([In] int en);

    [DispId(300)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    int EnableIme([In] int en);

    [DispId(301)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    int Reg([MarshalAs(UnmanagedType.BStr)][In] string code, [MarshalAs(UnmanagedType.BStr)][In] string Ver);

    [DispId(302)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    [return: MarshalAs(UnmanagedType.BStr)]
    string SelectFile();

    [DispId(303)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    [return: MarshalAs(UnmanagedType.BStr)]
    string SelectDirectory();

    [DispId(304)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    int LockDisplay([In] int @lock);

    [DispId(305)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    int FoobarSetSave([In] int hwnd, [MarshalAs(UnmanagedType.BStr)][In] string file, [In] int en, [MarshalAs(UnmanagedType.BStr)][In] string header);

    [DispId(306)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    [return: MarshalAs(UnmanagedType.BStr)]
    string EnumWindowSuper([MarshalAs(UnmanagedType.BStr)][In] string spec1, [In] int flag1, [In] int type1, [MarshalAs(UnmanagedType.BStr)][In] string spec2, [In] int flag2, [In] int type2,
        [In] int sort);

    [DispId(307)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    int DownloadFile([MarshalAs(UnmanagedType.BStr)][In] string url, [MarshalAs(UnmanagedType.BStr)][In] string save_file, [In] int timeout);

    [DispId(308)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    int EnableKeypadMsg([In] int en);

    [DispId(309)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    int EnableMouseMsg([In] int en);

    [DispId(310)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    int RegNoMac([MarshalAs(UnmanagedType.BStr)][In] string code, [MarshalAs(UnmanagedType.BStr)][In] string Ver);

    [DispId(311)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    int RegExNoMac([MarshalAs(UnmanagedType.BStr)][In] string code, [MarshalAs(UnmanagedType.BStr)][In] string Ver, [MarshalAs(UnmanagedType.BStr)][In] string ip);

    [DispId(312)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    int SetEnumWindowDelay([In] int delay);

    [DispId(313)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    int FindMulColor([In] int x1, [In] int y1, [In] int x2, [In] int y2, [MarshalAs(UnmanagedType.BStr)][In] string color, [In] double sim);
}

#endregion
