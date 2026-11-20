namespace ScreenshotMaker;

// Анімований GIF (GIF89a) без сторонніх бібліотек: спільна палітра на 256 кольорів (найчастіші кольори кадрів),
// кожен наступний кадр записується лише там, де щось змінилося, стиснення LZW, нескінченний повтор
internal static class GifWriter
{
    // frames — кадри однакового розміру у форматі RGB (по 3 байти на піксель), delays — тривалість кожного, 1/100 с
    public static void Save(string path, int width, int height, IReadOnlyList<byte[]> frames, IReadOnlyList<int> delays)
    {
        int[] palette = BuildPalette(frames);
        Dictionary<int, byte> nearest = new();

        using BinaryWriter w = new(File.Create(path));
        w.Write("GIF89a"u8.ToArray());
        w.Write((ushort)width);
        w.Write((ushort)height);
        w.Write((byte)0xF7);   // глобальна палітра, 256 кольорів
        w.Write((byte)0);
        w.Write((byte)0);

        foreach (int color in palette)
        {
            w.Write((byte)(color >> 16));
            w.Write((byte)(color >> 8));
            w.Write((byte)color);
        }

        // Розширення NETSCAPE2.0: повторювати без кінця
        w.Write(new byte[] { 0x21, 0xFF, 0x0B });
        w.Write("NETSCAPE2.0"u8.ToArray());
        w.Write(new byte[] { 0x03, 0x01, 0x00, 0x00, 0x00 });

        byte[]? previous = null;
        List<(Rectangle Area, byte[] Pixels, int Delay)> written = new();

        for (int f = 0; f < frames.Count; f++)
        {
            byte[] indices = ToIndices(frames[f], palette, nearest);
            Rectangle area = previous == null ? new Rectangle(0, 0, width, height) : Changed(previous, indices, width, height);

            if (area.IsEmpty)
            {
                // Нічого не змінилося — подовжуємо попередній кадр
                var last = written[^1];
                written[^1] = (last.Area, last.Pixels, last.Delay + delays[f]);
                continue;
            }

            written.Add((area, Crop(indices, width, area), delays[f]));
            previous = indices;
        }

        foreach ((Rectangle area, byte[] pixels, int delay) in written)
        {
            // Керування кадром: попередній кадр лишається під новим (disposal 1), затримка
            w.Write(new byte[] { 0x21, 0xF9, 0x04, 0x04 });
            w.Write((ushort)Math.Max(2, delay));
            w.Write(new byte[] { 0x00, 0x00 });

            w.Write((byte)0x2C);
            w.Write((ushort)area.X);
            w.Write((ushort)area.Y);
            w.Write((ushort)area.Width);
            w.Write((ushort)area.Height);
            w.Write((byte)0);
            WriteLzw(w, pixels);
        }

        w.Write((byte)0x3B);
    }

    // Палітра: найчастіші кольори (з точністю 5 біт на канал) з кожного п'ятого кадру
    private static int[] BuildPalette(IReadOnlyList<byte[]> frames)
    {
        Dictionary<int, (long Count, long R, long G, long B)> buckets = new();

        for (int f = 0; f < frames.Count; f += 5)
        {
            byte[] rgb = frames[f];

            for (int i = 0; i < rgb.Length; i += 3)
            {
                int key = (rgb[i] >> 3) << 10 | (rgb[i + 1] >> 3) << 5 | rgb[i + 2] >> 3;
                buckets.TryGetValue(key, out var b);
                buckets[key] = (b.Count + 1, b.R + rgb[i], b.G + rgb[i + 1], b.B + rgb[i + 2]);
            }
        }

        int[] palette = buckets.Values
            .OrderByDescending(b => b.Count)
            .Take(256)
            .Select(b => (int)(b.R / b.Count) << 16 | (int)(b.G / b.Count) << 8 | (int)(b.B / b.Count))
            .ToArray();

        return palette.Concat(Enumerable.Repeat(0, 256 - palette.Length)).ToArray();
    }

