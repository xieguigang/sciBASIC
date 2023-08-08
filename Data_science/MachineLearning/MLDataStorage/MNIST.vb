Imports System.Drawing
Imports System.Drawing.Imaging
Imports System.IO
Imports System.Runtime.InteropServices
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports std = System.Math

Public Module MNIST

    Public Iterator Function ExtractImages(imagesFile As String, labelsFile As String) As IEnumerable(Of NamedValue(Of Image))
        Dim imageReader As BinaryReader = New BinaryReader(New FileStream(imagesFile, FileMode.Open))
        Dim labelReader As BinaryReader = New BinaryReader(New FileStream(labelsFile, FileMode.Open))

        If ReadInt(imageReader) <> 2051 OrElse ReadInt(labelReader) <> 2049 Then
            Throw New Exception("Invalid magic number.")
        End If

        Dim count1 = ReadInt(imageReader)
        Dim count2 = ReadInt(labelReader)

        If count1 <> count2 Then
            Throw New Exception("Images and Labels count mismatched.")
        End If

        Dim count As Integer = count1
        Dim rows = ReadInt(imageReader)
        Dim columns = ReadInt(imageReader)

        For i As Integer = 0 To count - 1
            Dim label As Byte = labelReader.ReadByte()
            Dim image As Bitmap = New Bitmap(columns, rows)
            Dim rect As Rectangle = New Rectangle(0, 0, image.Width, image.Height)
            Dim data = image.LockBits(rect, ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb)
            Dim ptr = data.Scan0
            Dim bytes = std.Abs(data.Stride) * image.Height
            Dim rgbValues = New Byte(bytes - 1) {}
            Marshal.Copy(ptr, rgbValues, 0, bytes)
            For j = 0 To rows * columns - 1
                rgbValues(j * 3 + 2) = imageReader.ReadByte()
                rgbValues(j * 3 + 1) = rgbValues(j * 3 + 2)
                rgbValues(j * 3) = rgbValues(j * 3 + 1)
            Next
            Marshal.Copy(rgbValues, 0, ptr, bytes)
            image.UnlockBits(data)

            Yield New NamedValue(Of Image) With {
                .Name = $"{label}-{i}",
                .Description = label,
                .Value = image
            }
        Next
    End Function

    Private Function ReadInt(reader As BinaryReader) As Integer
        Dim value As Byte() = reader.ReadBytes(4)
        If BitConverter.IsLittleEndian Then
            Call Array.Reverse(value)
        End If
        Return BitConverter.ToInt32(value, 0)
    End Function
End Module
