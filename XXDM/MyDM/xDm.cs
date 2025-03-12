using System;
using System.Diagnostics;
using System.IO;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;

using XXDM.DM;
using XXDM.Helper;
using XXDM.Model;

namespace XXDM.MyDM
{
    public class XDm
    {
        private dmsoft dm = new dmsoft();
        //程序运行时间
        private Stopwatch sw = new Stopwatch();

        public XDm()
        {
            ConLog("大漠版本: " + dm.Ver());
        }
        /// <summary>
        /// 游戏窗口大小
        /// </summary>
        public int[] GameWindowsSize = { 1280, 772 };

        /// <summary>
        /// 控制台 输出msg
        /// </summary>
        /// <param name="msg"></param>
        public string ConLog(object msg)
        {
            string showMsg = "『" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff") + "』: " + msg;
            Console.WriteLine(showMsg);
            return showMsg;
        }

        /// <summary>
        /// 程序开始计时
        /// </summary>
        public void StartRunTime()
        {
            sw.Start();//开始记录时间
        }

        /// <summary>
        /// 停止程序计时
        /// </summary>
        public void StopRunTime()
        {
            sw.Stop();
        }

        /// <summary>
        /// 显示时间
        /// </summary>
        public void ShowRunTime()
        {
            Console.WriteLine($"\n**************程序的 运行时间 ：{sw.Elapsed} **************\n");
        }

        #region my

        /// <summary>
        /// 区域多点找色 (可点击)
        /// </summary>
        /// <param name="colorInfo">例子:  string 城镇展开箭头按钮 ="645,48,789,97,add3ad-111111,-5|-6|a5cfa5-111111,-4|6|a5c7a5-111111";</param>
        /// <param name="isClick">是否单击</param>
        /// <returns>找图判定结果</returns>
        public bool Find(string colorInfo, bool isClick = false)
        {
            Result res = Find(colorInfo);

            if (isClick && res.Success && res.X > 0 && res.Y > 0)
            {
                MoveTo(EnumHelper.MouseEnum.鼠标左键, res.X, res.Y);

            }

            return res.Success;
        }

        /// <summary>
        /// 区域多点找色
        /// </summary>
        /// <param name="colorInfo">例子:  string 城镇展开箭头按钮 ="城镇_展开箭头按钮,645,48,789,97,add3ad-111111,-5|-6|a5cfa5-111111,-4|6|a5c7a5-111111";</param>
        /// <returns>Result对象</returns>
        public Result Find(string colorInfo)
        {
            string[] color = colorInfo.Split(',');
            StringBuilder offsetColor = new StringBuilder();
            for (int i = 6; i < color.Length; i++)
            {
                offsetColor.Append("," + color[i]);
            }

            int res = dm.FindMultiColor(
                Convert.ToInt32(color[1]),
                Convert.ToInt32(color[2]),
                Convert.ToInt32(color[3]),
                Convert.ToInt32(color[4]),
                color[5],
                offsetColor.ToString().Trim(','),
                0.8, 1, out int x, out int y);

            if (res == 1 && x > 0 && y > 0)
            {
                Console.WriteLine("XDm.Find: " + color[0] + $"  {x} , {y} ");
            }
            else
            {
                Console.WriteLine("XDm.Find: " + color[0] + $" [未找到] ");
                KeepPng(colorInfo);
            }

            return new Result()
            {
                Success = (res == 1),
                X = x,
                Y = y
            };
        }

        public void KeepPng(string colorInfo)
        {
            string[] color = colorInfo.Split(',');


            dm.delay(1000);
            string pngPath = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, $"{DateTime.Now.ToString("yyyyMMdd-HH-mm-ss-fff")}.png");
            dm.Capture(
                Convert.ToInt32(color[1]),
                Convert.ToInt32(color[2]),
                Convert.ToInt32(color[3]),
                Convert.ToInt32(color[4]),
                pngPath);

            Console.WriteLine("Debug: " + color[0] + "  [截图完成] " + colorInfo + "  位置:" + pngPath);
        }

        #endregion my

        #region XXXX

        /// <summary>
        /// 返回当前插件版本号
        /// </summary>
        /// <returns></returns>
        public string Ver()
        {
            return dm.Ver();
        }

        /// <summary>
        /// 设置全局路径,设置了此路径后,所有接口调用中,相关的文件都相对于此路径. 比如图片,字库等.
        /// </summary>
        /// <param name="path"> 路径,可以是相对路径,也可以是绝对路径 </param>
        /// <returns> 0: 失败 1: 成功</returns>
        public int SetPath(string path)
        {
            return dm.SetPath(path);
        }

        /// <summary>
        /// 识别屏幕范围(x1,y1,x2,y2)内符合color_format的字符串,并且相似度为sim,sim取值范围(0.1-1.0),这个值越大越精确,越大速度越快,越小速度越慢,请斟酌使用!
        /// </summary>
        /// <param name="x1">区域的左上X坐标</param>
        /// <param name="y1">区域的左上Y坐标</param>
        /// <param name="x2">区域的右下X坐标</param>
        /// <param name="y2">区域的右下Y坐标</param>
        /// <param name="color">颜色格式串. 可以包含换行分隔符,语法是","后加分割字符串. 具体可以查看下面的示例.注意，RGB和HSV格式都支持.</param>
        /// <param name="sim">相似度,取值范围0.1-1.0</param>
        /// <returns></returns>
        public string Ocr(int x1, int y1, int x2, int y2, string color, double sim)
        {
            return dm.Ocr(x1, y1, x2, y2, color, sim);
        }

        /// <summary>
        /// 在屏幕范围(x1,y1,x2,y2)内,查找string(可以是任意个字符串的组合)
        /// </summary>
        /// <param name="x1">区域的左上X坐标</param>
        /// <param name="y1">区域的左上Y坐标</param>
        /// <param name="x2">区域的右下X坐标</param>
        /// <param name="y2">区域的右下Y坐标</param>
        /// <param name="str">待查找的字符串,可以是字符串组合，比如"长安|洛阳|大雁塔",中间用"|"来分割字符串</param>
        /// <param name="color">颜色格式串, 可以包含换行分隔符,语法是","后加分割字符串. 具体可以查看下面的示例 .注意，RGB和HSV格式都支持.</param>
        /// <param name="sim">相似度,取值范围0.1-1.0</param>
        /// <param name="x">返回X坐标没找到返回-1</param>
        /// <param name="y">返回Y坐标没找到返回-1</param>
        /// <returns></returns>
        public int FindStr(int x1, int y1, int x2, int y2, string str, string color, double sim, out int x, out int y)
        {
            return dm.FindStr(x1, y1, x2, y2, str, color, sim, out x, out y);
        }

        /// <summary>
        /// 对插件部分接口的返回值进行解析,并返回ret中的坐标个数
        /// </summary>
        /// <param name="str">部分接口的返回串</param>
        /// <returns></returns>
        public int GetResultCount(string str)
        {
            return dm.GetResultCount(str);
        }

        /// <summary>
        /// 对插件部分接口的返回值进行解析,并根据指定的第index个坐标,返回具体的值
        /// </summary>
        /// <param name="ret">部分接口的返回串</param>
        /// <param name="index">第几个坐标</param>
        /// <param name="x">返回X坐标</param>
        /// <param name="y">返回Y坐标</param>
        /// <returns>0:失败1:成功</returns>
        public int GetResultPos(string ret, int index, out int x, out int y)
        {
            return dm.GetResultPos(ret, index, out x, out y);
        }

        /// <summary>
        /// 未知
        /// </summary>
        /// <param name="s"></param>
        /// <param name="str"></param>
        /// <returns></returns>
        public int StrStr(string s, string str)
        {
            return dm.StrStr(s, str);
        }

        /// <summary>
        /// 未知
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns></returns>
        public int SendCommand(string cmd)
        {
            return dm.SendCommand(cmd);
        }

        /// <summary>
        /// 表示使用哪个字库文件进行识别(index范围:0-9)设置之后，永久生效，除非再次设定
        /// </summary>
        /// <param name="index">字库编号(0-9)</param>
        /// <returns></returns>
        public int UseDict(int index)
        {
            return dm.UseDict(index);
        }

        /// <summary>
        /// 获取注册在系统中的dm.dll的路径
        /// </summary>
        /// <returns></returns>
        public string GetBasePath()
        {
            return dm.GetBasePath();
        }

        /// <summary>
        /// 设置字库的密码,在SetDict前调用,目前的设计是,所有字库通用一个密码.
        /// </summary>
        /// <param name="pwd">字库密码</param>
        /// <returns></returns>
        public int SetDictPwd(string pwd)
        {
            return dm.SetDictPwd(pwd);
        }

        /// <summary>
        /// 识别位图中区域(x1,y1,x2,y2)的文字
        /// </summary>
        /// <param name="x1">区域的左上X坐标</param>
        /// <param name="y1">区域的左上Y坐标</param>
        /// <param name="x2">区域的右下X坐标</param>
        /// <param name="y2">区域的右下Y坐标</param>
        /// <param name="pic_name">图片文件名</param>
        /// <param name="color">颜色格式串.注意，RGB和HSV格式都支持.</param>
        /// <param name="sim">相似度,取值范围0.1-1.0</param>
        /// <returns></returns>
        public string OcrInFile(int x1, int y1, int x2, int y2, string pic_name, string color, double sim)
        {
            return dm.OcrInFile(x1, y1, x2, y2, pic_name, color, sim);
        }

        /// <summary>
        /// 抓取指定区域(x1, y1, x2, y2)的图像,保存为file(24位位图)
        /// </summary>
        /// <param name="x1">区域的左上X坐标</param>
        /// <param name="y1">区域的左上Y坐标</param>
        /// <param name="x2">区域的右下X坐标</param>
        /// <param name="y2">区域的右下Y坐标</param>
        /// <param name="file_">保存的文件名,保存的地方一般为SetPath中设置的目录当然这里也可以指定全路径名.</param>
        /// <returns></returns>
        public int Capture(int x1, int y1, int x2, int y2, string file_)
        {
            return dm.Capture(x1, y1, x2, y2, file_);
        }

        /// <summary>
        /// 按下指定的虚拟键码
        /// </summary>
        /// <param name="vk">虚拟按键码</param>
        /// <returns>0:失败1:成功</returns>
        public int KeyPress(EnumHelper.KeyCode vk)
        {
            return dm.KeyPress((int)vk);
        }

        /// <summary>
        /// 按住指定的虚拟键码
        /// </summary>
        /// <param name="vk">虚拟按键码</param>
        /// <returns></returns>
        public int KeyDown(EnumHelper.KeyCode vk)
        {
            return dm.KeyDown((int)vk);
        }

        /// <summary>
        /// 弹起来虚拟键
        /// </summary>
        /// <param name="vk">虚拟按键码</param>
        /// <returns></returns>
        public int KeyUp(EnumHelper.KeyCode vk)
        {
            return dm.KeyUp((int)vk);
        }

        /// <summary>
        /// 按下鼠标左键
        /// </summary>
        /// <returns></returns>
        public int LeftClick()
        {
            return dm.LeftClick();
        }

        /// <summary>
        /// 按下鼠标右键
        /// </summary>
        /// <returns></returns>
        public int RightClick()
        {
            return dm.RightClick();
        }

        /// <summary>
        /// 按下鼠标中键
        /// </summary>
        /// <returns></returns>
        public int MiddleClick()
        {
            return dm.MiddleClick();
        }

        /// <summary>
        /// 双击鼠标左键
        /// </summary>
        /// <returns></returns>
        public int LeftDoubleClick()
        {
            return dm.LeftDoubleClick();
        }

        /// <summary>
        /// 按住鼠标左键
        /// </summary>
        /// <returns></returns>
        public int LeftDown()
        {
            return dm.LeftDown();
        }

        /// <summary>
        /// 弹起鼠标左键
        /// </summary>
        /// <returns></returns>
        public int LeftUp()
        {
            return dm.LeftUp();
        }

        /// <summary>
        /// 按住鼠标右键
        /// </summary>
        /// <returns></returns>
        public int RightDown()
        {
            return dm.RightDown();
        }

        /// <summary>
        /// 弹起鼠标右键
        /// </summary>
        /// <returns></returns>
        public int RightUp()
        {
            return dm.RightUp();
        }

        /// <summary>
        /// 移动到某点并单击
        /// </summary>
        /// <param name="mouseClick"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public int MoveTo(EnumHelper.MouseEnum mouseClick, int x, int y)
        {
            int i = dm.MoveTo(x, y);
            if (i > 0)
            {
                Sleep(200);

                if (mouseClick == EnumHelper.MouseEnum.鼠标左键)
                {
                    i = i + LeftClick();

                }
                else if (mouseClick == EnumHelper.MouseEnum.鼠标右键)
                {
                    i = i + RightClick();
                }
            }
            return i == 2 ? 1 : 0;
        }

        /// <summary>
        /// 把鼠标移动到目的点(x,y)
        /// </summary>
        /// <param name="x">X坐标</param>
        /// <param name="y">Y坐标</param>
        /// <returns></returns>
        public int MoveTo(int x, int y)
        {
            return dm.MoveTo(x, y);
        }

        /// <summary>
        /// 鼠标相对于上次的位置移动rx,ry
        /// </summary>
        /// <param name="rx">相对于上次的X偏移</param>
        /// <param name="ry">相对于上次的Y偏移</param>
        /// <returns></returns>
        public int MoveR(int rx, int ry)
        {
            return dm.MoveR(rx, ry);
        }

