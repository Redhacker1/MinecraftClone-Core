using System;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using Silk.NET.Assimp;

namespace Engine.AssetLoading.AssimpIntegration;

public struct ManagedMaterialProperty
{
    internal byte[] data;
    public string Key;
    public readonly TextureType Semantic;
    public readonly PropertyTypeInfo type;
    
    public unsafe ManagedMaterialProperty(byte* data, uint count, string Key, TextureType Semantic, PropertyTypeInfo typeInfo )
    {
        this.data = new Span<byte>(data, (int)count).ToArray();
        this.Key = Key;
        this.Semantic = Semantic;
        type = typeInfo;
    }
	
    public unsafe ManagedMaterialProperty(MaterialProperty property)
    {
        data = new Span<byte>(property.MData, (int)property.MDataLength).ToArray();
        Key = property.MKey;
        Semantic = (TextureType)property.MSemantic;
        type = property.MType;
    }
    

    /// <summary>
    ///  Gets the value of the materialProperty as a string if successful, if not, String.Empty
    /// </summary>
    /// <returns>Whether the it was indeed possible to read the data as a string, and that the type specified was indeed a string</returns>
    public bool TryGetPropertyString(out string result)
    {
        // Ensure the data is valid and is defined as a string
        if (data.Length > 5 && type == PropertyTypeInfo.PtiString)
        {
            // snip the data to only what we need with as few allocations as possible
            Span<byte> Stringbytes = new Span<byte>(data, 4, data.Length - 5);
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
    /// convert the type into the struct expected. This is the SUPPORTED way to get arbitrary data out.
    /// </summary>
    public bool TryGetPropertyAsBytes(out ReadOnlySpan<byte> result)
    {
        Span<byte> bytes = new Span<byte>(data, 0, data.Length);
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
        if (type == PropertyTypeInfo.PtiBuffer || type == PropertyTypeInfo.PtiDouble)
        {
            result = MemoryMarshal.Cast<byte, double>(data.AsSpan());
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
        if (type == PropertyTypeInfo.PtiBuffer || type == PropertyTypeInfo.PtiFloat)
        {
            result = MemoryMarshal.Cast<byte, float>(data.AsSpan());
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
        if (type == PropertyTypeInfo.PtiBuffer || type == PropertyTypeInfo.PtiInteger)
        {
            result = MemoryMarshal.Cast<byte, int>(data.AsSpan());
            
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
            result = result.Slice(0, Math.Min(MaxElements, result.Length));   
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
        result = MemoryMarshal.Cast<byte, T>(data.AsSpan());
    }
    
    
    /// <summary>
    ///  The usage of this method is not supported, it is implemented for completeness in case a type that was implemented later in ASSIMP is not covered by this API,
    ///  It has NO validation checks and will force the data to be casted to this type, regardless of whether the data is even valid. Use at your own risk!
    /// </summary>
    /// <param name="result">the result of the property after the force bitwise cast</param>
    /// <param name="MaxElements">the maximum amount of elements, </param>
    /// <typeparam name="T"></typeparam>
    public void DangerousGetPropertyAs<T>(out ReadOnlySpan<T> result, int MaxElements) where T : unmanaged
    {
        DangerousGetPropertyAs<T>(out result);
        if (MaxElements > 0)
        {
            result = result.Slice(0, Math.Min(MaxElements, result.Length));
        }
    }
}