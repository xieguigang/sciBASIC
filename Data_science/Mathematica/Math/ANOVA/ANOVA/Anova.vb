#Region "Microsoft.VisualBasic::423c757aab61c6fcfa00491d4aaa90c6, Data_science\Mathematica\Math\ANOVA\ANOVA\Anova.vb"

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

    '   Total Lines: 205
    '    Code Lines: 128
    ' Comment Lines: 38
    '   Blank Lines: 39
    '     File Size: 6.43 KB


    ' Class AnovaTest
    ' 
    '     Properties: criticalNumber, denomenator, numenator, type
    ' 
    '     Function: fScore_determineIt, ToString
    ' 
    '     Sub: divide_by_degrees_of_freedom, findWithinGroupMeans, populate, setSumOfSquaresOfGroups, setTotalSumOfSquares
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Math.Statistics.Distributions.LinearMoments
Imports stdNum = System.Math

Public Class AnovaTest

    Public SSB As Double
    Public SSW As Double
    Public F_score As Double
    Public singlePvalue As Double
    Public doublePvalue As Double

    Public Const P_FIVE_PERCENT As String = "p<.05"
    Public Const P_ONE_PERCENT As String = "p<.01"

    Public SSW_sum_of_squares_within_groups As Double
    Public SSB_sum_of_squares_between_groups As Double
    Public SS_total_sum_of_squares As Double
    Public allObservationsMean As Double
    Public groups As New List(Of Group)()

    Private numenator_degrees_of_freedom As Integer = -1
    Private denomenator_degrees_of_freedom As Integer = -1

    Dim m_type As String = "this will be a constant to chose 1 or 5 %"

    ''' <summary>
    ''' numenator_degrees_of_freedom(Groups degrees of freedom)
    ''' </summary>
    ''' <returns></returns>
    Public Overridable ReadOnly Property numenator As Integer
        Get
            ' return groups.size() -1...
            ' for use with the Distribution table
            Return numenator_degrees_of_freedom
        End Get
    End Property

    ''' <summary>
    ''' denomenator_degrees_of_freedom(Observations degrees of freedom)
    ''' </summary>
    ''' <returns></returns>
    Public Overridable ReadOnly Property denomenator As Integer
        Get
            ' return number of all observations - groups.size()... 
            ' for use with the Distribution table
            Return denomenator_degrees_of_freedom
        End Get
    End Property

    ''' <summary>
    ''' return whether this is a 1% or a 5% test
    ''' </summary>
    ''' <returns></returns>
    Public Overridable ReadOnly Property type As String
        Get
            Return m_type
        End Get
    End Property

    Public Overridable ReadOnly Property criticalNumber As Double
        Get
            Dim critical = DistributionTable.getCriticalNumber(denomenator, numenator, type)
            Return critical
        End Get
    End Property

    ''' <summary>
    ''' step7
    ''' </summary>
    ''' <returns></returns>
    Public Overridable Function fScore_determineIt() As Double
        F_score = SSB / SSW
        singlePvalue = New FDistribution(numenator_degrees_of_freedom, denomenator_degrees_of_freedom).GetCDF(F_score)
        doublePvalue = singlePvalue * 2

        Return F_score
    End Function

    ''' <summary>
    ''' step6
    ''' </summary>
    Public Overridable Sub divide_by_degrees_of_freedom()
        Dim observations = 0

        numenator_degrees_of_freedom = groups.Count - 1
        SSB = SSB_sum_of_squares_between_groups / numenator_degrees_of_freedom

        For Each g In groups
            observations += g.ary.Length
        Next

        'degrees_of_freedom = observations - groups.size(); 
        denomenator_degrees_of_freedom = observations - groups.Count
        SSW = SSW_sum_of_squares_within_groups / denomenator_degrees_of_freedom
    End Sub

    ''' <summary>
    ''' step1
    ''' </summary>
    ''' <param name="matrix"></param>
    ''' <param name="type"></param>
    Public Overridable Sub populate(matrix As IEnumerable(Of Double()), type As String)
        m_type = type

        For Each v As Double() In matrix
            Call groups.Add(New Group(v))
        Next
    End Sub

    ''' <summary>
    ''' step2
    ''' </summary>
    Public Overridable Sub findWithinGroupMeans()
        Dim total As Double = 0
        Dim observationsCount = 0

        For i As Integer = 0 To groups.Count - 1
            Dim g = groups(i)
            Dim groupTotal As Double = 0

            For j As Integer = 0 To g.ary.Length - 1
                groupTotal += g.ary(j)
            Next

            total += groupTotal
            observationsCount += g.ary.Length
            g.mean = groupTotal / g.ary.Length
        Next

        allObservationsMean = total / observationsCount
    End Sub

    ''' <summary>
    ''' step3
    ''' </summary>
    Public Overridable Sub setSumOfSquaresOfGroups()
        For i As Integer = 0 To groups.Count - 1
            Dim g As Group = groups(i)

            For j As Integer = 0 To g.ary.Length - 1
                Dim observation = g.ary(j)
                Dim result = observation - g.mean
                Dim answer = stdNum.Pow(result, 2)

                g.raisedSum += answer
            Next

            SSW_sum_of_squares_within_groups += g.raisedSum
        Next
    End Sub

    ''' <summary>
    ''' step4
    ''' </summary>
    Public Overridable Sub setTotalSumOfSquares()
        SS_total_sum_of_squares = 0

        For i As Integer = 0 To groups.Count - 1
            Dim g As Group = groups(i)

            For j As Integer = 0 To g.ary.Length - 1
                Dim observation = g.ary(j)
                Dim result = observation - allObservationsMean

                SS_total_sum_of_squares += stdNum.Pow(result, 2)
            Next
        Next

        SSB_sum_of_squares_between_groups = SS_total_sum_of_squares - SSW_sum_of_squares_within_groups
    End Sub

    Public Overrides Function ToString() As String
        Dim f_score As Double = fScore_determineIt()
        Dim criticalNumber = Me.criticalNumber
        Dim result As String = "The null hypothesis is supported! There is no especial difference in these groups. "

        If f_score > criticalNumber Then
            result = "The null hypothesis is rejected! These groups are different."
        End If

        Return $"
Call:
   aov(formula = v ~ group, data = observations)

Terms:
                group Residuals
Sum of Squares     {SSB_sum_of_squares_between_groups.ToString("F1")}        {SSW_sum_of_squares_within_groups.ToString("F1")}
Deg. of Freedom     {numenator}        {denomenator}

Residual standard error: {stdNum.Sqrt(SSW)}
SSB: {SSB}
SSW: {SSW}
SS_total_sum_of_squares: {SS_total_sum_of_squares}
Estimated effects may be unbalanced
Probability level: {type}
allObservationsMean: {allObservationsMean}
Critical number: {criticalNumber}

*** F Score:  {f_score}
*** P-Value:  {singlePvalue.ToString("G3")}  [double_tailed: {doublePvalue.ToString("G3")}]

{result}
"
    End Function
End Class
