#Region "Microsoft.VisualBasic::e4c66540bb4c757815e0625777a6e6d2, Data_science\Mathematica\Math\Math.Statistics\MomentFunctions\BasicProductMoments.vb"

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

    '   Total Lines: 168
    '    Code Lines: 85
    ' Comment Lines: 61
    '   Blank Lines: 22
    '     File Size: 7.17 KB


    '     Class BasicProductMoments
    ' 
    '         Properties: IsConverged, Max, Min, SampleVariance, StDev
    ' 
    '         Constructor: (+2 Overloads) Sub New
    '         Sub: AddObservation, AddObservations, SetConvergenceTolerance, SetMinValuesBeforeConvergenceTest, SetZAlphaForConvergence
    '              TestForConvergence
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports stdNum = System.Math

'
' * To change this license header, choose License Headers in Project Properties.
' * To change this template file, choose Tools | Templates
' * and open the template in the editor.
' 
Namespace MomentFunctions

    ''' <summary>
    ''' 可以利用这个模块来进行一组数据的正态分布的参数的估计
    ''' 
    ''' @author Will_and_Sara
    ''' </summary>
    Public Class BasicProductMoments : Inherits DataSample(Of Double)

        ''' <summary>
        ''' 样本的变异程度
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property SampleVariance As Double

        Dim _ZAlphaForConvergence As Double = 1.96039491692453
        Dim _ToleranceForConvergence As Double = 0.01
        Dim _MinValuesBeforeConvergenceTest As Integer = 100

        ''' <summary>
        ''' 标准差
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property StDev() As Double
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return stdNum.Sqrt(_SampleVariance)
            End Get
        End Property

        Public Overrides ReadOnly Property Min As Double
        Public Overrides ReadOnly Property Max As Double

        ''' <summary>
        ''' This function can be used to determine if enough samples have been added to determine convergence 
        ''' of the data stream. 
        ''' </summary>
        ''' <returns> 
        ''' this function will return false until the minimum number of observations have been added, and then 
        ''' will return the result of the convergence test after the most recent observation. 
        ''' </returns>
        Public Overridable ReadOnly Property IsConverged() As Boolean = False

        ''' <summary>
        ''' This method allows the user to define a minimum number of observations before testing for convergence.
        ''' This would help to mitigate early convergence if similar observations are in close sequence early in the dataset. </summary>
        ''' <param name="numobservations"> the minimum number of observations to wait until testing for convergence. </param>
        Public Overridable Sub SetMinValuesBeforeConvergenceTest(numobservations As Integer)
            _MinValuesBeforeConvergenceTest = numobservations
        End Sub

        ''' <summary>
        ''' This method sets the tolerance for convergence.  This tolerance is used as an epsilon neighborhood around the confidence defined in SetZalphaForConvergence. </summary>
        ''' <param name="tolerance"> the distance that is determined to be close enough to the alpha in question. </param>
        Public Overridable Sub SetConvergenceTolerance(tolerance As Double)
            _ToleranceForConvergence = tolerance
        End Sub

        ''' <summary>
        ''' This method defines the alpha value used to determine convergence.  
        ''' This value is based on a two sided confidence interval.  
        ''' It uses the upper Confidence Limit.
        ''' </summary>
        ''' <param name="ConfidenceInterval"> 
        ''' The value that would be used to determine the normal alpha value.  
        ''' The default is a .9 Confidence interval, which corresponds to 1.96 alpha value. 
        ''' </param>
        Public Overridable Sub SetZAlphaForConvergence(ConfidenceInterval As Double)
            Dim sn As New Distributions.MethodOfMoments.Normal
            _ZAlphaForConvergence = sn.GetInvCDF(ConfidenceInterval + ((1 - ConfidenceInterval) / 2))
        End Sub

        ''' <summary>
        ''' This constructor allows one to create an instance without adding any data.
        ''' </summary>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub New()
            Call Me.New({})
        End Sub

        ''' <summary>
        ''' This constructor allows one to create an instance with some initial data, 
        ''' observations can be added after the constructor through the 
        ''' "AddObservations(double observation) call. 
        ''' </summary>
        ''' <param name="data"> the dataset to calculate mean and standard deviation for. </param>
        Public Sub New(data As IEnumerable(Of Double))
            buffer = data.AsList

            If buffer = 0 Then
                means = 0
                Min = 0
                Max = 0
                SampleVariance = 0
            Else
                means = buffer.Average
                Min = buffer.Min
                Max = buffer.Max
                SampleVariance = Aggregate n As Double
                                 In buffer
                                 Let d = (n - means) ^ 2
                                 Into Sum(d)
                SampleVariance /= buffer.Count

                Call TestForConvergence()
            End If
        End Sub

        ''' <summary>
        ''' An inline algorithm for incrementing mean and standard of deviation. 
        ''' After this method call, the properties of this class should be 
        ''' updated to include this observation.
        ''' </summary>
        ''' <param name="observation"> the observation to be added </param>
        Public Overridable Sub AddObservation(observation As Double)
            Dim count = buffer.Count

            If count = 0 Then
                _Min = observation
                _Max = observation
                means = observation
            Else
                ' single pass algorithm. 
                If observation > _Max Then _Max = observation
                If observation < _Min Then _Min = observation

                ' check for integer rounding issues.
                Dim newmean As Double = Mean + ((observation - Mean) / count)
                ' 2018-1-15 当元素只有1个的时候，在下面的计算表达式之中count-1是等于零的，会导致_SampleVariance的结果值为NaN
                Dim N21 = (CDbl(count - 2) / CDbl(count - 1)) * _SampleVariance

                If SampleVariance = 0R OrElse count = 2 Then
                    N21 = 0
                End If

                _SampleVariance = N21 + (stdNum.Pow(observation - Mean, 2.0)) / count
                means = newmean
            End If

            Call buffer.Add(observation)
            Call TestForConvergence()
        End Sub

        Public Overridable Sub AddObservations(data As Double())
            For Each d As Double In data
                Call AddObservation(d)
            Next
        End Sub

        Private Sub TestForConvergence()
            If Count > _MinValuesBeforeConvergenceTest Then
                If Not IsConverged Then
                    Dim var As Double = (_ZAlphaForConvergence * Me.StDev()) / (Me.Mean() * stdNum.Abs(Me.StDev()))
                    _IsConverged = (stdNum.Abs(var) <= _ToleranceForConvergence)
                End If
            End If
        End Sub
    End Class
End Namespace
