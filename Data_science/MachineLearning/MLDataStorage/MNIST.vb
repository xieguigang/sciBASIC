#Region "Microsoft.VisualBasic::a56e43876fe554ebde1e9e246f6e82e2, Data_science\MachineLearning\MLDataStorage\MNIST.vb"

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

    '   Total Lines: 170
    '    Code Lines: 113 (66.47%)
    ' Comment Lines: 26 (15.29%)
    '    - Xml Docs: 57.69%
    ' 
    '   Blank Lines: 31 (18.24%)
    '     File Size: 5.86 KB


    ' Class MNIST
    ' 
    '     Properties: ImageSize
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    '     Function: ConvertImage, ExtractImage, ExtractImages, ExtractRaw, ExtractVectors
    '               GetImageSize, ReadInt
    ' 
    '     Sub: (+2 Overloads) Dispose
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Drawing.Imaging
Imports System.IO
Imports System.Runtime.CompilerServices
Imports System.Runtime.InteropServices
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports std = System.Math

#If NET48 Then
Imports Pen = System.Drawing.Pen
Imports Pens = System.Drawing.Pens
Imports Brush = System.Drawing.Brush
Imports Font = System.Drawing.Font
Imports Brushes = System.Drawing.Brushes
Imports SolidBrush = System.Drawing.SolidBrush
Imports DashStyle = System.Drawing.Drawing2D.DashStyle
Imports Image = System.Drawing.Image
Imports Bitmap = System.Drawing.Bitmap
Imports GraphicsPath = System.Drawing.Drawing2D.GraphicsPath
Imports FontStyle = System.Drawing.FontStyle
#Else
Imports Pen = Microsoft.VisualBasic.Imaging.Pen
Imports Pens = Microsoft.VisualBasic.Imaging.Pens
Imports Brush = Microsoft.VisualBasic.Imaging.Brush
Imports Font = Microsoft.VisualBasic.Imaging.Font
Imports Brushes = Microsoft.VisualBasic.Imaging.Brushes
Imports SolidBrush = Microsoft.VisualBasic.Imaging.SolidBrush
Imports DashStyle = Microsoft.VisualBasic.Imaging.DashStyle
Imports Image = Microsoft.VisualBasic.Imaging.Image
Imports Bitmap = Microsoft.VisualBasic.Imaging.Bitmap
Imports GraphicsPath = Microsoft.VisualBasic.Imaging.GraphicsPath
Imports FontStyle = Microsoft.VisualBasic.Imaging.FontStyle
#End If

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
        imageReader = New BinaryReader(imagesFile.Open(FileMode.Open, doClear:=False, [readOnly]:=True))
        labelReader = New BinaryReader(labelsFile.Open(FileMode.Open, doClear:=False, [readOnly]:=True))

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
        Dim imageReader = New BinaryReader(imagesfile.Open(FileMode.Open, doClear:=False, [readOnly]:=True))

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

    ''' <summary>
    ''' Extract all vector data from the given dataset file
    ''' </summary>
    ''' <returns>
    ''' name - unique reference id
    ''' value - image data vector
    ''' description - class label
    ''' </returns>
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

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <returns>
    ''' name - unique reference id
    ''' value - image data vector
    ''' description - class label
    ''' </returns>
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
