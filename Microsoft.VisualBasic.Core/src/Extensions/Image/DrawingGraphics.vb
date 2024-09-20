Imports System.IO
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Imaging.Math2D

Namespace Imaging

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <remarks>
    ''' An abstract interface model for hide the .NET Framework gdi+ on linux for .net 8 application
    ''' </remarks>
    Public MustInherit Class DrawingGraphics

        Protected ReadOnly w As Integer
        Protected ReadOnly h As Integer

        Public ReadOnly Property Width As Integer
            Get
                Return w
            End Get
        End Property

        Public ReadOnly Property Height As Integer
            Get
                Return h
            End Get
        End Property

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Sub New(width As Integer, height As Integer)
            w = width
            h = height
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public MustOverride Sub Clear(fill As ArgbColor)

        Public MustOverride Sub DrawString(s As String, fontName As String, fontSize As Single, color As ArgbColor, x As Single, y As Single)
        Public MustOverride Sub DrawLine(x1 As Single, y1 As Single, x2 As Single, y2 As Single, color As ArgbColor, width As Single, Optional dash As Single() = Nothing)
        Public MustOverride Sub DrawPath(path As Polygon2D, color As ArgbColor, width As Single, Optional fill As ArgbColor? = Nothing, Optional dash As Single() = Nothing)
        Public MustOverride Function MeasureString(text As String, fontName As String, fontSize As Single) As (Width As Single, Height As Single)

        ''' <summary>
        ''' save default image file
        ''' </summary>
        ''' <param name="file"></param>
        Public Sub Save(file As String)
            Using s As Stream = file.Open(FileMode.OpenOrCreate, doClear:=True,)
                Call Save(s)
            End Using
        End Sub

        Public MustOverride Sub Save(file As Stream)

    End Class
End Namespace