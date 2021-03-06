﻿// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace Microsoft.Xna.Framework.Content.Pipeline.Graphics
{
    public static class GraphicsUtil
    {
        public static byte[] GetData(this Bitmap bmp)
        {
            // Any bitmap using this function should use 32bpp ARGB pixel format, since we have to
            // swizzle the channels later
            System.Diagnostics.Debug.Assert(bmp.PixelFormat == PixelFormat.Format32bppArgb);

            var bitmapData = bmp.LockBits(new System.Drawing.Rectangle(0, 0, bmp.Width, bmp.Height),
                                    ImageLockMode.ReadOnly,
                                    bmp.PixelFormat);

            var length = bitmapData.Stride * bitmapData.Height;
            var output = new byte[length];

            // Copy bitmap to byte[]
            Marshal.Copy(bitmapData.Scan0, output, 0, length);
            bmp.UnlockBits(bitmapData);

            // NOTE: According to http://msdn.microsoft.com/en-us/library/dd183449%28VS.85%29.aspx
            // and http://stackoverflow.com/questions/8104461/pixelformat-format32bppargb-seems-to-have-wrong-byte-order
            // Image data from any GDI based function are stored in memory as BGRA/BGR, even if the format says RBGA.
            // Because of this we flip the R and B channels.

            BGRAtoRGBA(output);
  
            return output;
        }

        public static void BGRAtoRGBA(byte[] data)
        {
            for (var x = 0; x < data.Length; x += 4)
            {
                data[x] ^= data[x + 2];
                data[x + 2] ^= data[x];
                data[x] ^= data[x + 2];
            }
        }

        public static bool IsPowerOfTwo(int x)
        {
            return (x & (x - 1)) == 0;
        }

        /// <summary>
        /// Returns the next power of two. Returns same value if already is PoT.
        /// </summary>
        public static int GetNextPowerOfTwo(int value)
        {
            if (IsPowerOfTwo(value))
                return value;

            var nearestPower = 1;
            while (nearestPower < value)
                nearestPower = nearestPower << 1;

            return nearestPower;
        }

        /// <summary>
        /// Compares a System.Drawing.Color to a Microsoft.Xna.Framework.Color
        /// </summary>
        internal static bool ColorsEqual(this System.Drawing.Color a, Color b)
        {
            if (a.A != b.A)
                return false;

            if (a.R != b.R)
                return false;

            if (a.G != b.G)
                return false;

            if (a.B != b.B)
                return false;

            return true;
        }

        internal static void Resize(this TextureContent content, int newWidth, int newHeight)
        {
            var resizedBmp = new Bitmap(newWidth, newHeight);

            using (var graphics = System.Drawing.Graphics.FromImage(resizedBmp))
            {
                graphics.DrawImage(content._bitmap, 0, 0, newWidth, newHeight);

                content._bitmap.Dispose();
                content._bitmap = resizedBmp;
            }

            var imageData = content._bitmap.GetData();

            var bitmapContent = new PixelBitmapContent<Color>(content._bitmap.Width, content._bitmap.Height);
            bitmapContent.SetPixelData(imageData);

            content.Faces.Clear();
            content.Faces.Add(new MipmapChain(bitmapContent));
        }
    }
}
