using System.Numerics;

namespace GLHDN.Views
{
    /// <summary>
    /// Structure that represents a color. Can be implicitly converted to a RGB <see cref="Vector3"/> or RGBA <see cref="Vector4"/>.
    /// </summary>
    public struct Color
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Color"/> struct.
        /// </summary>
        /// <param name="r">The red component of the color.</param>
        /// <param name="g">The green component of the color.</param>
        /// <param name="b">The blue component of the color.</param>
        /// <param name="a">The alpha component of the color.</param>
        public Color(float r, float g, float b, float a) => (R, G, B, A) = (r, g, b, a);

        /// <summary>
        /// Gets the red component of the color.
        /// </summary>
        public float R { get; }

        /// <summary>
        /// Gets the green component of the color.
        /// </summary>
        public float G { get; }

        /// <summary>
        /// Gets the blue component of the color.
        /// </summary>
        public float B { get; }

        /// <summary>
        /// Gets the alpha component of the color.
        /// </summary>
        public float A { get; }

        /// <summary>
        /// Defines the implicit conversion of a color to a <see cref="Vector3"/> value.
        /// </summary>
        /// <param name="color">The color instance to convert.</param>
        public static implicit operator Vector3(Color color)
        {
            return new Vector3(color.R, color.G, color.B);
        }

        /// <summary>
        /// Defines the implicit conversion of a color to a <see cref="Vector4"/> value.
        /// </summary>
        /// <param name="color">The color instance to convert.</param>
        public static implicit operator Vector4(Color color)
        {
            return new Vector4(color.R, color.G, color.B, color.A);
        }

        /// <summary>
        /// Gets a completely transparent 'color'.
        /// </summary>
        /// <returns>An appropriate <see cref="Color"/> instance.</returns>
        public static Color Transparent() => new Color(0, 0, 0, 0);

        /// <summary>
        /// Returns white.
        /// </summary>
        /// <param name="alpha">The alpha of the returned color.</param>
        /// <returns>An appropriate <see cref="Color"/> instance.</returns>
        public static Color White(float alpha = 1) => new Color(1, 1, 1, alpha);

        /// <summary>
        /// Returns black.
        /// </summary>
        /// <param name="alpha">The alpha of the returned color.</param>
        /// <returns>An appropriate <see cref="Color"/> instance.</returns>
        public static Color Black(float alpha = 1) => new Color(0, 0, 0, alpha);

        /// <summary>
        /// Returns a color that is a shade of grey.
        /// </summary>
        /// <param name="brightness">The brightness of the returned color.</param>
        /// <param name="alpha">The alpha of the returned color.</param>
        /// <returns>An appropriate <see cref="Color"/> instance.</returns>
        public static Color Grey(float brightness = 0.5f, float alpha = 1) => new Color(brightness, brightness, brightness, alpha);

        /// <summary>
        /// Returns pure red.
        /// </summary>
        /// <param name="brightness">The brightness of the returned color.</param>
        /// <param name="alpha">The alpha of the returned color.</param>
        /// <returns>An appropriate <see cref="Color"/> instance.</returns>
        public static Color Red(float brightness = 1, float alpha = 1) => new Color(brightness, 0, 0, alpha);

        /// <summary>
        /// Returns pure green.
        /// </summary>
        /// <param name="brightness">The brightness of the returned color.</param>
        /// <param name="alpha">The alpha of the returned color.</param>
        /// <returns>An appropriate <see cref="Color"/> instance.</returns>
        public static Color Green(float brightness = 1, float alpha = 1) => new Color(0, brightness, 0, alpha);

        /// <summary>
        /// Returns pure blue.
        /// </summary>
        /// <param name="brightness">The brightness of the returned color.</param>
        /// <param name="alpha">The alpha of the returned color.</param>
        /// <returns>An appropriate <see cref="Color"/> instance.</returns>
        public static Color Blue(float brightness = 1, float alpha = 1) => new Color(0, 0, brightness, alpha);

        /// <summary>
        /// Returns yellow.
        /// </summary>
        /// <param name="brightness">The brightness of the returned color.</param>
        /// <param name="alpha">The alpha of the returned color.</param>
        /// <returns>An appropriate <see cref="Color"/> instance.</returns>
        public static Color Yellow(float brightness = 1, float alpha = 1) => new Color(brightness, brightness, 0, alpha);

        /// <summary>
        /// Returns cyan.
        /// </summary>
        /// <param name="brightness">The brightness of the returned color.</param>
        /// <param name="alpha">The alpha of the returned color.</param>
        /// <returns>An appropriate <see cref="Color"/> instance.</returns>
        public static Color Cyan(float brightness = 1, float alpha = 1) => new Color(0, brightness, brightness, alpha);

        /// <summary>
        /// Returns magenta.
        /// </summary>
        /// <param name="brightness">The brightness of the returned color.</param>
        /// <param name="alpha">The alpha of the returned color.</param>
        /// <returns>An appropriate <see cref="Color"/> instance.</returns>
        public static Color Magenta(float brightness = 1, float alpha = 1) => new Color(brightness, 0, brightness, alpha);

        /// <summary>
        /// Returns brown.
        /// </summary>
        /// <param name="brightness">The brightness of the returned color.</param>
        /// <param name="alpha">The alpha of the returned color.</param>
        /// <returns>An appropriate <see cref="Color"/> instance.</returns>
        public static Color Brown(float brightness = 1, float alpha = 1) => new Color(0.65f * brightness, 0.16f * brightness, 0.16f * brightness, alpha);

        /// <summary>
        /// Returns orange.
        /// </summary>
        /// <param name="brightness">The brightness of the returned color.</param>
        /// <param name="alpha">The alpha of the returned color.</param>
        /// <returns>An appropriate <see cref="Color"/> instance.</returns>
        public static Color Orange(float brightness = 1, float alpha = 1) => new Color(brightness, 0.65f * brightness, 0, alpha);
    }
}
