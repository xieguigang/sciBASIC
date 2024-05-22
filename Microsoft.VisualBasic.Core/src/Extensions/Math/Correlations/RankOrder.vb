#Region "Microsoft.VisualBasic::a0249f66af05e3450120379159202465, Microsoft.VisualBasic.Core\src\Extensions\Math\Correlations\RankOrder.vb"

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

    '   Total Lines: 68
    '    Code Lines: 38 (55.88%)
    ' Comment Lines: 17 (25.00%)
    '    - Xml Docs: 94.12%
    ' 
    '   Blank Lines: 13 (19.12%)
    '     File Size: 2.15 KB


    '     Structure RankOrder
    ' 
    '         Function: Input, Ranking, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace Math.Correlations

    Public Structure RankOrder(Of T As IComparable(Of T))

        ''' <summary>
        ''' 排序之后得到的位置
        ''' </summary>
        Public rank As Double

        ''' <summary>
        ''' 在序列之中原有的位置
        ''' </summary>
        Public i As Integer

        ''' <summary>
        ''' xi in the input data sequence
        ''' </summary>
        Public value As T

        Public Overrides Function ToString() As String
            Return $"[{i + 1}] rank={rank}, value:{value.GetJson}"
        End Function

        Public Shared Iterator Function Input(data As IEnumerable(Of T)) As IEnumerable(Of RankOrder(Of T))
            Dim i As i32 = Scan0

            For Each xi As T In data
                Yield New RankOrder(Of T) With {
                    .i = ++i,
                    .rank = 0,
                    .value = xi
                }
            Next
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="data"></param>
        ''' <param name="desc"></param>
        ''' <returns>
        ''' i -> new order in new sequence after data sort
        ''' </returns>
        Public Shared Function Ranking(data As IEnumerable(Of RankOrder(Of T)), Optional desc As Boolean = False) As RankOrder(Of T)()
            Dim orders As RankOrder(Of T)()

            If desc Then
                orders = (From xi As RankOrder(Of T) In data Order By xi.value Descending).ToArray
            Else
                orders = (From xi As RankOrder(Of T) In data Order By xi.value Ascending).ToArray
            End If

            For rank As Integer = 0 To orders.Length - 1
                orders(rank) = New RankOrder(Of T) With {
                    .rank = rank + 1,
                    .i = orders(rank).i,
                    .value = orders(rank).value
                }
            Next

            Return orders
        End Function

    End Structure
End Namespace
