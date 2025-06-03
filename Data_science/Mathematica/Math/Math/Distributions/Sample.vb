#Region "Microsoft.VisualBasic::94b1aa1e631b9ed5c9794d9331f1f7fe, Data_science\Mathematica\Math\Math\Distributions\Sample.vb"

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

    '   Total Lines: 157
    '    Code Lines: 99 (63.06%)
    ' Comment Lines: 37 (23.57%)
    '    - Xml Docs: 94.59%
    ' 
    '   Blank Lines: 21 (13.38%)
    '     File Size: 5.18 KB


    '     Class SampleDistribution
    ' 
    '         Properties: average, CI95Range, max, min, mode
    '                     outlierBoundary, quantile, size, stdErr
    ' 
    '         Constructor: (+3 Overloads) Sub New
    '         Function: EvaluateMode, GetRange, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.ComponentModel.Ranges.Model
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math.Quantile
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace Distributions

    ''' <summary>
    ''' The data sample xml model
    ''' </summary>
    ''' <remarks>
    ''' summary of the sample data vector
    ''' </remarks>
    Public Class SampleDistribution

        <XmlAttribute> Public Property min As Double
        <XmlAttribute> Public Property max As Double
        <XmlAttribute> Public Property average As Double

        ''' <summary>
        ''' standard deviation of the population
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks>
        ''' evaluated from the <see cref="SD"/> function.
        ''' </remarks>
        <XmlAttribute> Public Property stdErr As Double

        ''' <summary>
        ''' length of the raw data vector
        ''' </summary>
        ''' <returns></returns>
        <XmlAttribute> Public Property size As Integer

        ''' <summary>
        ''' 分别为0%, 25%, 50%, 75%, 100%
        ''' </summary>
        ''' <returns></returns>
        <XmlAttribute> Public Property quantile As Double()

        <XmlAttribute> Public Property mode As Double

        Public ReadOnly Property CI95Range As Double()
            Get
                Return {
                    average - 1.96 * stdErr,
                    average + 1.96 * stdErr
                }
            End Get
        End Property

        Public ReadOnly Property outlierBoundary As Double()
            Get
                Dim Q1 = quantile(1)
                Dim Q3 = quantile(3)
                Dim IQR = Q3 - Q1

                Return {
                    Q1 - 1.5 * IQR,
                    Q3 + 1.5 * IQR
                }
            End Get
        End Property

        Sub New()
        End Sub

        ''' <summary>
        ''' Construct a feature data based on a specific dataframe column data
        ''' </summary>
        ''' <param name="data">the raw data matrix column data</param>
        ''' <param name="estimateQuantile"></param>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Sub New(data As IEnumerable(Of Double), Optional estimateQuantile As Boolean = True)
            Call Me.New(data.SafeQuery.ToArray, estimateQuantile)
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Sub New(data As IEnumerable(Of Single), Optional estimateQuantile As Boolean = True)
            Call Me.New(data.SafeQuery.Select(Function(f) CDbl(f)).ToArray, estimateQuantile)
        End Sub

        ''' <summary>
        ''' Construct a feature data based on a specific dataframe column data
        ''' </summary>
        ''' <param name="v">the raw data matrix column data</param>
        ''' <param name="estimateQuantile"></param>
        Sub New(v As Double(), Optional estimateQuantile As Boolean = True)
            If v.Length = 0 Then
                min = Double.NaN
                max = Double.NaN
                average = Double.NaN
                stdErr = Double.NaN
                size = 0
            Else
                min = v.Min
                max = v.Max
                average = v.Average
                stdErr = v.SD
                size = v.Length
                mode = EvaluateMode(v.OrderBy(Function(d) d).ToArray)
            End If

            If estimateQuantile AndAlso v.Length > 0 Then
                With v.GKQuantile
                    quantile = {
                        .Query(0),
                        .Query(0.25),
                        .Query(0.5),
                        .Query(0.75),
                        .Query(1)
                    }
                End With
            End If
        End Sub

        Public Shared Function EvaluateMode(data As Double()) As Double
            Dim modeValue As Double = Double.NaN
            Dim modeCount As Integer = 0
            Dim currValue = data(0)
            Dim currCount = 1

            ' Count the amount of repeat And update mode variables
            For i As Integer = 1 To data.Length - 1
                If data(i) = currValue Then
                    currCount += 1
                Else
                    If (currCount >= modeCount) Then
                        modeCount = currCount
                        modeValue = currValue
                    End If

                    currValue = data(i)
                    currCount = 1
                End If
            Next

            ' Check the last count
            If (currCount >= modeCount) Then
                modeCount = currCount
                modeValue = currValue
            End If

            Return modeValue
        End Function

        ''' <summary>
        ''' <see cref="DoubleRange"/> = ``[<see cref="min"/>, <see cref="max"/>]``
        ''' </summary>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function GetRange() As DoubleRange
            Return {min, max}
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function ToString() As String
            Return GetJson
        End Function
    End Class
End Namespace
