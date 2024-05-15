#Region "Microsoft.VisualBasic::5786d956ce34f2a2d7e5fcdefa8fec1d, Data_science\Mathematica\Math\KaplanMeierEstimator\KaplanMeierEstimate.vb"

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

    '   Total Lines: 222
    '    Code Lines: 156
    ' Comment Lines: 27
    '   Blank Lines: 39
    '     File Size: 7.95 KB


    ' Class KaplanMeierEstimate
    ' 
    '     Properties: GroupAEvents, GroupBEvents, MergedEvents, PValue, TotalFailingA
    '                 TotalFailingB
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    '     Function: RunGroup
    ' 
    '     Sub: ComputePValue, MergeEvents, RunEstimate
    '     Class KaplanMeierStatus
    ' 
    '         Properties: NumberAtRisk, NumberFailing, SurvivalProbability, Time
    ' 
    '     Class JoinedEvent
    ' 
    '         Properties: AtRiskA, AtRiskB, FailingA, FailingB, Time
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Math.KaplanMeierEstimator.Models
Imports Microsoft.VisualBasic.Math.Statistics.Distributions
Imports stdNum = System.Math

''' <summary>
''' Performs the Kaplan Meier algorithm over 2 groups, 
''' and computes the statistic significance for the 
''' groups behaving similarly over time.
''' </summary>
Public Class KaplanMeierEstimate

    Private _GroupAEvents As IReadOnlyList(Of KaplanMeierStatus), _GroupBEvents As IReadOnlyList(Of KaplanMeierStatus), _TotalFailingA As Integer, _TotalFailingB As Integer, _PValue As Double
    ''' <summary>
    ''' Group time event
    ''' </summary>
    Public Class KaplanMeierStatus
        Public Property Time As Integer

        Public Property NumberAtRisk As Integer

        Public Property NumberFailing As Integer

        Public Property SurvivalProbability As Double
    End Class

    Private Class JoinedEvent
        Public Property Time As Integer
        Public Property AtRiskA As Integer
        Public Property AtRiskB As Integer
        Public Property FailingA As Integer
        Public Property FailingB As Integer
    End Class

    Private ReadOnly m_groupA As IEnumerable(Of Patient)
    Private ReadOnly m_groupB As IEnumerable(Of Patient)

    Private Property MergedEvents As List(Of JoinedEvent) = New List(Of JoinedEvent)()

    Public Property GroupAEvents As IReadOnlyList(Of KaplanMeierStatus)
        Get
            Return _GroupAEvents
        End Get
        Private Set(ByVal value As IReadOnlyList(Of KaplanMeierStatus))
            _GroupAEvents = value
        End Set
    End Property

    Public Property GroupBEvents As IReadOnlyList(Of KaplanMeierStatus)
        Get
            Return _GroupBEvents
        End Get
        Private Set(ByVal value As IReadOnlyList(Of KaplanMeierStatus))
            _GroupBEvents = value
        End Set
    End Property

    Public Property TotalFailingA As Integer
        Get
            Return _TotalFailingA
        End Get
        Private Set(ByVal value As Integer)
            _TotalFailingA = value
        End Set
    End Property

    Public Property TotalFailingB As Integer
        Get
            Return _TotalFailingB
        End Get
        Private Set(ByVal value As Integer)
            _TotalFailingB = value
        End Set
    End Property

    Public Property PValue As Double
        Get
            Return _PValue
        End Get
        Private Set(ByVal value As Double)
            _PValue = value
        End Set
    End Property

    Public Sub New(ByVal groupA As IEnumerable(Of Patient), ByVal groupB As IEnumerable(Of Patient))
        If groupA Is Nothing OrElse groupB Is Nothing Then
            Throw New ArgumentNullException(If(groupA Is Nothing, "groupA", "groupB"))
        End If

        m_groupA = groupA
        m_groupB = groupB
    End Sub

    ''' <summary>
    ''' Performs the algorithm
    ''' </summary>
    Public Sub RunEstimate()
        GroupAEvents = RunGroup(m_groupA)
        GroupBEvents = RunGroup(m_groupB)

        TotalFailingA = GroupAEvents.Sum(Function(e) e.NumberFailing)
        TotalFailingB = GroupBEvents.Sum(Function(e) e.NumberFailing)

        MergeEvents()
        ComputePValue()
    End Sub

    ''' <summary>
    ''' Converts the given <paramrefname="patients"/> to a list of KM events holding the failure and censoring events as well as computing the
    ''' survival probability for each point in time
    ''' </summary>
    ''' <paramname="patients">The patient collection to convert</param>
    ''' <returns></returns>
    Private Shared Function RunGroup(ByVal patients As IEnumerable(Of Patient)) As IReadOnlyList(Of KaplanMeierStatus)
        Dim retVal As List(Of KaplanMeierStatus) = New List(Of KaplanMeierStatus)()
        Dim atRisk As Integer = patients.Count()
        Dim prevSurvivalProbability = 1.0

        Dim orderedGroup = patients.GroupBy(Function(x) x.CensorEventTime).OrderBy(Function(p) p.Key)
        For Each patientGroup In orderedGroup
            Dim died = patientGroup.Count(Function(patient) patient.CensorEvent = EventFreeSurvival.Death)
            Dim survivalProbability = prevSurvivalProbability * (1 - died / atRisk)

            retVal.Add(New KaplanMeierStatus With {
.Time = patientGroup.Key,
.NumberAtRisk = atRisk,
.NumberFailing = died,
.SurvivalProbability = survivalProbability
})

            prevSurvivalProbability = survivalProbability
            atRisk -= patientGroup.Count()
        Next

        Return retVal
    End Function

    ''' <summary>
    ''' Joins the two groups events into a single collection to allow further processing
    ''' </summary>
    Private Sub MergeEvents()
        Dim iA = 0, iB = 0
        While iA < GroupAEvents.Count AndAlso iB < GroupBEvents.Count
            Dim currentTime = stdNum.Min(GroupAEvents(iA).Time, GroupBEvents(iB).Time)
            Dim failingA = If(GroupAEvents(iA).Time = currentTime, GroupAEvents(iA).NumberFailing, 0)
            Dim failingB = If(GroupBEvents(iB).Time = currentTime, GroupBEvents(iB).NumberFailing, 0)

            If failingA + failingB <> 0 Then
                MergedEvents.Add(New JoinedEvent With {
.Time = currentTime,
.AtRiskA = GroupAEvents(iA).NumberAtRisk,
.AtRiskB = GroupBEvents(iB).NumberAtRisk,
.FailingA = failingA,
.FailingB = failingB
})
            End If

            If GroupAEvents(iA).Time = currentTime Then iA += 1

            If GroupBEvents(iB).Time = currentTime Then iB += 1
        End While

        Dim aAtRisk = GroupAEvents(GroupAEvents.Count - 1).NumberAtRisk - GroupAEvents(GroupAEvents.Count - 1).NumberFailing
        Dim bAtRisk = GroupBEvents(GroupBEvents.Count - 1).NumberAtRisk - GroupBEvents(GroupBEvents.Count - 1).NumberFailing

        While iA < GroupAEvents.Count
            Dim currentTime = GroupAEvents(iA).Time
            MergedEvents.Add(New JoinedEvent With {
.Time = currentTime,
.AtRiskA = GroupAEvents(iA).NumberAtRisk,
.AtRiskB = bAtRisk,
.FailingA = GroupAEvents(iA).NumberFailing,
.FailingB = 0
})

            iA += 1
        End While

        While iB < GroupBEvents.Count
            Dim currentTime = GroupBEvents(iB).Time
            MergedEvents.Add(New JoinedEvent With {
.Time = currentTime,
.AtRiskA = aAtRisk,
.AtRiskB = GroupBEvents(iB).NumberAtRisk,
.FailingA = 0,
.FailingB = GroupBEvents(iB).NumberFailing
})

            iB += 1
        End While
    End Sub

    ''' <summary>
    ''' Computes the statistical significance of the observation
    ''' </summary>
    Private Sub ComputePValue()
        Dim sumEA As Double = 0, sumEB As Double = 0

        ' Compute the expected number of failures for each group, should have they been taken together
        For Each kmEvent In MergedEvents.OrderBy(Function(x) x.Time)
            Dim totalFailing As Double = kmEvent.FailingA + kmEvent.FailingB
            Dim totalAtRisk As Double = kmEvent.AtRiskA + kmEvent.AtRiskB

            Dim eA = totalFailing / totalAtRisk * kmEvent.AtRiskA
            Dim eB = totalFailing / totalAtRisk * kmEvent.AtRiskB

            sumEA += eA
            sumEB += eB
        Next

        Debug.Assert(Not sumEA = 0 OrElse sumEB = 0) ' (sumEA == 0) ==> (sumEB == 0)

        Dim statistic As Double = 0
        If sumEA <> 0 AndAlso sumEB <> 0 Then
            ' The test statistic is the deviation from the expected for both groups
            statistic = stdNum.Pow(TotalFailingA - sumEA, 2) / sumEA + stdNum.Pow(TotalFailingB - sumEB, 2) / sumEB
        End If

        ' The PValue is computed using the Chi-Square statistic, with degrees of freedom =1
        ' original expression at here: ChiSquared.CDF(1, statistic)
        PValue = 1 - Distribution.ChiSquare(statistic, freedom:=1)
    End Sub
End Class
