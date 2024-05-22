#Region "Microsoft.VisualBasic::2de67d1f6236fcaff1a385fc6cd62ea2, Microsoft.VisualBasic.Core\src\Extensions\Security\Md5.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 325
    '    Code Lines: 171 (52.62%)
    ' Comment Lines: 116 (35.69%)
    '    - Xml Docs: 85.34%
    ' 
    '   Blank Lines: 38 (11.69%)
    '     File Size: 13.04 KB


    '     Module MD5Hash
    ' 
    '         Function: Fletcher32, GetFileMd5, (+2 Overloads) GetHashCode, (+2 Overloads) GetMd5Hash, GetMd5Hash2
    '                   GetSha1Hash, NewUid, SaltValue, Sha256ByteString, StringToByteArray
    '                   (+2 Overloads) ToLong, VerifyFile, VerifyMd5Hash
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Runtime.CompilerServices
Imports System.Security.Cryptography
Imports System.Text
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Scripting.MetaData

Namespace SecurityString

    ''' <summary>
    ''' Represents the abstract class from which all implementations of 
    ''' the System.Security.Cryptography.MD5 hash algorithm inherit.
    ''' </summary>
    Public Module MD5Hash

        <ExportAPI("Uid")>
        Public Function NewUid() As String
            Dim input As String = Guid.NewGuid.ToString & Now.ToString
            Return GetMd5Hash(input)
        End Function

        ReadOnly hashProvider As New Md5HashProvider

        ''' <summary>
        ''' Calculate md5 hash value for the input string.
        ''' </summary>
        ''' <param name="input"></param>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <ExportAPI("Md5")>
        <Extension>
        Public Function GetMd5Hash(input As String) As String
            SyncLock hashProvider
                Return hashProvider.GetMd5Hash(input)
            End SyncLock
        End Function

        ''' <summary>
        ''' Gets the hashcode of the input string. (<paramref name="input"/> => <see cref="MD5Hash.GetMd5Hash"/> => <see cref="MD5Hash.ToLong(String)"/>)
        ''' </summary>
        ''' <param name="input">任意字符串</param>
        ''' <returns></returns>
        Public Function GetHashCode(input As String) As Long
            Dim md5 As String = MD5Hash.GetMd5Hash(input)
            Return ToLong(md5)
        End Function

        ''' <summary>
        ''' Gets the hashcode of the input string.
        ''' </summary>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function GetHashCode(data As IEnumerable(Of Byte)) As Long
            Return GetMd5Hash(data.ToArray).ToLong
        End Function

        ''' <summary>
        ''' Gets the hashcode of the md5 string.
        ''' </summary>
        ''' <param name="md5">计算所得到的MD5哈希值</param>
        ''' <returns></returns>
        <Extension> Public Function ToLong(md5 As String) As Long
            Dim bytes = StringToByteArray(md5)
            Return ToLong(bytes)
        End Function

        ''' <summary>
        ''' CityHash algorithm for convert the md5 hash value as a <see cref="Int64"/> value.
        ''' </summary>
        ''' <param name="bytes">
        ''' this input value should compute from <see cref="Md5HashProvider.GetMd5Bytes(Byte())"/>
        ''' </param>
        ''' <returns></returns>
        ''' <remarks>
        ''' http://stackoverflow.com/questions/9661227/convert-md5-to-long
        ''' 
        ''' The very best solution I found (based on my needs... mix of speed and good hash function) is Google's CityHash. 
        ''' The input can be any byte array including an MD5 result and the output is an unsigned 64-bit long.
        '''
        ''' CityHash has a very good but Not perfect hash distribution, And Is very fast.
        '''
        ''' I ported CityHash from C++ To C# In half an hour. A Java port should be straightforward too.
        '''
        ''' Just XORing the bits doesn't give as good a distribution (though admittedly that will be very fast).
        '''
        ''' I'm not familiar enough with Java to tell you exactly how to populate a long from a byte array 
        ''' (there could be a good helper I'm not familiar with, or I could get some details of arithmetic 
        ''' in Java wrong). 
        ''' Essentially, though, you'll want to do something like this:
        '''
        ''' ```
        ''' Long a = md5[0] * 256 * md5[1] + 256 * 256 * md5[2] + 256 * 256 * 256 * md5[3];
        ''' Long b = md5[4] * 256 * md5[5] + 256 * 256 * md5[6] + 256 * 256 * 256 * md5[7];
        ''' Long result = a ^ b;
        ''' ```
        ''' 
        ''' Note I have made no attempt To deal With endianness. If you just care about a consistent hash value, 
        ''' though, endianness should Not matter.
        ''' </remarks>
        <ExportAPI("As.Long")>
        <Extension>
        Public Function ToLong(bytes As Byte()) As Long
            Dim md5 As Long()

            If bytes.Length = 32 Then
                md5 = (From chunk As Byte()
                       In bytes.Split(4)
                       Let i32 As Integer = BitConverter.ToInt32(chunk, Scan0)
                       Select CLng(i32)).ToArray
            Else
                md5 = (From chunk As Byte()
                       In bytes.Split(2)
                       Let i16 As Short = BitConverter.ToInt16(chunk, Scan0)
                       Select CLng(i16)).ToArray
            End If

            Dim a As Long = md5(0) * 256 * md5(1) + 256 * 256 * md5(2) + 256 * 256 * 256 * md5(3)
            Dim b As Long = md5(4) * 256 * md5(5) + 256 * 256 * md5(6) + 256 * 256 * 256 * md5(7)
            Dim result As Long = a Xor b

            Return result
        End Function

        ''' <summary>
        ''' 由于md5是大小写无关的，故而在这里都会自动的被转换为小写形式，所以调用这个函数的时候不需要在额外的转换了
        ''' </summary>
        ''' <param name="hex"></param>
        ''' <returns></returns>
        <ExportAPI("As.Bytes")>
        Public Function StringToByteArray(hex As String) As Byte()
            Dim NumberChars As Integer = hex.Length
            Dim bytes As Byte() = New Byte(NumberChars / 2 - 1) {}

            hex = hex.ToLower  ' MD5是大小写无关的

            For i As Integer = 0 To NumberChars - 2 Step 2
                bytes(i / 2) = Convert.ToByte(hex.Substring(i, 2), 16)
            Next
            Return bytes
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <ExportAPI("Md5")>
        <Extension>
        Public Function GetMd5Hash(input As Byte()) As String
            Return New Md5HashProvider().GetMd5Hash(input)
        End Function

        <Extension>
        Public Function GetMd5Hash2(Of T)(x As T) As String
            Dim raw As String = x.ToString & x.GetHashCode
            Return raw.GetMd5Hash
        End Function

        ''' <summary>
        ''' Verify a hash against a string. 
        ''' </summary>
        ''' <param name="input"></param>
        ''' <param name="comparedHash"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ''' 
        Public Function VerifyMd5Hash(input As String, comparedHash As String) As Boolean
            If String.IsNullOrEmpty(input) OrElse String.IsNullOrEmpty(comparedHash) Then
                Return False
            End If

            Dim hashOfInput As String = GetMd5Hash(input)  ' Hash the input. 
            Return String.Equals(hashOfInput, comparedHash, StringComparison.OrdinalIgnoreCase)
        End Function 'VerifyMd5Hash

        ''' <summary>
        ''' 校验两个文件的哈希值是否一致
        ''' </summary>
        ''' <param name="query"></param>
        ''' <param name="subject"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ''' 
        <ExportAPI("File.Equals")>
        Public Function VerifyFile(query As String, subject As String) As Boolean
            Dim md5 As New Md5HashProvider()
            Dim a1 As Byte() = IO.File.ReadAllBytes(query)
            Dim a2 As Byte() = IO.File.ReadAllBytes(subject)
            Dim m1 As String = md5.GetMd5Hash(a1)
            Dim m2 As String = md5.GetMd5Hash(a2)

            Return String.Equals(m1, m2)
        End Function

        ''' <summary>
        ''' Get the md5 hash calculation value for a specific file.(获取文件对象的哈希值，请注意，当文件不存在或者文件的长度为零的时候，会返回空字符串)
        ''' </summary>
        ''' <param name="PathUri">The file path of the target file to be calculated.</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ''' 
        <Extension>
        Public Function GetFileMd5(<Parameter("Path.Uri", "The file path of the target file to be calculated.")> PathUri As String) As String
            Dim size& = PathUri.FileLength

            If size <= 0 Then
                Return ""
            ElseIf size < 1024 * 1024 * 5 Then
                ' small files
                Dim bufs As Byte() = File.ReadAllBytes(PathUri)

                Return GetMd5Hash(bufs)
            Else
                ' large files
                Using stream As New FileStream(PathUri, FileMode.Open, FileAccess.Read, FileShare.Read, 1024 * 1024 * 2)
                    Dim sha As New SHA256Managed()
                    Dim checksum = sha.ComputeHash(stream)

                    Return BitConverter _
                        .ToString(checksum) _
                        .Replace("-", String.Empty) _
                        .ToLower _
                        .Substring(0, 32)
                End Using
            End If
        End Function

        ''' <summary>
        ''' SHA256 8 bits salt value for the private key.
        ''' </summary>
        ''' <param name="value"></param>
        ''' <returns></returns>
        Public Function SaltValue(value As String) As String
            Dim hash As String = GetMd5Hash(value)
            Dim chars() = {
                hash(0),
                hash(1),
                hash(3),
                hash(5),
                hash(15),
                hash(23),
                hash(28),
                hash(31)
            }
            Return chars.CharString
        End Function

        ''' <summary>
        ''' sha256 computed byte array in a readable format.
        ''' </summary>
        ''' <param name="array"></param>
        ''' <returns></returns>
        ''' 
        <Extension>
        Public Function Sha256ByteString(array() As Byte, Optional delimiter As Char = " "c) As String
            Dim sb As New StringBuilder

            For i As Integer = 0 To array.Length - 1
                sb.Append($"{array(i):X2}")

                If i Mod 4 = 3 Then
                    sb.Append(delimiter)
                End If
            Next

            Return sb.ToString.Trim(delimiter)
        End Function

        ''' <summary>
        ''' Calculate the Fletcher32 checksum.
        ''' </summary>
        ''' <param name="bytes">the bytes data for verify</param>
        ''' <param name="offset">initial offset</param>
        ''' <param name="length">the message length (if odd, 0 is appended)</param>
        ''' <returns></returns>
        ''' <remarks>
        ''' https://github.com/AKafakA/CSE5234_Group1/blob/ae055d2bb45be9ccf30bbf2bf9dfac4e6e982b1c/h2/src/main/org/h2/mvstore/DataUtils.java#L798
        ''' </remarks>
        <Extension>
        Public Function Fletcher32(bytes As Byte(), offset%, length%) As Integer
            Dim s1 As Integer = &HFFFF
            Dim s2 As Integer = &HFFFF
            Dim i As i32 = offset
            Dim len As Integer = offset + (length And (Not 1))

            Do While i < len
                ' reduce after 360 words (each word is two bytes)
                Dim [end] As Integer = System.Math.Min(CInt(i) + 720, len)

                Do While i < [end]
                    'ORIGINAL LINE: int x = ((bytes[i++] & &Hff) << 8) | (bytes[i++] & &Hff);
                    Dim x As Integer = ((bytes(++i) And &HFF) << 8) Or (bytes(++i) And &HFF)
                    s1 += x
                    s2 += s1
                Loop

                s1 = (s1 And &HFFFF) + (CInt(CUInt(s1) >> 16))
                s2 = (s2 And &HFFFF) + (CInt(CUInt(s2) >> 16))
            Loop

            If (length And 1) <> 0 Then
                ' odd length: append 0
                Dim x As Integer = (bytes(i) And &HFF) << 8
                s1 += x
                s2 += s1
            End If

            s1 = (s1 And &HFFFF) + (CInt(CUInt(s1) >> 16))
            s2 = (s2 And &HFFFF) + (CInt(CUInt(s2) >> 16))

            Return (s2 << 16) Or s1
        End Function

        ''' <summary>
        ''' # Generate SHA1 checksum of a file
        ''' </summary>
        ''' <param name="filePath"></param>
        ''' <returns></returns>
        Public Function GetSha1Hash(filePath As String) As String
            Using fs As FileStream = File.OpenRead(filePath)
                Dim sha As SHA1 = New SHA1Managed()
                Return BitConverter.ToString(sha.ComputeHash(fs)).Replace("-", "").ToLower
            End Using
        End Function
    End Module
End Namespace
