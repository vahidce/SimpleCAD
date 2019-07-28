﻿using System;
using System.Globalization;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using Newtonsoft.Json;

namespace Viewer
{
    public abstract class ShapeDefinition
    {
        [JsonProperty(PropertyName = "type")]
        public abstract string Type { get; }

        [JsonProperty(PropertyName = "color")]
        public string Color { get; set; }

        [JsonProperty(PropertyName = "lineType")]
        public string LineType { get; set; }

        public abstract Shape Convert();

        protected Pen Pen()
        {
            return new Pen(new SolidColorBrush(GetColor()), 1d);
        }

        protected Point Point(string coordinates)
        {
            string[] tokens = coordinates.Split(';');

            var culture = new CultureInfo("de");
            if (double.TryParse(tokens[0], NumberStyles.Float, culture, out double x) &&
                double.TryParse(tokens[1], NumberStyles.Float, culture, out double y))
            {
                return new Point(x, y);
            }

            return default(Point);
        }

        protected Brush RandomBrush()
        {
            Random rand = new Random();
            return new SolidColorBrush(Argb(
                (byte) rand.Next(0, 256),
                (byte) rand.Next(0, 256),
                (byte) rand.Next(0, 256),
                (byte) rand.Next(0, 256)));
        }

        private Color GetColor()
        {
            string[] tokens = Color.Split(';');
            if (byte.TryParse(tokens[0], out byte a) &&
                byte.TryParse(tokens[1], out byte r) &&
                byte.TryParse(tokens[2], out byte g) &&
                byte.TryParse(tokens[3], out byte b))
            {
                return Argb(a, r, g, b);
            }

            return Colors.Black;
        }

        private Color Argb(byte a, byte r, byte g, byte b)
        {
            return System.Windows.Media.Color.FromArgb(a, r, g, b);
        }
    }
}