        /// <summary>
        /// 获取(x,y)的颜色,颜色返回格式"RRGGBB",注意,和按键的颜色格式相反
        /// </summary>
        /// <param name="x">X坐标</param>
        /// <param name="y">Y坐标</param>
        /// <returns></returns>
        public string GetColor(int x, int y)
        {
            return dm.GetColor(x, y);
        }

        /// <summary>
        /// 获取(x,y)的颜色,颜色返回格式"BBGGRR"
        /// </summary>
        /// <param name="x">窗口X坐标</param>
        /// <param name="y">窗口Y坐标</param>
        /// <returns></returns>
        public string GetColorBGR(int x, int y)
        {
            return dm.GetColorBGR(x, y);
        }

        /// <summary>
        /// 把RGB的颜色格式转换为BGR(按键格式)
        /// </summary>
        /// <param name="rgb_color">rgb格式的颜色字符串</param>
        /// <returns></returns>
        public string RGB2BGR(string rgb_color)
        {
            return dm.RGB2BGR(rgb_color);
        }

        /// <summary>
        /// 把BGR(按键格式)的颜色格式转换为RGB
        /// </summary>
        /// <param name="bgr_color">bgr格式的颜色字符串</param>
        /// <returns></returns>
        public string BGR2RGB(string bgr_color)
        {
            return dm.BGR2RGB(bgr_color);
        }

        /// <summary>
        /// 解除绑定窗口,并释放系统资源.一般在OnScriptExit调用
        /// </summary>
        /// <returns></returns>
        public int UnBindWindow()
        {
            return dm.UnBindWindow();
        }

        /// <summary>
        /// 比较指定坐标点(x,y)的颜色
        /// </summary>
        /// <param name="x">窗口X坐标</param>
        /// <param name="y">窗口Y坐标</param>
        /// <param name="color">颜色字符串,可以支持偏色,多色,例如 "ffffff-202020|000000-000000" 这个表示白色偏色为202020,和黑色偏色为000000.颜色最多支持10种颜色组合. 注意，这里只支持RGB颜色.</param>
        /// <param name="sim">相似度(0.1-1.0)</param>
        /// <returns></returns>
        public int CmpColor(int x, int y, string color, double sim)
        {
            return dm.CmpColor(x, y, color, sim);
        }

        ///// <summary>
        ///// 把窗口坐标转换为屏幕坐标
        ///// </summary>
        ///// <param name="hwnd">指定的窗口句柄</param>
        ///// <param name="x">窗口X坐标</param>
        ///// <param name="y">窗口Y坐标</param>
        ///// <returns></returns>
        //public    int ClientToScreen(int hwnd, ref object x, ref object y)
        //{
        //    return dm.ClientToScreen(hwnd, ref x, ref y);
        //}

        ///// <summary>
        ///// 把屏幕坐标转换为窗口坐标
        ///// </summary>
        ///// <param name="hwnd">指定的窗口句柄</param>
        ///// <param name="x">窗口X坐标</param>
        ///// <param name="y">窗口Y坐标</param>
        ///// <returns></returns>
        //public    int ScreenToClient(int hwnd, ref object x, ref object y)
        //{
        //    return dm.ScreenToClient(hwnd, ref x, ref y);
        //}

        /// <summary>
        ///  未知
        /// </summary>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <param name="x2"></param>
        /// <param name="y2"></param>
        /// <param name="msg"></param>
        /// <param name="color"></param>
        /// <returns></returns>
        public int ShowScrMsg(int x1, int y1, int x2, int y2, string msg, string color)
        {
            return dm.ShowScrMsg(x1, y1, x2, y2, msg, color);
        }

        /// <summary>
        /// 在识别前,如果待识别区域有多行文字,可以设定行间距,默认的行间距是1,如果根据情况设定,可以提高识别精度。一般不用设定。
        /// </summary>
        /// <param name="row_gap">最小行间距</param>
        /// <returns></returns>
        public int SetMinRowGap(int row_gap)
        {
            return dm.SetMinRowGap(row_gap);
        }

        /// <summary>
        ///  在识别前,如果待识别区域有多行文字,可以设定列间距,默认的列间距是0,如果根据情况设定,可以提高识别精度。一般不用设定。
        /// </summary>
        /// <param name="col_gap">最小列间距</param>
        /// <returns></returns>
        public int SetMinColGap(int col_gap)
        {
            return dm.SetMinColGap(col_gap);
        }

        /// <summary>
        /// 查找指定区域内的颜色,颜色格式"RRGGBB-DRDGDB",注意,和按键的颜色格式相反
        /// </summary>
        /// <param name="x1">区域的左上X坐标</param>
        /// <param name="y1">区域的左上Y坐标</param>
        /// <param name="x2">区域的右下X坐标</param>
        /// <param name="y2">区域的右下Y坐标</param>
        /// <param name="color">颜色 格式为"RRGGBB-DRDGDB",比如"123456-000000|aabbcc-202020".注意，这里只支持RGB颜色.</param>
        /// <param name="sim">相似度,取值范围0.1-1.0</param>
        /// <param name="dir">查找方向0:从左到右,从上到下1:从左到右,从下到上2:从右到左,从上到下3:从右到左,从下到上4：从中心往外查找5:从上到下,从左到右6:从上到下,从右到左7:从下到上,从左到右8:从下到上,从右到左</param>
        /// <param name="x">返回X坐标</param>
        /// <param name="y">返回Y坐标</param>
        /// <returns></returns>
        public int FindColor(int x1, int y1, int x2, int y2, string color, double sim, int dir, out int x, out int y)
        {
            return dm.FindColor(x1, y1, x2, y2, color, sim, dir, out x, out y);
        }

