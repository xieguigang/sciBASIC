Option Strict On
Option Explicit On

Imports System.IO
Imports Xunit

''' <summary>XXH32 validation against published vectors and the generated vector file.</summary>
Public Class XxHash32Tests

    Private Shared ReadOnly TestData As String = Path.Combine(AppContext.BaseDirectory, "TestData")

    <Theory>
    <InlineData("", &H2CC5D05)>        ' xxh32("") = 0x02CC5D05
    <InlineData("a", &H550D7456)>      ' xxh32("a") = 0x550D7456
    <InlineData("abc", &H32D153FF)>    ' xxh32("abc") = 0x32D153FF
    Public Sub KnownVectors(input As String, expected As Integer)
        Dim bytes = System.Text.Encoding.ASCII.GetBytes(input)
        Assert.Equal(CUInt(expected), XxHash32.ComputeHash(bytes, 0, bytes.Length))
    End Sub

    <Fact>
    Public Sub VectorFile_MatchesReferenceImplementation()
        ' xxh32vectors.txt was produced by the reference python xxhash package.
        Dim vectorFile = Path.Combine(TestData, "shared", "xxh32vectors.txt")
        Assert.True(File.Exists(vectorFile), "xxh32vectors.txt is missing")
        Dim count = 0
        For Each line In File.ReadAllLines(vectorFile)
            If line.Length = 0 Then Continue For
            Dim parts = line.Split(":"c)
            Dim data = HexToBytes(parts(1))
            Dim expected = UInteger.Parse(parts(0), Globalization.NumberStyles.HexNumber)
            Assert.Equal(expected, XxHash32.ComputeHash(data, 0, data.Length))
            count += 1
        Next
        Assert.True(count >= 5)
    End Sub

    <Fact>
    Public Sub StreamingUpdate_MatchesOneShot()
        Dim data = File.ReadAllBytes(Path.Combine(TestData, "shared", "text500k.bin"))
        Dim expected = XxHash32.ComputeHash(data, 0, data.Length)
        For Each chunkSize In {1, 3, 7, 15, 17, 64 * 1024}
            Dim h = New XxHash32()
            Dim pos = 0
            While pos < data.Length
                Dim n = Math.Min(chunkSize, data.Length - pos)
                h.Update(data, pos, n)
                pos += n
            End While
            Assert.Equal(expected, h.GetValue())
        Next
    End Sub

    Friend Shared Function HexToBytes(hex As String) As Byte()
        Dim result((hex.Length \ 2) - 1) As Byte
        For i = 0 To result.Length - 1
            result(i) = Convert.ToByte(hex.Substring(i * 2, 2), 16)
        Next
        Return result
    End Function

End Class
