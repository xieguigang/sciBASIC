#Region "Microsoft.VisualBasic::6988b6c8b9cd499bf82c3af88999f289, sciBASIC#\Data_science\DataMining\DataMining\ComponentModel\Normalizer\Normalizations.vb"

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

    '   Total Lines: 81
    '    Code Lines: 62
    ' Comment Lines: 7
    '   Blank Lines: 12
    '     File Size: 2.85 KB


    '     Module Normalizations
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: ParseMethod, RangeDiscretizer, RelativeNormalize, ScalerNormalize
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.Ranges.Model
Imports Microsoft.VisualBasic.DataMining.ComponentModel.Discretion
Imports Microsoft.VisualBasic.Math.Distributions
Imports stdNum = System.Math

Namespace ComponentModel.Normalizer

    Public Module Normalizations

        ReadOnly methodTable As Dictionary(Of String, Methods)

        Sub New()
            methodTable = Enums(Of Methods).ToDictionary(Function(name) name.ToString.ToLower)
        End Sub

        Public Function ParseMethod(name As String) As Methods
            Return methodTable.TryGetValue(Strings.LCase(name), [default]:=Methods.NormalScaler)
        End Function

        ReadOnly normalRange As DoubleRange = {0, 1}

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function ScalerNormalize(samples As SampleDistribution, x#) As Double
            If x > samples.max Then
                Return 1
            ElseIf x < samples.min Then
                Return 0
            Else
                x = samples.GetRange.ScaleMapping(x, normalRange)
            End If

            If x.IsNaNImaginary Then
                Return samples.average
            Else
                Return x
            End If
        End Function

        ''' <summary>
        ''' 正实数和负实数是分开进行归一化的
        ''' </summary>
        ''' <param name="samples"></param>
        ''' <param name="x#"></param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function RelativeNormalize(samples As SampleDistribution, x#) As Double
            If x > 0 Then
                If x > samples.max Then
                    Return 1
                Else
                    Return x / samples.max
                End If
            ElseIf x = 0R Then
                Return 0
            Else
                ' 负实数需要考察一下
                If x < samples.min Then
                    Return -1
                ElseIf samples.min >= 0 Then
                    Return -1
                Else
                    Return x / stdNum.Abs(samples.min)
                End If
            End If
        End Function

        Public Function RangeDiscretizer(samples As SampleDistribution, x#) As Double
            Static cache As New Dictionary(Of SampleDistribution, NormalRangeDiscretizer)

            Return cache.ComputeIfAbsent(
                key:=samples,
                lazyValue:=Function(dist)
                               Return New NormalRangeDiscretizer(dist)
                           End Function) _
               .Normalize(x)
        End Function

    End Module
End Namespace
