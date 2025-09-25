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

            ' 使用拓扑排序算法处理依赖关系
            Dim visited As New Dictionary(Of String, Boolean)()
            Dim tempMark As New Dictionary(Of String, Boolean)()
            Dim result As New List(Of FormulaDependency)()

            ' 初始化访问标记
            For Each formula As FormulaDependency In formulaIndex.Values
                visited(formula.symbol) = False
                tempMark(formula.symbol) = False
            Next

            ' 对每个未访问的节点进行深度优先搜索
            For Each formula As FormulaDependency In formulaIndex.Values
                If Not visited(formula.symbol) Then
                    If Not Visit(formula, formulaIndex, visited, tempMark, result) Then
                        Throw New InvalidOperationException("A circular symbol dependency has been detected, and sorting cannot be performed.")
                    End If
                End If
            Next

            Return result.AsEnumerable.Reverse().ToArray()
        End Function

        ''' <summary>
        ''' 深度优先访问节点，检测循环依赖
        ''' </summary>
        Private Shared Function Visit(ByRef node As FormulaDependency,
                                      ByRef index As Dictionary(Of String, FormulaDependency),
                                      ByRef visited As Dictionary(Of String, Boolean),
                                      ByRef tempMark As Dictionary(Of String, Boolean),
                                      ByRef result As List(Of FormulaDependency)) As Boolean

            If tempMark(node.symbol) Then
                ' 发现循环依赖
                Return False
            End If

            If visited(node.symbol) Then
                Return True
            End If

            ' 标记为临时访问
            tempMark(node.symbol) = True

            ' 递归访问所有依赖项
            If node.dependency IsNot Nothing Then
                For Each depSymbol In node.dependency
                    If index.ContainsKey(depSymbol) Then
                        Dim depNode As FormulaDependency = index(depSymbol)

                        If Not Visit(depNode, index, visited, tempMark, result) Then
                            Return False
                        End If
                    End If
                Next
            End If

            ' 取消临时标记，标记为永久访问
            tempMark(node.symbol) = False
            visited(node.symbol) = True

            ' 将节点添加到结果列表
            result.Add(node)

            Return True
        End Function
    End Class
End Namespace