    private static byte[] ToIndices(byte[] rgb, int[] palette, Dictionary<int, byte> nearest)
    {
        byte[] indices = new byte[rgb.Length / 3];

        for (int i = 0, p = 0; i < rgb.Length; i += 3, p++)
        {
            int color = rgb[i] << 16 | rgb[i + 1] << 8 | rgb[i + 2];

            if (!nearest.TryGetValue(color, out byte index))
            {
                int best = int.MaxValue;

                for (int c = 0; c < palette.Length; c++)
                {
                    int dr = (palette[c] >> 16 & 0xFF) - rgb[i], dg = (palette[c] >> 8 & 0xFF) - rgb[i + 1], db = (palette[c] & 0xFF) - rgb[i + 2];
                    int d = dr * dr * 3 + dg * dg * 4 + db * db * 2;

                    if (d < best)
                    {
                        best = d;
                        index = (byte)c;
                    }
                }

                nearest[color] = index;
            }

            indices[p] = index;
        }

        return indices;
    }

    // Прямокутник, у якому кадр відрізняється від попереднього (порожній — кадри однакові)
    private static Rectangle Changed(byte[] a, byte[] b, int width, int height)
    {
        int left = width, top = height, right = -1, bottom = -1;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (a[y * width + x] != b[y * width + x])
                {
                    left = Math.Min(left, x);
                    right = Math.Max(right, x);
                    top = Math.Min(top, y);
                    bottom = Math.Max(bottom, y);
                }
            }
        }

        return right < 0 ? Rectangle.Empty : Rectangle.FromLTRB(left, top, right + 1, bottom + 1);
    }

    private static byte[] Crop(byte[] indices, int width, Rectangle area)
    {
        byte[] result = new byte[area.Width * area.Height];

        for (int y = 0; y < area.Height; y++)
        {
            Array.Copy(indices, (area.Y + y) * width + area.X, result, y * area.Width, area.Width);
        }

        return result;
    }

    // Стиснення LZW у форматі GIF (як у відомому кодувальнику gif.h): коди змінної довжини 9–12 біт
    private static void WriteLzw(BinaryWriter w, byte[] pixels)
    {
        const int minCodeSize = 8;
        const int clearCode = 1 << minCodeSize;
        const int endCode = clearCode + 1;

        w.Write((byte)minCodeSize);
        BitPacker bits = new(w);
        Dictionary<int, int> dictionary = new();
        int codeSize = minCodeSize + 1;
        int maxCode = endCode;

        bits.Write(clearCode, codeSize);
        int current = pixels[0];

        for (int i = 1; i < pixels.Length; i++)
        {
            int next = pixels[i];
            int key = current << 8 | next;

            if (dictionary.TryGetValue(key, out int code))
            {
                current = code;
                continue;
            }

            bits.Write(current, codeSize);
            dictionary[key] = ++maxCode;

            if (maxCode >= 1 << codeSize)
            {
                codeSize++;
            }

            if (maxCode == 4095)
            {
                bits.Write(clearCode, codeSize);
                dictionary.Clear();
                codeSize = minCodeSize + 1;
                maxCode = endCode;
            }

            current = next;
        }

        bits.Write(current, codeSize);
        bits.Write(endCode, codeSize);
        bits.Finish();
    }

    // Біти — молодшими вперед, байти — блоками до 255 з довжиною попереду
    private sealed class BitPacker
    {
        private readonly BinaryWriter writer;
        private readonly byte[] block = new byte[255];
        private int blockLength;
        private int buffer;
        private int bitCount;

        public BitPacker(BinaryWriter writer)
        {
            this.writer = writer;
        }

        public void Write(int code, int size)
        {
            buffer |= code << bitCount;
            bitCount += size;

            while (bitCount >= 8)
            {
                AddByte((byte)buffer);
                buffer >>= 8;
                bitCount -= 8;
            }
        }

        public void Finish()
        {
            if (bitCount > 0)
            {
                AddByte((byte)buffer);
            }

            FlushBlock();
            writer.Write((byte)0);
        }

        private void AddByte(byte value)
        {
            block[blockLength++] = value;

            if (blockLength == 255)
            {
                FlushBlock();
            }
        }

        private void FlushBlock()
        {
            if (blockLength > 0)
            {
                writer.Write((byte)blockLength);
                writer.Write(block, 0, blockLength);
                blockLength = 0;
            }
        }
    }
}
