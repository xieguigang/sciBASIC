#Region "Microsoft.VisualBasic::820050434d007b701f6908ac21255825, Microsoft.VisualBasic.Core\src\Drawing\netcore8.0\Pen.vb"

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

    '   Total Lines: 204
    '    Code Lines: 100 (49.02%)
    ' Comment Lines: 62 (30.39%)
    '    - Xml Docs: 33.87%
    ' 
    '   Blank Lines: 42 (20.59%)
    '     File Size: 6.66 KB


    '     Class Pen
    ' 
    '         Properties: Alignment, Brush, Color, CustomEndCap, DashCap
    '                     DashOffset, DashStyle, EndCap, LineJoin, MiterLimit
    '                     StartCap, Width
    ' 
    '         Constructor: (+2 Overloads) Sub New
    ' 
    '         Function: ToString
    ' 
    '         Sub: (+2 Overloads) Dispose
    ' 
    '     Enum DashCap
    ' 
    ' 
    '  
    ' 
    ' 
    ' 
    '     Enum PenAlignment
    ' 
    '         Center, Inset, Left, Outset, Right
    ' 
    '  
    ' 
    ' 
    ' 
    '     Enum DashStyle
    ' 
    '         Custom, Dash, DashDot, DashDotDot, Dot
    '         Solid
    ' 
    '  
    ' 
    ' 
    ' 
    '     Enum LineJoin
    ' 
    '         Bevel, Miter, MiterClipped, Round
    ' 
    '  
    ' 
    ' 
    ' 
    '     Enum LineCap
    ' 
    ' 
    '  
    ' 
    ' 
    ' 
    '     Class CustomLineCap
    ' 
    ' 
    ' 
    '     Class AdjustableArrowCap
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '     Class Pens
    ' 
    '         Properties: Black, Gray, Green, LightGray, Red
    '                     White, WhiteSmoke
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    ' 
    ' /********************************************************************************/

#End Region

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
        Public Property Alignment As PenAlignment
        Public Property StartCap As LineCap
        Public Property MiterLimit As Single
        Public Property DashCap As DashCap
        Public Property DashOffset As Single

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

        Public Overrides Function ToString() As String
            Return $"{Width}px {DashStyle.ToString.ToLower} ({Color.ToHtmlColor})"
        End Function

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

    '     Specifies the type of graphic shape to use on both ends of each dash in a dashed
    '     line.
    Public Enum DashCap

        '     Specifies a square cap that squares off both ends of each dash.
        Flat = 0
        '     Specifies a circular cap that rounds off both ends of each dash.
        Round = 2
        '     Specifies a triangular cap that points both ends of each dash.
        Triangle = 3
    End Enum

    '     Specifies the alignment of a System.Drawing.Pen object in relation to the theoretical,
    '     zero-width line.
    Public Enum PenAlignment

        '
        '     Specifies that the System.Drawing.Pen object Is centered over the theoretical
        '     line.
        Center

        '     Specifies that the System.Drawing.Pen Is positioned on the inside of the theoretical
        '     line.
        Inset
        '   Specifies the System.Drawing.Pen Is positioned On the outside Of the theoretical
        '     line.
        Outset
        '     Specifies the System.Drawing.Pen Is positioned to the left of the theoretical
        '     line.
        Left

        '     Specifies the System.Drawing.Pen Is positioned to the right of the theoretical
        '     line.
        Right
    End Enum

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
        Public Shared ReadOnly Property Red As New Pen(Color.Red)
        Public Shared ReadOnly Property Green As New Pen(Color.Green)

        Private Sub New()
        End Sub

    End Class
#End If
End Namespace
