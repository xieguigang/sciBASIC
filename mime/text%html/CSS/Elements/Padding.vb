#Region "Microsoft.VisualBasic::2f0f8fb0b613ab76fdf1189896ca166a, mime\text%html\CSS\Elements\Padding.vb"

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

    '   Total Lines: 320
    '    Code Lines: 180 (56.25%)
    ' Comment Lines: 102 (31.87%)
    '    - Xml Docs: 98.04%
    ' 
    '   Blank Lines: 38 (11.88%)
    '     File Size: 12.55 KB


    '     Structure Padding
    ' 
    '         Properties: Bottom, Horizontal, IsEmpty, LayoutVector, Left
    '                     Right, Top, Vertical, Zero
    ' 
    '         Constructor: (+6 Overloads) Sub New
    '         Function: Equals, GetCanvasRegion, Offset2D, ToString, TryParse
    '         Operators: -, +, <>, =
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
    ''' Represents padding or margin information associated with a gdi element. 
    ''' (padding: top, right, bottom, left)
    ''' </summary>
    Public Structure Padding

        ''' <summary>
        ''' Gets the combined padding for the right and left edges.
        ''' </summary>
        ''' <returns></returns>
        <Browsable(False)>
        Public ReadOnly Property Horizontal As Single
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return Left + Right
            End Get
        End Property

        Public Shared ReadOnly Property Zero As Padding
            Get
                Return New Padding
            End Get
        End Property

        ''' <summary>
        ''' Gets or sets the padding value for the top edge.
        ''' </summary>
        ''' <returns>The padding, in pixels, for the top edge.</returns>
        <RefreshProperties(RefreshProperties.All)>
        Public Property Top As Single

        ''' <summary>
        ''' Gets or sets the padding value for the right edge.
        ''' </summary>
        ''' <returns>The padding, in pixels, for the right edge.</returns>
        <RefreshProperties(RefreshProperties.All)>
        Public Property Right As Single

        ''' <summary>
        ''' Gets or sets the padding value for the left edge.
        ''' </summary>
        ''' <returns>The padding, in pixels, for the left edge.</returns>
        <RefreshProperties(RefreshProperties.All)>
        Public Property Left As Single

        ''' <summary>
        ''' Gets or sets the padding value for the bottom edge.
        ''' </summary>
        ''' <returns>The padding, in pixels, for the bottom edge.</returns>
        <RefreshProperties(RefreshProperties.All)>
        Public Property Bottom As Single

        ''' <summary>
        ''' Gets the combined padding for the top and bottom edges.
        ''' </summary>
        ''' <returns></returns>
        <Browsable(False)>
        Public ReadOnly Property Vertical As Single
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return Top + Bottom
            End Get
        End Property

        ''' <summary>
        ''' all padding value is ZERO then it means empty
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property IsEmpty As Boolean
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return Top = 0 AndAlso
                    Bottom = 0 AndAlso
                    Left = 0 AndAlso
                    Right = 0
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
                .Left = left
                .Top = top
                .Right = right
                .Bottom = bottom
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

        Public Function GetCanvasRegion(size As Size) As Rectangle
            Dim location As New Point(Left, Top)
            Dim width = size.Width - Horizontal
            Dim height = size.Height - Vertical

            Return New Rectangle(location, New Size(width, height))
        End Function

        <DebuggerStepThrough>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Offset2D(dx As Double, dy As Double) As Padding
            Return New Padding With {
                .Left = Left + dx,
                .Right = Right - dx,
                .Top = Top + dy,
                .Bottom = Bottom - dy
            }
        End Function

        ''' <summary>
        ''' padding: top, right, bottom, left
        ''' </summary>
        ''' <returns></returns>
        Public Overrides Function ToString() As String
            Return $"padding:{Top}px {Right}px {Bottom}px {Left}px;"
        End Function

        ''' <summary>
        ''' padding: top, right, bottom, left
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property LayoutVector As Single()
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return Me
            End Get
        End Property

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
            Dim vector%() = tokens _
                .Select(Function(s) CInt(s.ParseNumeric)) _
                .ToArray

            If vector.Length = 1 Then  ' all
                Return New Padding(all:=vector(Scan0))
            Else
                Return New Padding(vector)
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
        ''' Performs vector addition on the two specified System.Windows.Forms.Padding objects,
        ''' resulting in a new System.Windows.Forms.Padding.
        ''' </summary>
        ''' <param name="p1">The first System.Windows.Forms.Padding to add.</param>
        ''' <param name="p2">The second System.Windows.Forms.Padding to add.</param>
        ''' <returns>A new System.Windows.Forms.Padding that results from adding p1 and p2.</returns>
        Public Shared Operator +(p1 As Padding, p2 As Padding) As Padding
            Dim a = p1.LayoutVector
            Dim b = p2.LayoutVector
            Dim out%() = New Integer(4) {}

            For i As Integer = 0 To out.Length - 1
                out(i) = a(i) + b(i)
            Next

            Return New Padding(layoutVector:=out)
        End Operator

        ''' <summary>
        ''' Performs vector subtraction on the two specified System.Windows.Forms.Padding
        ''' objects, resulting in a new System.Windows.Forms.Padding.
        ''' </summary>
        ''' <param name="p1">The System.Windows.Forms.Padding to subtract from (the minuend).</param>
        ''' <param name="p2">The System.Windows.Forms.Padding to subtract from (the subtrahend).</param>
        ''' <returns>The System.Windows.Forms.Padding result of subtracting p2 from p1.</returns>
        Public Shared Operator -(p1 As Padding, p2 As Padding) As Padding
            Dim a = p1.LayoutVector
            Dim b = p2.LayoutVector
            Dim out%() = New Integer(4) {}

            For i As Integer = 0 To out.Length - 1
                out(i) = a(i) - b(i)
            Next

            Return New Padding(layoutVector:=out)
        End Operator

        ''' <summary>
        ''' Tests whether two specified System.Windows.Forms.Padding objects are equivalent.
        ''' </summary>
        ''' <param name="p1">A System.Windows.Forms.Padding to test.</param>
        ''' <param name="p2">A System.Windows.Forms.Padding to test.</param>
        ''' <returns>true if the two System.Windows.Forms.Padding objects are equal; otherwise, false.</returns>
        Public Shared Operator =(p1 As Padding, p2 As Padding) As Boolean
            Return p1.LayoutVector.SequenceEqual(p2.LayoutVector)
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
