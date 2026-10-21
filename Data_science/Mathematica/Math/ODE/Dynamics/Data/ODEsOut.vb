#Region "Microsoft.VisualBasic::5c6c5e5250ea09d390ae7e3bd6d28bf3, Data_science\Mathematica\Math\ODE\Dynamics\Data\ODEsOut.vb"

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

    '   Total Lines: 107
    '    Code Lines: 65 (60.75%)
    ' Comment Lines: 27 (25.23%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 15 (14.02%)
    '     File Size: 3.78 KB


    '     Class ODEsOut
    ' 
    '         Properties: dx, HaveNaN, params, Resolution, x
    '                     y, y0
    ' 
    '         Function: GetEnumerator, GetY0, IEnumerable_GetEnumerator, Join, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Data
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Scripting.Runtime
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace Dynamics.Data

    ''' <summary>
    ''' ODEs output, this object can populates the <see cref="ODEsOut.y"/> 
    ''' variables values through its enumerator interface.
    ''' </summary>
    Public Class ODEsOut : Implements Enumeration(Of NamedCollection(Of Double))

        Public Property x As Double()
        Public Property y As Dictionary(Of NamedCollection(Of Double))

        ''' <summary>
        ''' 方程组的初始值，积分结果会受到初始值的极大的影响
        ''' </summary>
        ''' <returns></returns>
        Public Property y0 As Dictionary(Of String, Double)
        Public Property params As Dictionary(Of String, Double)

        ''' <summary>
        ''' 得到进行积分的步进值，采样点不足的时候返回零
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property dx As Double
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                If x Is Nothing OrElse x.Length < 2 Then
                    Return 0.0
                Else
                    Return x(1) - x(0)
                End If
            End Get
        End Property

        ''' <summary>
        ''' 获取得到积分计算的分辨率的数值，采样点不足的时候返回零
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Resolution As Double
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Dim dh As Double = dx

                If x Is Nothing OrElse x.Length < 2 OrElse dh = 0.0 Then
                    Return 0.0
                Else
                    Return (x(x.Length - 1) - x(0)) / dh
                End If
            End Get
        End Property

        ''' <summary>
        ''' Using the first value of <see cref="y"/> as ``y0``
        ''' </summary>
        ''' <returns></returns>
        Public Function GetY0() As Dictionary(Of String, Double)
            Return y.ToDictionary(Function(x) x.Key,
                                  Function(x)
                                      Return x.Value(Scan0)
                                  End Function)
        End Function

        ''' <summary>
        ''' Is there NAN value in the function value <see cref="y"/> ???
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property HaveNaN As Boolean
            Get
                For Each val As NamedCollection(Of Double) In y.Values
                    For Each x As Double In val.value
                        If x.IsNaNImaginary Then
                            Return True
                        End If
                    Next
                Next

                Return False
            End Get
        End Property

        ''' <summary>
        ''' Merge <see cref="y0"/> into <see cref="params"/>
        ''' </summary>
        Public Function Join() As ODEsOut
            Dim params As New Dictionary(Of String, Double)(Me.params)

            For Each v In y0
                Call params.Add(v.Key, v.Value)
            Next

            Return New ODEsOut With {
                .params = params,
                .x = x,
                .y = y,
                .y0 = y0
            }
        End Function

        ''' <summary>
        ''' Cast the integration result into a <see cref="NumericTable"/>: each row is a
        ''' sampling time point and each feature column is a state variable.
        ''' (将积分结果转换为数值表：每一行是一个时间采样点，每一列是一个状态变量)
        ''' </summary>
        ''' <returns></returns>
        Public Function AsTable() As NumericTable
            Dim columns As NamedCollection(Of Double)() = y.Values.ToArray
            Dim tbl As New NumericTable(
                columns.ToList(Function(v) v.name),
                columns.Select(Function(v) v.value),
                x.ToList(Function(xi) CStr(xi))) With {
                .description = "ODE output data",
                .name = "ODEs Output Table"
            }

            Return tbl
        End Function

        Public Overrides Function ToString() As String
            Return Me.GetJson
        End Function

        Public Iterator Function GenericEnumerator() As IEnumerator(Of NamedCollection(Of Double)) Implements Enumeration(Of NamedCollection(Of Double)).GenericEnumerator
            For Each var As NamedCollection(Of Double) In y.Values
                Yield var
            Next
        End Function
    End Class
End Namespace
