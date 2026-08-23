using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Aurora.Pooling;
using UnityEngine;

namespace Aurora.Unity
{
    [StructLayout(LayoutKind.Explicit)]
    public struct AuroraColor : IEquatable<AuroraColor>
    {
        [FieldOffset(0)]
        private int _rgba;

        /// <summary>
        /// Red.
        /// </summary>
        [FieldOffset(0)]
        public byte R;

        /// <summary>
        /// Green.
        /// </summary>
        [FieldOffset(1)]
        public byte G;

        /// <summary>
        /// Blue.
        /// </summary>
        [FieldOffset(2)]
        public byte B;

        /// <summary>
        /// Alpha.
        /// </summary>
        [FieldOffset(3)]
        public byte A;

        /// <summary>
        /// Gets or sets a color component by index:
        /// <list type="table">
        /// <listheader><term>Index</term><description>Color component</description></listheader>
        /// <item><term>0</term><description><see cref="R"/></description></item>
        /// <item><term>1</term><description><see cref="G"/></description></item>
        /// <item><term>2</term><description><see cref="B"/></description></item>
        /// <item><term>3</term><description><see cref="A"/></description></item>
        /// <item><term>Other</term><description>Throws an <see cref="IndexOutOfRangeException"/></description></item>
        /// </list>
        /// </summary>
        /// <param name="index">The index.</param>
        /// <exception cref="IndexOutOfRangeException">The index is out of range.</exception>
        public byte this[int index]
        {
            get
            {
                return index switch
                {
                    0 => R,
                    1 => G,
                    2 => B,
                    3 => A,
                    _ => throw new IndexOutOfRangeException()
                };
            }
            set
            {
                switch (index)
                {
                    case 0:
                        R = value;
                        break;
                    case 1:
                        G = value;
                        break;
                    case 2:
                        B = value;
                        break;
                    case 3:
                        A = value;
                        break;
                    default:
                        throw new IndexOutOfRangeException();
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AuroraColor"/> struct.
        /// </summary>
        /// <param name="r">Red.</param>
        /// <param name="g">Green.</param>
        /// <param name="b">Blue.</param>
        /// <param name="a">Alpha.</param>
        public AuroraColor(byte r, byte g, byte b, byte a = byte.MaxValue)
        {
            _rgba = 0;

            R = r;
            G = g;
            B = b;
            A = a;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AuroraColor"/> struct based on the specified HTML color string.
        /// </summary>
        /// <param name="htmlColor">
        /// The HTML color string.
        /// <br/>
        /// Allowed formats are listed in the table below:
        /// <list type="bullet">
        /// <item><description><c>#RGB</c></description></item>
        /// <item><description><c>#RRGGBB</c></description></item>
        /// <item><description><c>#RGBA</c></description></item>
        /// <item><description><c>#RRGGBBAA</c></description></item>
        /// </list>
        /// The leading <c>#</c> may be omitted.
        /// <br/>
        /// <c>R</c> <c>G</c> <c>B</c> are case-insensitive.
        /// <br/>
        /// Color names such as <c>red</c> are not supported.
        /// </param>
        /// <exception cref="ArgumentNullException"><paramref name="htmlColor"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException"><paramref name="htmlColor"/> is improperly formatted.</exception>
        public AuroraColor(string htmlColor)
        {
            _rgba = 0;

            if (htmlColor == null)
            {
                throw new ArgumentNullException(nameof(htmlColor));
            }
            if (htmlColor.Length == 0)
            {
                throw new ArgumentException(htmlColor, nameof(htmlColor));
            }
            var stringBuilder = PredefinedPools.StringBuilder.Get();
            try
            {
                stringBuilder.Append(htmlColor);
                if (stringBuilder[0] is '#')
                {
                    stringBuilder.Remove(0, 1);
                }
                switch (stringBuilder.Length)
                {
                    case 3:
                    {
                        if (TryParseOne(stringBuilder, 0, out var r) && TryParseOne(stringBuilder, 1, out var g) &&
                            TryParseOne(stringBuilder, 2, out var b))
                        {
                            R = r;
                            G = g;
                            B = b;
                            A = byte.MaxValue;
                        }
                        else
                        {
                            throw new ArgumentException(htmlColor, nameof(htmlColor));
                        }
                        break;
                    }
                    case 4:
                    {
                        if (TryParseOne(stringBuilder, 0, out var r) && TryParseOne(stringBuilder, 1, out var g) &&
                            TryParseOne(stringBuilder, 2, out var b) && TryParseOne(stringBuilder, 3, out var a))
                        {
                            R = r;
                            G = g;
                            B = b;
                            A = a;
                        }
                        else
                        {
                            throw new ArgumentException(htmlColor, nameof(htmlColor));
                        }
                        break;
                    }
                    case 6:
                    {
                        if (TryParseTwo(stringBuilder, 0, out var r) && TryParseTwo(stringBuilder, 2, out var g) &&
                            TryParseTwo(stringBuilder, 4, out var b))
                        {
                            R = r;
                            G = g;
                            B = b;
                            A = byte.MaxValue;
                        }
                        else
                        {
                            throw new ArgumentException(htmlColor, nameof(htmlColor));
                        }
                        break;
                    }
                    case 8:
                    {
                        if (TryParseTwo(stringBuilder, 0, out var r) && TryParseTwo(stringBuilder, 2, out var g) &&
                            TryParseTwo(stringBuilder, 4, out var b) && TryParseTwo(stringBuilder, 6, out var a))
                        {
                            R = r;
                            G = g;
                            B = b;
                            A = a;
                        }
                        else
                        {
                            throw new ArgumentException(htmlColor, nameof(htmlColor));
                        }
                        break;
                    }
                    default:
                        throw new ArgumentException(htmlColor, nameof(htmlColor));
                }
            }
            finally
            {
                PredefinedPools.StringBuilder.Return(stringBuilder);
            }

            static bool TryParseOne(StringBuilder stringBuilder, int index, out byte result)
            {
                if (HexCharParseUtility.TryParse(stringBuilder[index], out var value))
                {
                    result = (byte)(value * 17);
                    return true;
                }
                result = 0;
                return false;
            }

            static bool TryParseTwo(StringBuilder stringBuilder, int index, out byte result)
            {
                if (HexCharParseUtility.TryParse(stringBuilder[index],     out var high) &&
                    HexCharParseUtility.TryParse(stringBuilder[index + 1], out var low))
                {
                    result = (byte)((high << 4) | low);
                    return true;
                }
                result = 0;
                return false;
            }
        }

        public static implicit operator Color32(AuroraColor color)
        {
            return new Color32(color.R, color.G, color.B, color.A);
        }

        public static implicit operator AuroraColor(Color32 color)
        {
            return new AuroraColor(color.r, color.g, color.b, color.a);
        }

        public static implicit operator Color(AuroraColor color)
        {
            return (Color32)color;
        }

        public static implicit operator AuroraColor(Color color)
        {
            return (Color32)color;
        }

        /// <summary>
        /// Compares whether two <see cref="AuroraColor"/> are equal.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns><see langword="true"/> if <paramref name="left"/> equals <paramref name="right"/>; otherwise <see langword="false"/>.</returns>
        public static bool operator ==(AuroraColor left, AuroraColor right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Compares whether two <see cref="AuroraColor"/> are not equal.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns><see langword="true"/> if <paramref name="left"/> does not equal <paramref name="right"/>; otherwise <see langword="false"/>.</returns>
        public static bool operator !=(AuroraColor left, AuroraColor right)
        {
            return !(left == right);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"RGBA({R}, {G}, {B}, {A})";
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return _rgba;
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return obj is AuroraColor other && Equals(other);
        }

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(AuroraColor other)
        {
            return _rgba == other._rgba;
        }
    }
}
