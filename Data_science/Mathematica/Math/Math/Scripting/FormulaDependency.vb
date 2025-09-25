Imports Microsoft.VisualBasic.Math.Scripting.MathExpression.Impl

Namespace Scripting

    Public Class FormulaDependency

        Public Property symbol As String
        ''' <summary>
        ''' the value expression of the target symbol: symbol = formula_expression
        ''' </summary>
        ''' <returns></returns>
        Public Property formula As Expression

        ''' <summary>
        ''' a collection of the <see cref="symbol"/> dependency in the 
        ''' current <see cref="formula"/> expression.
        ''' </summary>
        ''' <returns></returns>
        Public Property dependency As String()

        Public Overrides Function ToString() As String
            If dependency.IsNullOrEmpty Then
                Return $"{symbol} = None"
            End If

            Return $"{symbol} = {dependency.JoinBy(" ~ ")}"
        End Function

        ''' <summary>
        ''' sort of the given formula list via the formula <see cref="dependency"/>
        ''' </summary>
        ''' <param name="formulas"></param>
        ''' <returns></returns>
        Public Shared Function Sort(formulas As IEnumerable(Of FormulaDependency)) As FormulaDependency()
            Dim formulaIndex As Dictionary(Of String, FormulaDependency) = formulas.ToDictionary(Function(x) x.symbol)

            ' sort the formula via dependency numbers in asc order
            ' no dependency: zero
            ' dependency reference to other formula target symbol: n
            ' sort in asc order: zero -> n
            ' example dependency sort order:
            '
            ' dependency.length, dependency
            ' [0] {}
            ' [1] x
            ' [1] y
            ' [2] {x, y}

            ' 1. 构建后继图：graph(key: 符号, value: 该符号的后继符号列表)
            Dim graph As New Dictionary(Of String, List(Of String))()

            For Each symbol As String In formulaIndex.Keys
                graph(symbol) = New List(Of String)()
            Next

            For Each fd In formulas
                If fd.dependency IsNot Nothing Then
                    For Each dep In fd.dependency
                        If formulaIndex.ContainsKey(dep) Then
                            ' 如果公式fd依赖dep，则dep是前驱，fd.symbol是dep的后继
                            graph(dep).Add(fd.symbol)
                        End If
                    Next
                End If
            Next

            ' 2. 初始化DFS标记
            Dim visited As New Dictionary(Of String, Boolean)()
            Dim tempMark As New Dictionary(Of String, Boolean)()
            Dim result As New List(Of FormulaDependency)()

            For Each formula As FormulaDependency In formulaIndex.Values
                visited(formula.symbol) = False
                tempMark(formula.symbol) = False
            Next

            ' 3. 对每个未访问节点执行DFS
            For Each formula As FormulaDependency In formulaIndex.Values
                If Not visited(formula.symbol) Then
                    If Not Visit(formula, formulaIndex, graph, visited, tempMark, result) Then
                        Throw New InvalidOperationException("A circular symbol dependency has been detected, and sorting cannot be performed.")
                    End If
                End If
            Next

            ' 4. 反转后序序列得到拓扑顺序
            Return result.AsEnumerable().Reverse().ToArray()
        End Function

        ''' <summary>
        ''' 深度优先访问节点，检测循环依赖
        ''' </summary>
        Private Shared Function Visit(ByRef node As FormulaDependency,
                                      ByRef index As Dictionary(Of String, FormulaDependency),
                                      ByRef graph As Dictionary(Of String, List(Of String)),
                                      ByRef visited As Dictionary(Of String, Boolean),
                                      ByRef tempMark As Dictionary(Of String, Boolean),
                                      ByRef result As List(Of FormulaDependency)) As Boolean

            If tempMark(node.symbol) Then
                Return False ' 发现循环依赖
            End If

            If visited(node.symbol) Then
                Return True ' 已访问过，跳过
            End If

            tempMark(node.symbol) = True ' 标记为临时访问

            ' 遍历当前节点的后继节点（从graph中获取）
            If graph.ContainsKey(node.symbol) Then
                For Each nextSymbol In graph(node.symbol)
                    If index.ContainsKey(nextSymbol) Then
                        Dim nextNode As FormulaDependency = index(nextSymbol)
                        If Not Visit(nextNode, index, graph, visited, tempMark, result) Then
                            Return False
                        End If
                    End If
                Next
            End If

            ' 完成后继遍历，标记为永久访问并添加节点到结果
            tempMark(node.symbol) = False
            visited(node.symbol) = True
            result.Add(node)

            Return True
        End Function
    End Class
End Namespace