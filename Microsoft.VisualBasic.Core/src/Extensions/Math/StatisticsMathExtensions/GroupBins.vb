#Region "Microsoft.VisualBasic::50198cbfa03528281f782fbbe2de206d, Microsoft.VisualBasic.Core\src\Extensions\Math\StatisticsMathExtensions\GroupBins.vb"

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

    '   Total Lines: 66
    '    Code Lines: 48 (72.73%)
    ' Comment Lines: 7 (10.61%)
    '    - Xml Docs: 71.43%
    ' 
    '   Blank Lines: 11 (16.67%)
    '     File Size: 2.47 KB


    '     Class GroupBins
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: GroupBy
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.ComponentModel.DataStructures
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq

Namespace Math.Statistics

    Public Class GroupBins(Of T)

        ReadOnly evaluate As Func(Of T, Double)
        ReadOnly assert_equals As GenericLambda(Of Double).IEquals
        ReadOnly left_margin_bin As Boolean

        Sub New(evaluate As Func(Of T, Double),
                assert_equals As GenericLambda(Of Double).IEquals,
                Optional left_margin_bin As Boolean = False)

            Me.left_margin_bin = left_margin_bin
            Me.evaluate = evaluate
            Me.assert_equals = assert_equals
        End Sub

        ''' <summary>
        ''' 将一维的数据按照一定的偏移量分组输出
        ''' </summary>
        ''' <param name="source"></param>
        ''' <returns></returns>
        Public Iterator Function GroupBy(source As IEnumerable(Of T)) As IEnumerable(Of NamedCollection(Of T))
            ' 先进行预处理：求值然后进行排序
            Dim tagValues = source _
                .Select(Function(o) (evaluate(o), o)) _
                .OrderBy(Function(o) o.Item1) _
                .ToArray
            Dim means As New Average
            Dim members As New List(Of T)
            Dim left As Double

            ' 根据分组的平均值来进行分组操作
            For Each x As (val#, o As T) In tagValues
                If means.N = 0 Then
                    means += x.Item1
                    members += x.Item2
                    left = x.Item1
                Else
                    If (left_margin_bin AndAlso assert_equals(left, x.Item1)) OrElse
                        assert_equals(means.Average, x.Item1) Then

                        means += x.Item1
                        members += x.Item2
                    Else
                        Yield New NamedCollection(Of T)(CStr(means.Average), members)

                        means = New Average({x.Item1})
                        members = New List(Of T) From {x.Item2}
                        left = x.Item1
                    End If
                End If
            Next

            If members > 0 Then
                Yield New NamedCollection(Of T)(CStr(means.Average), members)
            End If
        End Function

    End Class
End Namespace
