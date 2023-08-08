Imports System.Drawing
Imports System.Drawing.Imaging
Imports System.IO
Imports System.Runtime.CompilerServices
Imports System.Runtime.InteropServices
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports std = System.Math

Public Class MNIST : Implements IDisposable

    Private disposedValue As Boolean

    Dim imageReader As BinaryReader
    Dim labelReader As BinaryReader
    Dim count As Integer
    Dim rows, columns As Integer
    Dim rect As Rectangle

    Public ReadOnly Property ImageSize As Size
        Get
            Return New Size(width:=columns, height:=rows)
        End Get
    End Property

    Sub New(imagesFile As String, labelsFile As String)
        imageReader = New BinaryReader(New FileStream(imagesFile, FileMode.Open))
        labelReader = New BinaryReader(New FileStream(labelsFile, FileMode.Open))

        If ReadInt(imageReader) <> 2051 OrElse ReadInt(labelReader) <> 2049 Then
            Throw New Exception("Invalid magic number.")
        End If

        Dim count1 = ReadInt(imageReader)
        Dim count2 = ReadInt(labelReader)

        If count1 <> count2 Then
            Throw New Exception("Images and Labels count mismatched.")
        Else
            count = count1
        End If

        rows = ReadInt(imageReader)
        columns = ReadInt(imageReader)
        rect = New Rectangle(0, 0, columns, rows)
    End Sub

    Public Shared Function GetImageSize(imagesfile As String) As Size
        Dim imageReader = New BinaryReader(New FileStream(imagesfile, FileMode.Open))

        If ReadInt(imageReader) <> 2051 Then
            Throw New InvalidDataException("Invalid magic number.")
        End If

        Dim count = ReadInt(imageReader)
        Dim rows = ReadInt(imageReader)
        Dim columns = ReadInt(imageReader)

        Call imageReader.Dispose()

        Return New Size(width:=columns, height:=rows)
    End Function

    Public Iterator Function ExtractImages() As IEnumerable(Of NamedValue(Of Image))
        For i As Integer = 0 To count - 1
            Yield ExtractImage()
        Next
    End Function

    Public Iterator Function ExtractVectors() As IEnumerable(Of NamedCollection(Of Byte))
        For i As Integer = 0 To count - 1
            Yield ExtractRaw()
        Next
    End Function

    Public Function ConvertImage(raw As NamedCollection(Of Byte)) As NamedValue(Of Image)
        Dim image As Bitmap = New Bitmap(columns, rows)
        Dim data = image.LockBits(rect, ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb)
        Dim ptr As IntPtr = data.Scan0
        Dim bytes = std.Abs(data.Stride) * image.Height
        Dim rgbValues = New Byte(bytes - 1) {}
        Dim bit As Byte

        Marshal.Copy(ptr, rgbValues, 0, bytes)

        For j As Integer = 0 To rows * columns - 1
            bit = raw(j)
            rgbValues(j * 3) = bit
            rgbValues(j * 3 + 1) = bit
            rgbValues(j * 3 + 2) = bit
        Next

        Marshal.Copy(rgbValues, 0, ptr, bytes)

        image.UnlockBits(data)

        Return New NamedValue(Of Image) With {
            .Name = raw.Last,
            .Description = raw.description,
            .Value = image
        }
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Private Function ExtractImage() As NamedValue(Of Image)
        Return ConvertImage(raw:=ExtractRaw())
    End Function

    Private Function ExtractRaw() As NamedCollection(Of Byte)
        Dim label As Byte = labelReader.ReadByte()
        Dim rgbValues = New Byte(rows * columns - 1) {}
        Dim labelStr As String = $"{label}-{labelReader.BaseStream.Position}"

        For j As Integer = 0 To rows * columns - 1
            rgbValues(j) = imageReader.ReadByte()
        Next

        Return New NamedCollection(Of Byte)(labelStr, rgbValues, description:=label.ToString)
    End Function

    Private Shared Function ReadInt(reader As BinaryReader) As Integer
        Dim value As Byte() = reader.ReadBytes(4)
        If BitConverter.IsLittleEndian Then
            Call Array.Reverse(value)
        End If
        Return BitConverter.ToInt32(value, 0)
    End Function

    Protected Overridable Sub Dispose(disposing As Boolean)
        If Not disposedValue Then
            If disposing Then
                ' TODO: 释放托管状态(托管对象)
                imageReader.Dispose()
                labelReader.Dispose()
            End If

            ' TODO: 释放未托管的资源(未托管的对象)并重写终结器
            ' TODO: 将大型字段设置为 null
            disposedValue = True
        End If
    End Sub

    ' ' TODO: 仅当“Dispose(disposing As Boolean)”拥有用于释放未托管资源的代码时才替代终结器
    ' Protected Overrides Sub Finalize()
    '     ' 不要更改此代码。请将清理代码放入“Dispose(disposing As Boolean)”方法中
    '     Dispose(disposing:=False)
    '     MyBase.Finalize()
    ' End Sub

    Public Sub Dispose() Implements IDisposable.Dispose
        ' 不要更改此代码。请将清理代码放入“Dispose(disposing As Boolean)”方法中
        Dispose(disposing:=True)
        GC.SuppressFinalize(Me)
    End Sub
End Class
