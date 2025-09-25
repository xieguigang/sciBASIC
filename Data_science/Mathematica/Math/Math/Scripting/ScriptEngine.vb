#Region "Microsoft.VisualBasic::a5e1e748745572157845a3a1a46d7126, Data_science\Mathematica\Math\Math\Scripting\ScriptEngine.vb"

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

    '   Total Lines: 155
    '    Code Lines: 78 (50.32%)
    ' Comment Lines: 54 (34.84%)
    '    - Xml Docs: 83.33%
    ' 
    '   Blank Lines: 23 (14.84%)
    '     File Size: 5.71 KB


    '     Module ScriptEngine
    ' 
    '         Properties: Expression, Scripts, StatementEngine
    ' 
    '         Function: Evaluate, ParseExpression, Shell
    ' 
    '         Sub: setFunction, SetFunction, setSymbol, (+2 Overloads) SetVariable
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math.Scripting.MathExpression
Imports Microsoft.VisualBasic.Math.Scripting.MathExpression.Impl
Imports r = System.Text.RegularExpressions.Regex

Namespace Scripting

    ''' <summary>
    ''' Math expression script engine.
    ''' </summary>
    Public Module ScriptEngine

        ''' <summary>
        ''' The default expression engine.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Expression As New ExpressionEngine

        ''' <summary>
        ''' all of the commands are stored at here
        ''' </summary>
        ''' <remarks>
        ''' .quit for do nothing and end of this program.
        ''' </remarks>
        Public ReadOnly Property StatementEngine As New Dictionary(Of String, Action(Of String)) From {
            {"const", AddressOf setSymbol},
            {"function", AddressOf setFunction},
            {"func", AddressOf setFunction},
            {"var", AddressOf setSymbol},
            {".quit", Sub(null$) null = Nothing}
        }

        ''' <summary>
        ''' Lambda expression table.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Scripts As New Hashtable

        ''' <summary>
        ''' var x = 100
        ''' const x = 100
        ''' </summary>
        ''' <param name="run"></param>
        Private Sub setSymbol(run As String)
            Dim assign As NamedValue(Of String) = run.GetTagValue("=", trim:=True)
            Dim value As Double = Expression.Evaluate(assign.Value)

            Expression.SetSymbol(assign.Name, value)
        End Sub

        ''' <summary>
        ''' func add(x, y) x+y
        ''' </summary>
        ''' <param name="run"></param>
        Private Sub setFunction(run As String)
            Call SetFunction(Expression, run)
        End Sub

        ''' <summary>
        ''' func add(x, y) x+y
        ''' </summary>
        ''' <param name="run"></param>
        ''' 
        <Extension>
        Public Sub SetFunction(engine As ExpressionEngine, run As String)
            Dim declares As String = r.Match(run, ".+\(.+?\)").Value
            Dim lambda As String = Mid(run, declares.Length)
            Dim name As String = declares.Split("("c).First
            Dim parameters As String() = declares.GetStackValue("(", ")").StringSplit("\s*,\s*")

            Call engine.AddFunction(declares, parameters, lambda)
        End Sub

        ''' <summary>
        ''' Run the simple script that stores in the <see cref="Scripts"/> table.
        ''' </summary>
        ''' <param name="statement"></param>
        ''' <returns></returns>
        Public Function Shell(statement$) As Double
            If InStr(statement, "<-") Then
                ' This is a value assignment statement
                Dim tokens As NamedValue(Of String) = statement.GetTagValue("<-", trim:=True)
                Dim result As Double = tokens.Value.DoCall(AddressOf Expression.Evaluate)

                Expression.SetSymbol(tokens.Name, result)

                Return result
            End If

            Dim token As String = statement.Split.First.ToLower

            If StatementEngine.ContainsKey(token) Then
                Call StatementEngine(token)(Mid(statement, Len(token) + 1).Trim)
                Return 0
            Else
                ' if the statement input from the user is not appears 
                ' in the engine dictionary, then maybe is a mathematics expression. 
                Dim result As Double = Expression.Evaluate(statement)
                ' You can treat the variable 'last' as a system variable for return 
                ' the Result of a multiple function script in the future of this 
                ' feature will add to this module.
                Expression.SetSymbol("$", result)
                Return result
            End If
        End Function

        ' 字符串插值脚本
        ' expr$ = "&blablabla $x + $y = $z blablabla...."

        ''' <summary>
        ''' <see cref="Shell"/> function name alias.
        ''' </summary>
        ''' <param name="statement$"></param>
        ''' <returns></returns>
        <Extension> Public Function Evaluate(statement$, Optional echo As Boolean = True) As Double
            Dim x# = Shell(statement)

            If echo Then
                Call statement.debug
                Call $" = {x}".debug
            End If

            Return x
        End Function

        ''' <summary>
        ''' Set variable value
        ''' </summary>
        ''' <param name="name"></param>
        ''' <param name="expr"></param>
        Public Sub SetVariable(name$, expr$)
            Call Expression.SetSymbol(name, expr)
        End Sub

        Public Sub SetVariable(name$, value As Double)
            Call Expression.SetSymbol(name, value)
        End Sub

        Public Function ParseExpression(expression As String, Optional throwEx As Boolean = True) As Expression
            Try
                Dim tokenSet = New ExpressionTokenIcer(expression) _
                    .GetTokens _
                    .ToArray
                Dim exp As Expression = ExpressionBuilder.BuildExpression(tokenSet)

                Return exp
            Catch ex As Exception
                Call App.LogException(New Exception(expression, ex))
                Return Nothing
            End Try
        End Function

        ''' <summary>
        ''' check of the formula symbol dependency
        ''' </summary>
        ''' <param name="formulas">
        ''' a collection of the symbol value assigned expression: [symbol &lt;- expression]
        ''' </param>
        ''' <param name="ignores"></param>
        ''' <returns>
        ''' the given formula dependency result list is sorted via the dependency order
        ''' </returns>
        <Extension>
        Public Function CheckFormulaDependency(formulas As Dictionary(Of String, Expression), Optional ignores As Index(Of String) = Nothing) As FormulaDependency()
            Dim check As New List(Of FormulaDependency)

            If ignores Is Nothing Then
                ignores = New String() {}
            End If

            For Each symbol As KeyValuePair(Of String, Expression) In formulas
                Call check.Add(New FormulaDependency With {
                    .symbol = symbol.Key,
                    .formula = symbol.Value,
                    .dependency = .formula _
                        .GetVariableSymbols _
                        .Where(Function(x) Not x Like ignores) _
                        .ToArray
                })
            Next

            Return FormulaDependency.Sort(check)
        End Function
    End Module

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
