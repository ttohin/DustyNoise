using System;

namespace Common {
    public class Buffer<T> {
        private T[, ] items;
        public int width = 0;
        public int height = 0;
        public Buffer () { }

        public Buffer (int width, int height) {
            this.items = new T[width, height];
            this.width = width;
            this.height = height;
        }

        public T Get (int x, int y) {
            return items[x, y];
        }

        public bool Get (int x, int y, out T value) {
            value = default (T);
            if (IsInside (x, y)) {
                value = items[x, y];
                return true;
            }

            return false;
        }

        public bool Set (T value, int x, int y) {
            if (!IsInside (x, y))
                return false;

            items[x, y] = value;
            return true;
        }

        public void ForEach (Action<T, int, int> func) {
            for (int i = 0; i < width; ++i) {
                for (int j = 0; j < height; ++j) {
                    func (Get (i, j), i, j);
                }
            }
        }

        public void Scale (int scale, out Buffer<T> result) {
            var temp = new Buffer<T> (width * scale, height * scale);

            ForEach ((value, i, j) => {
                for (int iScale = 0; iScale < scale; ++iScale) {
                    for (int jScale = 0; jScale < scale; ++jScale) {
                        temp.Set (value, i * scale + iScale, j * scale + jScale);
                    }
                }
            });

            result = temp;
        }

        public void Fill (T value) {
            for (int i = 0; i < width; ++i) {
                for (int j = 0; j < height; ++j) {
                    items[i, j] = value;
                }
            }
        }

        private bool IsInside (int x, int y) {
            if (x < 0 || x >= width || y < 0 || y >= height) {
                return false;
            }

            return true;
        }
    }
}