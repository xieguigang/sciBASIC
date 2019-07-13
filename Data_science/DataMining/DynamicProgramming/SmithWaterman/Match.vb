#Region "Microsoft.VisualBasic::444e7081357cbfae5ba88bfa3d1b18a2, Data_science\DataMining\DynamicProgramming\SmithWaterman\Match.vb"

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

    '     Class Match
    ' 
    '         Properties: FromA, FromB, Score, ToA, ToB
    ' 
    '         Constructor: (+2 Overloads) Sub New
    '         Function: isChainable, notOverlap, ToString
    '         Operators: -
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports System.Xml.Serialization

Namespace SmithWaterman

    ''' <summary>
    '''  Match class defintion
    ''' </summary>
    Public Class Match

        Sub New()
        End Sub

        Sub New(fA As Integer, tA As Integer, fB As Integer, tB As Integer, s As Double)
            _FromA = fA
            _FromB = fB
            _ToA = tA
            _ToB = tB
            _Score = s
        End Sub

        ''' <summary>
        ''' Returns the value of fromA.
        ''' </summary>
        ''' <returns></returns>
        <XmlAttribute> Public Property FromA As Integer

        ''' <summary>
        ''' Returns the value of fromB.
        ''' </summary>
        <XmlAttribute> Public Property FromB As Integer

        ''' <summary>
        ''' Returns the value of toA.
        ''' </summary>
        <XmlAttribute> Public Property ToA As Integer

        ''' <summary>
        ''' Returns the value of toB.
        ''' </summary>
        <XmlAttribute> Public Property ToB As Integer

        ''' <summary>
        ''' Returns the value of score.
        ''' </summary>
        <XmlAttribute> Public Property Score As Double

        ''' <summary>
        ''' Check whether this Match onecjt overlap with input Match m;
        ''' return true if two objects do not overlap
        ''' </summary>
        ''' <param name="m"></param>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function notOverlap(m As Match) As Boolean
            Return (m.FromA > _ToA OrElse _FromA > m.ToA) AndAlso (m.FromB > _ToB OrElse _FromB > m.ToB)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function isChainable(m As Match) As Boolean
            Return (m.FromA > _ToA AndAlso m.FromB > _ToB)
        End Function

        Public Overrides Function ToString() As String
            Return $"[query: {{{FromA}, {ToA}}}, ref: {{{FromB}, {ToB}}}], score:={Score}"
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Operator -(match As Match, offset%) As Match
            Return New Match With {
                .FromA = match.FromA - offset,
                .FromB = match.FromB - offset,
                .Score = match.Score,
                .ToA = match.ToA - offset,
                .ToB = match.ToB - offset
            }
        End Operator
    End Class
End Namespace
