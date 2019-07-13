#Region "Microsoft.VisualBasic::75f778e755b91dab0b2fcf45cb398236, Data_science\Mathematica\Math\DataFittings\TestPoint.vb"

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

    ' Structure TestPoint
    ' 
    '     Properties: Err, X, Y, Yfit
    ' 
    '     Function: ToString
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports System.Xml.Serialization

<XmlType("point", [Namespace]:="http://scibasic.net/math/Bootstrapping")>
Public Structure TestPoint

    <XmlAttribute("x")> Public Property X As Double
    <XmlAttribute("y")> Public Property Y As Double
    <XmlAttribute("fx")> Public Property Yfit As Double

    <XmlIgnore>
    Public ReadOnly Property Err As Double
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Get
            Return Y - Yfit
        End Get
    End Property

    Public Overrides Function ToString() As String
        Return $"[{X.ToString("F2")}, {Y.ToString("F2")}] {Yfit.ToString("F2")}"
    End Function

    Public Shared Narrowing Operator CType(point As TestPoint) As PointF
        Return New PointF(point.X, point.Y)
    End Operator
End Structure
