using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XXDM.Helper
{
   public class EnumHelper
    {
        #region 键码

        /// <summary>
        /// 键盘 键码表
        /// </summary>
        public enum KeyCode
        {
            数字1 = 49,
            数字2 = 50,
            数字3 = 51,
            数字4 = 52,
            数字5 = 53,
            数字6 = 54,
            数字7 = 55,
            数字8 = 56,
            数字9 = 57,
            数字0 = 48,

            减号 = 189,
            等于号 = 187,
            back = 8,

            a = 65,
            b = 66,
            c = 67,
            d = 68,
            e = 69,
            f = 70,
            g = 71,
            h = 72,
            i = 73,
            j = 74,
            k = 75,
            l = 76,
            m = 77,
            n = 78,
            o = 79,
            p = 80,
            q = 81,
            r = 82,
            s = 83,
            t = 84,
            u = 85,
            v = 86,
            w = 87,
            x = 88,
            y = 89,
            z = 90,

            ctrl = 17,
            alt = 18,
            shift = 16,
            win = 91,
            space = 32,
            cap = 20,
            tab = 9,
            波浪线 = 192,
            esc = 27,
            enter = 13,

            up = 38,
            down = 40,
            left = 37,
            right = 39,

            option = 93,

            print = 44,
            delete = 46,
            home = 36,
            end = 35,
            pgup = 33,
            pgdn = 34,

            f1 = 112,
            f2 = 113,
            f3 = 114,
            f4 = 115,
            f5 = 116,
            f6 = 117,
            f7 = 118,
            f8 = 119,
            f9 = 120,
            f10 = 121,
            f11 = 122,
            f12 = 123,
            左书名号 = 219,
            右书名号 = 221,
            双右斜杠 = 220,
            分号 = 186,
            单引号 = 222,
            逗号 = 188,
            句号 = 190,
            左斜杠 = 191
        }

        /// <summary>
        /// 鼠标左键右键
        /// </summary>
        public enum MouseEnum
        {
            鼠标左键 = 1,
            鼠标右键 = 2

        }

        #endregion
    }
}
