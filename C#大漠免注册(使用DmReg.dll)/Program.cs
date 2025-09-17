using System.Runtime.InteropServices;

internal static class Program
{
    // winform 显示控制台
    //[DllImport("kernel32.dll")]
    //static extern bool AllocConsole();

    // winform 关闭控制台
    //[DllImport("kernel32.dll")]
    //static extern bool FreeConsole();

    // 不注册调用大漠插件，实际上是使用 DmReg.dll 来配合实现，
    // 这个文件有 2 个导出接口 SetDllPathW 和 SetDllPathA。
    // SetDllPathW 对应 unicode，
    // SetDllPathA 对应 ascii 接口。
    [DllImport("./dll/DmReg.dll")]
    static extern int SetDllPathA(string path, int mode);

    static bool RegisterDmDll()
    {
        var setDllPathResult = SetDllPathA("./dll/dm.dll", 1);
        return setDllPathResult != 0;
    }

    static DmSoft dm;

    static void Main()
    {
        // !! 项目一定要改为 x86, 把相关dll全部设置为 始终复制到输出目录
        if (!RegisterDmDll())
            throw new Exception("免注册调用大漠插件失败");

        dm = new DmSoft();
        Console.WriteLine("版本号:"+dm.Ver());
        Console.WriteLine(dm.GetCursorPos(out int x, out int y));
        Console.WriteLine($"鼠标位置:{x},{y}");
        Console.WriteLine($"屏幕宽高{dm.GetScreenWidth()},{dm.GetScreenHeight()}");
    }
}