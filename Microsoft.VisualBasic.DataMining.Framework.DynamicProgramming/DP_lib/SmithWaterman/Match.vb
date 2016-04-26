Imports System.Collections.Generic
Imports System.Text
Imports System.Xml.Serialization

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
    Public Function notOverlap(m As Match) As Boolean
        Return (m.FromA > _ToA OrElse _FromA > m.ToA) AndAlso (m.FromB > _ToB OrElse _FromB > m.ToB)
    End Function

    Public Function isChainable(m As Match) As Boolean
        Return (m.FromA > _ToA AndAlso m.FromB > _ToB)
    End Function

    Public Overrides Function ToString() As String
        Return $"[query: {FromA}  ===> {ToA}] <---> [subject: {FromB}  ===> {ToB}], score:={Score}"
    End Function

    Public Shared ReadOnly Property FROMA_COMPARATOR As IComparer(Of Match) =
        New ComparatorAnonymousInnerClassHelper()

    Private Class ComparatorAnonymousInnerClassHelper
        Implements IComparer(Of Match)

        Public Sub New()
        End Sub

        Public Function Compare(x As Match, y As Match) As Integer Implements IComparer(Of Match).Compare
            Return x.FromA - y.FromA
        End Function
    End Class
End Class