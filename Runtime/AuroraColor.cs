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
        /// 红。
        /// </summary>
        [FieldOffset(0)]
        public byte R;

        /// <summary>
        /// 绿。
        /// </summary>
        [FieldOffset(1)]
        public byte G;

        /// <summary>
        /// 蓝。
        /// </summary>
        [FieldOffset(2)]
        public byte B;

        /// <summary>
        /// 不透明度。
        /// </summary>
        [FieldOffset(3)]
        public byte A;

        /// <summary>
        /// 根据索引获取或设置颜色分量：
        /// <list type="table">
        /// <listheader><term>索引</term><description>颜色分量</description></listheader>
        /// <item><term>0</term><description><see cref="R"/></description></item>
        /// <item><term>1</term><description><see cref="G"/></description></item>
        /// <item><term>2</term><description><see cref="B"/></description></item>
        /// <item><term>3</term><description><see cref="A"/></description></item>
        /// <item><term>其他</term><description>抛出 <see cref="IndexOutOfRangeException"/> 异常</description></item>
        /// </list>
        /// </summary>
        /// <param name="index">索引。</param>
        /// <exception cref="IndexOutOfRangeException">索引超出范围。</exception>
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
        /// 初始化 <see cref="AuroraColor"/> 结构的新实例。
        /// </summary>
        /// <param name="r">红。</param>
        /// <param name="g">绿。</param>
        /// <param name="b">蓝。</param>
        /// <param name="a">不透明度。</param>
        public AuroraColor(byte r, byte g, byte b, byte a = byte.MaxValue)
        {
            _rgba = 0;

            R = r;
            G = g;
            B = b;
            A = a;
        }

        /// <summary>
        /// 根据指定的 HTML 颜色字符串，初始化 <see cref="AuroraColor"/> 结构的新实例。
        /// </summary>
        /// <param name="htmlColor">
        /// HTML 颜色字符串。
        /// <br/>
        /// 允许的格式见下表：
        /// <list type="bullet">
        /// <item><description><c>#RGB</c></description></item>
        /// <item><description><c>#RRGGBB</c></description></item>
        /// <item><description><c>#RGBA</c></description></item>
        /// <item><description><c>#RRGGBBAA</c></description></item>
        /// </list>
        /// 前导 <c>#</c> 可省略。
        /// <br/>
        /// <c>R</c> <c>G</c> <c>B</c> 大小写不敏感。
        /// <br/>
        /// 不支持颜色名称，例如 <c>red</c>。
        /// </param>
        /// <exception cref="ArgumentNullException"><paramref name="htmlColor"/> 为 <see langword="null"/>。</exception>
        /// <exception cref="ArgumentException"><paramref name="htmlColor"/> 格式不正确。</exception>
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
        /// 比较两个 <see cref="AuroraColor"/> 是否相等。
        /// </summary>
        /// <param name="left">左操作数。</param>
        /// <param name="right">右操作数。</param>
        /// <returns>如果 <paramref name="left"/> 与 <paramref name="right"/> 相等，则为 <see langword="true"/>；否则为 <see langword="false"/>。</returns>
        public static bool operator ==(AuroraColor left, AuroraColor right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// 比较两个 <see cref="AuroraColor"/> 是否不相等。
        /// </summary>
        /// <param name="left">左操作数。</param>
        /// <param name="right">右操作数。</param>
        /// <returns>如果 <paramref name="left"/> 与 <paramref name="right"/> 不相等，则为 <see langword="true"/>；否则为 <see langword="false"/>。</returns>
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
