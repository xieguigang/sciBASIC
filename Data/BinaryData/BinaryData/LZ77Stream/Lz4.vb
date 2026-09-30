Option Strict On
Option Explicit On

Imports System.IO

''' <summary>
''' LZ4 block decompression. The "block" format is the raw LZ4 payload used
''' inside LZ4 frames — a token stream of literal runs followed by matches,
''' with no container header of its own.
''' </summary>
Public NotInheritable Class Lz4

    Private Sub New()
    End Sub

    ''' <summary>Minimum match length used by the LZ4 block format.</summary>
    Public Const MinMatch As Integer = 4

    ''' <summary>
    ''' Decompresses a complete LZ4 frame (or a concatenation of frames) read
    ''' from <paramref name="input"/> into a single byte array.
    ''' </summary>
    Public Shared Function Decompress(input As Stream) As Byte()
        If input Is Nothing Then Throw New ArgumentNullException(NameOf(input))
        Using output As New MemoryStream()
            Using decoder As New Lz4Stream(input, leaveOpen:=True)
                decoder.CopyTo(output)
            End Using
            Return output.ToArray()
        End Using
    End Function

    ''' <summary>
    ''' Decompresses one raw LZ4 block whose decompressed size is known in advance.
    ''' </summary>
    ''' <exception cref="InvalidDataException">The block is malformed.</exception>
    Public Shared Function DecodeBlock(compressed As Byte(), decompressedSize As Integer) As Byte()
        If compressed Is Nothing Then Throw New ArgumentNullException(NameOf(compressed))
        If decompressedSize < 0 Then Throw New ArgumentOutOfRangeException(NameOf(decompressedSize))

        Dim dst = New Byte(decompressedSize - 1) {}
        Dim endPos = DecodeBlock(compressed, 0, compressed.Length, dst, 0, 0, decompressedSize)
        If endPos <> decompressedSize Then
            Throw New InvalidDataException($"LZ4 block: decoded {endPos - 0} bytes but {decompressedSize} were expected.")
        End If
        Return dst
    End Function

    ''' <summary>
    ''' Core block decoder. Decodes <paramref name="srcCount"/> bytes starting at
    ''' <paramref name="srcIndex"/> into <paramref name="dst"/> starting at
    ''' <paramref name="dstIndex"/>, never writing beyond <paramref name="dstEnd"/>.
    ''' Matches may reference <paramref name="dictStart"/>..dstIndex (the external
    ''' dictionary window used by LZ4 linked blocks). Returns the end write position.
    ''' </summary>
    Friend Shared Function DecodeBlock(src As Byte(),
                                        srcIndex As Integer, srcCount As Integer,
                                        dst As Byte(), dictStart As Integer,
                                        dstIndex As Integer, dstEnd As Integer) As Integer

        If src Is Nothing Then Throw New ArgumentNullException(NameOf(src))
        If dst Is Nothing Then Throw New ArgumentNullException(NameOf(dst))
        If srcIndex < 0 OrElse srcCount < 0 OrElse srcIndex + srcCount > src.Length Then
            Throw New ArgumentOutOfRangeException("srcIndex/srcCount are outside of src.")
        End If
        If dictStart < 0 OrElse dstIndex < dictStart OrElse dstIndex > dstEnd OrElse dstEnd > dst.Length Then
            Throw New ArgumentOutOfRangeException("dstIndex/dstEnd/dictStart are outside of dst.")
        End If

        ' An empty block decodes to an empty output.
        If srcCount = 0 Then Return dstIndex

        Dim s = srcIndex
        Dim sEnd = srcIndex + srcCount
        Dim d = dstIndex

        Do
            If s >= sEnd Then Throw New InvalidDataException("LZ4 block: truncated sequence token.")
            Dim token = CInt(src(s))
            s += 1

            ' ---------------- literal run ----------------
            Dim litLen = token >> 4
            If litLen = 15 Then
                Dim b As Integer
                Do
                    If s >= sEnd Then Throw New InvalidDataException("LZ4 block: truncated literal length extension.")
                    b = CInt(src(s))
                    s += 1
                    litLen += b
                Loop While b = 255
            End If

            If litLen > 0 Then
                If litLen > sEnd - s Then Throw New InvalidDataException("LZ4 block: literal run runs past the end of the input.")
                If litLen > dstEnd - d Then Throw New InvalidDataException("LZ4 block: literal run exceeds the output limit.")
                Buffer.BlockCopy(src, s, dst, d, litLen)
                s += litLen
                d += litLen
            End If

            ' The last sequence of a block carries literals only — no match follows.
            If s = sEnd Then Exit Do

            ' ---------------- match ----------------
            If sEnd - s < 2 Then Throw New InvalidDataException("LZ4 block: truncated match offset.")
            Dim offset = CInt(src(s)) Or (CInt(src(s + 1)) << 8)      ' little-endian
            s += 2
            If offset = 0 Then Throw New InvalidDataException("LZ4 block: match offset is zero.")

            Dim matchLen = (token And &HF) + MinMatch
            If (token And &HF) = 15 Then
                Dim b As Integer
                Do
                    If s >= sEnd Then Throw New InvalidDataException("LZ4 block: truncated match length extension.")
                    b = CInt(src(s))
                    s += 1
                    matchLen += b
                Loop While b = 255
            End If

            If matchLen > dstEnd - d Then Throw New InvalidDataException("LZ4 block: match exceeds the output limit.")

            Dim mSrc = d - offset
            If mSrc < dictStart Then
                Throw New InvalidDataException("LZ4 block: match offset reaches outside the 64 KB window.")
            End If

            If offset >= matchLen Then
                ' No overlap between source and destination — copy in one go.
                Array.Copy(dst, mSrc, dst, d, matchLen)
                d += matchLen
            Else
                ' Overlapping copy (e.g. offset 1 = run-length): must go byte by byte.
                Do While matchLen > 0
                    dst(d) = dst(mSrc)
                    d += 1
                    mSrc += 1
                    matchLen -= 1
                Loop
            End If

            ' A block may also end right after a match (input fully consumed).
            If s = sEnd Then Exit Do
        Loop

        Return d
    End Function

End Class
