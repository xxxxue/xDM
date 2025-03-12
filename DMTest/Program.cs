using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using XXDM.MyDM;

namespace DMTest
{
    internal class Program
    {
        private static XDm dm = new XDm();
        private static Colors colors = new Colors();
        /// <summary>
        /// 游戏窗口句柄
        /// </summary>
        private static readonly int Hwnd = dm.FindWindow("Notepad++", "");

        private static void Main(string[] args)
        {

            Console.ReadLine();
            //dm.ConLog(dm.Reg("", ""));//收费版  记得先注册
            dm.StartRunTime();

            //设置游戏分辨率
            dm.SetWindowSize(Hwnd, dm.GameWindowsSize[0], dm.GameWindowsSize[1]);

            var mainTask = Task.Run(() =>
              {
                  dm.ConLog("开始执行...");

                  RunCode();

                  dm.ConLog("执行完毕...");
                  dm.ShowRunTime();
              });

            Task.WaitAny(mainTask);
            dm.StopRunTime();
            Console.ReadLine();
        }

        /// <summary>
        /// 执行代码
        /// </summary>
        public static void RunCode()
        {
            dm.ConLog(Hwnd);

            // (使用前 先绑定窗口)



            // 找图
            //dm.FindPic(0, 0, 1920, 1080, "E:\\Desktop\\dmtest.bmp", "000000",0.8,0,out int x,out int y);

            //Console.WriteLine($"{x} : {y}");

            //dm.MoveTo(x,y);

            // 多点找色
            //dm.FindMultiColor(0, 0, 74, 69, "fd6161-111111", "2|6|e55656-111111,7|0|f46565-111111,11|-1|f0f0f0-111111", 0.8,0,out int x , out int y);

            //Console.WriteLine($"{x} : {y}");
            //dm.MoveTo(x, y);

            // 测试一下自己封装的
            //var res = dm.Find("测试,0,0,74,69,fd6161-111111,2|6|e55656-111111,7|0|f46565-111111,11|-1|f0f0f0-111111", true);
            //Console.WriteLine(res);

            // 图片保存到 exe 同级
            //dm.KeepPng("测试,0,0,74,69,fd6161-111111,2|6|e55656-111111,7|0|f46565-111111,11|-1|f0f0f0-111111");







            //if (dm.Find(colors.城镇_展开箭头按钮, true))
            //{
            //    dm.delay(2000);
            //    dm.Find(colors.城镇_修行按钮, true);

            //    dm.delay(2000);

            //    dm.Find(colors.修行界面_关闭状态, true);

            //    Console.WriteLine("ok");
            //}
            //else
            //{
            //    Console.WriteLine("no");
            //}

        }
    }
}