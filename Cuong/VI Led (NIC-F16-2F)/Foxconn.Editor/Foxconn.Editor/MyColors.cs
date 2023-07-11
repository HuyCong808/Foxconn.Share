using Foxconn.Editor.Enums;
using System.Windows.Media;

namespace Foxconn.Editor
{
    public static class MyColors
    {
        public static SolidColorBrush Unknow = ConvertHexToBrushColor("#FFF0F0F0");
        public static SolidColorBrush White = ConvertHexToBrushColor("#FFF0F0F0");
        public static SolidColorBrush Black = ConvertHexToBrushColor("#FF000000");
        public static SolidColorBrush Red = ConvertHexToBrushColor("#FFFF453A");
        public static SolidColorBrush Orange = ConvertHexToBrushColor("#FFFF9F0A");
        public static SolidColorBrush Yellow = ConvertHexToBrushColor("#FFFFD60A");
        public static SolidColorBrush Green = ConvertHexToBrushColor("#FF32D74B");
        public static SolidColorBrush Mint = ConvertHexToBrushColor("#FF66D4CF");
        public static SolidColorBrush Teal = ConvertHexToBrushColor("#FF6CC4DC");
        public static SolidColorBrush Cyan = ConvertHexToBrushColor("#FF5AC8F5");
        public static SolidColorBrush Blue = ConvertHexToBrushColor("#FF0A84FF");
        public static SolidColorBrush Indigo = ConvertHexToBrushColor("#FF5E5CE6");
        public static SolidColorBrush Purple = ConvertHexToBrushColor("#FFBF5AF2");
        public static SolidColorBrush Pink = ConvertHexToBrushColor("#FFFF375F");
        public static SolidColorBrush Brown = ConvertHexToBrushColor("#FFAC8E68");
        public static SolidColorBrush Gray = ConvertHexToBrushColor("#FF98989D");
        public static SolidColorBrush Gray1 = ConvertHexToBrushColor("#FF8E8E93");
        public static SolidColorBrush Gray2 = ConvertHexToBrushColor("#FF636366");
        public static SolidColorBrush Gray3 = ConvertHexToBrushColor("#FF48484A");
        public static SolidColorBrush Gray4 = ConvertHexToBrushColor("#FF3A3A3C");
        public static SolidColorBrush Gray5 = ConvertHexToBrushColor("#FF2C2C2E");
        public static SolidColorBrush Gray6 = ConvertHexToBrushColor("#FF1C1C1E");

        public static SolidColorBrush GetColor(SystemColors color)
        {
            SolidColorBrush solidColor = (SolidColorBrush)new BrushConverter().ConvertFromString("#FFF0F0F0");
            switch (color)
            {
                case SystemColors.Unknow:
                    solidColor = Unknow;
                    break;
                case SystemColors.White:
                    solidColor = White;
                    break;
                case SystemColors.Black:
                    solidColor = Black;
                    break;
                case SystemColors.Red:
                    solidColor = Red;
                    break;
                case SystemColors.Orange:
                    solidColor = Orange;
                    break;
                case SystemColors.Yellow:
                    solidColor = Yellow;
                    break;
                case SystemColors.Green:
                    solidColor = Green;
                    break;
                case SystemColors.Mint:
                    solidColor = Mint;
                    break;
                case SystemColors.Teal:
                    solidColor = Teal;
                    break;
                case SystemColors.Cyan:
                    solidColor = Cyan;
                    break;
                case SystemColors.Blue:
                    solidColor = Blue;
                    break;
                case SystemColors.Indigo:
                    solidColor = Indigo;
                    break;
                case SystemColors.Purple:
                    solidColor = Purple;
                    break;
                case SystemColors.Pink:
                    solidColor = Pink;
                    break;
                case SystemColors.Brown:
                    solidColor = Brown;
                    break;
                case SystemColors.Gray:
                    solidColor = Gray;
                    break;
                case SystemColors.Gray1:
                    solidColor = Gray1;
                    break;
                case SystemColors.Gray2:
                    solidColor = Gray2;
                    break;
                case SystemColors.Gray3:
                    solidColor = Gray3;
                    break;
                case SystemColors.Gray4:
                    solidColor = Gray4;
                    break;
                case SystemColors.Gray5:
                    solidColor = Gray5;
                    break;
                case SystemColors.Gray6:
                    solidColor = Gray6;
                    break;
                default:
                    break;
            }
            return solidColor;
        }

        public static SolidColorBrush ConvertHexToBrushColor(this string hexaColor)
        {
            return (SolidColorBrush)new BrushConverter().ConvertFromString(hexaColor);
        }

        private static SolidColorBrush ConvertRGBToBrushColor(Color color)
        {
            return new SolidColorBrush(Color.FromArgb(color.A, color.R, color.G, color.B));
        }
    }
}
