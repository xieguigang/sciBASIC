#Region "Microsoft.VisualBasic::c3ead70707eaeceeab390be78659c0a9, mime\text%html\CSS\Elements\Padding.vb"

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

    '   Total Lines: 400
    '    Code Lines: 243 (60.75%)
    ' Comment Lines: 108 (27.00%)
    '    - Xml Docs: 98.15%
    ' 
    '   Blank Lines: 49 (12.25%)
    '     File Size: 15.41 KB


    '     Structure PaddingLayout
    ' 
    '         Properties: Horizontal, LayoutVector, Vertical
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: EvaluateFromCSS, ToString
    '         Operators: -, +
    ' 
    '     Structure Padding
    ' 
    '         Properties: Bottom, IsEmpty, Left, Right, Top
    '                     Zero
    ' 
    '         Constructor: (+7 Overloads) Sub New
    '         Function: Equals, GetCanvasRegion, Horizontal, LayoutVector, Offset2D
    '                   ToString, TryParse, Vertical
    '         Operators: <>, =
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.ComponentModel
Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Scripting.Runtime

Namespace CSS

    ''' <summary>
    ''' A fix numeric layout data 
    ''' </summary>
    Public Structure PaddingLayout

        Dim Top As Single
        Dim Right As Single
        Dim Bottom As Single
        Dim Left As Single

        Public ReadOnly Property LayoutVector As Single()
            Get
                Return New Single() {Top, Right, Bottom, Left}
            End Get
        End Property

        Public ReadOnly Property Horizontal As Single
            Get
                Return Left + Right
            End Get
        End Property

        Public ReadOnly Property Vertical As Single
            Get
                Return Top + Bottom
            End Get
        End Property

        Sub New(layoutVector As Single())
            Top = layoutVector(0)
            Right = layoutVector(1)
            Bottom = layoutVector(2)
            Left = layoutVector(3)
        End Sub

        Public Overrides Function ToString() As String
            Return $"padding: {Top}px {Right}px {Bottom}px {Left}px;"
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function EvaluateFromCSS(css As CSSEnvirnment, layout As Padding) As PaddingLayout
            Return New PaddingLayout With {
                .Bottom = css.GetHeight(layout.Bottom),
                .Left = css.GetWidth(layout.Left),
                .Right = css.GetWidth(layout.Right),
                .Top = css.GetHeight(layout.Top)
            }
        End Function

        ''' <summary>
        ''' Performs vector addition on the two specified System.Windows.Forms.Padding objects,
        ''' resulting in a new System.Windows.Forms.Padding.
        ''' </summary>
        ''' <param name="p1">The first System.Windows.Forms.Padding to add.</param>
        ''' <param name="p2">The second System.Windows.Forms.Padding to add.</param>
        ''' <returns>A new System.Windows.Forms.Padding that results from adding p1 and p2.</returns>
        Public Shared Operator +(p1 As PaddingLayout, p2 As PaddingLayout) As PaddingLayout
            Dim a = p1.LayoutVector
            Dim b = p2.LayoutVector
            Dim out!() = New Single(3) {}

            For i As Integer = 0 To out.Length - 1
                out(i) = a(i) + b(i)
            Next

            Return New PaddingLayout(layoutVector:=out)
        End Operator

        ''' <summary>
        ''' Performs vector subtraction on the two specified System.Windows.Forms.Padding
        ''' objects, resulting in a new System.Windows.Forms.Padding.
        ''' </summary>
        ''' <param name="p1">The System.Windows.Forms.Padding to subtract from (the minuend).</param>
        ''' <param name="p2">The System.Windows.Forms.Padding to subtract from (the subtrahend).</param>
        ''' <returns>The System.Windows.Forms.Padding result of subtracting p2 from p1.</returns>
        Public Shared Operator -(p1 As PaddingLayout, p2 As PaddingLayout) As PaddingLayout
            Dim a = p1.LayoutVector
            Dim b = p2.LayoutVector
            Dim out!() = New Single(3) {}

            For i As Integer = 0 To out.Length - 1
                out(i) = a(i) - b(i)
            Next

            Return New PaddingLayout(layoutVector:=out)
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Narrowing Operator CType(padding As PaddingLayout) As Integer()
            Return New Integer() {
                padding.Top,
                padding.Right,
                padding.Bottom,
                padding.Left
            }
        End Operator

    End Structure

    ''' <summary>
    ''' Represents padding or margin information associated with a gdi+ element. 
    ''' (padding: top, right, bottom, left)
    ''' </summary>
    Public Structure Padding

        ''' <summary>
        ''' A empty padding layout css data
        ''' </summary>
        ''' <returns>
        ''' zero padding layout element, but not empty
        ''' </returns>
        Public Shared ReadOnly Property Zero As Padding
            Get
                Return New Padding With {
                    .Left = "0px",
                    .Bottom = "0px",
                    .Right = "0px",
                    .Top = "0px"
                }
            End Get
        End Property

        ''' <summary>
        ''' Gets or sets the padding value for the top edge.
        ''' </summary>
        ''' <returns>The padding, in pixels, for the top edge.</returns>
        <RefreshProperties(RefreshProperties.All)>
        Public Property Top As String

        ''' <summary>
        ''' Gets or sets the padding value for the right edge.
        ''' </summary>
        ''' <returns>The padding, in pixels, for the right edge.</returns>
        <RefreshProperties(RefreshProperties.All)>
        Public Property Right As String

        ''' <summary>
        ''' Gets or sets the padding value for the left edge.
        ''' </summary>
        ''' <returns>The padding, in pixels, for the left edge.</returns>
        <RefreshProperties(RefreshProperties.All)>
        Public Property Left As String

        ''' <summary>
        ''' Gets or sets the padding value for the bottom edge.
        ''' </summary>
        ''' <returns>The padding, in pixels, for the bottom edge.</returns>
        <RefreshProperties(RefreshProperties.All)>
        Public Property Bottom As String

        ''' <summary>
        ''' all padding value is ZERO then it means empty
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property IsEmpty As Boolean
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return Top.StringEmpty(, True) AndAlso
                    Bottom.StringEmpty(, True) AndAlso
                    Left.StringEmpty(, True) AndAlso
                    Right.StringEmpty(, True)
            End Get
        End Property

        ''' <summary>
        ''' Initializes a new instance of the <see cref="Padding"/> class using the
        ''' supplied padding size for all edges.
        ''' </summary>
        ''' <param name="all">The number of pixels to be used for padding for all edges.</param>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub New(all As Integer)
            Call Me.New(all, all, all, all)
        End Sub

        ''' <summary>
        ''' Initializes a new instance of the <see cref="Padding"/> class using a
        ''' separate padding size for each edge.
        ''' </summary>
        ''' <param name="left">The padding size, in pixels, for the left edge.</param>
        ''' <param name="top">The padding size, in pixels, for the top edge.</param>
        ''' <param name="right">The padding size, in pixels, for the right edge.</param>
        ''' <param name="bottom">The padding size, in pixels, for the bottom edge.</param>
        Public Sub New(left%, top%, right%, bottom%)
            With Me
                .Left = left & "px"
                .Top = top & "px"
                .Right = right & "px"
                .Bottom = bottom & "px"
            End With
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Sub New(margin As Size)
            Call Me.New(margin.Width, margin.Height)
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Sub New(width%, height%)
            Call Me.New(left:=width, right:=width, top:=height, bottom:=height)
        End Sub

        ''' <summary>
        ''' create padding object with a given layout pixels vector: top, right, bottom, left
        ''' </summary>
        ''' <param name="layoutVector"><see cref="LayoutVector"/></param>
        Sub New(layoutVector%())
            Top = layoutVector(0)
            Right = layoutVector(1)
            Bottom = layoutVector(2)
            Left = layoutVector(3)
        End Sub

        Sub New(layoutVector As Double())
            Top = layoutVector(0)
            Right = layoutVector(1)
            Bottom = layoutVector(2)
            Left = layoutVector(3)
        End Sub

        Sub New(layout As PaddingLayout)
            Top = layout.Top
            Right = layout.Right
            Bottom = layout.Bottom
            Left = layout.Left
        End Sub

        Public Function GetCanvasRegion(css As CSSEnvirnment) As Rectangle
            Dim location As New Point(css.GetWidth(Left), css.GetHeight(Top))
            Dim size As Size = css.canvas
            Dim width = size.Width - Horizontal(css)
            Dim height = size.Height - Vertical(css)

            Return New Rectangle(location, New Size(width, height))
        End Function

        ''' <summary>
        ''' Gets the combined padding for the right and left edges.
        ''' </summary>
        ''' <returns></returns>
        Public Function Horizontal(css As CSSEnvirnment) As Single
            Return css.GetWidth(Left) + css.GetWidth(Right)
        End Function

        ''' <summary>
        ''' Gets the combined padding for the top and bottom edges.
        ''' </summary>
        ''' <returns></returns>
        Public Function Vertical(css As CSSEnvirnment) As Single
            Return css.GetHeight(Top) + css.GetHeight(Bottom)
        End Function

        <DebuggerStepThrough>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Offset2D(dx As Double, dy As Double, css As CSSEnvirnment) As Padding
            Return New Padding With {
                .Left = css.GetWidth(Left) + dx,
                .Right = css.GetWidth(Right) - dx,
                .Top = css.GetHeight(Top) + dy,
                .Bottom = css.GetHeight(Bottom) - dy
            }
        End Function

        ''' <summary>
        ''' padding: top, right, bottom, left
        ''' </summary>
        ''' <returns></returns>
        Public Overrides Function ToString() As String
            Return $"padding:{Top} {Right} {Bottom} {Left};"
        End Function

        ''' <summary>
        ''' padding: top, right, bottom, left
        ''' </summary>
        ''' <returns></returns>
        Public Function LayoutVector(css As CSSEnvirnment) As Single()
            Return New Single() {
                css.GetHeight(Top),
                css.GetWidth(Right),
                css.GetHeight(Bottom),
                css.GetWidth(Left)
            }
        End Function

        ''' <summary>
        ''' 转换为css字符串
        ''' </summary>
        ''' <param name="padding"></param>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Narrowing Operator CType(padding As Padding) As String
            Return padding.ToString
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Narrowing Operator CType(padding As Padding) As Integer()
            Return New Integer() {
                padding.Top,
                padding.Right,
                padding.Bottom,
                padding.Left
            }
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Narrowing Operator CType(padding As Padding) As Single()
            Return New Single() {
                padding.Top,
                padding.Right,
                padding.Bottom,
                padding.Left
            }
        End Operator

        ''' <summary>
        ''' 同时兼容padding css，以及使用size表达式统一赋值
        ''' </summary>
        ''' <param name="css$"></param>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Widening Operator CType(css As String) As Padding
            Return Padding.TryParse(css)
        End Operator

        Public Shared Function TryParse(css$, Optional [default] As Padding = Nothing) As Padding
            Dim value As NamedValue(Of String) = css _
                .GetTagValue(":", trim:=True)

            If value.Name.StringEmpty AndAlso css.IndexOf(","c) > -1 Then
                Dim size As Size = css.SizeParser
                Return New Padding(margin:=size)
            End If

            Dim tokens$() = (+value).Trim$(";"c).Split

            If tokens.Length = 1 Then  ' all
                Return New Padding With {
                    .Bottom = tokens(0),
                    .Left = tokens(0),
                    .Right = tokens(0),
                    .Top = tokens(0)
                }
            Else
                Return New Padding With {
                    .Top = tokens(0),
                    .Right = tokens(1),
                    .Bottom = tokens(2),
                    .Left = tokens(3)
                }
            End If
        End Function

        ''' <summary>
        ''' Determines whether the value of the specified object is equivalent to the current
        ''' System.Windows.Forms.Padding.
        ''' </summary>
        ''' <param name="other">The object to compare to the current System.Windows.Forms.Padding.</param>
        ''' <returns>true if the System.Windows.Forms.Padding objects are equivalent; otherwise, false.</returns>
        Public Overrides Function Equals(other As Object) As Boolean
            If other Is Nothing Then
                Return False
            ElseIf Not other.GetType.Equals(GetType(Padding)) Then
                Return False
            Else
                With DirectCast(other, Padding)
                    Return Left = .Left AndAlso Right = .Right AndAlso Top = .Top AndAlso Bottom = .Bottom
                End With
            End If
        End Function

        ''' <summary>
        ''' Tests whether two specified System.Windows.Forms.Padding objects are equivalent.
        ''' </summary>
        ''' <param name="p1">A System.Windows.Forms.Padding to test.</param>
        ''' <param name="p2">A System.Windows.Forms.Padding to test.</param>
        ''' <returns>true if the two System.Windows.Forms.Padding objects are equal; otherwise, false.</returns>
        Public Shared Operator =(p1 As Padding, p2 As Padding) As Boolean
            Return p1.Top = p2.Top AndAlso
                p1.Right = p2.Right AndAlso
                p1.Bottom = p2.Bottom AndAlso
                p1.Left = p2.Left
        End Operator

        ''' <summary>
        ''' Tests whether two specified System.Windows.Forms.Padding objects are not equivalent.
        ''' </summary>
        ''' <param name="p1">A System.Windows.Forms.Padding to test.</param>
        ''' <param name="p2">A System.Windows.Forms.Padding to test.</param>
        ''' <returns>true if the two System.Windows.Forms.Padding objects are different; otherwise,
        ''' false.</returns>
        Public Shared Operator <>(p1 As Padding, p2 As Padding) As Boolean
            Return Not (p1 = p2)
        End Operator
    End Structure
End Namespace
