Imports Microsoft.VisualBasic.DocumentFormat.Csv.StorageProvider.Reflection

Namespace AprioriAlgorithm.Entities

    ''' <summary>
    ''' 事件关联规则
    ''' </summary>
    ''' <remarks></remarks>
    Public Class Rule : Implements IComparable(Of Rule)

#Region "Member Variables"

        Dim combination As String, remaining As String
        Dim _confidence As Double

#End Region

#Region "Constructor"

        Public Sub New(combination As String, remaining As String, confidence As Double)
            Me.combination = combination
            Me.remaining = remaining
            Me._confidence = confidence
        End Sub

#End Region

#Region "Public Properties"

        <Column("rule.X")> Public ReadOnly Property X() As String
            Get
                Return combination
            End Get
        End Property

        <Column("rule.Y")> Public ReadOnly Property Y() As String
            Get
                Return remaining
            End Get
        End Property

        ''' <summary>
        ''' 当前的这个事件关联规则的置信度的高低
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        <Column("confidence")> Public ReadOnly Property Confidence() As Double
            Get
                Return _confidence
            End Get
        End Property

        Public Overrides Function ToString() As String
            Return String.Format("({0})  {1}  --> {2}", Confidence, X, Y)
        End Function

#End Region

#Region "IComparable<clssRules> Members"

        Public Function CompareTo(other As Rule) As Integer Implements IComparable(Of Rule).CompareTo
            Return X.CompareTo(other.X)
        End Function
#End Region

        Public Overrides Function GetHashCode() As Integer
            Dim sortedXY As String = Apriori.SorterSortTokens(X & Y)
            Return sortedXY.GetHashCode()
        End Function

        Public Overrides Function Equals(obj As Object) As Boolean
            Dim other = TryCast(obj, Rule)
            If other Is Nothing Then
                Return False
            End If

            Return other.X = Me.X AndAlso other.Y = Me.Y OrElse other.X = Me.Y AndAlso other.Y = Me.X
        End Function
    End Class
End Namespace