using System;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using Silk.NET.Assimp;

namespace Engine.AssetLoading.AssimpIntegration;

public readonly struct ManagedMaterialProperty
{
    internal readonly byte[] Data;
    public readonly string Key;
    public readonly TextureType Semantic;
    readonly PropertyTypeInfo _type;
    
    public unsafe ManagedMaterialProperty(byte* data, uint count, string key, TextureType semantic, PropertyTypeInfo typeInfo )
    {
        Data = new Span<byte>(data, (int)count).ToArray();
        Key = key;
        Semantic = semantic;
        _type = typeInfo;
    }
    public unsafe ManagedMaterialProperty(Span<byte> data, string key, TextureType semantic, PropertyTypeInfo typeInfo )
    {
        Data = data.ToArray();
        Key = key ?? throw new ArgumentNullException(nameof(key));
        Semantic = semantic;
        _type = typeInfo;
    }
	
    public unsafe ManagedMaterialProperty(MaterialProperty property)
    {
        Data = new Span<byte>(property.MData, (int)property.MDataLength).ToArray();
        Key = property.MKey;
        Semantic = (TextureType)property.MSemantic;
        _type = property.MType;
    }
    

    /// <summary>
    ///  Gets the value of the materialProperty as a string if successful, if not, String.Empty
    /// </summary>
    /// <returns>Whether it was indeed possible to read the data as a string, and that the type specified was indeed a string</returns>
    public bool TryGetPropertyString(out string result)
    {
        // Ensure the data is valid and is defined as a string
        if (Data.Length > 5 && _type == PropertyTypeInfo.String)
        {
            // snip the data to only what we need with as few allocations as possible
            Span<byte> Stringbytes = new Span<byte>(Data, 4, Data.Length - 5);
            // set the output variable as defined below.
            result = Encoding.UTF8.GetString(Stringbytes);
            return true;
        }
        // the string is either invalid or the data contained is not a string!
        else
        {
            result = string.Empty;
            return false;
        }
    }

    /// <summary>
    /// Gets a span of the bytes backing the property. Useful for hacking in support for types that are not covered
    /// by ASSIMP's system, or alternatively, ARE supported, however the wrapper does not currently support.
    /// Assuming you can verify the type yourself via name or whatever else have you,
    /// this should be enough to get you the backing data to then use <see cref="MemoryMarshal"/>'s Cast method to
    /// convert the type into the struct/datatype expected.
    /// </summary>
    public bool TryGetPropertyAsBytes(out ReadOnlySpan<byte> result)
    {
        Span<byte> bytes = new Span<byte>(Data);
        result = bytes;
        return true;
    }
    
    public bool TryGetPropertyAsBytes(out ReadOnlySpan<byte> result, int MaxElements)
    {
        bool success = TryGetPropertyAsBytes(out result);
        if (success)
        {
            result = result.Slice(0, Math.Min(MaxElements, result.Length));   
        }
        return success;
    }
    
    
    public bool TryGetPropertyAsDoubles(out ReadOnlySpan<double> result)
    {
        if (_type == PropertyTypeInfo.Buffer || _type == PropertyTypeInfo.Double)
        {
            result = MemoryMarshal.Cast<byte, double>(Data.AsSpan());
            return true;
        }
        result = ReadOnlySpan<double>.Empty;
        return false;
    }

    public bool TryGetPropertyAsDoubles(out ReadOnlySpan<double> result, int MaxElements)
    {
        bool success = TryGetPropertyAsDoubles(out result);
        if (success)
        {
            result = result.Slice(0, Math.Min(MaxElements, result.Length));   
        }
        return success;
    }
    
    public bool TryGetPropertyAsFloats(out ReadOnlySpan<float> result)
    {
        if (_type == PropertyTypeInfo.Buffer || _type == PropertyTypeInfo.Float)
        {
            result = MemoryMarshal.Cast<byte, float>(Data.AsSpan());
            return true;
        }

        result = ReadOnlySpan<float>.Empty;
        return false;
    }

    public bool TryGetPropertyAsFloats(out ReadOnlySpan<float> result, int MaxElements)
    {
        bool success = TryGetPropertyAsFloats(out result);
        if (success)
        {
            result = result.Slice(0, Math.Min(MaxElements, result.Length));   
        }
        return success;
    }
    
    
    public bool TryGetPropertyAsInts(out ReadOnlySpan<int> result)
    {
        if (_type == PropertyTypeInfo.Buffer || _type == PropertyTypeInfo.Integer)
        {
            result = MemoryMarshal.Cast<byte, int>(Data.AsSpan());
            
            return true;
        }
        result = ReadOnlySpan<int>.Empty;
        return false;
    }
    
    public bool TryGetPropertyAsInts(out ReadOnlySpan<int> result, int MaxElements)
    {
        bool success = TryGetPropertyAsInts(out result);
        if (success)
        {
            result = result[..Math.Min(MaxElements, result.Length)];   
        }
        return success;
    }
    
    public bool TryGetPropertyAsColor(out Vector4 result)
    {
        ReadOnlySpan<float> Elements;
        bool success = TryGetPropertyAsFloats(out Elements, 4);
        if (success && Elements.Length >= 3)
        {
            result = new Vector4(Elements[0], Elements[1], Elements[2], Elements.Length >= 4 ? Elements[3] : 1);
            return true;
        }
        result = Vector4.Zero;
        return false;
    }
    
    public bool TryGetPropertyAsVec4(out Vector4 result)
    {
        ReadOnlySpan<float> Elements;
        bool success = TryGetPropertyAsFloats(out Elements, 4);
        if (success && Elements.Length >= 3)
        {
            result = new Vector4(Elements[0], Elements[1], Elements[2], Elements.Length >= 4 ? Elements[3] : 0);
            return true;
        }
        result = Vector4.Zero;
        return false;
    }
    
    
    
    /// <summary>
    ///  The usage of this method is not supported, it is implemented for completeness in case a type that was implemented later in ASSIMP is not covered by this API,
    ///  It has NO validation checks and will force the data to be casted to this type, regardless of whether the data is even valid. Use at your own risk!
    /// </summary>
    /// <param name="result">the result of the property after the force bitwise cast</param>
    /// <typeparam name="T"></typeparam>
    public void DangerousGetPropertyAs<T>(out ReadOnlySpan<T> result) where T : unmanaged
    {
        result = MemoryMarshal.Cast<byte, T>(Data.AsSpan());
    }
    
    
    /// <summary>
    ///  The usage of this method is not supported, it is implemented for completeness in case a type that was implemented later in ASSIMP is not covered by this API,
    ///  It has NO validation checks and will force the data to be casted to this type, regardless of whether the data is even valid. Use at your own risk!
    /// </summary>
    /// <param name="result">the result of the property after the force bitwise cast</param>
    /// <param name="MaxElements">the maximum amount of elements</param>
    /// <typeparam name="T"></typeparam>
    public void DangerousGetPropertyAs<T>(out ReadOnlySpan<T> result, int MaxElements) where T : unmanaged
    {
        DangerousGetPropertyAs(out result);
        if (MaxElements > 0)
        {
            result = result.Slice(0, Math.Min(MaxElements, result.Length));
        }
    }
}