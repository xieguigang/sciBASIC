#Region "Microsoft.VisualBasic::879e63ac0e7ff0f8b60489dab4dd3fea, ..\sciBASIC#\Data_science\Mathematical\Plots\g\Axis\AxisScalling.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Ranges
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace Graphic.Axis

    Public Module AxisScalling

        <Extension>
        Public Function GetAxisByTick(range As DoubleRange, tick#) As Double()
            Dim list As New List(Of Double)
            Dim pos# = range.Min

            Do While pos < range.Max
                pos += tick
                list += pos
            Loop

            If list.Last > range.Max Then
                list.RemoveLast
            End If

            Return list
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="range"></param>
        ''' <param name="absoluteScale">range的最大值和最小值在这个参数为True的时候不会自动浮动，假若为False则会根据情况进行相对的浮动</param>
        ''' <param name="n%"></param>
        ''' <returns></returns>
        <Extension>
        Public Function GetAxisValues(range As DoubleRange, Optional absoluteScale As Boolean = False, Optional n% = 10) As Double()
            Dim tick# = New AxisAssists(range.Length, mostticks:=n).Tick
            Return range.GetAxisByTick(tick)
        End Function
    End Module

    ''' <summary>
    ''' https://stackoverflow.com/questions/361681/algorithm-for-nice-grid-line-intervals-on-a-graph
    ''' </summary>
    Public Class AxisAssists

        Public Property Tick() As Double

        Public Sub New(aTick As Double)
            Tick = aTick
        End Sub

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="range">最大值减去最小值</param>
        ''' <param name="mostticks">Tick的个数</param>
        Public Sub New(range#, Optional mostticks% = 10)
            Dim minimum = range / mostticks
            Dim magnitude = Math.Pow(10.0, (Math.Floor(Math.Log(minimum) / Math.Log(10))))
            Dim residual = minimum / magnitude

            If residual > 5 Then
                Tick = 10 * magnitude
            ElseIf residual > 2 Then
                Tick = 5 * magnitude
            ElseIf residual > 1 Then
                Tick = 2 * magnitude
            Else
                Tick = magnitude
            End If
        End Sub

        Public Function GetClosestTickBelow(v As Double) As Double
            Return Tick * Math.Floor(v / Tick)
        End Function

        Public Function GetClosestTickAbove(v As Double) As Double
            Return Tick * Math.Ceiling(v / Tick)
        End Function

        Public Overrides Function ToString() As String
            Return Tick
        End Function
    End Class
End Namespace
