#Region "Microsoft.VisualBasic::2e3d93b97f0a161887473ba73086a1ed, Microsoft.VisualBasic.Core\src\ComponentModel\Algorithm\base\Combination\NDimensionCartesianProduct.vb"

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

    '   Total Lines: 53
    '    Code Lines: 35 (66.04%)
    ' Comment Lines: 7 (13.21%)
    '    - Xml Docs: 85.71%
    ' 
    '   Blank Lines: 11 (20.75%)
    '     File Size: 1.81 KB


    '     Class NDimensionCartesianProduct
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: CreateMultiCartesianProduct
    '         Class InternalLambda
    ' 
    '             Function: helper
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace ComponentModel.Algorithm.base

    ''' <summary>
    ''' Create a vs b vs c ...
    ''' </summary>
    Public NotInheritable Class NDimensionCartesianProduct

        Private Sub New()
        End Sub

        Private Class InternalLambda(Of T)

            Public sequences As T()()

            ' 递归辅助函数
            Public Function helper(depth As Integer, currentCombination As List(Of T)) As IEnumerable(Of T())
                If depth = sequences.Length Then
                    Return {currentCombination.ToArray()}
                Else
                    Dim result As New List(Of T())()

                    For Each item As T In sequences(depth)
                        Dim newCombination As New List(Of T)(currentCombination)

                        Call newCombination.Add(item)
                        Call result.AddRange(helper(depth + 1, newCombination))
                    Next

                    Return result
                End If
            End Function
        End Class

        ''' <summary>
        ''' 生成任意数量集合的笛卡尔积（递归实现）
        ''' </summary>
        Public Shared Iterator Function CreateMultiCartesianProduct(Of T)(ParamArray sequences As IEnumerable(Of T)()) As IEnumerable(Of T())
            If sequences Is Nothing OrElse sequences.Length = 0 Then
                Return
            End If

            Dim recursive As New InternalLambda(Of T) With {
                .sequences = sequences _
                    .Select(Function(a) a.ToArray) _
                    .ToArray
            }

            For Each combo As T() In recursive.helper(0, New List(Of T)())
                Yield combo
            Next
        End Function
    End Class
End Namespace