        /// <summary>
        ///  查找指定区域内的所有颜色,颜色格式"RRGGBB-DRDGDB",注意,和按键的颜色格式相反
        /// </summary>
        /// <param name="x1">区域的左上X坐标</param>
        /// <param name="y1">区域的左上Y坐标</param>
        /// <param name="x2">区域的右下X坐标</param>
        /// <param name="y2">区域的右下Y坐标</param>
        /// <param name="color">颜色 格式为"RRGGBB-DRDGDB" 比如"aabbcc-000000|123456-202020".注意，这里只支持RGB颜色.</param>
        /// <param name="sim">相似度,取值范围0.1-1.0</param>
        /// <param name="dir">查找方向0:从左到右,从上到下1:从左到右,从下到上2:从右到左,从上到下3:从右到左,从下到上5:从上到下,从左到右6:从上到下,从右到左7:从下到上,从左到右8:从下到上,从右到左</param>
        /// <returns></returns>
        public string FindColorEx(int x1, int y1, int x2, int y2, string color, double sim, int dir)
        {
            return dm.FindColorEx(x1, y1, x2, y2, color, sim, dir);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="line_height"></param>
        /// <returns></returns>
        public int SetWordLineHeight(int line_height)
        {
            return dm.SetWordLineHeight(line_height);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="word_gap"></param>
        /// <returns></returns>
        public int SetWordGap(int word_gap)
        {
            return dm.SetWordGap(word_gap);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="row_gap"></param>
        /// <returns></returns>
        public int SetRowGapNoDict(int row_gap)
        {
            return dm.SetRowGapNoDict(row_gap);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="col_gap"></param>
        /// <returns></returns>
        public int SetColGapNoDict(int col_gap)
        {
            return dm.SetColGapNoDict(col_gap);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="line_height"></param>
        /// <returns></returns>
        public int SetWordLineHeightNoDict(int line_height)
        {
            return dm.SetWordLineHeightNoDict(line_height);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="word_gap"></param>
        /// <returns></returns>
        public int SetWordGapNoDict(int word_gap)
        {
            return dm.SetWordGapNoDict(word_gap);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public int GetWordResultCount(string str)
        {
            return dm.GetWordResultCount(str);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="str"></param>
        /// <param name="index"></param>
        /// <param name="x">窗口X坐标</param>
        /// <param name="y">窗口Y坐标</param>
        /// <returns></returns>
        public int GetWordResultPos(string str, int index, out int x, out int y)
        {
            return dm.GetWordResultPos(str, index, out x, out y);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="str"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public string GetWordResultStr(string str, int index)
        {
            return dm.GetWordResultStr(str, index);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <param name="x2"></param>
        /// <param name="y2"></param>
        /// <param name="color"></param>
        /// <param name="sim"></param>
        /// <returns></returns>
        public string GetWords(int x1, int y1, int x2, int y2, string color, double sim)
        {
            return dm.GetWords(x1, y1, x2, y2, color, sim);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <param name="x2"></param>
        /// <param name="y2"></param>
        /// <param name="color"></param>
        /// <returns></returns>
        public string GetWordsNoDict(int x1, int y1, int x2, int y2, string color)
        {
            return dm.GetWordsNoDict(x1, y1, x2, y2, color);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="show"></param>
        /// <returns></returns>
        public int SetShowErrorMsg(int show)
        {
            return dm.SetShowErrorMsg(show);
        }

        /// <summary>
        /// 获取客户端的尺寸
        /// </summary>
        /// <param name="hwnd">指定的窗口句柄</param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        public int GetClientSize(int hwnd, out int width, out int height)
        {
            return dm.GetClientSize(hwnd, out width, out height);
        }

        /// <summary>
        /// 移动窗口
        /// </summary>
        /// <param name="hwnd">指定的窗口句柄</param>
        /// <param name="x">x坐标</param>
        /// <param name="y">y坐标</param>
        /// <returns></returns>
        public int MoveWindow(int hwnd, int x, int y)
        {
            return dm.MoveWindow(hwnd, x, y);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="x">窗口X坐标</param>
        /// <param name="y">窗口Y坐标</param>
        /// <returns></returns>
        public string GetColorHSV(int x, int y)
        {
            return dm.GetColorHSV(x, y);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <param name="x2"></param>
        /// <param name="y2"></param>
        /// <returns></returns>
        public string GetAveRGB(int x1, int y1, int x2, int y2)
        {
            return dm.GetAveRGB(x1, y1, x2, y2);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <param name="x2"></param>
        /// <param name="y2"></param>
        /// <returns></returns>
        public string GetAveHSV(int x1, int y1, int x2, int y2)
        {
            return dm.GetAveHSV(x1, y1, x2, y2);
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public int GetForegroundWindow()
        {
            return dm.GetForegroundWindow();
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public int GetForegroundFocus()
        {
            return dm.GetForegroundFocus();
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public int GetMousePointWindow()
        {
            return dm.GetMousePointWindow();
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="x">窗口X坐标</param>
        /// <param name="y">窗口Y坐标</param>
        /// <returns></returns>
        public int GetPointWindow(int x, int y)
        {
            return dm.GetPointWindow(x, y);
        }

        /// <summary>
        /// 根据指定条件,枚举系统中符合条件的窗口,可以枚举到按键自带的无法枚举到的窗口
        /// </summary>
        /// <param name="parent">获得的窗口句柄是该窗口的子窗口的窗口句柄,取0时为获得桌面句柄</param>
        /// <param name="title">窗口标题. 此参数是模糊匹配.</param>
        /// <param name="class_name">窗口类名. 此参数是模糊匹配.</param>
        /// <param name="filter">取值定义如下:1 : 匹配窗口标题,参数title有效 .   2 : 匹配窗口类名,参数class_name有效.     4 : 只匹配指定父窗口的第一层孩子窗口    8 : 匹配所有者窗口为0的窗口,即顶级窗口     16 : 匹配可见的窗口   32 : 匹配出的窗口按照窗口打开顺序依次排列 ....收费功能，具体详情点击查看 ......  这些值可以相加,比如4+8+16就是类似于任务管理器中的窗口列表   </param>
        /// <returns></returns>
        public string EnumWindow(int parent, string title, string class_name, int filter)
        {
            return dm.EnumWindow(parent, title, class_name, filter);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="hwnd">指定的窗口句柄</param>
        /// <param name="flag"></param>
        /// <returns></returns>
        public int GetWindowState(int hwnd, int flag)
        {
            return dm.GetWindowState(hwnd, flag);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="hwnd">指定的窗口句柄</param>
        /// <param name="flag"></param>
        /// <returns></returns>
        public int GetWindow(int hwnd, int flag)
        {
            return dm.GetWindow(hwnd, flag);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="flag"></param>
        /// <returns></returns>
        public int GetSpecialWindow(int flag)
        {
            return dm.GetSpecialWindow(flag);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="hwnd">指定的窗口句柄</param>
        /// <param name="text"></param>
        /// <returns></returns>
        public int SetWindowText(int hwnd, string text)
        {
            return dm.SetWindowText(hwnd, text);
        }

        /// <summary>
        /// 设置窗口的宽高
        /// </summary>
        /// <param name="hwnd">指定的窗口句柄</param>
        /// <param name="width">宽</param>
        /// <param name="height">高</param>
        /// <returns></returns>
        public int SetWindowSize(int hwnd, int width, int height)
        {
            return dm.SetWindowSize(hwnd, width, height);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="hwnd">指定的窗口句柄</param>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <param name="x2"></param>
        /// <param name="y2"></param>
        /// <returns></returns>
        public int GetWindowRect(int hwnd, out int x1, out int y1, out int x2, out int y2)
        {
            return dm.GetWindowRect(hwnd, out x1, out y1, out x2, out y2);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="hwnd">指定的窗口句柄</param>
        /// <returns></returns>
        public string GetWindowTitle(int hwnd)
        {
            return dm.GetWindowTitle(hwnd);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="hwnd">指定的窗口句柄</param>
        /// <returns></returns>
        public string GetWindowClass(int hwnd)
        {
            return dm.GetWindowClass(hwnd);
        }

        /// <summary>
        ///  设置窗口的状态
        /// </summary>
        /// <param name="hwnd">指定的窗口句柄</param>
        /// <param name="flag">状态 F12查看详情</param>
        /// <returns></returns>
        public int SetWindowState(int hwnd, int flag)
        {
            #region 说明

            /*

             取值定义如下

                0 : 关闭指定窗口
                1 : 激活指定窗口
                2 : 最小化指定窗口,但不激活
                3 : 最小化指定窗口,并释放内存,但同时也会激活窗口.
                4 : 最大化指定窗口,同时激活窗口.
                5 : 恢复指定窗口 ,但不激活
                6 : 隐藏指定窗口
                7 : 显示指定窗口
                8 : 置顶指定窗口
                9 : 取消置顶指定窗口
                10 : 禁止指定窗口
                11 : 取消禁止指定窗口
                12 : 恢复并激活指定窗口
                13 : 强制结束窗口所在进程

             */

            #endregion 说明

            return dm.SetWindowState(hwnd, flag);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="hwnd">指定的窗口句柄</param>
        /// <param name="x">窗口X坐标</param>
        /// <param name="y">窗口Y坐标</param>
        /// <param name="w"></param>
        /// <param name="h"></param>
        /// <returns></returns>
        public int CreateFoobarRect(int hwnd, int x, int y, int w, int h)
        {
            return dm.CreateFoobarRect(hwnd, x, y, w, h);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="hwnd">指定的窗口句柄</param>
        /// <param name="x">窗口X坐标</param>
        /// <param name="y">窗口Y坐标</param>
        /// <param name="w"></param>
        /// <param name="h"></param>
        /// <param name="rw"></param>
        /// <param name="rh"></param>
        /// <returns></returns>
        public int CreateFoobarRoundRect(int hwnd, int x, int y, int w, int h, int rw, int rh)
        {
            return dm.CreateFoobarRoundRect(hwnd, x, y, w, h, rw, rh);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="hwnd">指定的窗口句柄</param>
        /// <param name="x">窗口X坐标</param>
        /// <param name="y">窗口Y坐标</param>
        /// <param name="w"></param>
        /// <param name="h"></param>
        /// <returns></returns>
        public int CreateFoobarEllipse(int hwnd, int x, int y, int w, int h)
        {
            return dm.CreateFoobarEllipse(hwnd, x, y, w, h);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="hwnd">指定的窗口句柄</param>
        /// <param name="x">窗口X坐标</param>
        /// <param name="y">窗口Y坐标</param>
        /// <param name="pic"></param>
        /// <param name="trans_color"></param>
        /// <param name="sim"></param>
        /// <returns></returns>
        public int CreateFoobarCustom(int hwnd, int x, int y, string pic, string trans_color, double sim)
        {
            return dm.CreateFoobarCustom(hwnd, x, y, pic, trans_color, sim);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="hwnd">指定的窗口句柄</param>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <param name="x2"></param>
        /// <param name="y2"></param>
        /// <param name="color"></param>
        /// <returns></returns>
        public int FoobarFillRect(int hwnd, int x1, int y1, int x2, int y2, string color)
        {
            return dm.FoobarFillRect(hwnd, x1, y1, x2, y2, color);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="hwnd">指定的窗口句柄</param>
        /// <param name="x">窗口X坐标</param>
        /// <param name="y">窗口Y坐标</param>
        /// <param name="w"></param>
        /// <param name="h"></param>
        /// <param name="text"></param>
        /// <param name="color"></param>
        /// <param name="align"></param>
        /// <returns></returns>
        public int FoobarDrawText(int hwnd, int x, int y, int w, int h, string text, string color, int align)
        {
            return dm.FoobarDrawText(hwnd, x, y, w, h, text, color, align);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="hwnd">指定的窗口句柄</param>
        /// <param name="x">窗口X坐标</param>
        /// <param name="y">窗口Y坐标</param>
        /// <param name="pic"></param>
        /// <param name="trans_color"></param>
        /// <returns></returns>
        public int FoobarDrawPic(int hwnd, int x, int y, string pic, string trans_color)
        {
            return dm.FoobarDrawPic(hwnd, x, y, pic, trans_color);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="hwnd">指定的窗口句柄</param>
        /// <returns></returns>
        public int FoobarUpdate(int hwnd)
        {
            return dm.FoobarUpdate(hwnd);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="hwnd">指定的窗口句柄</param>
        /// <returns></returns>
        public int FoobarLock(int hwnd)
        {
            return dm.FoobarLock(hwnd);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="hwnd">指定的窗口句柄</param>
        /// <returns></returns>
        public int FoobarUnlock(int hwnd)
        {
            return dm.FoobarUnlock(hwnd);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="hwnd">指定的窗口句柄</param>
        /// <param name="font_name"></param>
        /// <param name="size"></param>
        /// <param name="flag"></param>
        /// <returns></returns>
        public int FoobarSetFont(int hwnd, string font_name, int size, int flag)
        {
            return dm.FoobarSetFont(hwnd, font_name, size, flag);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="hwnd">指定的窗口句柄</param>
        /// <param name="x">窗口X坐标</param>
        /// <param name="y">窗口Y坐标</param>
        /// <param name="w"></param>
        /// <param name="h"></param>
        /// <returns></returns>
        public int FoobarTextRect(int hwnd, int x, int y, int w, int h)
        {
            return dm.FoobarTextRect(hwnd, x, y, w, h);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="hwnd">指定的窗口句柄</param>
        /// <param name="text"></param>
        /// <param name="color"></param>
        /// <returns></returns>
        public int FoobarPrintText(int hwnd, string text, string color)
        {
            return dm.FoobarPrintText(hwnd, text, color);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="hwnd">指定的窗口句柄</param>
        /// <returns></returns>
        public int FoobarClearText(int hwnd)
        {
            return dm.FoobarClearText(hwnd);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="hwnd">指定的窗口句柄</param>
        /// <param name="gap"></param>
        /// <returns></returns>
        public int FoobarTextLineGap(int hwnd, int gap)
        {
            return dm.FoobarTextLineGap(hwnd, gap);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="file_"></param>
        /// <returns></returns>
        public int Play(string file_)
        {
            return dm.Play(file_);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <param name="x2"></param>
        /// <param name="y2"></param>
        /// <param name="quality"></param>
        /// <param name="delay"></param>
        /// <param name="time"></param>
        /// <returns></returns>
        public int FaqCapture(int x1, int y1, int x2, int y2, int quality, int delay, int time)
        {
            return dm.FaqCapture(x1, y1, x2, y2, quality, delay, time);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="handle"></param>
        /// <returns></returns>
        public int FaqRelease(int handle)
        {
            return dm.FaqRelease(handle);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="server"></param>
        /// <param name="handle"></param>
        /// <param name="request_type"></param>
        /// <param name="time_out"></param>
        /// <returns></returns>
        public string FaqSend(string server, int handle, int request_type, int time_out)
        {
            return dm.FaqSend(server, handle, request_type, time_out);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="fre"></param>
        /// <param name="delay"></param>
        /// <returns></returns>
        public int Beep(int fre, int delay)
        {
            return dm.Beep(fre, delay);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="hwnd">指定的窗口句柄</param>
        /// <returns></returns>
        public int FoobarClose(int hwnd)
        {
            return dm.FoobarClose(hwnd);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="dx"></param>
        /// <param name="dy"></param>
        /// <returns></returns>
        public int MoveDD(int dx, int dy)
        {
            return dm.MoveDD(dx, dy);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="handle"></param>
        /// <returns></returns>
        public int FaqGetSize(int handle)
        {
            return dm.FaqGetSize(handle);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="pic_name"></param>
        /// <returns></returns>
        public int LoadPic(string pic_name)
        {
            return dm.LoadPic(pic_name);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="pic_name"></param>
        /// <returns></returns>
        public int FreePic(string pic_name)
        {
            return dm.FreePic(pic_name);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <param name="x2"></param>
        /// <param name="y2"></param>
        /// <returns></returns>
        public int GetScreenData(int x1, int y1, int x2, int y2)
        {
            return dm.GetScreenData(x1, y1, x2, y2);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="handle"></param>
        /// <returns></returns>
        public int FreeScreenData(int handle)
        {
            return dm.FreeScreenData(handle);
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public int WheelUp()
        {
            return dm.WheelUp();
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public int WheelDown()
        {
            return dm.WheelDown();
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="type_"></param>
        /// <param name="delay"></param>
        /// <returns></returns>
        public int SetMouseDelay(string type_, int delay)
        {
            return dm.SetMouseDelay(type_, delay);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="type_"></param>
        /// <param name="delay"></param>
        /// <returns></returns>
        public int SetKeypadDelay(string type_, int delay)
        {
            return dm.SetKeypadDelay(type_, delay);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="index"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public string GetEnv(int index, string name)
        {
            return dm.GetEnv(index, name);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="index"></param>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public int SetEnv(int index, string name, string value)
        {
            return dm.SetEnv(index, name, value);
        }

        /// <summary>
        /// 输入字符串
        /// </summary>
        /// <param name="hwnd">指定的窗口句柄</param>
        /// <param name="str">内容</param>
        /// <returns></returns>
        public int SendString(int hwnd, string str)
        {
            return dm.SendString(hwnd, str);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="index"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public int DelEnv(int index, string name)
        {
            return dm.DelEnv(index, name);
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public string GetPath()
        {
            return dm.GetPath();
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="index"></param>
        /// <param name="dict_name"></param>
        /// <returns></returns>
        public int SetDict(int index, string dict_name)
        {
            return dm.SetDict(index, dict_name);
        }

        /// <summary>
        ///  查找指定区域内的图片,位图必须是24位色格式,支持透明色,当图像上下左右4个顶点的颜色一样时,则这个颜色将作为透明色处理.        /// </summary>
        /// <param name="x1">x1 整形数:区域的左上X坐标</param>
        /// <param name="y1">y1 整形数:区域的左上Y坐标</param>
        /// <param name="x2">x2 整形数:区域的右下X坐标</param>
        /// <param name="y2">y2 整形数:区域的右下Y坐标</param>
        /// <param name="pic_name">图片名,可以是多个图片,比如"test.bmp|test2.bmp|test3.bmp"</param>
        /// <param name="delta_color">颜色色偏比如"203040" 表示RGB的色偏分别是20 30 40 (这里是16进制表示) eg: 000000</param>
        /// <param name="sim">相似度,取值范围0.1-1.0</param>
        /// <param name="dir">查找方向 0: 从左到右,从上到下 1: 从左到右,从下到上 2: 从右到左,从上到下 3: 从右到左, 从下到上</param>
        /// <param name="x">X坐标</param>
        /// <param name="y">Y坐标</param>
        /// <returns>返回找到的图片的序号</returns>
        public int FindPic(int x1, int y1, int x2, int y2, string pic_name, string delta_color, double sim, int dir, out int x, out int y)
        {
            return dm.FindPic(x1, y1, x2, y2, pic_name, delta_color, sim, dir, out x, out y);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <param name="x2"></param>
        /// <param name="y2"></param>
        /// <param name="pic_name"></param>
        /// <param name="delta_color"></param>
        /// <param name="sim"></param>
        /// <param name="dir"></param>
        /// <returns></returns>
        public string FindPicEx(int x1, int y1, int x2, int y2, string pic_name, string delta_color, double sim, int dir)
        {
            return dm.FindPicEx(x1, y1, x2, y2, pic_name, delta_color, sim, dir);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="hwnd">指定的窗口句柄</param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        public int SetClientSize(int hwnd, int width, int height)
        {
            return dm.SetClientSize(hwnd, width, height);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="hwnd">指定的窗口句柄</param>
        /// <param name="addr"></param>
        /// <param name="type_"></param>
        /// <returns></returns>
        public long ReadInt(int hwnd, string addr, int type_)
        {
            return dm.ReadInt(hwnd, addr, type_);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="hwnd">指定的窗口句柄</param>
        /// <param name="addr"></param>
        /// <returns></returns>
        public float ReadFloat(int hwnd, string addr)
        {
            return dm.ReadFloat(hwnd, addr);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="hwnd">指定的窗口句柄</param>
        /// <param name="addr"></param>
        /// <returns></returns>
        public double ReadDouble(int hwnd, string addr)
        {
            return dm.ReadDouble(hwnd, addr);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="hwnd">指定的窗口句柄</param>
        /// <param name="addr_range"></param>
        /// <param name="int_value_min"></param>
        /// <param name="int_value_max"></param>
        /// <param name="type_"></param>
        /// <returns></returns>
        public string FindInt(int hwnd, string addr_range, int int_value_min, int int_value_max, int type_)
        {
            return dm.FindInt(hwnd, addr_range, int_value_min, int_value_max, type_);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="hwnd">指定的窗口句柄</param>
        /// <param name="addr_range"></param>
        /// <param name="float_value_min"></param>
        /// <param name="float_value_max"></param>
        /// <returns></returns>
        public string FindFloat(int hwnd, string addr_range, Single float_value_min, Single float_value_max)
        {
            return dm.FindFloat(hwnd, addr_range, float_value_min, float_value_max);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="hwnd">指定的窗口句柄</param>
        /// <param name="addr_range"></param>
        /// <param name="double_value_min"></param>
        /// <param name="double_value_max"></param>
        /// <returns></returns>
        public string FindDouble(int hwnd, string addr_range, double double_value_min, double double_value_max)
        {
            return dm.FindDouble(hwnd, addr_range, double_value_min, double_value_max);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="hwnd">指定的窗口句柄</param>
        /// <param name="addr_range"></param>
        /// <param name="string_value"></param>
        /// <param name="type_"></param>
        /// <returns></returns>
        public string FindString(int hwnd, string addr_range, string string_value, int type_)
        {
            return dm.FindString(hwnd, addr_range, string_value, type_);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="hwnd">指定的窗口句柄</param>
        /// <param name="module_name"></param>
        /// <returns></returns>
        public long GetModuleBaseAddr(int hwnd, string module_name)
        {
            return dm.GetModuleBaseAddr(hwnd, module_name);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="x">窗口X坐标</param>
        /// <param name="y">窗口Y坐标</param>
        /// <param name="w"></param>
        /// <param name="h"></param>
        /// <returns></returns>
        public string MoveToEx(int x, int y, int w, int h)
        {
            return dm.MoveToEx(x, y, w, h);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="pic_name"></param>
        /// <returns></returns>
        public string MatchPicName(string pic_name)
        {
            return dm.MatchPicName(pic_name);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="index"></param>
        /// <param name="dict_info"></param>
        /// <returns></returns>
        public int AddDict(int index, string dict_info)
        {
            return dm.AddDict(index, dict_info);
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public int EnterCri()
        {
            return dm.EnterCri();
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public int LeaveCri()
        {
            return dm.LeaveCri();
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="hwnd">指定的窗口句柄</param>
        /// <param name="addr"></param>
        /// <param name="type_"></param>
        /// <param name="v"></param>
        /// <returns></returns>
        public int WriteInt(int hwnd, string addr, int type_, int v)
        {
            return dm.WriteInt(hwnd, addr, type_, v);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="hwnd">指定的窗口句柄</param>
        /// <param name="addr"></param>
        /// <param name="v"></param>
        /// <returns></returns>
        public int WriteFloat(int hwnd, string addr, Single v)
        {
            return dm.WriteFloat(hwnd, addr, v);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="hwnd">指定的窗口句柄</param>
        /// <param name="addr"></param>
        /// <param name="v"></param>
        /// <returns></returns>
        public int WriteDouble(int hwnd, string addr, double v)
        {
            return dm.WriteDouble(hwnd, addr, v);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="hwnd">指定的窗口句柄</param>
        /// <param name="addr"></param>
        /// <param name="type_"></param>
        /// <param name="v"></param>
        /// <returns></returns>
        public int WriteString(int hwnd, string addr, int type_, string v)
        {
            return dm.WriteString(hwnd, addr, type_, v);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="asm_ins"></param>
        /// <returns></returns>
        public int AsmAdd(string asm_ins)
        {
            return dm.AsmAdd(asm_ins);
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public int AsmClear()
        {
            return dm.AsmClear();
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="hwnd">指定的窗口句柄</param>
        /// <param name="mode"></param>
        /// <returns></returns>
        public long AsmCall(int hwnd, int mode)
        {
            return dm.AsmCall(hwnd, mode);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <param name="x2"></param>
        /// <param name="y2"></param>
        /// <param name="first_color"></param>
        /// <param name="offset_color"></param>
        /// <param name="sim"></param>
        /// <param name="dir"></param>
        /// <param name="x">窗口X坐标</param>
        /// <param name="y">窗口Y坐标</param>
        /// <returns></returns>
        public int FindMultiColor(int x1, int y1, int x2, int y2, string first_color, string offset_color, double sim, int dir, out int x, out int y) { return dm.FindMultiColor(x1, y1, x2, y2, first_color, offset_color, sim, dir, out x, out y); }

        /// <summary>
        ///
        /// </summary>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <param name="x2"></param>
        /// <param name="y2"></param>
        /// <param name="first_color"></param>
        /// <param name="offset_color"></param>
        /// <param name="sim"></param>
        /// <param name="dir"></param>
        /// <returns></returns>
        public string FindMultiColorEx(int x1, int y1, int x2, int y2, string first_color, string offset_color, double sim, int dir)
        {
            return dm.FindMultiColorEx(x1, y1, x2, y2, first_color, offset_color, sim, dir);
        }

        ///// <summary>
        /////
        ///// </summary>
        ///// <param name="base_addr"></param>
        ///// <returns></returns>
        //public    string AsmCode(int base_addr)
        //{
        //    return dm.AsmCode(base_addr);
        //}

        /// <summary>
        ///
        /// </summary>
        /// <param name="asm_code"></param>
        /// <param name="base_addr"></param>
        /// <param name="is_upper"></param>
        /// <returns></returns>
        public string Assemble(int base_addr, int is_upper)
        {
            return dm.Assemble(base_addr, is_upper);
        }

        /// <summary>
        /// 设置窗口的透明度 (注 :  此接口不支持WIN98)
        /// </summary>
        /// <param name="hwnd">指定的窗口句柄</param>
        /// <param name="v">透明度取值(0-255) 越小透明度越大 0为完全透明(不可见) 255为完全显示(不透明)</param>
        /// <returns></returns>
        public int SetWindowTransparent(int hwnd, int v)
        {
            return dm.SetWindowTransparent(hwnd, v);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="hwnd">指定的窗口句柄</param>
        /// <param name="addr"></param>
        /// <param name="len"></param>
        /// <returns></returns>
        public string ReadData(int hwnd, string addr, int len)
        {
            return dm.ReadData(hwnd, addr, len);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="hwnd">指定的窗口句柄</param>
        /// <param name="addr"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public int WriteData(int hwnd, string addr, string data)
        {
            return dm.WriteData(hwnd, addr, data);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="hwnd">指定的窗口句柄</param>
        /// <param name="addr_range"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public string FindData(int hwnd, string addr_range, string data)
        {
            return dm.FindData(hwnd, addr_range, data);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="pwd"></param>
        /// <returns></returns>
        public int SetPicPwd(string pwd)
        {
            return dm.SetPicPwd(pwd);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        public int Log(string info)
        {
            return dm.Log(info);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <param name="x2"></param>
        /// <param name="y2"></param>
        /// <param name="str"></param>
        /// <param name="color"></param>
        /// <param name="sim"></param>
        /// <returns></returns>
        public string FindStrE(int x1, int y1, int x2, int y2, string str, string color, double sim)
        {
            return dm.FindStrE(x1, y1, x2, y2, str, color, sim);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <param name="x2"></param>
        /// <param name="y2"></param>
        /// <param name="color"></param>
        /// <param name="sim"></param>
        /// <param name="dir"></param>
        /// <returns></returns>
        public string FindColorE(int x1, int y1, int x2, int y2, string color, double sim, int dir)
        {
            return dm.FindColorE(x1, y1, x2, y2, color, sim, dir);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <param name="x2"></param>
        /// <param name="y2"></param>
        /// <param name="pic_name"></param>
        /// <param name="delta_color"></param>
        /// <param name="sim"></param>
        /// <param name="dir"></param>
        /// <returns></returns>
        public string FindPicE(int x1, int y1, int x2, int y2, string pic_name, string delta_color, double sim, int dir)
        {
            return dm.FindPicE(x1, y1, x2, y2, pic_name, delta_color, sim, dir);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <param name="x2"></param>
        /// <param name="y2"></param>
        /// <param name="first_color"></param>
        /// <param name="offset_color"></param>
        /// <param name="sim"></param>
        /// <param name="dir"></param>
        /// <returns></returns>
        public string FindMultiColorE(int x1, int y1, int x2, int y2, string first_color, string offset_color, double sim, int dir)
        {
            return dm.FindMultiColorE(x1, y1, x2, y2, first_color, offset_color, sim, dir);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="exact_ocr"></param>
        /// <returns></returns>
        public int SetExactOcr(int exact_ocr)
        {
            return dm.SetExactOcr(exact_ocr);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="hwnd">指定的窗口句柄</param>
        /// <param name="addr"></param>
        /// <param name="type_"></param>
        /// <param name="len"></param>
        /// <returns></returns>
        public string ReadString(int hwnd, string addr, int type_, int len)
        {
            return dm.ReadString(hwnd, addr, type_, len);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="hwnd">指定的窗口句柄</param>
        /// <param name="dir"></param>
        /// <returns></returns>
        public int FoobarTextPrintDir(int hwnd, int dir)
        {
            return dm.FoobarTextPrintDir(hwnd, dir);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <param name="x2"></param>
        /// <param name="y2"></param>
        /// <param name="color"></param>
        /// <param name="sim"></param>
        /// <returns></returns>
        public string OcrEx(int x1, int y1, int x2, int y2, string color, double sim)
        {
            return dm.OcrEx(x1, y1, x2, y2, color, sim);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="mode"></param>
        /// <returns></returns>
        public int SetDisplayInput(string mode)
        {
            return dm.SetDisplayInput(mode);
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public int GetTime()
        {
            return dm.GetTime();
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public int GetScreenWidth()
        {
            return dm.GetScreenWidth();
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public int GetScreenHeight()
        {
            return dm.GetScreenHeight();
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="hwnd">指定的窗口句柄</param>
        /// <param name="display"></param>
        /// <param name="mouse"></param>
        /// <param name="keypad"></param>
        /// <param name="public_desc"></param>
        /// <param name="mode"></param>
        /// <returns></returns>
        public int BindWindowEx(int hwnd, string display, string mouse, string keypad, string public_desc, int mode)
        {
            return dm.BindWindowEx(hwnd, display, mouse, keypad, public_desc, mode);
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public string GetDiskSerial()
        {
            return dm.GetDiskSerial();
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public string Md5(string str)
        {
            return dm.Md5(str);
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public string GetMac()
        {
            return dm.GetMac();
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="hwnd">指定的窗口句柄</param>
        /// <param name="id"></param>
        /// <returns></returns>
        public int ActiveInputMethod(int hwnd, string id)
        {
            return dm.ActiveInputMethod(hwnd, id);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="hwnd">指定的窗口句柄</param>
        /// <param name="id"></param>
        /// <returns></returns>
        public int CheckInputMethod(int hwnd, string id)
        {
            return dm.CheckInputMethod(hwnd, id);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public int FindInputMethod(string id)
        {
            return dm.FindInputMethod(id);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="x">窗口X坐标</param>
        /// <param name="y">窗口Y坐标</param>
        /// <returns></returns>
        public int GetCursorPos(out int x, out int y)
        {
            return dm.GetCursorPos(out x, out y);
        }

        /// <summary>
        /// 绑定指定的窗口
        /// </summary>
        /// <param name="hwnd">窗口句柄</param>
        /// <returns></returns>
        public int BindWindow(int hwnd)
        {
            //BindWindow(hwnd, "normal", "windows", "windows", 0);
            return BindWindow(hwnd, "normal", "normal", "normal", 0);
        }

        /// <summary>
        /// 绑定指定的窗口,  (F12 参数说明)    高级用户可以参考BindWindowEx更加灵活强大.
        /// </summary>
        /// <param name="hwnd">指定的窗口句柄</param>
        /// <param name="display">屏幕颜色获取方式</param>
        /// <param name="mouse">鼠标仿真模式</param>
        /// <param name="keypad">键盘仿真模式</param>
        /// <param name="mode">模式</param>
        /// <returns></returns>
        public int BindWindow(int hwnd, string display, string mouse, string keypad, int mode)
        {
            #region 说明

            /*
             参数定义:

             hwnd 整形数: 指定的窗口句柄

             display 字符串: 屏幕颜色获取方式 取值有以下几种
             "normal" : 正常模式,平常我们用的前台截屏模式
             "gdi" : gdi模式,用于窗口采用GDI方式刷新时. 此模式占用CPU较大.
             "gdi2" : gdi2模式,此模式兼容性较强,但是速度比gdi模式要慢许多,如果gdi模式发现后台不刷新时,可以考虑用gdi2模式.
             "dx2" : dx2模式,用于窗口采用dx模式刷新,如果dx方式会出现窗口所在进程崩溃的状况,可以考虑采用这种.采用这种方式要保证窗口有一部分在屏幕外.win7或者vista不需要移动也可后台.此模式占用CPU较大.
             "dx3" : dx3模式,同dx2模式,但是如果发现有些窗口后台不刷新时,可以考虑用dx3模式,此模式比dx2模式慢许多. 此模式占用CPU较大.
             "dx" : dx模式,等同于BindWindowEx中，display设置的"dx.graphic.2d|dx.graphic.3d",具体参考BindWindowEx
             注意此模式需要管理员权限

             mouse 字符串: 鼠标仿真模式 取值有以下几种
             "normal" : 正常模式,平常我们用的前台鼠标模式
             "windows": Windows模式,采取模拟windows消息方式 同按键自带后台插件.
             "windows2": Windows2 模式,采取模拟windows消息方式(锁定鼠标位置) 此模式等同于BindWindowEx中的mouse为以下组合
             "dx.mouse.position.lock.api|dx.mouse.position.lock.message|dx.mouse.state.message"
             注意此模式需要管理员权限
             "windows3": Windows3模式，采取模拟windows消息方式,可以支持有多个子窗口的窗口后台.
             "dx": dx模式,采用模拟dx后台鼠标模式,这种方式会锁定鼠标输入.有些窗口在此模式下绑定时，需要先激活窗口再绑定(或者绑定以后激活)，否则可能会出现绑定后鼠标无效的情况.此模式等同于BindWindowEx中的mouse为以下组合
             "dx.public  .active.api|dx.public  .active.message|dx.mouse.position.lock.api|dx.mouse.position.lock.message|dx.mouse.state.api|dx.mouse.state.message|dx.mouse.api|dx.mouse.focus.input.api|dx.mouse.focus.input.message|dx.mouse.clip.lock.api|dx.mouse.input.lock.api|dx.mouse.cursor"
             注意此模式需要管理员权限
             "dx2"：dx2模式,这种方式类似于dx模式,但是不会锁定外部鼠标输入.
             有些窗口在此模式下绑定时，需要先激活窗口再绑定(或者绑定以后手动激活)，否则可能会出现绑定后鼠标无效的情况. 此模式等同于BindWindowEx中的mouse为以下组合
             "dx.public  .active.api|dx.public  .active.message|dx.mouse.position.lock.api|dx.mouse.state.api|dx.mouse.api|dx.mouse.focus.input.api|dx.mouse.focus.input.message|dx.mouse.clip.lock.api|dx.mouse.input.lock.api| dx.mouse.cursor"
             注意此模式需要管理员权限

             keypad 字符串: 键盘仿真模式 取值有以下几种
             "normal" : 正常模式,平常我们用的前台键盘模式
             "windows": Windows模式,采取模拟windows消息方式 同按键的后台插件.
             "dx": dx模式。有些窗口在此模式下绑定时，需要先激活窗口再绑定(或者绑定以后激活)，否则可能会出现绑定后键盘无效的情况. 此模式等同于BindWindowEx中的keypad为以下组合
             "dx.public  .active.api|dx.public  .active.message| dx.keypad.state.api|dx.keypad.api|dx.keypad.input.lock.api"
             注意此模式需要管理员权限
             mode 整形数: 模式。 取值有以下两种
             0 : 推荐模式此模式比较通用，而且后台效果是最好的.
             1 : 和模式0效果一样，如果模式0会失败时，可以尝试此模式. 收费功能，具体详情点击查看
             2 : 同模式0,此模式为老的模式0,尽量不要用此模式，除非有兼容性问题.
             3 : 同模式1,此模式为老的模式1,尽量不要用此模式，除非有兼容性问题. 收费功能，具体详情点击查看
             4 : 同模式0,如果模式0有崩溃问题，可以尝试此模式.
             5 : 同模式1, 如果模式0有崩溃问题，可以尝试此模式. 收费功能，具体详情点击查看
             6 : 同模式0，如果模式0有崩溃问题，可以尝试此模式. 收费功能，具体详情点击查看
             7 : 同模式1，如果模式1有崩溃问题，可以尝试此模式. 收费功能，具体详情点击查看
             101 : 超级绑定模式. 可隐藏目标进程中的dm.dll.避免被恶意检测.效果要比dx.public  .hide.dll好. 推荐使用. 收费功能，具体详情点击查看
             103 : 同模式101，如果模式101有崩溃问题，可以尝试此模式. 收费功能，具体详情点击查看
             需要注意的是: 模式1 3 5 7 101 103在大部分窗口下绑定都没问题。但也有少数特殊的窗口，比如有很多子窗口的窗口，对于这种窗口，在绑定时，一定要把
             鼠标指向一个可以输入文字的窗口，比如一个文本框，最好能激活这个文本框，这样可以保证绑定的成功.

            返回值:
             整形数:
             0: 失败
             1: 成功
             如果返回0，可以调用GetLastError来查看具体失败错误码,帮助分析问题.

             */

            #endregion 说明

            return dm.BindWindow(hwnd, display, mouse, keypad, mode);
        }

        /// <summary>
        /// 查找符合 类名/标题名 的顶层可见窗口 (存在则绑定)
        /// </summary>
        /// <param name="class_name">窗口类名，如果为空，则匹配所有. 这里的匹配是模糊匹配.</param>
        /// <param name="title_name">窗口标题,如果为空，则匹配所有.这里的匹配是模糊匹配.</param>
        /// <param name="isBind"> 0不绑定  ， 是否自动绑定 默认为绑定</param>
        /// <returns>整形数表示的窗口句柄，没找到返回0</returns>
        public int FindWindow(string class_name, string title_name, int isBind = 1)
        {
            int hwnd = dm.FindWindow(class_name, title_name);
            if (isBind == 1 && hwnd > 0)
            {
                if (BindWindow(hwnd) == 1)
                {
                    Console.WriteLine(hwnd + " 窗口绑定成功.");
                }
                else
                {
                    Console.WriteLine(hwnd + " 窗口绑定失败.");
                    Console.ReadLine();

                }
            }
            return hwnd;
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public int GetScreenDepth()
        {
            return dm.GetScreenDepth();
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="depth"></param>
        /// <returns></returns>
        public int SetScreen(int width, int height, int depth)
        {
            return dm.SetScreen(width, height, depth);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="type_"></param>
        /// <returns></returns>
        public int ExitOs(int type_)
        {
            return dm.ExitOs(type_);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="type_"></param>
        /// <returns></returns>
        public string GetDir(int type_)
        {
            return dm.GetDir(type_);
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public int GetOsType()
        {
            return dm.GetOsType();
        }

        /// <summary>
        /// 查找符合类名或者标题名的顶层可见窗口,如果指定了parent,则在parent的第一层子窗口中查找.
        /// </summary>
        /// <param name="parent">获得的窗口句柄是该窗口的子窗口的窗口句柄,取0时为获得桌面句柄</param>
        /// <param name="class_name">窗口类名. 此参数是模糊匹配.</param>
        /// <param name="title_name">标题</param>
        /// <param name="isBind">0 不自动绑定,   默认为自动绑定窗口</param>
        /// <returns></returns>
        public int FindWindowEx(int parent, string class_name, string title_name, int isBind = 1)
        {
            int hwnd = dm.FindWindowEx(parent, class_name, title_name);

            if (isBind == 1 && hwnd > 0)
            {
                BindWindow(hwnd);
            }
            return hwnd;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="index"></param>
        /// <param name="dict_name"></param>
        /// <returns></returns>
        public int SetExportDict(int index, string dict_name)
        {
            return dm.SetExportDict(index, dict_name);
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public string GetCursorShape()
        {
            return dm.GetCursorShape();
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="rate"></param>
        /// <returns></returns>
        public int DownCpu(int rate)
        {
            return dm.DownCpu(rate);
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public string GetCursorSpot()
        {
            return dm.GetCursorSpot();
        }

        /// <summary>
        ///  注: 此接口为老的SendString，如果新的SendString不能输入，可以尝试此接口.
        /// </summary>
        /// <param name="hwnd">指定的窗口句柄</param>
        /// <param name="str"></param>
        /// <returns></returns>
        public int SendString2(int hwnd, string str)
        {
            return dm.SendString2(hwnd, str);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="server"></param>
        /// <param name="handle"></param>
        /// <param name="request_type"></param>
        /// <param name="time_out"></param>
        /// <returns></returns>
        public int FaqPost(string server, int handle, int request_type, int time_out)
        {
            return dm.FaqPost(server, handle, request_type, time_out);
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public string FaqFetch()
        {
            return dm.FaqFetch();
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <param name="x2"></param>
        /// <param name="y2"></param>
        /// <param name="color"></param>
        /// <param name="word"></param>
        /// <returns></returns>
        public string FetchWord(int x1, int y1, int x2, int y2, string color, string word)
        {
            return dm.FetchWord(x1, y1, x2, y2, color, word);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <param name="x2"></param>
        /// <param name="y2"></param>
        /// <param name="file_"></param>
        /// <param name="quality"></param>
        /// <returns></returns>
        public int CaptureJpg(int x1, int y1, int x2, int y2, string file_, int quality)
        {
            return dm.CaptureJpg(x1, y1, x2, y2, file_, quality);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <param name="x2"></param>
        /// <param name="y2"></param>
        /// <param name="str"></param>
        /// <param name="color"></param>
        /// <param name="sim"></param>
        /// <param name="font_name"></param>
        /// <param name="font_size"></param>
        /// <param name="flag"></param>
        /// <param name="x">窗口X坐标</param>
        /// <param name="y">窗口Y坐标</param>
        /// <returns></returns>
        public int FindStrWithFont(int x1, int y1, int x2, int y2, string str, string color, double sim, string font_name, int font_size, int flag, out int x, out int y)
        {
            return dm.FindStrWithFont(x1, y1, x2, y2, str, color, sim, font_name, font_size, flag, out x, out y);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <param name="x2"></param>
        /// <param name="y2"></param>
        /// <param name="str"></param>
        /// <param name="color"></param>
        /// <param name="sim"></param>
        /// <param name="font_name"></param>
        /// <param name="font_size"></param>
        /// <param name="flag"></param>
        /// <returns></returns>
        public string FindStrWithFontE(int x1, int y1, int x2, int y2, string str, string color, double sim, string font_name, int font_size, int flag)
        {
            return dm.FindStrWithFontE(x1, y1, x2, y2, str, color, sim, font_name, font_size, flag);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <param name="x2"></param>
        /// <param name="y2"></param>
        /// <param name="str"></param>
        /// <param name="color"></param>
        /// <param name="sim"></param>
        /// <param name="font_name"></param>
        /// <param name="font_size"></param>
        /// <param name="flag"></param>
        /// <returns></returns>
        public string FindStrWithFontEx(int x1, int y1, int x2, int y2, string str, string color, double sim, string font_name, int font_size, int flag)
        {
            return dm.FindStrWithFontEx(x1, y1, x2, y2, str, color, sim, font_name, font_size, flag);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="str"></param>
        /// <param name="font_name"></param>
        /// <param name="font_size"></param>
        /// <param name="flag"></param>
        /// <returns></returns>
        public string GetDictInfo(string str, string font_name, int font_size, int flag)
        {
            return dm.GetDictInfo(str, font_name, font_size, flag);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="index"></param>
        /// <param name="file_"></param>
        /// <returns></returns>
        public int SaveDict(int index, string file_)
        {
            return dm.SaveDict(index, file_);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="hwnd">指定的窗口句柄</param>
        /// <returns></returns>
        public int GetWindowProcessId(int hwnd)
        {
            return dm.GetWindowProcessId(hwnd);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="hwnd">指定的窗口句柄</param>
        /// <returns></returns>
        public string GetWindowProcessPath(int hwnd)
        {
            return dm.GetWindowProcessPath(hwnd);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="lock1"></param>
        /// <returns></returns>
        public int LockInput(int lock1)
        {
            return dm.LockInput(lock1);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="pic_name"></param>
        /// <returns></returns>
        public string GetPicSize(string pic_name)
        {
            return dm.GetPicSize(pic_name);
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public int GetID()
        {
            return dm.GetID();
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <param name="x2"></param>
        /// <param name="y2"></param>
        /// <param name="file_"></param>
        /// <returns></returns>
        public int CapturePng(int x1, int y1, int x2, int y2, string file_)
        {
            return dm.CapturePng(x1, y1, x2, y2, file_);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <param name="x2"></param>
        /// <param name="y2"></param>
        /// <param name="file_"></param>
        /// <param name="delay"></param>
        /// <param name="time"></param>
        /// <returns></returns>
        public int CaptureGif(int x1, int y1, int x2, int y2, string file_, int delay, int time)
        {
            return dm.CaptureGif(x1, y1, x2, y2, file_, delay, time);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="pic_name"></param>
        /// <param name="bmp_name"></param>
        /// <returns></returns>
        public int ImageToBmp(string pic_name, string bmp_name)
        {
            return dm.ImageToBmp(pic_name, bmp_name);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <param name="x2"></param>
        /// <param name="y2"></param>
        /// <param name="str"></param>
        /// <param name="color"></param>
        /// <param name="sim"></param>
        /// <param name="x">窗口X坐标</param>
        /// <param name="y">窗口Y坐标</param>
        /// <returns></returns>
        public int FindStrFast(int x1, int y1, int x2, int y2, string str, string color, double sim, out int x, out int y)
        {
            return dm.FindStrFast(x1, y1, x2, y2, str, color, sim, out x, out y);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <param name="x2"></param>
        /// <param name="y2"></param>
        /// <param name="str"></param>
        /// <param name="color"></param>
        /// <param name="sim"></param>
        /// <returns></returns>
        public string FindStrFastEx(int x1, int y1, int x2, int y2, string str, string color, double sim)
        {
            return dm.FindStrFastEx(x1, y1, x2, y2, str, color, sim);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <param name="x2"></param>
        /// <param name="y2"></param>
        /// <param name="str"></param>
        /// <param name="color"></param>
        /// <param name="sim"></param>
        /// <returns></returns>
        public string FindStrFastE(int x1, int y1, int x2, int y2, string str, string color, double sim)
        {
            return dm.FindStrFastE(x1, y1, x2, y2, str, color, sim);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="enable_debug"></param>
        /// <returns></returns>
        public int EnableDisplayDebug(int enable_debug)
        {
            return dm.EnableDisplayDebug(enable_debug);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="file_"></param>
        /// <returns></returns>
        public int CapturePre(string file_)
        {
            return dm.CapturePre(file_);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="code"></param>
        /// <param name="Ver"></param>
        /// <param name="ip"></param>
        /// <returns></returns>
        public int RegEx(string code, string Ver, string ip)
        {
            return dm.RegEx(code, Ver, ip);
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public string GetMachineCode()
        {
            return dm.GetMachineCode();
        }

        /// <summary>
        /// 设置粘贴板内容
        /// </summary>
        /// <param name="data">数据</param>
        /// <returns></returns>
        public int SetClipboard(string data)
        {
            return dm.SetClipboard(data);
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public string GetClipboard()
        {
            return dm.GetClipboard();
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public int GetNowDict()
        {
            return dm.GetNowDict();
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public int Is64Bit()
        {
            return dm.Is64Bit();
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <param name="x2"></param>
        /// <param name="y2"></param>
        /// <param name="color"></param>
        /// <param name="sim"></param>
        /// <returns></returns>
        public int GetColorNum(int x1, int y1, int x2, int y2, string color, double sim)
        {
            return dm.GetColorNum(x1, y1, x2, y2, color, sim);
        }

        /// <summary>
        /// 根据指定进程以及其它条件,枚举系统中符合条件的窗口,可以枚举到按键自带的无法枚举到的窗口
        /// </summary>
        /// <param name="process_name">进程映像名.比如(svchost.exe). 此参数是精确匹配,但不区分大小写.</param>
        /// <param name="title">窗口标题. 此参数是模糊匹配.</param>
        /// <param name="class_name">窗口类名. 此参数是模糊匹配.</param>
        /// <param name="filter">取值定义如下:1 : 匹配窗口标题,参数title有效 .   2 : 匹配窗口类名,参数class_name有效.     4 : 只匹配指定父窗口的第一层孩子窗口    8 : 匹配所有者窗口为0的窗口,即顶级窗口     16 : 匹配可见的窗口   32 : 匹配出的窗口按照窗口打开顺序依次排列 ....收费功能，具体详情点击查看 ......  这些值可以相加,比如4+8+16就是类似于任务管理器中的窗口列表   </param>
        /// <returns></returns>
        public string EnumWindowByProcess(string process_name, string title, string class_name, int filter)
        {
            return dm.EnumWindowByProcess(process_name, title, class_name, filter);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public int GetDictCount(int index)
        {
            return dm.GetDictCount(index);
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public int GetLastError()
        {
            return dm.GetLastError();
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public string GetNetTime()
        {
            return dm.GetNetTime();
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="en"></param>
        /// <returns></returns>
        public int EnableGetColorByCapture(int en)
        {
            return dm.EnableGetColorByCapture(en);
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public int CheckUAC()
        {
            return dm.CheckUAC();
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="uac"></param>
        /// <returns></returns>
        public int SetUAC(int uac)
        {
            return dm.SetUAC(uac);
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public int DisableFontSmooth()
        {
            return dm.DisableFontSmooth();
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public int CheckFontSmooth()
        {
            return dm.CheckFontSmooth();
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="level"></param>
        /// <returns></returns>
        public int SetDisplayAcceler(int level)
        {
            return dm.SetDisplayAcceler(level);
        }

        /// <summary>
        /// 根据指定的进程名字，来查找可见窗口
        /// </summary>
        /// <param name="process_name">进程映像名.比如(svchost.exe). 此参数是精确匹配,但不区分大小写.</param>
        /// <param name="class_name">窗口类名. 此参数是模糊匹配.</param>
        /// <param name="title_name">标题</param>
        /// <returns></returns>
        public int FindWindowByProcess(string process_name, string class_name, string title_name)
        {
            return dm.FindWindowByProcess(process_name, class_name, title_name);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="process_id"></param>
        /// <param name="class_name">窗口类名. 此参数是模糊匹配.</param>
        /// <param name="title_name"></param>
        /// <returns></returns>
        public int FindWindowByProcessId(int process_id, string class_name, string title_name)
        {
            return dm.FindWindowByProcessId(process_id, class_name, title_name);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="section"></param>
        /// <param name="key"></param>
        /// <param name="file_"></param>
        /// <returns></returns>
        public string ReadIni(string section, string key, string file_)
        {
            return dm.ReadIni(section, key, file_);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="section"></param>
        /// <param name="key"></param>
        /// <param name="v"></param>
        /// <param name="file_"></param>
        /// <returns></returns>
        public int WriteIni(string section, string key, string v, string file_)
        {
            return dm.WriteIni(section, key, v, file_);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="path"></param>
        /// <param name="mode"></param>
        /// <returns></returns>
        public int RunApp(string path, int mode)
        {
            return dm.RunApp(path, mode);
        }

        /// <summary>
        /// C# 线程 延时 (毫秒)   -- 大漠延时为 vip功能
        /// </summary>
        /// <param name="mis"></param>
        /// <returns></returns>
        public void Sleep(int mis)
        {
            Thread.Sleep(mis);
            //return dm.delay(mis);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="spec1"></param>
        /// <param name="flag1"></param>
        /// <param name="type1"></param>
        /// <param name="spec2"></param>
        /// <param name="flag2"></param>
        /// <param name="type2"></param>
        /// <returns></returns>
        public int FindWindowSuper(string spec1, int flag1, int type1, string spec2, int flag2, int type2)
        {
            return dm.FindWindowSuper(spec1, flag1, type1, spec2, flag2, type2);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="all_pos"></param>
        /// <param name="type_"></param>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <param name="x2"></param>
        /// <param name="y2"></param>
        /// <returns></returns>
        public string ExcludePos(string all_pos, int type_, int x1, int y1, int x2, int y2)
        {
            return dm.ExcludePos(all_pos, type_, x1, y1, x2, y2);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="all_pos"></param>
        /// <param name="type_"></param>
        /// <param name="x">窗口X坐标</param>
        /// <param name="y">窗口Y坐标</param>
        /// <returns></returns>
        public string FindNearestPos(string all_pos, int type_, int x, int y)
        {
            return dm.FindNearestPos(all_pos, type_, x, y);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="all_pos"></param>
        /// <param name="type_"></param>
        /// <param name="x">窗口X坐标</param>
        /// <param name="y">窗口Y坐标</param>
        /// <returns></returns>
        public string SortPosDistance(string all_pos, int type_, int x, int y)
        {
            return dm.SortPosDistance(all_pos, type_, x, y);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <param name="x2"></param>
        /// <param name="y2"></param>
        /// <param name="pic_info"></param>
        /// <param name="delta_color"></param>
        /// <param name="sim"></param>
        /// <param name="dir"></param>
        /// <param name="x">窗口X坐标</param>
        /// <param name="y">窗口Y坐标</param>
        /// <returns></returns>
        public int FindPicMem(int x1, int y1, int x2, int y2, string pic_info, string delta_color, double sim, int dir, out int x, out int y)
        {
            return dm.FindPicMem(x1, y1, x2, y2, pic_info, delta_color, sim, dir, out x, out y);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <param name="x2"></param>
        /// <param name="y2"></param>
        /// <param name="pic_info"></param>
        /// <param name="delta_color"></param>
        /// <param name="sim"></param>
        /// <param name="dir"></param>
        /// <returns></returns>
        public string FindPicMemEx(int x1, int y1, int x2, int y2, string pic_info, string delta_color, double sim, int dir)
        {
            return dm.FindPicMemEx(x1, y1, x2, y2, pic_info, delta_color, sim, dir);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <param name="x2"></param>
        /// <param name="y2"></param>
        /// <param name="pic_info"></param>
        /// <param name="delta_color"></param>
        /// <param name="sim"></param>
        /// <param name="dir"></param>
        /// <returns></returns>
        public string FindPicMemE(int x1, int y1, int x2, int y2, string pic_info, string delta_color, double sim, int dir)
        {
            return dm.FindPicMemE(x1, y1, x2, y2, pic_info, delta_color, sim, dir);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="pic_info"></param>
        /// <param name="addr"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public string AppendPicAddr(string pic_info, int addr, int size)
        {
            return dm.AppendPicAddr(pic_info, addr, size);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="file_"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        public int WriteFile(string file_, string content)
        {
            return dm.WriteFile(file_, content);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public int Stop(int id)
        {
            return dm.Stop(id);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="index"></param>
        /// <param name="addr"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public int SetDictMem(int index, int addr, int size)
        {
            return dm.SetDictMem(index, addr, size);
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public string GetNetTimeSafe()
        {
            return dm.GetNetTimeSafe();
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="hwnd">指定的窗口句柄</param>
        /// <returns></returns>
        public int ForceUnBindWindow(int hwnd)
        {
            return dm.ForceUnBindWindow(hwnd);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="section"></param>
        /// <param name="key"></param>
        /// <param name="file_"></param>
        /// <param name="pwd"></param>
        /// <returns></returns>
        public string ReadIniPwd(string section, string key, string file_, string pwd)
        {
            return dm.ReadIniPwd(section, key, file_, pwd);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="section"></param>
        /// <param name="key"></param>
        /// <param name="v"></param>
        /// <param name="file_"></param>
        /// <param name="pwd"></param>
        /// <returns></returns>
        public int WriteIniPwd(string section, string key, string v, string file_, string pwd)
        {
            return dm.WriteIniPwd(section, key, v, file_, pwd);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="file_"></param>
        /// <param name="pwd"></param>
        /// <returns></returns>
        public int DecodeFile(string file_, string pwd)
        {
            return dm.DecodeFile(file_, pwd);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="key_str"></param>
        /// <returns></returns>
        public int KeyDownChar(string key_str)
        {
            return dm.KeyDownChar(key_str);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="key_str"></param>
        /// <returns></returns>
        public int KeyUpChar(string key_str)
        {
            return dm.KeyUpChar(key_str);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="key_str"></param>
        /// <returns></returns>
        public int KeyPressChar(string key_str)
        {
            return dm.KeyPressChar(key_str);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="key_str"></param>
        /// <param name="delay"></param>
        /// <returns></returns>
        public int KeyPressStr(string key_str, int delay)
        {
            return dm.KeyPressStr(key_str, delay);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="en"></param>
        /// <returns></returns>
        public int EnableKeypadPatch(int en)
        {
            return dm.EnableKeypadPatch(en);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="en"></param>
        /// <param name="time_out"></param>
        /// <returns></returns>
        public int EnableKeypadSync(int en, int time_out)
        {
            return dm.EnableKeypadSync(en, time_out);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="en"></param>
        /// <param name="time_out"></param>
        /// <returns></returns>
        public int EnableMouseSync(int en, int time_out)
        {
            return dm.EnableMouseSync(en, time_out);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="en"></param>
        /// <param name="type_"></param>
        /// <returns></returns>
        public int DmGuard(int en, string type_)
        {
            return dm.DmGuard(en, type_);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <param name="x2"></param>
        /// <param name="y2"></param>
        /// <param name="file_"></param>
        /// <param name="quality"></param>
        /// <returns></returns>
        public int FaqCaptureFromFile(int x1, int y1, int x2, int y2, string file_, int quality)
        {
            return dm.FaqCaptureFromFile(x1, y1, x2, y2, file_, quality);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="hwnd">指定的窗口句柄</param>
        /// <param name="addr_range"></param>
        /// <param name="int_value_min"></param>
        /// <param name="int_value_max"></param>
        /// <param name="type_"></param>
        /// <param name="step"></param>
        /// <param name="multi_thread"></param>
        /// <param name="mode"></param>
        /// <returns></returns>
        public string FindIntEx(int hwnd, string addr_range, int int_value_min, int int_value_max, int type_, int step, int multi_thread, int mode)
        {
            return dm.FindIntEx(hwnd, addr_range, int_value_min, int_value_max, type_, step, multi_thread, mode);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="hwnd">指定的窗口句柄</param>
        /// <param name="addr_range"></param>
        /// <param name="float_value_min"></param>
        /// <param name="float_value_max"></param>
        /// <param name="step"></param>
        /// <param name="multi_thread"></param>
        /// <param name="mode"></param>
        /// <returns></returns>
        public string FindFloatEx(int hwnd, string addr_range, Single float_value_min, Single float_value_max, int step, int multi_thread, int mode)
        {
            return dm.FindFloatEx(hwnd, addr_range, float_value_min, float_value_max, step, multi_thread, mode);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="hwnd">指定的窗口句柄</param>
        /// <param name="addr_range"></param>
        /// <param name="double_value_min"></param>
        /// <param name="double_value_max"></param>
        /// <param name="step"></param>
        /// <param name="multi_thread"></param>
        /// <param name="mode"></param>
        /// <returns></returns>
        public string FindDoubleEx(int hwnd, string addr_range, double double_value_min, double double_value_max, int step, int multi_thread, int mode)
        {
            return dm.FindDoubleEx(hwnd, addr_range, double_value_min, double_value_max, step, multi_thread, mode);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="hwnd">指定的窗口句柄</param>
        /// <param name="addr_range"></param>
        /// <param name="string_value"></param>
        /// <param name="type_"></param>
        /// <param name="step"></param>
        /// <param name="multi_thread"></param>
        /// <param name="mode"></param>
        /// <returns></returns>
        public string FindStringEx(int hwnd, string addr_range, string string_value, int type_, int step, int multi_thread, int mode)
        {
            return dm.FindStringEx(hwnd, addr_range, string_value, type_, step, multi_thread, mode);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="hwnd">指定的窗口句柄</param>
        /// <param name="addr_range"></param>
        /// <param name="data"></param>
        /// <param name="step"></param>
        /// <param name="multi_thread"></param>
        /// <param name="mode"></param>
        /// <returns></returns>
        public string FindDataEx(int hwnd, string addr_range, string data, int step, int multi_thread, int mode)
        {
            return dm.FindDataEx(hwnd, addr_range, data, step, multi_thread, mode);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="en"></param>
        /// <param name="mousedelay"></param>
        /// <param name="mousestep"></param>
        /// <returns></returns>
        public int EnableRealMouse(int en, int mousedelay, int mousestep)
        {
            return dm.EnableRealMouse(en, mousedelay, mousestep);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="en"></param>
        /// <returns></returns>
        public int EnableRealKeypad(int en)
        {
            return dm.EnableRealKeypad(en);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public int SendStringIme(string str)
        {
            return dm.SendStringIme(str);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="hwnd">指定的窗口句柄</param>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <param name="x2"></param>
        /// <param name="y2"></param>
        /// <param name="color"></param>
        /// <param name="style"></param>
        /// <param name="width"></param>
        /// <returns></returns>
        public int FoobarDrawLine(int hwnd, int x1, int y1, int x2, int y2, string color, int style, int width)
        {
            return dm.FoobarDrawLine(hwnd, x1, y1, x2, y2, color, style, width);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <param name="x2"></param>
        /// <param name="y2"></param>
        /// <param name="str"></param>
        /// <param name="color"></param>
        /// <param name="sim"></param>
        /// <returns></returns>
        public string FindStrEx(int x1, int y1, int x2, int y2, string str, string color, double sim)
        {
            return dm.FindStrEx(x1, y1, x2, y2, str, color, sim);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="hwnd">指定的窗口句柄</param>
        /// <returns></returns>
        public int IsBind(int hwnd)
        {
            return dm.IsBind(hwnd);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public int SetDisplayDelay(int t)
        {
            return dm.SetDisplayDelay(t);
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public int GetDmCount()
        {
            return dm.GetDmCount();
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public int DisableScreenSave()
        {
            return dm.DisableScreenSave();
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public int DisablePowerSave()
        {
            return dm.DisablePowerSave();
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="en"></param>
        /// <returns></returns>
        public int SetMemoryHwndAsProcessId(int en)
        {
            return dm.SetMemoryHwndAsProcessId(en);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <param name="x2"></param>
        /// <param name="y2"></param>
        /// <param name="offset_color"></param>
        /// <param name="sim"></param>
        /// <param name="dir"></param>
        /// <param name="x">窗口X坐标</param>
        /// <param name="y">窗口Y坐标</param>
        /// <returns></returns>
        public int FindShape(int x1, int y1, int x2, int y2, string offset_color, double sim, int dir, out int x, out int y)
        {
            return dm.FindShape(x1, y1, x2, y2, offset_color, sim, dir, out x, out y);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <param name="x2"></param>
        /// <param name="y2"></param>
        /// <param name="offset_color"></param>
        /// <param name="sim"></param>
        /// <param name="dir"></param>
        /// <returns></returns>
        public string FindShapeE(int x1, int y1, int x2, int y2, string offset_color, double sim, int dir)
        {
            return dm.FindShapeE(x1, y1, x2, y2, offset_color, sim, dir);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <param name="x2"></param>
        /// <param name="y2"></param>
        /// <param name="offset_color"></param>
        /// <param name="sim"></param>
        /// <param name="dir"></param>
        /// <returns></returns>
        public string FindShapeEx(int x1, int y1, int x2, int y2, string offset_color, double sim, int dir)
        {
            return dm.FindShapeEx(x1, y1, x2, y2, offset_color, sim, dir);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <param name="x2"></param>
        /// <param name="y2"></param>
        /// <param name="str"></param>
        /// <param name="color"></param>
        /// <param name="sim"></param>
        /// <param name="x">窗口X坐标</param>
        /// <param name="y">窗口Y坐标</param>
        /// <returns></returns>
        public string FindStrS(int x1, int y1, int x2, int y2, string str, string color, double sim, out int x, out int y)
        {
            return dm.FindStrS(x1, y1, x2, y2, str, color, sim, out x, out y);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <param name="x2"></param>
        /// <param name="y2"></param>
        /// <param name="str"></param>
        /// <param name="color"></param>
        /// <param name="sim"></param>
        /// <returns></returns>
        public string FindStrExS(int x1, int y1, int x2, int y2, string str, string color, double sim)
        {
            return dm.FindStrExS(x1, y1, x2, y2, str, color, sim);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <param name="x2"></param>
        /// <param name="y2"></param>
        /// <param name="str"></param>
        /// <param name="color"></param>
        /// <param name="sim"></param>
        /// <param name="x">窗口X坐标</param>
        /// <param name="y">窗口Y坐标</param>
        /// <returns></returns>
        public string FindStrFastS(int x1, int y1, int x2, int y2, string str, string color, double sim, out int x, out int y)
        {
            return dm.FindStrFastS(x1, y1, x2, y2, str, color, sim, out x, out y);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <param name="x2"></param>
        /// <param name="y2"></param>
        /// <param name="str"></param>
        /// <param name="color"></param>
        /// <param name="sim"></param>
        /// <returns></returns>
        public string FindStrFastExS(int x1, int y1, int x2, int y2, string str, string color, double sim)
        {
            return dm.FindStrFastExS(x1, y1, x2, y2, str, color, sim);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <param name="x2"></param>
        /// <param name="y2"></param>
        /// <param name="pic_name"></param>
        /// <param name="delta_color"></param>
        /// <param name="sim"></param>
        /// <param name="dir"></param>
        /// <param name="x">窗口X坐标</param>
        /// <param name="y">窗口Y坐标</param>
        /// <returns></returns>
        public string FindPicS(int x1, int y1, int x2, int y2, string pic_name, string delta_color, double sim, int dir, out int x, out int y)
        {
            return dm.FindPicS(x1, y1, x2, y2, pic_name, delta_color, sim, dir, out x, out y);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <param name="x2"></param>
        /// <param name="y2"></param>
        /// <param name="pic_name"></param>
        /// <param name="delta_color"></param>
        /// <param name="sim"></param>
        /// <param name="dir"></param>
        /// <returns></returns>
        public string FindPicExS(int x1, int y1, int x2, int y2, string pic_name, string delta_color, double sim, int dir)
        {
            return dm.FindPicExS(x1, y1, x2, y2, pic_name, delta_color, sim, dir);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public int ClearDict(int index)
        {
            return dm.ClearDict(index);
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public string GetMachineCodeNoMac()
        {
            return dm.GetMachineCodeNoMac();
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="hwnd">指定的窗口句柄</param>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <param name="x2"></param>
        /// <param name="y2"></param>
        /// <returns></returns>
        public int GetClientRect(int hwnd, out int x1, out int y1, out int x2, out int y2)
        {
            return dm.GetClientRect(hwnd, out x1, out y1, out x2, out y2);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="en"></param>
        /// <returns></returns>
        public int EnableFakeActive(int en)
        {
            return dm.EnableFakeActive(en);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <param name="x2"></param>
        /// <param name="y2"></param>
        /// <param name="data"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public int GetScreenDataBmp(int x1, int y1, int x2, int y2, out int data, out int size)
        {
            return dm.GetScreenDataBmp(x1, y1, x2, y2, out data, out size);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="file_"></param>
        /// <param name="pwd"></param>
        /// <returns></returns>
        public int EncodeFile(string file_, string pwd)
        {
            return dm.EncodeFile(file_, pwd);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="type_"></param>
        /// <returns></returns>
        public string GetCursorShapeEx(int type_)
        {
            return dm.GetCursorShapeEx(type_);
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public int FaqCancel()
        {
            return dm.FaqCancel();
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="int_value"></param>
        /// <param name="type_"></param>
        /// <returns></returns>
        public string IntToData(int int_value, int type_)
        {
            return dm.IntToData(int_value, type_);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="float_value"></param>
        /// <returns></returns>
        public string FloatToData(Single float_value)
        {
            return dm.FloatToData(float_value);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="double_value"></param>
        /// <returns></returns>
        public string DoubleToData(double double_value)
        {
            return dm.DoubleToData(double_value);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="string_value"></param>
        /// <param name="type_"></param>
        /// <returns></returns>
        public string StringToData(string string_value, int type_)
        {
            return dm.StringToData(string_value, type_);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="file_"></param>
        /// <returns></returns>
        public int SetMemoryFindResultToFile(string file_)
        {
            return dm.SetMemoryFindResultToFile(file_);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="en"></param>
        /// <returns></returns>
        public int EnableBind(int en)
        {
            return dm.EnableBind(en);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="mode"></param>
        /// <returns></returns>
        public int SetSimMode(int mode)
        {
            return dm.SetSimMode(mode);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <param name="x2"></param>
        /// <param name="y2"></param>
        /// <returns></returns>
        public int LockMouseRect(int x1, int y1, int x2, int y2)
        {
            return dm.LockMouseRect(x1, y1, x2, y2);
        }

        /// <summary>
        /// 发送粘贴板内容
        /// </summary>
        /// <param name="hwnd">指定的窗口句柄</param>
        /// <returns></returns>
        public int SendPaste(int hwnd)
        {
            return dm.SendPaste(hwnd);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <param name="x2"></param>
        /// <param name="y2"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        public int IsDisplayDead(int x1, int y1, int x2, int y2, int t)
        {
            return dm.IsDisplayDead(x1, y1, x2, y2, t);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="vk">虚拟按键码</param>
        /// <returns></returns>
        public int GetKeyState(int vk)
        {
            return dm.GetKeyState(vk);
        }

        /// <summary>
        /// 拷贝文件
        /// </summary>
        /// <param name="src_file">原始文件名</param>
        /// <param name="dst_file">目标文件名</param>
        /// <param name="over">取值如下,0:如果dst_file文件存在则不覆盖返回.1:如果dst_file文件存在则覆盖.</param>
        /// <returns></returns>
        public int CopyFile(string src_file, string dst_file, int over)
        {
            return dm.CopyFile(src_file, dst_file, over);
        }

        /// <summary>
        /// 判断指定文件是否存在 (F12 参数说明)
        /// </summary>
        /// <param name="file_"> 文件名 </param>
        /// <returns></returns>
        public int IsFileExist(string file_)
        {
            #region 说明

            //// 绝对路径
            //TracePrint dm.IsFileExist("c:\123.txt")

            //// 相对路径
            //dm.SetPath "c:\test_game"
            //TracePrint dm.IsFileExist("123.txt")

            #endregion 说明

            return dm.IsFileExist(file_);
        }

        /// <summary>
        /// 删除文件
        /// </summary>
        /// <param name="file_"> 文件名</param>
        /// <returns></returns>
        public int DeleteFile(string file_)
        {
            #region 说明

            /*
                // 绝对路径
                dm.DeleteFile "c:\123.txt"

                // 相对路径
                dm.SetPath "c:\test_game"
                dm.DeleteFile "123.txt"

             */

            #endregion 说明

            return dm.DeleteFile(file_);
        }

        /// <summary>
        ///  移动文件
        /// </summary>
        /// <param name="src_file"> 原始文件名</param>
        /// <param name="dst_file"> 目标文件名</param>
        /// <returns></returns>
        public int MoveFile(string src_file, string dst_file)
        {
            #region 说明

            /*
            // 绝对路径
                dm.MoveFile "c:\123.txt","d:\456.txt"

                // 相对路径
                dm.SetPath "c:\test_game"
                dm.MoveFile "123.txt","456.txt"

             */

            #endregion 说明

            return dm.MoveFile(src_file, dst_file);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="folder_name"></param>
        /// <returns></returns>
        public int CreateFolder(string folder_name)
        {
            return dm.CreateFolder(folder_name);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="folder_name"></param>
        /// <returns></returns>
        public int DeleteFolder(string folder_name)
        {
            return dm.DeleteFolder(folder_name);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="file_"></param>
        /// <returns></returns>
        public int GetFileLength(string file_)
        {
            return dm.GetFileLength(file_);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="file_"></param>
        /// <returns></returns>
        public string ReadFile(string file_)
        {
            return dm.ReadFile(file_);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="key_code"></param>
        /// <param name="time_out"></param>
        /// <returns></returns>
        public int WaitKey(int key_code, int time_out)
        {
            return dm.WaitKey(key_code, time_out);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="section"></param>
        /// <param name="key"></param>
        /// <param name="file_"></param>
        /// <returns></returns>
        public int DeleteIni(string section, string key, string file_)
        {
            return dm.DeleteIni(section, key, file_);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="section"></param>
        /// <param name="key"></param>
        /// <param name="file_"></param>
        /// <param name="pwd"></param>
        /// <returns></returns>
        public int DeleteIniPwd(string section, string key, string file_, string pwd)
        {
            return dm.DeleteIniPwd(section, key, file_, pwd);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="en"></param>
        /// <returns></returns>
        public int EnableSpeedDx(int en)
        {
            return dm.EnableSpeedDx(en);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="en"></param>
        /// <returns></returns>
        public int EnableIme(int en)
        {
            return dm.EnableIme(en);
        }

        /// <summary>
        /// 注册
        /// </summary>
        /// <param name="code">自己的注册码</param>
        /// <param name="ver">版本号,可为空</param>
        /// <returns></returns>
        public string Reg(string code, string ver)
        {
            string msg = "";
            int state = dm.Reg(code, ver);
            switch (state)
            {
                case 0:
                    msg = "失败(未知错误)";
                    break;

                case 1:
                    msg = "注册成功";
                    break;

                case 2:
                    msg = "余额不足";
                    break;

                case 3:
                    msg = "绑定了本机器，但是账户余额不足50元.";
                    break;

                case 4:
                    msg = "注册码错误";
                    break;

                case 5:
                    msg = "你的机器或者IP在黑名单列表中或者不在白名单列表中.";
                    break;

                case -1:
                    msg = "无法连接网络,(可能防火墙拦截, 如果可以正常访问大漠插件网站，那就可以肯定是被防火墙拦截)";
                    break;

                case -2:
                    msg = "进程没有以管理员方式运行. (出现在win7 vista 2008.建议关闭uac)";
                    break;

                case -8:
                    msg = "版本附加信息长度超过了10";
                    break;

                case -9:
                    msg = "版本附加信息里包含了非法字母.";
                    break;

                default:
                    msg = "未知的返回状态:" + state;
                    break;
            }
            return msg;
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public string SelectFile()
        {
            return dm.SelectFile();
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public string SelectDirectory()
        {
            return dm.SelectDirectory();
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="lock1"></param>
        /// <returns></returns>
        public int LockDisplay(int lock1)
        {
            return dm.LockDisplay(lock1);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="hwnd">指定的窗口句柄</param>
        /// <param name="file_"></param>
        /// <param name="en"></param>
        /// <param name="header"></param>
        /// <returns></returns>
        public int FoobarSetSave(int hwnd, string file_, int en, string header)
        {
            return dm.FoobarSetSave(hwnd, file_, en, header);
        }

        /// <summary>
        ///根据两组设定条件来枚举指定窗口. 收费功能，具体详情点击查看
        /// </summary>
        /// <param name="spec1">查找串1. (内容取决于flag1的值)</param>
        /// <param name="flag1"></param>
        /// <param name="type1"></param>
        /// <param name="spec2"></param>
        /// <param name="flag2"></param>
        /// <param name="type2"></param>
        /// <param name="sort"></param>
        /// <returns></returns>
        public string EnumWindowSuper(string spec1, int flag1, int type1, string spec2, int flag2, int type2, int sort)
        {
            return dm.EnumWindowSuper(spec1, flag1, type1, spec2, flag2, type2, sort);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="url"></param>
        /// <param name="save_file"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public int DownloadFile(string url, string save_file, int timeout)
        {
            return dm.DownloadFile(url, save_file, timeout);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="en"></param>
        /// <returns></returns>
        public int EnableKeypadMsg(int en)
        {
            return dm.EnableKeypadMsg(en);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="en"></param>
        /// <returns></returns>
        public int EnableMouseMsg(int en)
        {
            return dm.EnableMouseMsg(en);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="code"></param>
        /// <param name="Ver"></param>
        /// <returns></returns>
        public int RegNoMac(string code, string Ver)
        {
            return dm.RegNoMac(code, Ver);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="code"></param>
        /// <param name="Ver"></param>
        /// <param name="ip"></param>
        /// <returns></returns>
        public int RegExNoMac(string code, string Ver, string ip)
        {
            return dm.RegExNoMac(code, Ver, ip);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="delay"></param>
        /// <returns></returns>
        public int SetEnumWindowDelay(int delay)
        {
            return dm.SetEnumWindowDelay(delay);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <param name="x2"></param>
        /// <param name="y2"></param>
        /// <param name="color"></param>
        /// <param name="sim"></param>
        /// <returns></returns>
        public int FindMulColor(int x1, int y1, int x2, int y2, string color, double sim)
        {
            return dm.FindMulColor(x1, y1, x2, y2, color, sim);
        }

        #endregion XXXX

        #region new

        public int KeyPress(int vk)
        {
            return dm.KeyPress(vk);
        }

        public int KeyDown(int vk)
        {
            return dm.KeyDown(vk);
        }

        public int KeyUp(int vk)
        {
            return dm.KeyUp(vk);
        }

        public string FindInt(int hwnd, string addr_range, long int_value_min, long int_value_max, int tpe)
        {
            return dm.FindInt(hwnd, addr_range, int_value_min, int_value_max, tpe);
        }

        public int WriteInt(int hwnd, string addr, int tpe, long v)
        {
            return dm.WriteInt(hwnd, addr, tpe, v);
        }

        public string Assemble(long base_addr, int is_64bit)
        {
            return dm.Assemble(base_addr, is_64bit);
        }

        public string DisAssemble(string asm_code, long base_addr, int is_64bit)
        {
            return dm.DisAssemble(asm_code, base_addr, is_64bit);
        }

        public int delay(int mis)
        {
            if (dm.delay(mis) == 0)
            {
                Sleep(mis);
            }
            return 1;
        }

        public string FindIntEx(int hwnd, string addr_range, long int_value_min, long int_value_max, int tpe, int steps, int multi_thread, int mode)
        {
            return dm.FindIntEx(hwnd, addr_range, int_value_min, int_value_max, tpe, steps, multi_thread, mode);
        }

        public string IntToData(long int_value, int tpe)
        {
            return dm.IntToData(int_value, tpe);
        }

        public string GetDict(int index, int font_index)
        {
            return dm.GetDict(index, font_index);
        }

        public int GetBindWindow()
        {
            return dm.GetBindWindow();
        }

        public int FoobarStartGif(int hwnd, int x, int y, string pic_name, int repeat_limit, int delay)
        {
            return dm.FoobarStartGif(hwnd, x, y, pic_name, repeat_limit, delay);
        }

        public int FoobarStopGif(int hwnd, int x, int y, string pic_name)
        {
            return dm.FoobarStopGif(hwnd, x, y, pic_name);
        }

        public int FreeProcessMemory(int hwnd)
        {
            return dm.FreeProcessMemory(hwnd);
        }

        public string ReadFileData(string file_name, int start_pos, int end_pos)
        {
            return dm.ReadFileData(file_name, start_pos, end_pos);
        }

        public long VirtualAllocEx(int hwnd, long addr, int size, int tpe)
        {
            return dm.VirtualAllocEx(hwnd, addr, size, tpe);
        }

        public int VirtualFreeEx(int hwnd, long addr)
        {
            return dm.VirtualFreeEx(hwnd, addr);
        }

        public string GetCommandLine(int hwnd)
        {
            return dm.GetCommandLine(hwnd);
        }

        public int TerminateProcess(int pid)
        {
            return dm.TerminateProcess(pid);
        }

        public string GetNetTimeByIp(string ip)
        {
            return dm.GetNetTimeByIp(ip);
        }

        public string EnumProcess(string name)
        {
            return dm.EnumProcess(name);
        }

        public string GetProcessInfo(int pid)
        {
            return dm.GetProcessInfo(pid);
        }

        public long ReadIntAddr(int hwnd, long addr, int tpe)
        {
            return dm.ReadIntAddr(hwnd, addr, tpe);
        }

        public string ReadDataAddr(int hwnd, long addr, int length)
        {
            return dm.ReadDataAddr(hwnd, addr, length);
        }

        public double ReadDoubleAddr(int hwnd, long addr)
        {
            return dm.ReadDoubleAddr(hwnd, addr);
        }

        public float ReadFloatAddr(int hwnd, long addr)
        {
            return dm.ReadFloatAddr(hwnd, addr);
        }

        public string ReadStringAddr(int hwnd, long addr, int tpe, int length)
        {
            return dm.ReadStringAddr(hwnd, addr, tpe, length);
        }

        public int WriteDataAddr(int hwnd, long addr, string data)
        {
            return dm.WriteDataAddr(hwnd, addr, data);
        }

        public int WriteDoubleAddr(int hwnd, long addr, double v)
        {
            return dm.WriteDoubleAddr(hwnd, addr, v);
        }

        public int WriteFloatAddr(int hwnd, long addr, float v)
        {
            return dm.WriteFloatAddr(hwnd, addr, v);
        }

        public int WriteIntAddr(int hwnd, long addr, int tpe, long v)
        {
            return dm.WriteIntAddr(hwnd, addr, tpe, v);
        }

        public int WriteStringAddr(int hwnd, long addr, int tpe, string v)
        {
            return dm.WriteStringAddr(hwnd, addr, tpe, v);
        }

        public int Delays(int min_s, int max_s)
        {
            return dm.Delays(min_s, max_s);
        }

        public int FindColorBlock(int x1, int y1, int x2, int y2, string color, double sim, int count, int width, int height, out int x, out int y)
        {
            return dm.FindColorBlock(x1, y1, x2, y2, color, sim, count, width, height, out x, out y);
        }

        public string FindColorBlockEx(int x1, int y1, int x2, int y2, string color, double sim, int count, int width, int height)
        {
            return dm.FindColorBlockEx(x1, y1, x2, y2, color, sim, count, width, height);
        }

        public int OpenProcess(int pid)
        {
            return dm.OpenProcess(pid);
        }

        public string EnumIniSection(string file_name)
        {
            return dm.EnumIniSection(file_name);
        }

        public string EnumIniSectionPwd(string file_name, string pwd)
        {
            return dm.EnumIniSectionPwd(file_name, pwd);
        }

        public string EnumIniKey(string section, string file_name)
        {
            return dm.EnumIniKey(section, file_name);
        }

        public string EnumIniKeyPwd(string section, string file_name, string pwd)
        {
            return dm.EnumIniKeyPwd(section, file_name, pwd);
        }

        public int SwitchBindWindow(int hwnd)
        {
            return dm.SwitchBindWindow(hwnd);
        }

        public int InitCri()
        {
            return dm.InitCri();
        }

        public int SendStringIme2(int hwnd, string str, int mode)
        {
            return dm.SendStringIme2(hwnd, str, mode);
        }

        public string EnumWindowByProcessId(int pid, string title, string class_name, int filter)
        {
            return dm.EnumWindowByProcessId(pid, title, class_name, filter);
        }

        public string GetDisplayInfo()
        {
            return dm.GetDisplayInfo();
        }

        public int EnableFontSmooth()
        {
            return dm.EnableFontSmooth();
        }

        public string OcrExOne(int x1, int y1, int x2, int y2, string color, double sim)
        {
            return dm.OcrExOne(x1, y1, x2, y2, color, sim);
        }

        public int SetAero(int en)
        {
            return dm.SetAero(en);
        }

        public int FoobarSetTrans(int hwnd, int trans, string color, double sim)
        {
            return dm.FoobarSetTrans(hwnd, trans, color, sim);
        }

        public int EnablePicCache(int en)
        {
            return dm.EnablePicCache(en);
        }

        public int FaqIsPosted()
        {
            return dm.FaqIsPosted();
        }

        public int LoadPicByte(int addr, int size, string name)
        {
            return dm.LoadPicByte(addr, size, name);
        }

        public int MiddleDown()
        {
            return dm.MiddleDown();
        }

        public int MiddleUp()
        {
            return dm.MiddleUp();
        }

        public int FaqCaptureString(string str)
        {
            return dm.FaqCaptureString(str);
        }

        public int VirtualProtectEx(int hwnd, long addr, int size, int tpe, int old_protect)
        {
            return dm.VirtualProtectEx(hwnd, addr, size, tpe, old_protect);
        }

        public int SetMouseSpeed(int speed)
        {
            return dm.SetMouseSpeed(speed);
        }

        public int GetMouseSpeed()
        {
            return dm.GetMouseSpeed();
        }

        public int EnableMouseAccuracy(int en)
        {
            return dm.EnableMouseAccuracy(en);
        }

        public int SetExcludeRegion(int tpe, string info)
        {
            return dm.SetExcludeRegion(tpe, info);
        }

        public int EnableShareDict(int en)
        {
            return dm.EnableShareDict(en);
        }

        public int DisableCloseDisplayAndSleep()
        {
            return dm.DisableCloseDisplayAndSleep();
        }

        public int Int64ToInt32(long v)
        {
            return dm.Int64ToInt32(v);
        }

        public int GetLocale()
        {
            return dm.GetLocale();
        }

        public int SetLocale()
        {
            return dm.SetLocale();
        }

        public int ReadDataToBin(int hwnd, string addr, int length)
        {
            return dm.ReadDataToBin(hwnd, addr, length);
        }

        public int WriteDataFromBin(int hwnd, string addr, int data, int length)
        {
            return dm.WriteDataFromBin(hwnd, addr, data, length);
        }

        public int ReadDataAddrToBin(int hwnd, long addr, int length)
        {
            return dm.ReadDataAddrToBin(hwnd, addr, length);
        }

        public int WriteDataAddrFromBin(int hwnd, long addr, int data, int length)
        {
            return dm.WriteDataAddrFromBin(hwnd, addr, data, length);
        }

        public int SetParam64ToPointer()
        {
            return dm.SetParam64ToPointer();
        }

        public int GetDPI()
        {
            return dm.GetDPI();
        }

        public int SetDisplayRefreshDelay(int t)
        {
            return dm.SetDisplayRefreshDelay(t);
        }

        public int IsFolderExist(string folder)
        {
            return dm.IsFolderExist(folder);
        }

        public int GetCpuType()
        {
            return dm.GetCpuType();
        }

        public int ReleaseRef()
        {
            return dm.ReleaseRef();
        }

        public int SetExitThread(int en)
        {
            return dm.SetExitThread(en);
        }

        public int GetFps()
        {
            return dm.GetFps();
        }

        public string VirtualQueryEx(int hwnd, long addr, int pmbi)
        {
            return dm.VirtualQueryEx(hwnd, addr, pmbi);
        }

        public long AsmCallEx(int hwnd, int mode, string base_addr)
        {
            return dm.AsmCallEx(hwnd, mode, base_addr);
        }

        public long GetRemoteApiAddress(int hwnd, long base_addr, string fun_name)
        {
            return dm.GetRemoteApiAddress(hwnd, base_addr, fun_name);
        }

        public string ExecuteCmd(string cmd, string current_dir, int time_out)
        {
            return dm.ExecuteCmd(cmd, current_dir, time_out);
        }

        public int SpeedNormalGraphic(int en)
        {
            return dm.SpeedNormalGraphic(en);
        }

        public int UnLoadDriver()
        {
            return dm.UnLoadDriver();
        }

        public int GetOsBuildNumber()
        {
            return dm.GetOsBuildNumber();
        }

        public int HackSpeed(double rate)
        {
            return dm.HackSpeed(rate);
        }

        public string GetRealPath(string path)
        {
            return dm.GetRealPath(path);
        }

        public int ShowTaskBarIcon(int hwnd, int is_show)
        {
            return dm.ShowTaskBarIcon(hwnd, is_show);
        }

        #endregion new
    }
}