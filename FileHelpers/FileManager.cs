using System.Drawing;
using System.IO;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using AppLauncher.Enums;

namespace AppLauncher
{
    public static class FileManager
    {
        public static ImageSource GetImageSource(string filename)
        {
            return GetImageSource(filename, new Size(50, 50));
        }

        public static ImageSource GetImageSource(string filename, Size size)
        {
            //using (var icon = ShellManager.GetIcon(Path.GetExtension(filename), ItemType.File, IconSize.Large, ItemState.Undefined))
            using (var icon = ShellManager.GetIcon(filename, ItemType.File, IconSize.Large, ItemState.Undefined))
            {
                return Imaging.CreateBitmapSourceFromHIcon(icon.Handle,
                    System.Windows.Int32Rect.Empty,
                    BitmapSizeOptions.FromWidthAndHeight(size.Width, size.Height));
            }
        }
    }
}
