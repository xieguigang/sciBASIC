#Region "Microsoft.VisualBasic::d238ec20f267286831f51745634a07f3, Data_science\Mathematica\Math\Math\Quantile\DataQuartile.vb"

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

    '     Structure DataQuartile
    ' 
    '         Properties: IQR, ModelSamples, Q1, Q2, Q3
    '                     range
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: Outlier, ToString
    ' 
    '     Enum QuartileLevels
    ' 
    ' 
    '  
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Ranges.Model

Namespace Quantile

    ''' <summary>
    ''' A data quartile model based on a given sample data input
    ''' </summary>
    Public Structure DataQuartile

        ''' <summary>
        ''' 第一四分位数 (Q1)，又称“较小四分位数”，等于该样本中所有数值由小到大排列后第25%的数字。
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Q1 As Double
        ''' <summary>
        ''' 第二四分位数 (Q2)，又称“中位数”，等于该样本中所有数值由小到大排列后第50%的数字。
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Q2 As Double
        ''' <summary>
        ''' 第三四分位数 (Q3)，又称“较大四分位数”，等于该样本中所有数值由小到大排列后第75%的数字。
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Q3 As Double
        ''' <summary>
        ''' 第三四分位数与第一四分位数的差距又称四分位距（InterQuartile Range, IQR）。
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property IQR As Double
        ''' <summary>
        ''' 极值范围
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property range As DoubleRange

        ''' <summary>
        ''' The raw sample data input for create current quartile model
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property ModelSamples As (normal As Double(), outlier As Double())

        Friend Sub New(Q1#, Q2#, Q3#, IQR#, raw#())
            Me.Q1 = Q1
            Me.Q2 = Q2
            Me.Q3 = Q3
            Me.IQR = IQR
            Me.range = raw
            Me.ModelSamples = Me.Outlier(raw)
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Outlier(samples As IEnumerable(Of Double)) As (normal As Double(), outlier As Double())
            Return samples.AsVector.Outlier(Me)
        End Function

        Public Overrides Function ToString() As String
            Return $"{range.ToString} -> |{Q1}, {Q2}, {Q3}|"
        End Function
    End Structure

    Public Enum QuartileLevels As Integer
        Q1 = 1
        Q2 = 2
        Q3 = 3
    End Enum
End Namespace
