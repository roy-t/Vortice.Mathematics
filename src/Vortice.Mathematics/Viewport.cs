// Copyright (c) Amer Koleci and contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Diagnostics;
using static Vortice.Mathematics.MathHelper;

namespace Vortice.Mathematics;

/// <summary>
/// Represents a floating-point viewport struct.
/// </summary>
[Serializable]
[StructLayout(LayoutKind.Sequential, Pack = 4)]
public readonly struct Viewport : IEquatable<Viewport>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Viewport"/> struct.
    /// </summary>
    /// <param name="width">The width of the viewport in pixels.</param>
    /// <param name="height">The height of the viewport in pixels.</param>
    public Viewport(float width, float height)
    {
        X = 0.0f;
        Y = 0.0f;
        Width = width;
        Height = height;
        MinDepth = 0.0f;
        MaxDepth = 1.0f;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Viewport"/> struct.
    /// </summary>
    /// <param name="x">The x coordinate of the upper-left corner of the viewport in pixels.</param>
    /// <param name="y">The y coordinate of the upper-left corner of the viewport in pixels.</param>
    /// <param name="width">The width of the viewport in pixels.</param>
    /// <param name="height">The height of the viewport in pixels.</param>
    public Viewport(float x, float y, float width, float height)
    {
        X = x;
        Y = y;
        Width = width;
        Height = height;
        MinDepth = 0.0f;
        MaxDepth = 1.0f;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Viewport"/> struct.
    /// </summary>
    /// <param name="x">The x coordinate of the upper-left corner of the viewport in pixels.</param>
    /// <param name="y">The y coordinate of the upper-left corner of the viewport in pixels.</param>
    /// <param name="width">The width of the viewport in pixels.</param>
    /// <param name="height">The height of the viewport in pixels.</param>
    /// <param name="minDepth">The minimum depth of the clip volume.</param>
    /// <param name="maxDepth">The maximum depth of the clip volume.</param>
    public Viewport(float x, float y, float width, float height, float minDepth, float maxDepth)
    {
        X = x;
        Y = y;
        Width = width;
        Height = height;
        MinDepth = minDepth;
        MaxDepth = maxDepth;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Viewport"/> struct.
    /// </summary>
    /// <param name="bounds">A <see cref="Rect"/> that defines the location and size of the viewport in a render target.</param>
    public Viewport(Rect bounds)
    {
        X = bounds.X;
        Y = bounds.Y;
        Width = bounds.Width;
        Height = bounds.Height;
        MinDepth = 0.0f;
        MaxDepth = 1.0f;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Viewport"/> struct.
    /// </summary>
    /// <param name="bounds">A <see cref="RectI"/> that defines the location and size of the viewport in a render target.</param>
    public Viewport(RectI bounds)
    {
        X = bounds.X;
        Y = bounds.Y;
        Width = bounds.Width;
        Height = bounds.Height;
        MinDepth = 0.0f;
        MaxDepth = 1.0f;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Viewport"/> struct.
    /// </summary>
    /// <param name="bounds">A <see cref="Vector4"/> that defines the location and size of the viewport in a render target.</param>
    public Viewport(Vector4 bounds)
    {
        X = bounds.X;
        Y = bounds.Y;
        Width = bounds.Z;
        Height = bounds.W;
        MinDepth = 0.0f;
        MaxDepth = 1.0f;
    }

    /// <summary>
    /// Position of the pixel coordinate of the upper-left corner of the viewport.
    /// </summary>
    public float X { get; }

    /// <summary>
    /// Position of the pixel coordinate of the upper-left corner of the viewport.
    /// </summary>
    public float Y { get; }

    /// <summary>
    /// Width dimension of the viewport.
    /// </summary>
    public float Width { get; }

    /// <summary>
    /// Height dimension of the viewport.
    /// </summary>
    public float Height { get; }

    /// <summary>
    /// Gets or sets the minimum depth of the clip volume.
    /// </summary>
    public float MinDepth { get; }

    /// <summary>
    /// Gets or sets the maximum depth of the clip volume.
    /// </summary>
    public float MaxDepth { get; }

    /// <summary>
    /// Gets or sets the bounds of the viewport.
    /// </summary>
    /// <value>The bounds.</value>
    public Rect Bounds => new(X, Y, Width, Height);

    /// <summary>
    /// Gets the aspect ratio used by the viewport.
    /// </summary>
    /// <value>The aspect ratio.</value>
    public float AspectRatio
    {
        get
        {
            if (!MathHelper.IsZero(Height))
            {
                return Width / Height;
            }

            return 0.0f;
        }
    }

    /// <summary>
    /// Projects a 3D vector from object space into screen space.
    /// </summary>
    /// <param name="source">The vector to project.</param>
    /// <param name="projection">The projection matrix.</param>
    /// <param name="view">The view matrix.</param>
    /// <param name="world">The world matrix.</param>
    public Vector3 Project(Vector3 source, Matrix4x4 projection, Matrix4x4 view, Matrix4x4 world)
    {
        Matrix4x4 worldViewProjection = Matrix4x4.Multiply(Matrix4x4.Multiply(world, view), projection);
        return Project(source, worldViewProjection);
    }

    /// <summary>
    /// Projects a 3D vector from object space into screen space.
    /// </summary>
    /// <param name="source">The vector to project.</param>
    /// <param name="worldViewProjection">The World-View-Projection matrix.</param>
    /// <returns>The unprojected vector. </returns>
    public Vector3 Project(Vector3 source, Matrix4x4 worldViewProjection)
    {
        Vector3 vector = Vector3.Transform(source, worldViewProjection);
        float a = (source.X * worldViewProjection.M14) + (source.Y * worldViewProjection.M24) + (source.Z * worldViewProjection.M34) + worldViewProjection.M44;

        if (!MathHelper.IsOne(a))
        {
            vector.X /= a;
            vector.Y /= a;
            vector.Z /= a;
        }

        vector.X = ((vector.X + 1.0f) * 0.5f * Width) + X;
        vector.Y = ((-vector.Y + 1.0f) * 0.5f * Height) + Y;
        vector.Z = (vector.Z * (MaxDepth - MinDepth)) + MinDepth;
        return vector;
    }

    /// <summary>
    /// Converts a screen space point into a corresponding point in world space.
    /// </summary>
    /// <param name="source">The vector to project.</param>
    /// <param name="projection">The projection matrix.</param>
    /// <param name="view">The view matrix.</param>
    /// <param name="world">The world matrix.</param>
    /// <returns>The unprojected vector. </returns>
    public Vector3 Unproject(Vector3 source, Matrix4x4 projection, Matrix4x4 view, Matrix4x4 world)
    {
        Matrix4x4 worldViewProjection = Matrix4x4.Multiply(Matrix4x4.Multiply(world, view), projection);
        return Unproject(source, worldViewProjection);
    }

    /// <summary>
    /// Converts a screen space point into a corresponding point in world space.
    /// </summary>
    /// <param name="source">The vector to project.</param>
    /// <param name="worldViewProjection">The World-View-Projection matrix.</param>
    /// <returns>The unprojected vector. </returns>
    public Vector3 Unproject(Vector3 source, Matrix4x4 worldViewProjection)
    {
        Matrix4x4.Invert(worldViewProjection, out Matrix4x4 matrix);

        source.X = (((source.X - X) / Width) * 2.0f) - 1.0f;
        source.Y = -(((source.Y - Y) / Height * 2.0f) - 1.0f);
        source.Z = (source.Z - MinDepth) / (MaxDepth - MinDepth);

        float a = (source.X * matrix.M14) + (source.Y * matrix.M24) + (source.Z * matrix.M34) + matrix.M44;
        source = Vector3.Transform(source, matrix);

        if (!MathHelper.IsOne(a))
        {
            source /= a;
        }

        return source;
    }

    public static RectI ComputeDisplayArea(ViewportScaling scaling, int backBufferWidth, int backBufferHeight, int outputWidth, int outputHeight)
    {
        switch (scaling)
        {
            case ViewportScaling.Stretch:
                // Output fills the entire window area
                return new RectI(0, 0, outputWidth, outputHeight);

            case ViewportScaling.AspectRatioStretch:
                // Output fills the window area but respects the original aspect ratio, using pillar boxing or letter boxing as required
                // Note: This scaling option is not supported for legacy Win32 windows swap chains
                {
                    Debug.Assert(backBufferHeight > 0);
                    float aspectRatio = (float)backBufferWidth / backBufferHeight;

                    // Horizontal fill
                    float scaledWidth = outputWidth;
                    float scaledHeight = outputWidth / aspectRatio;
                    if (scaledHeight >= outputHeight)
                    {
                        // Do vertical fill
                        scaledWidth = outputHeight * aspectRatio;
                        scaledHeight = outputHeight;
                    }

                    float offsetX = (outputWidth - scaledWidth) * 0.5f;
                    float offsetY = (outputHeight - scaledHeight) * 0.5f;

                    // Clip to display window
                    return new RectI(
                        (int)Max(0, offsetX),
                        (int)Max(0, offsetY),
                        (int)Min(outputWidth, scaledWidth),
                        (int)Min(outputHeight, scaledHeight)
                        );
                }

            case ViewportScaling.None:
            default:
                // Output is displayed in the upper left corner of the window area
                return new RectI(0, 0, Min(backBufferWidth, outputWidth), Min(backBufferHeight, outputHeight));
        }
    }

    public static RectI ComputeTitleSafeArea(int backBufferWidth, int backBufferHeight)
    {
        float safew = (backBufferWidth + 19.0f) / 20.0f;
        float safeh = (backBufferHeight + 19.0f) / 20.0f;

        return RectI.FromLTRB(
            (int)safew,
            (int)safeh,
            (int)(backBufferWidth - safew + 0.5f),
            (int)(backBufferHeight - safeh + 0.5f)
            );
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is Viewport value && Equals(value);

    /// <summary>
    /// Determines whether the specified <see cref="Viewport"/> is equal to this instance.
    /// </summary>
    /// <param name="other">The <see cref="Viewport"/> to compare with this instance.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(Viewport other)
    {
        return CompareEqual(X, other.X, NearZeroEpsilon)
            && CompareEqual(Y, other.Y, NearZeroEpsilon)
            && CompareEqual(Width, other.Width, NearZeroEpsilon)
            && CompareEqual(Height, other.Height, NearZeroEpsilon)
            && CompareEqual(MinDepth, other.MinDepth, NearZeroEpsilon)
            && CompareEqual(MaxDepth, other.MaxDepth, NearZeroEpsilon);
    }

    /// <summary>
    /// Compares two <see cref="Viewport"/> objects for equality.
    /// </summary>
    /// <param name="left">The <see cref="Viewport"/> on the left hand of the operand.</param>
    /// <param name="right">The <see cref="Viewport"/> on the right hand of the operand.</param>
    /// <returns>
    /// True if the current left is equal to the <paramref name="right"/> parameter; otherwise, false.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(Viewport left, Viewport right) => left.Equals(right);

    /// <summary>
    /// Compares two <see cref="Viewport"/> objects for inequality.
    /// </summary>
    /// <param name="left">The <see cref="Viewport"/> on the left hand of the operand.</param>
    /// <param name="right">The <see cref="Viewport"/> on the right hand of the operand.</param>
    /// <returns>
    /// True if the current left is unequal to the <paramref name="right"/> parameter; otherwise, false.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(Viewport left, Viewport right) => !left.Equals(right);

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        var hashCode = new HashCode();
        {
            hashCode.Add(X);
            hashCode.Add(Y);
            hashCode.Add(Width);
            hashCode.Add(Height);
            hashCode.Add(MinDepth);
            hashCode.Add(MaxDepth);
        }
        return hashCode.ToHashCode();
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return $"{nameof(X)}: {X}, {nameof(Y)}: {Y}, {nameof(Width)}: {Width}, {nameof(Height)}: {Height}, {nameof(MinDepth)}: {MinDepth}, {nameof(MaxDepth)}: {MaxDepth}";
    }
}

public enum ViewportScaling
{
    Stretch,
    None,
    AspectRatioStretch
}
