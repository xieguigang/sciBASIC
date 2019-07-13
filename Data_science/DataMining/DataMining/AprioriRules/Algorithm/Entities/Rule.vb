#Region "Microsoft.VisualBasic::5573c8298d4b10e7472b93db76d4fba4, Data_science\DataMining\DataMining\AprioriRules\Algorithm\Entities\Rule.vb"

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

    '     Class Rule
    ' 
    '         Properties: Confidence, SupportX, SupportXY, X, Y
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: CompareTo, Equals, GetHashCode, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Data.Linq.Mapping
Imports Microsoft.VisualBasic.DataMining.AprioriRules.Impl

Namespace AprioriRules.Entities

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <remarks></remarks>
    Public Class Rule : Implements IComparable(Of Rule)

#Region "Member Variables"

        Dim combination As String
        Dim remaining As String

#End Region

#Region "Public Properties"

        <Column(Name:="rule.X")> Public ReadOnly Property X As String
            Get
                Return combination
            End Get
        End Property

        <Column(Name:="rule.Y")> Public ReadOnly Property Y As String
            Get
                Return remaining
            End Get
        End Property

        <Column(Name:="support(XY)")>
        Public ReadOnly Property SupportXY As Double
        <Column(Name:="support(X)")>
        Public ReadOnly Property SupportX As Double

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        <Column(Name:="confidence")> Public ReadOnly Property Confidence As Double

        Public Sub New(combination$, remaining$, confidence#, supports As (XY#, X#))
            Me.combination = combination
            Me.remaining = remaining
            Me.Confidence = confidence
            Me.SupportX = supports.X
            Me.SupportXY = supports.XY
        End Sub
#End Region

        Public Overrides Function ToString() As String
            Return $"({SupportXY}/{SupportX} = {Math.Round(Confidence, 4)}) {{ {X} }} -> {{ {Y} }}"
        End Function

#Region "IComparable<clssRules> Members"

        Public Function CompareTo(other As Rule) As Integer Implements IComparable(Of Rule).CompareTo
            Return X.CompareTo(other.X)
        End Function
#End Region

        Public Overrides Function GetHashCode() As Integer
            Dim sortedXY$ = Apriori.SorterSortTokens(X & Y)
            Return sortedXY.GetHashCode()
        End Function

        Public Overrides Function Equals(obj As Object) As Boolean
            Dim other = TryCast(obj, Rule)

            If other Is Nothing Then
                Return False
            End If

            Return other.X = X AndAlso other.Y = Y OrElse other.X = Y AndAlso other.Y = X
        End Function
    End Class
End Namespace
