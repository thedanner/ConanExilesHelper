using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ConanExilesHelper.SourceQuery;

public static class BinaryReaderExtensions
{
    public static string ReadNullTerminatedString(this BinaryReader br)
    {
        return ReadNullTerminatedString(br, Encoding.UTF8);
    }

    public static string ReadNullTerminatedString(this BinaryReader br, Encoding encoding)
    {
        if (encoding is null) throw new ArgumentNullException(nameof(encoding));

        var stringBytes = new List<byte>();
        byte charByte;
        while ((charByte = br.ReadByte()) != 0)
        {
            stringBytes.Add(charByte);
        }
        return encoding.GetString(stringBytes.ToArray());
    }
}
