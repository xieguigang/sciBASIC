Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Text.Xml.Models

Public Module Visualize

    <Extension>
    Public Iterator Function NodeImportance(grid As GridMatrix) As IEnumerable(Of NamedValue(Of Double))
        For Each factor As NumericVector In grid.correlations
            Yield New NamedValue(Of Double) With {
                .Name = factor.name,
                .Value = factor.vector.Sum
            }
        Next
    End Function

    ''' <summary>
    ''' Create network graph model from the grid system status.
    ''' </summary>
    ''' <param name="grid"></param>
    ''' <param name="cutoff">
    ''' 系统中的变量与结果之间的相关度因子的阈值，低于这个阈值的边都会被删掉，也就是只会留下相关度较高的边
    ''' </param>
    ''' <returns></returns>
    ''' <remarks>
    ''' 对于一个网格系统而言, 其计算格式为: ``Xi ^ (c * Xj)``. 因为对于c * Xj而言:
    ''' 
    ''' + 乘积结果肯定是越大,则Xi越大, 此时Xi项目对系统输出的结果的影响也越大
    ''' + 假若c为负数的时候,则Xi指数计算结果接近于零,c越小Xi项目越接近于零,此时Xi项目对系统输出的结果的影响也越小
    ''' + 当c接近于零的时候,其对Xi没有影响度,因为Xi^0 =1, 影响整个Xi的只能够从其他的系统变量因子获取
    ''' 
    ''' 所以c因子可以看作为Xj与Xi之间的相关度, 只不过这个相关度是位于整个[负无穷, 正无穷]之间的
    ''' </remarks>
    <Extension>
    Public Function CreateGraph(grid As GridMatrix, Optional cutoff# = 1) As NetworkGraph
        Dim g As New NetworkGraph
        Dim node As Node
        Dim variableNames As New List(Of String)
        Dim edge As EdgeData
        Dim importance As Dictionary(Of String, Double) = grid _
            .NodeImportance _
            .ToDictionary(Function(n) n.Name,
                          Function(n)
                              Return n.Value
                          End Function)

        For Each factor As NumericVector In grid.correlations
            node = New Node With {
                .data = New NodeData With {
                    .label = factor.name,
                    .origID = factor.name,
                    .mass = importance(factor.name),
                    .radius = importance(factor.name)
                },
                .Label = factor.name,
                .ID = 0
            }

            variableNames += factor.name
            g.AddNode(node)
        Next

        For Each factor As NumericVector In grid.correlations
            For i As Integer = 0 To factor.Length - 1
                ' PCC/SPCC相关度等，位于[-1,1]之间
                ' 而这个的相关度则是位于[负无穷, 正无穷]之间
                If factor.name <> variableNames(i) AndAlso Math.Abs(factor(i)) >= cutoff Then
                    ' 跳过自己和自己的链接
                    ' 以及低相关度的节点链接
                    edge = New EdgeData With {
                        .label = $"{factor.name} ^ {variableNames(i)}",
                        .weight = factor(i)
                    }
                    g.CreateEdge(factor.name, variableNames(i), edge)
                End If
            Next
        Next

        Return g
    End Function
End Module
