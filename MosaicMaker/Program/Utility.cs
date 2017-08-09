﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Windows.Forms;

namespace MosaicMakerNS
{
    public static class Utility
    {
        public static void EditBitmap(Bitmap bmp, EditAction action)
        {
            if (bmp == null)
                throw new ArgumentNullException("bmp");

            if (action == null)
                throw new ArgumentNullException("action");

            Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
            PixelFormat format = bmp.PixelFormat;

            BitmapData data = bmp.LockBits(rect, ImageLockMode.ReadWrite, format);
            BitmapProperties ppts = new BitmapProperties(data, format);

            action(ppts);

            bmp.UnlockBits(data);
        }

        public static void SingleCheck(int index, params ToolStripMenuItem[] items)
        {
            if (items == null)
                throw new ArgumentNullException("items");

            foreach (var item in items)
                item.Checked = false;

            items[index].Checked = true;
        }

        public static void SetEnabled(Control ctrl, bool enabled)
        {
            SetEnabled(ctrl, null, enabled);
        }

        public static void SetEnabled(Control ctrl, ToolStripItem item, bool enabled)
        {
            if (ctrl == null)
                throw new ArgumentNullException("ctrl");

            if (item != null)
                item.Enabled = enabled;

            ctrl.Enabled = enabled;
            ctrl.BackColor = enabled ?
                Color.FromArgb(255, ctrl.BackColor) :
                Color.FromArgb(125, ctrl.BackColor);
        }

        public static List<int> GetSteps(int heightInPixels, int elementHeight)
        {
            List<int> steps = new List<int>(heightInPixels / elementHeight);

            for (int i = 0; i < heightInPixels; i += elementHeight)
                steps.Add(i);

            return steps;
        }

        public static Size GetNewImageSize(Size imgSize, Size elementSize)
        {
            int imgWidth = imgSize.Width;
            int imgHeight = imgSize.Height;
            int elementWidth = elementSize.Width;
            int elementHeight = elementSize.Height;

            int modWidth = imgWidth % elementWidth;
            int modHeight = imgHeight % elementHeight;

            if (modWidth != 0)
            {
                int offset = modWidth < elementWidth / 2 ? 0 : 1;
                imgWidth = (imgWidth / elementWidth + offset) * elementWidth;
            }

            if (modHeight != 0)
            {
                int offset = modHeight < elementHeight / 2 ? 0 : 1;
                imgHeight = (imgHeight / elementHeight + offset) * elementHeight;
            }

            return new Size(imgWidth, imgHeight);
        }

        public static Size GetElementSize(Size imgSize, params RadioButton[] buttons)
        {
            if (buttons == null)
                throw new ArgumentNullException("buttons");

            Size size = new Size();

            foreach (var rb in buttons)
            {
                if (rb.Checked)
                {
                    string[] splits = rb.Text.Trim().Split(' ');
                    int w = int.Parse(splits[0], CultureInfo.InvariantCulture);
                    int h = int.Parse(splits[2], CultureInfo.InvariantCulture);

                    size.Width = w;
                    size.Height = h;

                    if (Settings.PixelStrip)
                    {
                        if (!Settings.PixelStripUsesElementWidth)
                            size.Width = 1;

                        size.Height = imgSize.Height;
                    }

                    break;
                }
            }

            return size;
        }

        public static int Clamp(int value, int min, int max)
        {
            if (value > max)
                return max;

            if (value < min)
                return min;

            return value;
        }

        /// <summary>
        /// May return null
        /// </summary>
        public static FileStream GetFileStream(string path)
        {
            try
            {
                return new FileStream(path, FileMode.Open,
                    FileAccess.Read, FileShare.Read);
            }
            catch
            {
                return null;
            }
        }

        public static ImageType GetImageType(string path)
        {
            const byte MAX_BYTES = 4;
            byte[] header = null;

            using (FileStream stream = GetFileStream(path))
            {
                if (stream == null)
                    return ImageType.Error;

                if (stream.Length < MAX_BYTES)
                    return ImageType.Unknown;

                header = new byte[MAX_BYTES];
                stream.Read(header, 0, MAX_BYTES);
            }

            return CheckHeader(header);
        }

        #region Image checks

        private static ImageType CheckHeader(byte[] header)
        {
            if (CheckJPEG(header))
                return ImageType.Jpeg;

            if (CheckPNG(header))
                return ImageType.Png;

            if (CheckBMP(header))
                return ImageType.Bmp;

            if (CheckTIFF(header))
                return ImageType.Tiff;

            return ImageType.Unknown;
        }

        private static bool CheckJPEG(byte[] header)
        {
            return header[0] == 0xFF && header[1] == 0xD8;
        }

        private static bool CheckPNG(byte[] header)
        {
            return header[0] == 0x89 && header[1] == 0x50 &&
                header[2] == 0x4E && header[3] == 0x47;
        }

        private static bool CheckBMP(byte[] header)
        {
            return header[0] == 0x42 && header[1] == 0x4D;
        }

        private static bool CheckTIFF(byte[] header)
        {
            bool ii = header[0] == 0x49 && header[1] == 0x49 &&
                header[2] == 0x2A && header[3] == 0x00;
            bool mm = header[0] == 0x4D && header[1] == 0x4D &&
                header[2] == 0x00 && header[3] == 0x2A;

            return ii || mm;
        }

        #endregion
    }
}