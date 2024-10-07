Imports System.Drawing

Namespace Imaging

#If NET8_0_OR_GREATER Then

    ''' <summary>
    ''' The stroke pen wrapper for .net 8.0
    ''' </summary>
    Public Class Pen : Implements IDisposable

        Private disposedValue As Boolean

        Public Property Color As Color
        Public Property Width As Single = 1
        Public Property DashStyle As DashStyle
        Public Property Brush As Brush
        Public Property LineJoin As LineJoin
        Public Property CustomEndCap As CustomLineCap
        Public Property EndCap As LineCap

        Sub New(color As Color, Optional width As Single = 1)
            _Color = color
            _Width = width
            _Brush = New SolidBrush(color)
        End Sub

        Sub New(brush As SolidBrush, Optional width As Single = 1)
            _Brush = brush
            _Color = brush.Color
            _Width = width
        End Sub

        Protected Overridable Sub Dispose(disposing As Boolean)
            If Not disposedValue Then
                If disposing Then
                    ' TODO: dispose managed state (managed objects)
                End If

                ' TODO: free unmanaged resources (unmanaged objects) and override finalizer
                ' TODO: set large fields to null
                disposedValue = True
            End If
        End Sub

        ' ' TODO: override finalizer only if 'Dispose(disposing As Boolean)' has code to free unmanaged resources
        ' Protected Overrides Sub Finalize()
        '     ' Do not change this code. Put cleanup code in 'Dispose(disposing As Boolean)' method
        '     Dispose(disposing:=False)
        '     MyBase.Finalize()
        ' End Sub

        Public Sub Dispose() Implements IDisposable.Dispose
            ' Do not change this code. Put cleanup code in 'Dispose(disposing As Boolean)' method
            Dispose(disposing:=True)
            GC.SuppressFinalize(Me)
        End Sub
    End Class

    Public Enum DashStyle
        Solid
        Dash
        Dot
        DashDot
        DashDotDot
        Custom
    End Enum

    ''' <summary>
    ''' Specifies how to join consecutive line Or curve segments in a figure (subpath)
    ''' contained in a System.Drawing.Drawing2D.GraphicsPath object.
    ''' </summary>
    Public Enum LineJoin
        ''' <summary>
        ''' Specifies a mitered join. This produces a sharp corner Or a clipped corner, depending
        ''' on whether the length of the miter exceeds the miter limit.
        ''' </summary>
        Miter
        ''' <summary>
        ''' Specifies a beveled join. This produces a diagonal corner.    
        ''' </summary>
        Bevel
        ''' <summary>
        ''' Specifies a circular join. This produces a smooth, circular arc between the lines.    
        ''' </summary>
        Round
        ''' <summary>
        ''' Specifies a mitered join. This produces a sharp corner Or a beveled corner, depending
        ''' on whether the length of the miter exceeds the miter limit.
        ''' </summary>
        MiterClipped
    End Enum

    '     Specifies the available cap styles with which a System.Drawing.Pen object can
    '     end a line.
    Public Enum LineCap

        '     Specifies a flat line cap.
        Flat = 0

        '     Specifies a square line cap.
        Square = 1

        ' Specifies a round line cap.
        Round = 2

        '     Specifies a triangular line cap.
        Triangle = 3

        '     Specifies no anchor.
        NoAnchor = 16

        '     Specifies a square anchor line cap.
        SquareAnchor = 17

        '     Specifies a round anchor cap.
        RoundAnchor = 18

        '     Specifies a diamond anchor cap.
        DiamondAnchor = 19

        '     Specifies an arrow-shaped anchor cap.
        ArrowAnchor = 20

        '     Specifies a custom line cap.
        Custom = 255

        '     Specifies a mask used to check whether a line cap Is an anchor cap.
        AnchorMask = 240
    End Enum


    Public Class CustomLineCap

    End Class

    Public Class AdjustableArrowCap : Inherits CustomLineCap

        Sub New(width As Single, height As Single)

        End Sub
    End Class

    Public NotInheritable Class Pens

        Public Shared ReadOnly Property Black As New Pen(Color.Black)
        Public Shared ReadOnly Property Gray As New Pen(Color.Gray)
        Public Shared ReadOnly Property LightGray As New Pen(Color.LightGray)
        Public Shared ReadOnly Property White As New Pen(Color.White)
        Public Shared ReadOnly Property WhiteSmoke As New Pen(Color.WhiteSmoke)

        Private Sub New()
        End Sub

    End Class
#End If
End Namespace