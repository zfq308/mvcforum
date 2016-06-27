using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace MVCForum.Utilities
{

    public enum ImagePosition
    {
        TopLeft,
        BottomLeft,
        BottomRight,
        TopRigth
    }

    public static class ImageHelper
    {
        #region 图像缩放

        /// <summary>
        /// 缩略图，按高度和宽度来缩略
        /// http://www.cnblogs.com/pooeo/
        /// </summary>
        /// <param name="image"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public static Image Scale(Image image, Size size)
        {
            return image.GetThumbnailImage(size.Width, size.Height, null, new IntPtr());
        }

        /// <summary>
        /// 缩略图，按倍数来缩略
        /// http://www.cnblogs.com/pooeo/
        /// </summary>
        /// <param name="image">原图</param>
        /// <param name="multiple">放大或缩小的倍数，负数表示缩小，正数表示放大</param>
        /// <returns></returns>
        public static Image Scale(Image image, Int32 multiple)
        {
            Int32 newWidth;
            Int32 newHeight;
            Int32 absMultiple = Math.Abs(multiple);
            if (multiple == 0)
            {
                return image.Clone() as Image;
            }
            if (multiple < 0)
            {
                newWidth = image.Width/absMultiple;
                newHeight = image.Height/absMultiple;
            }
            else
            {
                newWidth = image.Width*absMultiple;
                newHeight = image.Height*absMultiple;
            }
            return image.GetThumbnailImage(newWidth, newHeight, null, new IntPtr());
        }

        /// <summary>
        /// 固定宽度缩略
        /// http://www.cnblogs.com/pooeo/
        /// </summary>
        /// <param name="image"></param>
        /// <param name="width"></param>
        /// <returns></returns>
        public static Image ScaleFixWidth(Image image, Int32 width)
        {
            Int32 newWidth = width;
            Int32 newHeight;
            Double tempMultiple = newWidth/(Double) image.Width;
            newHeight = (Int32) ((image.Height)*tempMultiple);
            Image newImage = new Bitmap(newWidth, newHeight);
            using (Graphics newGp = Graphics.FromImage(newImage))
            {
                newGp.CompositingQuality = CompositingQuality.HighQuality;
                //设置高质量插值法 
                newGp.InterpolationMode = InterpolationMode.HighQualityBicubic;
                //设置高质量,低速度呈现平滑程度 
                newGp.SmoothingMode = SmoothingMode.HighQuality;
                //清空画布并以透明背景色填充 
                newGp.Clear(Color.Transparent);
                newGp.DrawImage(image, new Rectangle(0, 0, newWidth, newHeight));
            }
            return newImage;
        }

        /// <summary>
        /// 固定高度缩略
        /// http://www.cnblogs.com/pooeo/
        /// </summary>
        /// <param name="image"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        public static Image ScaleFixHeight(Image image, Int32 height)
        {
            Int32 newWidth;
            Int32 newHeight = height;
            Double tempMultiple = newHeight/(Double) image.Height;
            newWidth = (Int32) ((image.Width)*tempMultiple);
            Image newImage = new Bitmap(newWidth, newHeight);
            using (Graphics newGp = Graphics.FromImage(newImage))
            {
                newGp.CompositingQuality = CompositingQuality.HighQuality;
                //设置高质量插值法 
                newGp.InterpolationMode = InterpolationMode.HighQualityBicubic;
                //设置高质量,低速度呈现平滑程度 
                newGp.SmoothingMode = SmoothingMode.HighQuality;
                //清空画布并以透明背景色填充 
                newGp.Clear(Color.Transparent);
                newGp.DrawImage(image, new Rectangle(0, 0, newWidth, newHeight));
            }
            return newImage;
        }

        /// <summary>
        /// 裁减缩略，根据固定的高度和宽度
        /// http://www.cnblogs.com/pooeo/
        /// </summary>
        /// <param name="image"></param>
        /// <param name="width"></param>
        /// <param name="heigth"></param>
        /// <returns></returns>
        public static Image ScaleCut(Image image, Int32 width, Int32 height)
        {
            int x = 0;
            int y = 0;
            int ow = image.Width;
            int oh = image.Height;
            if (width >= ow && height >= oh)
            {
                return image;
            }
            //如果结果要比原来的宽 
            if (width > ow)
            {
                width = ow;
            }
            if (height > oh)
            {
                height = oh;
            }
            if (image.Width/(double) image.Height > width/(double) height)
            {
                oh = image.Height;
                ow = image.Height*width/height;
                y = 0;
                x = (image.Width - ow)/2;
            }
            else
            {
                ow = image.Width;
                oh = image.Width*height/width;
                x = 0;
                y = (image.Height - oh)/2;
            }
            Image newImage = new Bitmap(width, height);
            using (Graphics newGp = Graphics.FromImage(newImage))
            {
                newGp.CompositingQuality = CompositingQuality.HighQuality;
                //设置高质量插值法 
                newGp.InterpolationMode = InterpolationMode.HighQualityBicubic;
                //设置高质量,低速度呈现平滑程度 
                newGp.SmoothingMode = SmoothingMode.HighQuality;
                //清空画布并以透明背景色填充 
                newGp.Clear(Color.Transparent);
                newGp.DrawImage(image, new Rectangle(0, 0, width, height),
                                new Rectangle(x, y, ow, oh),
                                GraphicsUnit.Pixel);
            }
            return newImage;
        }

        #endregion

        #region 生成图像缩略图

        /// <summary>
        /// 生成缩略图
        /// </summary>
        /// <param name="originalImagePath">源图路径（物理路径）</param>
        /// <param name="thumbnailPath">缩略图路径（物理路径）</param>
        /// <param name="width">缩略图宽度</param>
        /// <param name="height">缩略图高度</param>
        /// <param name="mode">生成缩略图的方式</param>
        public static void MakeThumbnail(string originalImagePath, string thumbnailPath, int width, int height,
                                         string mode)
        {
            Image originalImage = Image.FromFile(originalImagePath);

            int towidth = width;
            int toheight = height;

            int x = 0;
            int y = 0;
            int ow = originalImage.Width;
            int oh = originalImage.Height;

            switch (mode)
            {
                case "HW": //指定高宽缩放（可能变形）                
                    break;
                case "W": //指定宽，高按比例                    
                    toheight = originalImage.Height*width/originalImage.Width;
                    break;
                case "H": //指定高，宽按比例
                    towidth = originalImage.Width*height/originalImage.Height;
                    break;
                case "Cut": //指定高宽裁减（不变形）                
                    if (originalImage.Width/(double) originalImage.Height > towidth/(double) toheight)
                    {
                        oh = originalImage.Height;
                        ow = originalImage.Height*towidth/toheight;
                        y = 0;
                        x = (originalImage.Width - ow)/2;
                    }
                    else
                    {
                        ow = originalImage.Width;
                        oh = originalImage.Width*height/towidth;
                        x = 0;
                        y = (originalImage.Height - oh)/2;
                    }
                    break;
                default:
                    break;
            }

            //新建一个bmp图片
            Image bitmap = new Bitmap(towidth, toheight);

            //新建一个画板
            Graphics g = Graphics.FromImage(bitmap);

            //设置高质量插值法
            g.InterpolationMode = InterpolationMode.High;

            //设置高质量,低速度呈现平滑程度
            g.SmoothingMode = SmoothingMode.HighQuality;

            //清空画布并以透明背景色填充
            g.Clear(Color.Transparent);

            //在指定位置并且按指定大小绘制原图片的指定部分
            g.DrawImage(originalImage, new Rectangle(0, 0, towidth, toheight),
                        new Rectangle(x, y, ow, oh),
                        GraphicsUnit.Pixel);

            try
            {
                //以jpg格式保存缩略图
                bitmap.Save(thumbnailPath, ImageFormat.Jpeg);
            }
            catch (Exception e)
            {
                throw e;
            }
            finally
            {
                originalImage.Dispose();
                bitmap.Dispose();
                g.Dispose();
            }
        }

        /// <summary>
        /// 生成缩略图
        /// </summary>
        /// <param name="originalImagePath">源图路径（物理路径）</param>
        /// <param name="thumbnailPath">缩略图路径（物理路径）</param>
        /// <param name="width">缩略图宽度</param>
        /// <param name="height">缩略图高度</param>
        /// <param name="mode">生成缩略图的方式</param>
        public static void MakeThumbnail(string originalImagePath, string thumbnailPath, int width, int height,
                                         string mode, bool del)
        {
            Image originalImage = Image.FromFile(originalImagePath);

            int towidth = width;
            int toheight = height;

            int x = 0;
            int y = 0;
            int ow = originalImage.Width;
            int oh = originalImage.Height;

            switch (mode)
            {
                case "HW": //指定高宽缩放（可能变形）                
                    break;
                case "W": //指定宽，高按比例                    
                    toheight = originalImage.Height*width/originalImage.Width;
                    break;
                case "H": //指定高，宽按比例
                    towidth = originalImage.Width*height/originalImage.Height;
                    break;
                case "Cut": //指定高宽裁减（不变形）                
                    if (originalImage.Width/(double) originalImage.Height > towidth/(double) toheight)
                    {
                        oh = originalImage.Height;
                        ow = originalImage.Height*towidth/toheight;
                        y = 0;
                        x = (originalImage.Width - ow)/2;
                    }
                    else
                    {
                        ow = originalImage.Width;
                        oh = originalImage.Width*height/towidth;
                        x = 0;
                        y = (originalImage.Height - oh)/2;
                    }
                    break;
                default:
                    break;
            }

            //新建一个bmp图片
            Image bitmap = new Bitmap(towidth, toheight);

            //新建一个画板
            Graphics g = Graphics.FromImage(bitmap);

            //设置高质量插值法
            g.InterpolationMode = InterpolationMode.High;

            //设置高质量,低速度呈现平滑程度
            g.SmoothingMode = SmoothingMode.HighQuality;

            //清空画布并以透明背景色填充
            g.Clear(Color.Transparent);

            //在指定位置并且按指定大小绘制原图片的指定部分
            g.DrawImage(originalImage, new Rectangle(0, 0, towidth, toheight),
                        new Rectangle(x, y, ow, oh),
                        GraphicsUnit.Pixel);

            try
            {
                //以jpg格式保存缩略图
                bitmap.Save(thumbnailPath, ImageFormat.Jpeg);
                bitmap.Dispose();
                originalImage.Dispose();
                g.Dispose();
                if (del)
                {
                    try
                    {
                        File.Delete(originalImagePath);
                    }
                    catch
                    {
                    }
                }
            }
            catch (Exception e)
            {
                throw e;
            }
            finally
            {
                originalImage.Dispose();
                bitmap.Dispose();
                g.Dispose();
                if (del)
                {
                    try
                    {
                        File.Delete(originalImagePath);
                    }
                    catch
                    {
                    }
                }
            }
        }

        #endregion

        #region 生成图像的水印

        /// <summary>
        /// 打水印，在某一点
        /// http://www.cnblogs.com/pooeo/
        /// </summary>
        /// <param name="image"></param>
        /// <param name="waterImagePath"></param>
        /// <param name="p"></param>
        public static void Makewater(Image image, String waterImagePath, Point p)
        {
            Makewater(image, waterImagePath, p, ImagePosition.TopLeft);
        }

        public static void Makewater(Image image, String waterImagePath, Point p, ImagePosition imagePosition)
        {
            using (Image warterImage = Image.FromFile(waterImagePath))
            {
                using (Graphics newGp = Graphics.FromImage(image))
                {
                    newGp.CompositingQuality = CompositingQuality.HighQuality;
                    //设置高质量插值法 
                    newGp.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    //设置高质量,低速度呈现平滑程度 
                    newGp.SmoothingMode = SmoothingMode.HighQuality;
                    switch (imagePosition)
                    {
                        case ImagePosition.BottomLeft:
                            p.Y = image.Height - warterImage.Height - p.Y;
                            break;
                        case ImagePosition.TopRigth:
                            p.X = image.Width - warterImage.Width - p.X;
                            break;
                        case ImagePosition.BottomRight:
                            p.Y = image.Height - warterImage.Height - p.Y;
                            p.X = image.Width - warterImage.Width - p.X;
                            break;
                    }
                    newGp.DrawImage(warterImage, new Rectangle(p, new Size(warterImage.Width, warterImage.Height)));
                }
            }
        }

        public static void Makewater(Image image, String waterStr, Font font, Brush brush, Point p)
        {
            Makewater(image, waterStr, font, brush, p, ImagePosition.TopLeft);
        }

        public static void Makewater(Image image, String waterStr, Font font, Brush brush, Point p,
                                     ImagePosition imagePosition)
        {
            using (Graphics newGp = Graphics.FromImage(image))
            {
                Int32 stringWidth;
                Int32 stringHeight;
                stringHeight = (int) font.Size;
                stringWidth = (int) ((GetBitLength(waterStr)/(float) 2)*(font.Size + 1));
                newGp.CompositingQuality = CompositingQuality.HighQuality;
                //设置高质量插值法 
                newGp.InterpolationMode = InterpolationMode.HighQualityBicubic;
                //设置高质量,低速度呈现平滑程度 
                newGp.SmoothingMode = SmoothingMode.HighQuality;
                //文字抗锯齿 
                newGp.TextRenderingHint = TextRenderingHint.AntiAlias;
                switch (imagePosition)
                {
                    case ImagePosition.BottomLeft:
                        p.Y = image.Height - stringHeight - p.Y;
                        break;
                    case ImagePosition.TopRigth:
                        p.X = image.Width - stringWidth - p.X;
                        break;
                    case ImagePosition.BottomRight:
                        p.Y = image.Height - stringHeight - p.Y;
                        p.X = image.Width - stringWidth - p.X;
                        break;
                }
                newGp.DrawString(waterStr, font, brush, p);
            }
        }

        /// <summary>
        /// 当字符串中有中文时，一个中文的长度表示为2
        /// 如 StringDeal.GetBitLength("123")没有中文返回的长度是3
        /// 如 StringDeal.GetBigLength("123四")有中文返回的长度是5，如果直接用"123四".Length返回的是4
        /// </summary>
        /// <param name="waterStr"></param>
        /// <returns></returns>
        private static int GetBitLength(string waterStr)
        {
            var regex = new Regex("[u4e00-u9fa5]+");
            int length = waterStr.Length;
            for (int i = 0; i < waterStr.Length; i++)
            {
                if (regex.IsMatch(waterStr.Substring(i, 1)))
                {
                    length++;
                }
            }
            return length;
        }

        #endregion

        #region 图像旋转

        /// <summary>
        /// 旋转图像，Creates a new Image containing the same image only rotated
        /// </summary>
        /// <param name="image">
        /// The <see cref="System.Drawing.Image" /> to rotate
        /// </param>
        /// <param name="angle">The amount to rotate the image, clockwise, in degrees</param>
        /// <returns>
        /// A new <see cref="System.Drawing.Bitmap" /> of the same size rotated.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <see cref="image" /> is null.
        /// </exception>
        public static Bitmap RotateImage(Image image, float angle)
        {
            return RotateImage(image, new PointF((float) image.Width/2, (float) image.Height/2), angle);
        }

        /// <summary>
        /// 旋转图像，Creates a new Image containing the same image only rotated
        /// </summary>
        /// <param name="image">
        /// The <see cref="System.Drawing.Image" /> to rotate
        /// </param>
        /// <param name="offset">The position to rotate from.</param>
        /// <param name="angle">The amount to rotate the image, clockwise, in degrees</param>
        /// <returns>
        /// A new <see cref="System.Drawing.Bitmap" /> of the same size rotated.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <see cref="image" /> is null.
        /// </exception>
        public static Bitmap RotateImage(Image image, PointF offset, float angle)
        {
            if (image == null)
                throw new ArgumentNullException("image");

            //create a new empty bitmap to hold rotated image
            var rotatedBmp = new Bitmap(image.Width, image.Height);
            rotatedBmp.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            //make a graphics object from the empty bitmap
            Graphics g = Graphics.FromImage(rotatedBmp);

            //Put the rotation point in the center of the image
            g.TranslateTransform(offset.X, offset.Y);

            //rotate the image
            g.RotateTransform(angle);

            //move the image back
            g.TranslateTransform(-offset.X, -offset.Y);

            //draw passed in image onto graphics object
            g.DrawImage(image, new PointF(0, 0));

            return rotatedBmp;
        }

        #endregion

        /// <summary>
        /// 高质量保存Image对象
        /// </summary>
        /// <param name="image">Image对象</param>
        /// <param name="path">文件保存路径</param>
        public static void SaveQuality(Image image, String path)
        {
            ImageCodecInfo myImageCodecInfo;
            Encoder myEncoder;
            EncoderParameter myEncoderParameter;
            EncoderParameters myEncoderParameters;
            myImageCodecInfo = ImageCodecInfo.GetImageEncoders()[0];
            myEncoder = Encoder.Quality;
            myEncoderParameters = new EncoderParameters(1);
            myEncoderParameter = new EncoderParameter(myEncoder, 100L); // 0-100 
            myEncoderParameters.Param[0] = myEncoderParameter;
            try
            {
                image.Save(path, myImageCodecInfo, myEncoderParameters);
            }
            finally
            {
                myEncoderParameter.Dispose();
                myEncoderParameters.Dispose();
            }
        }

        /// <summary>
        /// 镜面倒影效果
        /// </summary>
        /// <param name="img"></param>
        /// <param name="toBG"></param>
        /// <returns></returns>
        public static Image DrawReflection(Image img, Color toBG) // img is the original image.
        {
            //This is the static function that generates the reflection...
            int height = img.Height + 100; //Added height from the original height of the image.
            var bmp = new Bitmap(img.Width, height, PixelFormat.Format64bppPArgb); //A new bitmap.
            Brush brsh = new LinearGradientBrush(new Rectangle(0, 0, img.Width + 10, height), Color.Transparent, toBG,
                                                 LinearGradientMode.Vertical);
                //The Brush that generates the fading effect to a specific color of your background.
            bmp.SetResolution(img.HorizontalResolution, img.VerticalResolution); //Sets the new bitmap's resolution.
            using (Graphics grfx = Graphics.FromImage(bmp))
                //A graphics to be generated from an image (here, the new Bitmap we've created (bmp)).
            {
                var bm = (Bitmap) img; //Generates a bitmap from the original image (img).
                grfx.DrawImage(bm, 0, 0, img.Width, img.Height);
                    //Draws the generated bitmap (bm) to the new bitmap (bmp).
                var bm1 = (Bitmap) img; //Generates a bitmap again from the original image (img).
                bm1.RotateFlip(RotateFlipType.Rotate180FlipX); //Flips and rotates the image (bm1).
                grfx.DrawImage(bm1, 0, img.Height); //Draws (bm1) below (bm) so it serves as the reflection image.
                var rt = new Rectangle(0, img.Height, img.Width, 100); //A new rectangle to paint our gradient effect.
                grfx.FillRectangle(brsh, rt); //Brushes the gradient on (rt).
            }
            return bmp; //Returns the (bmp) with the generated image.
        }

        public static Color GetNearestWebColor(Color input_color)
        {
            var WebColors = new List<Color>();
            PropertyInfo[] properties = typeof (Color).GetProperties(BindingFlags.Public | BindingFlags.Static);
            foreach (PropertyInfo info in properties)
            {
                if (info.PropertyType.Equals(typeof (Color)))
                {
                    var color = (Color) info.GetValue(typeof (Color), null);
                    WebColors.Add(color);
                }
            }

            double Red = Convert.ToDouble(input_color.R);
            double Green = Convert.ToDouble(input_color.G);
            double Blue = Convert.ToDouble(input_color.B);
            double num4 = 500.0;

            Color empty = Color.Empty;
            foreach (Color mColor in WebColors)
            {
                double num6 = Math.Pow(Convert.ToDouble(mColor.R) - Red, 2.0);
                double num7 = Math.Pow(Convert.ToDouble(mColor.G) - Green, 2.0);
                double num8 = Math.Pow(Convert.ToDouble(mColor.B) - Blue, 2.0);
                double num5 = Math.Sqrt(num8 + num7 + num6);
                if (num5 < num4)
                {
                    num4 = num5;
                    empty = mColor;
                }
            }
            return empty;
        }


        /// <summary>
        /// 生成单色PNG图片
        /// </summary>
        /// <param name="width">图片宽度</param>
        /// <param name="height">图片高度</param>
        /// <param name="Opacity">透明度</param>
        /// <param name="BackColor">图片颜色</param>
        /// <param name="filePath">PNG文件生成路径</param>
        public static void CreateSingleColorPNG(int width, int height, int Opacity, Color BackColor, string filePath)
        {
            var bitmap = new Bitmap(width, height);
            Graphics g = Graphics.FromImage(bitmap);
            g.Clear(Color.FromArgb(100 - Opacity, BackColor));
            g.Save();
            bitmap.Save(filePath, ImageFormat.Png);
            g.Dispose();
            bitmap.Dispose();
        }

        /// <summary>
        /// 生成渐变颜色的PNG图片
        /// </summary>
        /// <param name="filePath">PNG文件生成路径</param>
        /// <param name="width">图片宽度</param>
        /// <param name="height">图片高度</param>
        /// <param name="beginColor">开始颜色</param>
        /// <param name="EndColor">结束颜色</param>
        /// <param name="OpacityBegin">开始透明度</param>
        /// <param name="OpacityEnd">结束透明度</param>
        public static void CreateLinearGradientPNG(string filePath, int width, int height, Color beginColor,
                                                   Color EndColor, int OpacityBegin, int OpacityEnd)
        {
            var bitmap = new Bitmap(width, height);
            Graphics g = Graphics.FromImage(bitmap);
            g.Clear(Color.Transparent);
            var rect = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
            Color colorBegin = Color.FromArgb(100 - OpacityBegin, beginColor);
            Color colorEnd = Color.FromArgb(100 - OpacityEnd, EndColor);
            Brush brush = new LinearGradientBrush(rect, colorBegin, colorEnd, 90);
            g.FillRectangle(brush, rect);
            g.Save();
            bitmap.Save(filePath, ImageFormat.Png);
            g.Dispose();
            bitmap.Dispose();
        }


        /// <summary>
        /// 根据图形获取图形的扩展名
        /// </summary>
        /// <param name="p_Image">图形</param>
        /// <returns>扩展名</returns>
        public static string GetImageExtension(Image p_Image)
        {
            Type Type = typeof (ImageFormat);
            PropertyInfo[] _ImageFormatList = Type.GetProperties(BindingFlags.Static | BindingFlags.Public);
            for (int i = 0; i != _ImageFormatList.Length; i++)
            {
                var _FormatClass = (ImageFormat) _ImageFormatList[i].GetValue(null, null);
                if (_FormatClass.Guid.Equals(p_Image.RawFormat.Guid))
                {
                    return _ImageFormatList[i].Name;
                }
            }
            return "";
        }


        /// <summary>
        /// 特定的图片实例转化为byte[]类型
        /// </summary>
        /// <param name="img"></param>
        /// <returns></returns>
        public static byte[] GetImageBytes(Image img)
        {
            byte[] bt = null;
            if (!img.Equals(null))
            {
                using (var mostream = new MemoryStream())
                {
                    var bmp = new Bitmap(img);
                    bmp.Save(mostream, ImageFormat.Jpeg); //将图像以指定的格式存入缓存内存流
                    bt = new byte[mostream.Length];
                    mostream.Position = 0; //设置留的初始位置
                    mostream.Read(bt, 0, Convert.ToInt32(bt.Length));
                }
            }
            return bt;
        }

        /// <summary>
        /// 特定路径的图片文件转化为byte[]类型
        /// </summary>
        /// <param name="strFile">图片路径</param>
        /// <returns>byte[]</returns>
        public static byte[] GetImageBytes(string strFile)
        {
            byte[] photo_byte = null;
            using (var fs =
                new FileStream(strFile, FileMode.Open, FileAccess.Read))
            {
                using (var br = new BinaryReader(fs))
                {
                    photo_byte = br.ReadBytes((int) fs.Length);
                }
            }
            return photo_byte;
        }

        /// <summary>
        /// 读取byte[]并转化为图片
        /// </summary>
        /// <param name="bytes">byte[]</param>
        /// <returns>Image</returns>
        public static Image GetImage(byte[] bytes)
        {
            Image photo = null;
            using (var ms = new MemoryStream(bytes))
            {
                ms.Write(bytes, 0, bytes.Length);
                photo = Image.FromStream(ms, true);
            }
            return photo;
        }

        /// <summary>
        /// 无文件锁方式读取特定路径的文件的Image实例
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static Image GetImage(string fileName)
        {
            byte[] bytes = File.ReadAllBytes(fileName);
            using (var ms = new MemoryStream(bytes))
            {
                return Image.FromStream(ms);
            }
        }

        public static Bitmap ResizeImage(Image image, int width, int height)
        {
            var result = new Bitmap(width, height);

            //use a graphics object to draw the resized image into the bitmap
            using (Graphics graphics = Graphics.FromImage(result))
            {
                //set the resize quality modes to high quality
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                //draw the image into the target bitmap
                graphics.DrawImage(image, 0, 0, result.Width, result.Height);
            }

            //return the resulting bitmap
            return result;
        }

        /// <summary>
        /// 图像灰度处理
        /// </summary>
        /// <param name="SourceImg">要进行灰度处理的图像源</param>
        /// <param name="style">灰度处理方式，支持move,float,int,avg,green</param>
        /// <returns></returns>
        public static Bitmap GrayImage(Image SourceImg, string style)
        {
            var bmp = new Bitmap(SourceImg);
            var newBmp = new Bitmap(SourceImg);
            var color = new Color();
            Color newColor;
            Byte r, g, b, gray = 0;

            for (int i = 0; i < bmp.Width; i++)
            {
                for (int j = 0; j < bmp.Height; j++)
                {
                    //获取颜色
                    color = bmp.GetPixel(i, j);
                    r = color.R;
                    g = color.G;
                    b = color.B;
                    switch (style.ToLower())
                    {
                        case "move":
                            //采用移位运算
                            gray = (Byte) ((r*19596 + g*38469 + b*7472) >> 16);
                            break;
                        case "float":
                            //采用浮点运算
                            gray = (Byte) (r*0.3 + g*0.59 + b*0.11);
                            break;
                        case "int":
                            //采用整数运算
                            gray = (Byte) ((r*30 + g*59 + b*11)/100);
                            break;
                        case "avg":
                            //采用平均值法运算
                            gray = (Byte) ((r + g + b)/3);
                            break;
                        case "green":
                            //仅取绿色
                            gray = g;
                            break;
                    }

                    newColor = Color.FromArgb(gray, gray, gray);
                    newBmp.SetPixel(i, j, newColor);
                }
            }
            return newBmp;
        }

        public static Bitmap MakeGrayscale(Bitmap original)
        {
            //create a blank bitmap the same size as original
            var newBitmap = new Bitmap(original.Width, original.Height);

            //get a graphics object from the new image
            Graphics g = Graphics.FromImage(newBitmap);

            //create the grayscale ColorMatrix
            var colorMatrix = new ColorMatrix(
                new[]
                    {
                        new[] {.3f, .3f, .3f, 0, 0},
                        new[] {.59f, .59f, .59f, 0, 0},
                        new[] {.11f, .11f, .11f, 0, 0},
                        new float[] {0, 0, 0, 1, 0},
                        new float[] {0, 0, 0, 0, 1}
                    });

            //create some image attributes
            var attributes = new ImageAttributes();

            //set the color matrix attribute
            attributes.SetColorMatrix(colorMatrix);

            //draw the original image on the new image
            //using the grayscale color matrix
            g.DrawImage(original, new Rectangle(0, 0, original.Width, original.Height),
                        0, 0, original.Width, original.Height, GraphicsUnit.Pixel, attributes);

            //dispose the Graphics object
            g.Dispose();
            return newBitmap;
        }


        /// <summary>
        /// Crops an image according to a selection rectangel
        /// </summary>
        /// <param name="image">
        /// the image to be cropped
        /// </param>
        /// <param name="selection">
        /// the selection
        /// </param>
        /// <returns>
        /// cropped image
        /// </returns>
        public static Image CropImage(this Image image, Rectangle selection)
        {
            var bmp = image as Bitmap;

            // Check if it is a bitmap:
            if (bmp == null)
                throw new ArgumentException("Kein gültiges Bild (Bitmap)");

            // Crop the image:
            Bitmap cropBmp = bmp.Clone(selection, bmp.PixelFormat);

            // Release the resources:
            image.Dispose();

            return cropBmp;
        }

        #region 判断图形里是否存在另外一个图形 并返回所在位置

        /// <summary>
        /// 判断图形里是否存在另外一个图形 并返回所在位置
        /// </summary>
        /// <param name=” p_SourceBitmap”>原始图形</param>
        /// <param name=” p_PartBitmap”>小图形</param>
        /// <param name=” p_Float”>溶差</param>
        /// <returns>坐标</returns>
        public static Point GetImageContains(Bitmap p_SourceBitmap, Bitmap p_PartBitmap, int p_Float)
        {
            int _SourceWidth = p_SourceBitmap.Width;
            int _SourceHeight = p_SourceBitmap.Height;
            int _PartWidth = p_PartBitmap.Width;
            int _PartHeight = p_PartBitmap.Height;
            var _SourceBitmap = new Bitmap(_SourceWidth, _SourceHeight);
            Graphics _Graphics = Graphics.FromImage(_SourceBitmap);
            _Graphics.DrawImage(p_SourceBitmap, new Rectangle(0, 0, _SourceWidth, _SourceHeight));
            _Graphics.Dispose();
            BitmapData _SourceData = _SourceBitmap.LockBits(new Rectangle(0, 0, _SourceWidth, _SourceHeight),
                                                            ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
            var _SourceByte = new byte[_SourceData.Stride*_SourceHeight];
            Marshal.Copy(_SourceData.Scan0, _SourceByte, 0, _SourceByte.Length); //复制出p_SourceBitmap的相素信息
            _SourceBitmap.UnlockBits(_SourceData);
            var _PartBitmap = new Bitmap(_PartWidth, _PartHeight);
            _Graphics = Graphics.FromImage(_PartBitmap);
            _Graphics.DrawImage(p_PartBitmap, new Rectangle(0, 0, _PartWidth, _PartHeight));
            _Graphics.Dispose();
            BitmapData _PartData = _PartBitmap.LockBits(new Rectangle(0, 0, _PartWidth, _PartHeight),
                                                        ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
            var _PartByte = new byte[_PartData.Stride*_PartHeight];
            Marshal.Copy(_PartData.Scan0, _PartByte, 0, _PartByte.Length); //复制出p_PartBitmap的相素信息
            _PartBitmap.UnlockBits(_PartData);
            for (int i = 0; i != _SourceHeight; i++)
            {
                if ((_SourceHeight - i) < _PartHeight) return new Point(-1, -1); //如果 剩余的高 比需要比较的高 还要小 就直接返回
                int _PointX = -1; //临时存放坐标 需要包正找到的是在一个X点上
                bool _SacnOver = true; //是否都比配的上
                for (int z = 0; z != (_PartHeight - 1); z++) //循环目标进行比较
                {
                    int _TrueX = GetImageContains(_SourceByte, _PartByte, (i + z)*_SourceData.Stride, z*_PartData.Stride,
                                                  _SourceWidth, _PartWidth, p_Float);
                    if (_TrueX == -1) //如果没找到
                    {
                        _PointX = -1; //设置坐标为没找到
                        _SacnOver = false; //设置不进行返回
                        break;
                    }
                    else
                    {
                        if (z == 0) _PointX = _TrueX;
                        if (_PointX != _TrueX) //如果找到了 也的保证坐标和上一行的坐标一样 否则也返回
                        {
                            _PointX = -1; //设置坐标为没找到
                            _SacnOver = false; //设置不进行返回
                            break;
                        }
                    }
                }
                if (_SacnOver) return new Point(_PointX, i);
            }
            return new Point(-1, -1);
        }

        /// <summary>
        /// 判断图形里是否存在另外一个图形 所在行的索引
        /// </summary>
        /// <param name=” p_Source”>原始图形数据</param>
        /// <param name=” p_Part”>小图形数据</param>
        /// <param name=” p_SourceIndex”>开始位置</param>
        /// <param name=” p_SourceWidth”>原始图形宽</param>
        /// <param name=” p_PartWidth”>小图宽</param>
        /// <param name=” p_Float”>溶差</param>
        /// <returns>所在行的索引 如果找不到返回-1</returns>
        private static int GetImageContains(byte[] p_Source, byte[] p_Part, int p_SourceIndex, int p_PartIndex,
                                            int p_SourceWidth, int p_PartWidth, int p_Float)
        {
            int _PartIndex = p_PartIndex; //
            int _PartRVA = _PartIndex; //p_PartX轴起点
            int _SourceIndex = p_SourceIndex; //p_SourceX轴起点
            for (int i = 0; i < p_SourceWidth; i++)
            {
                if ((p_SourceWidth - i) < p_PartWidth) return -1;
                Color _CurrentlyColor = Color.FromArgb(p_Source[_SourceIndex + 3], p_Source[_SourceIndex + 2],
                                                       p_Source[_SourceIndex + 1], p_Source[_SourceIndex]);
                Color _CompareColoe = Color.FromArgb(p_Part[_PartRVA + 3], p_Part[_PartRVA + 2], p_Part[_PartRVA + 1],
                                                     p_Part[_PartRVA]);
                _SourceIndex += 4; //成功，p_SourceX轴加4
                bool _ScanColor = ScanColor(_CurrentlyColor, _CompareColoe, p_Float);
                if (_ScanColor)
                {
                    _PartRVA += 4; //成功，p_PartX轴加4
                    int _SourceRVA = _SourceIndex;
                    bool _Equals = true;
                    for (int z = 0; z != p_PartWidth - 1; z++)
                    {
                        _CurrentlyColor = Color.FromArgb(p_Source[_SourceRVA + 3], p_Source[_SourceRVA + 2],
                                                         p_Source[_SourceRVA + 1], p_Source[_SourceRVA]);
                        _CompareColoe = Color.FromArgb(p_Part[_PartRVA + 3], p_Part[_PartRVA + 2], p_Part[_PartRVA + 1],
                                                       p_Part[_PartRVA]);
                        if (!ScanColor(_CurrentlyColor, _CompareColoe, p_Float))
                        {
                            _PartRVA = _PartIndex; //失败，重置p_PartX轴开始
                            _Equals = false;
                            break;
                        }
                        _PartRVA += 4; //成功，p_PartX轴加4
                        _SourceRVA += 4; //成功，p_SourceX轴加4
                    }
                    if (_Equals) return i;
                }
                else
                {
                    _PartRVA = _PartIndex; //失败，重置p_PartX轴开始
                }
            }
            return -1;
        }

        /// <summary>
        /// 检查色彩(可以根据这个更改比较方式
        /// </summary>
        /// <param name=” p_CurrentlyColor”>当前色彩</param>
        /// <param name=” p_CompareColor”>比较色彩</param>
        /// <param name=” p_Float”>溶差</param>
        /// <returns></returns>
        private static bool ScanColor(Color p_CurrentlyColor, Color p_CompareColor, int p_Float)
        {
            int _R = p_CurrentlyColor.R;
            int _G = p_CurrentlyColor.G;
            int _B = p_CurrentlyColor.B;
            return (_R <= p_CompareColor.R + p_Float && _R >= p_CompareColor.R - p_Float) &&
                   (_G <= p_CompareColor.G + p_Float && _G >= p_CompareColor.G - p_Float) &&
                   (_B <= p_CompareColor.B + p_Float && _B >= p_CompareColor.B - p_Float);
        }

        #endregion


        public static bool IsImage(string filepath)
        {
            System.Drawing.Image oImg = null;
            try
            {
                oImg = System.Drawing.Image.FromFile(filepath);
                oImg.Dispose();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

    }
}