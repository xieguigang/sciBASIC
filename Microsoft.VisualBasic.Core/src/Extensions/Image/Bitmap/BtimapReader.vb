Imports System.Drawing
Imports System.IO

Namespace Imaging.BitmapImage

    ''' <summary>
    ''' a bitmap image data reader for help huge scientific image data processing.
    ''' (more than the 2GB limitation of gdi+ <see cref="Bitmap"/>.)
    ''' </summary>
    Public Class BtimapReader : Implements IDisposable

        ReadOnly s As BinaryReader

        Dim disposedValue As Boolean

        Const B As Integer = Asc("B")
        Const M As Integer = Asc("M")

        Public ReadOnly Property FileSizeInBytes As Integer
        Public ReadOnly Property Reserved As Integer
        Public ReadOnly Property Offset As Integer
        Public ReadOnly Property HeaderSize As Integer
        Public ReadOnly Property ImageWidth As Integer
        Public ReadOnly Property ImageHeight As Integer
        Public ReadOnly Property NumberOfColorPlanes As Integer
        Public ReadOnly Property BitsPerPixel As BitsPerPixel
        Public ReadOnly Property Compression As Integer
        Public ReadOnly Property ImageSizeInBytes As Integer
        Public ReadOnly Property HorizontalPixelsPerMeter As Integer
        Public ReadOnly Property VerticalPixelsPerMeter As Integer
        Public ReadOnly Property NumberOfColorsUsed As Integer
        Public ReadOnly Property NumberOfImportantColors As Integer
        Public ReadOnly Property RowSize As Integer

        Sub New(file As Stream)
            Dim bin As New BinaryReader(file)

            s = bin

            ' check of the magic number 
            If bin.ReadByte <> B OrElse bin.ReadByte <> M Then
                Throw New InvalidDataException("invalid magic number!")
            End If

            FileSizeInBytes = s.ReadInt32
            Reserved = s.ReadInt32
            Offset = s.ReadInt32
            HeaderSize = s.ReadInt32
            ImageWidth = s.ReadInt32
            ImageHeight = s.ReadInt32
            NumberOfColorPlanes = s.ReadInt16
            BitsPerPixel = s.ReadInt16
            Compression = s.ReadInt32
            ImageSizeInBytes = s.ReadInt32
            HorizontalPixelsPerMeter = s.ReadInt32
            VerticalPixelsPerMeter = s.ReadInt32
            NumberOfColorsUsed = s.ReadInt32
            NumberOfImportantColors = s.ReadInt32
            RowSize = RoundUpToNearestFour(ImageWidth * BitsPerPixel / 8.0)
        End Sub

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="filename"></param>
        ''' <remarks>
        ''' the bitmap image data maybe load into memory at once if the file size is less then 2GB.
        ''' </remarks>
        Sub New(filename As String)
            Call Me.New(filename.Open(FileMode.Open, doClear:=False, [readOnly]:=True))
        End Sub

        Public Shared Function RoundUpToNearestFour(value As Integer) As Integer
            Return (value + 3) And Not 3
        End Function

        Private Function GetColorByteLocation(column As Integer, pixelByteLocation As Integer) As Integer
            If BitsPerPixel = BitsPerPixel.TwentyFour Then
                Return pixelByteLocation
            Else
                s.BaseStream.Seek(pixelByteLocation, SeekOrigin.Begin)
            End If

            Dim b As Byte = s.ReadByte
            Dim colorNumber As Integer

            Select Case BitsPerPixel
                Case BitsPerPixel.One : colorNumber = (b >> (7 - column Mod 8)) And &H1
                Case BitsPerPixel.Four : colorNumber = (b >> ((1 - column Mod 2) * 4)) And &HF
                Case BitsPerPixel.Eight
                    colorNumber = b
                Case Else
                    Throw New InvalidDataException
            End Select

            Return 54 + colorNumber * 4
        End Function

        Public Function GetOffset(row As Integer, column As Integer) As Integer
            row = ImageHeight - 1 - row

            Select Case BitsPerPixel
                Case BitsPerPixel.One : Return Offset + row * RowSize + column / 8
                Case BitsPerPixel.Four : Return Offset + row * RowSize + column / 2
                Case BitsPerPixel.Eight : Return Offset + row * RowSize + column * 1
                Case BitsPerPixel.TwentyFour
                    Return Offset + row * RowSize + column * 3
                Case Else
                    Throw New InvalidDataException
            End Select
        End Function

        Public Function GetPixelColor(row As Integer, column As Integer) As Color
            Dim pixelByteLocation = GetOffset(row, column)
            Dim colorByteLocation = GetColorByteLocation(column, pixelByteLocation)

            s.BaseStream.Seek(colorByteLocation, SeekOrigin.Begin)

            Dim Blue = s.ReadByte
            Dim Green = s.ReadByte
            Dim Red = s.ReadByte

            Return Color.FromArgb(Red, Green, Blue)
        End Function

        Protected Overridable Sub Dispose(disposing As Boolean)
            If Not disposedValue Then
                If disposing Then
                    ' TODO: 释放托管状态(托管对象)
                    Call s.Dispose()
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
End Namespace