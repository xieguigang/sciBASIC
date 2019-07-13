#Region "Microsoft.VisualBasic::e437f1e97ae03e2ffad19a618184450d, Microsoft.VisualBasic.Core\Text\Xml\Models\Coordinate.vb"

    ' Author:
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 



    ' /********************************************************************************/

    ' Summaries:

    '     Structure Coordinate
    ' 
    '         Properties: ID, X, Y
    ' 
    '         Constructor: (+3 Overloads) Sub New
    '         Function: ToString
    '         Operators: <>, =
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.Imaging.LayoutModel

Namespace Text.Xml.Models

    ''' <summary>
    ''' Improvements on the xml format layout compare with <see cref="PointF"/> type.
    ''' </summary>
    Public Structure Coordinate : Implements ILayoutCoordinate

        ' 2017-6-22
        ' 因为double类型可以兼容Integer类型，所以在这里改为double类型
        ' 所以从pointf构建可以不再经过转换了

        <XmlAttribute("x")> Public Property X As Double Implements ILayoutCoordinate.X
        <XmlAttribute("y")> Public Property Y As Double Implements ILayoutCoordinate.Y
        <XmlAttribute>
        Public Property ID As String Implements ILayoutCoordinate.ID

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Sub New(pt As Point)
            Call Me.New(pt.X, pt.Y)
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Sub New(pt As PointF)
            Call Me.New(pt.X, pt.Y)
        End Sub

        Sub New(x#, y#)
            Me.X = x
            Me.Y = y
        End Sub

        Public Overrides Function ToString() As String
            Return $"[{X}, {Y}]"
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Operator =(c As Coordinate, pt As Point) As Boolean
            Return c.X = pt.X AndAlso c.Y = pt.Y
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Operator <>(c As Coordinate, pt As Point) As Boolean
            Return Not c = pt
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Widening Operator CType(pt As Point) As Coordinate
            Return New Coordinate With {
                .X = pt.X,
                .Y = pt.Y
            }
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Narrowing Operator CType(x As Coordinate) As Point
            Return New Point(x.X, x.Y)
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Widening Operator CType(pt As Integer()) As Coordinate
            Return New Coordinate(pt.FirstOrDefault, pt.LastOrDefault)
        End Operator
    End Structure
End Namespace
