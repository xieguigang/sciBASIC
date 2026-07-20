Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.Math.Scripting.MathExpression.Impl

Namespace Microsoft.VisualBasic.Math.Scripting

    Friend Class UserFunction
        Public params As String()
        Public body As Expression
    End Class

    ''' <summary>
    ''' 数学脚本引擎：支持标量/向量变量、函数定义、axis 内置以及
    ''' scatter/line/surface 绘图指令。仅产出向量数据与绘图指令，
    ''' 不依赖任何绘图组件，可作为通用数学运算脚本引擎复用。
    '''
    ''' 设计要点：向量运算复用现有标量 ExpressionEngine，以
    ''' “逐元素标量循环 + 环境变量绑定”方式求值，避免改动表达式编译器。
    ''' </summary>
    Public Class MathScriptEngine

        Private engine As New ExpressionEngine()
        Private vars As New Dictionary(Of String, Object)()
        Private funcs As New Dictionary(Of String, UserFunction)()

        Private Structure ArgValue
            Public IsVector As Boolean
            Public Vector As Double()
            Public Scalar As Double
        End Structure

        ' ===================== 公共 API =====================

        Public Sub SetVariable(name As String, value As Object)
            vars(name) = value
        End Sub

        Public Function GetVariable(name As String) As Object
            If vars.ContainsKey(name) Then Return vars(name) Else Return Nothing
        End Function

        Public Sub AddFunction(name As String, params() As String, body As String)
            RegisterFunction(name, params, Expression.Parse(body))
        End Sub

        Public Function RunScript(script As String) As ScriptResult
            Dim result As New ScriptResult()
            Dim lines = If(script, "").Replace(vbCrLf, vbLf).Split(vbLf)
            Dim lineNo As Integer = 0
            Try
                For Each raw In lines
                    lineNo += 1
                    Dim line = raw.Trim()
                    If line.Length = 0 Then Continue For
                    If line.StartsWith("#"c) OrElse line.StartsWith("'"c) Then Continue For
                    ProcessLine(line, result)
                Next
                For Each kv In vars
                    result.Variables(kv.Key) = kv.Value
                Next
            Catch ex As Exception
                result.Success = False
                result.ErrorMessage = "第 " & lineNo & " 行: " & ex.Message
                result.Line = lineNo
            End Try
            Return result
        End Function

        ' ===================== 行解析 =====================

        Private Sub ProcessLine(line As String, result As ScriptResult)
            ' 1) 函数定义：name(params) = body
            Dim m = Regex.Match(line, "^([A-Za-z_]\w*)\s*\(([^)]*)\)\s*=\s*(.+)$")
            If m.Success Then
                Dim name = m.Groups(1).Value
                Dim plist = m.Groups(2).Value
                Dim body = m.Groups(3).Value
                Dim ps = If(String.IsNullOrWhiteSpace(plist),
                           New String() {},
                           plist.Split(","c).Select(Function(s) s.Trim()).Where(Function(s) s.Length > 0).ToArray())
                RegisterFunction(name, ps, Expression.Parse(body))
                Return
            End If

            ' 2) 绘图指令：scatter(...) / line(...) / surface(...)
            m = Regex.Match(line, "^(scatter|line|surface)\s*\((.*)\)\s*$", RegexOptions.IgnoreCase)
            If m.Success Then
                HandlePlot(m.Groups(1).Value.ToLower(), m.Groups(2).Value, result)
                Return
            End If

            ' 3) 赋值：lhs = rhs
            Dim eq = line.IndexOf("="c)
            If eq > 0 Then
                Dim lhs = line.Substring(0, eq).Trim()
                Dim rhs = line.Substring(eq + 1).Trim()
                If Regex.IsMatch(lhs, "^[A-Za-z_]\w*$") Then
                    ProcessAssignment(lhs, rhs, result)
                    Return
                End If
            End If

            ' 4) 其它：忽略（可扩展为表达式语句）
        End Sub

        Private Sub RegisterFunction(name As String, params() As String, body As Expression)
            funcs(name) = New UserFunction With {.params = params, .body = body}
            Try
                engine.AddFunction(name, params, Function(args) EvalUserFunc(name, args))
            Catch
            End Try
        End Sub

        Private Function EvalUserFunc(name As String, args As Double()) As Double
            Dim f = funcs(name)
            Dim saved As New Dictionary(Of String, Double)()
            For i = 0 To f.params.Length - 1
                If engine.symbols.ContainsKey(f.params(i)) Then
                    saved(f.params(i)) = engine.symbols(f.params(i))
                End If
                engine.SetSymbol(f.params(i), args(i))
            Next
            Dim r As Double
            Try
                r = engine.Evaluate(f.body)
            Finally
                For Each p In f.params
                    If saved.ContainsKey(p) Then
                        engine.SetSymbol(p, saved(p))
                    Else
                        engine.symbols.Remove(p)
                    End If
                Next
            End Try
            Return r
        End Function

        ' ===================== 赋值 =====================

        Private Sub ProcessAssignment(lhs As String, rhs As String, result As ScriptResult)
            ' axis 内置
            Dim m = Regex.Match(rhs, "^axis\s*\((.*)\)\s*$", RegexOptions.IgnoreCase)
            If m.Success Then
                vars(lhs) = MakeAxis(ParseArgs(m.Groups(1).Value))
                Return
            End If

            ' 函数调用 f(args)
            m = Regex.Match(rhs, "^([A-Za-z_]\w*)\s*\((.*)\)\s*$")
            If m.Success Then
                Dim fname = m.Groups(1).Value
                Dim args = ParseArgs(m.Groups(2).Value)
                vars(lhs) = EvaluateCall(fname, args)
                Return
            End If

            ' 普通表达式（可引用向量符号，逐元素求值）
            Dim expr = Expression.Parse(rhs)
            Dim syms = expr.GetVariableSymbols().ToList()
            Dim length = VectorLength(syms)
            If length <= 0 Then
                BindScalars(0)
                vars(lhs) = engine.Evaluate(expr)
            Else
                Dim out(length - 1) As Double
                For k = 0 To length - 1
                    BindScalars(k)
                    out(k) = engine.Evaluate(expr)
                Next
                vars(lhs) = out
            End If
        End Sub

        Private Function EvaluateCall(fname As String, args As List(Of String)) As Object
            If fname.Equals("axis", StringComparison.OrdinalIgnoreCase) Then
                Return MakeAxis(args)
            End If

            Dim resolved(args.Count - 1) As ArgValue
            Dim length As Integer = 0
            For i = 0 To args.Count - 1
                resolved(i) = ResolveArg(args(i))
                If resolved(i).IsVector Then length = resolved(i).Vector.Length
            Next

            If funcs.ContainsKey(fname) Then
                If length <= 0 Then
                    Dim a(resolved.Length - 1) As Double
                    For i = 0 To resolved.Length - 1 : a(i) = resolved(i).Scalar : Next
                    Return EvalUserFunc(fname, a)
                Else
                    Dim out(length - 1) As Double
                    For k = 0 To length - 1
                        BindScalars(k)
                        Dim a(resolved.Length - 1) As Double
                        For i = 0 To resolved.Length - 1
                            a(i) = If(resolved(i).IsVector, resolved(i).Vector(k), resolved(i).Scalar)
                        Next
                        out(k) = EvalUserFunc(fname, a)
                    Next
                    Return out
                End If
            ElseIf engine.functions.ContainsKey(fname) Then
                If length <= 0 Then
                    Dim a(resolved.Length - 1) As Double
                    For i = 0 To resolved.Length - 1 : a(i) = resolved(i).Scalar : Next
                    Return engine.GetFunction(fname)(a)
                Else
                    Dim out(length - 1) As Double
                    For k = 0 To length - 1
                        BindScalars(k)
                        Dim a(resolved.Length - 1) As Double
                        For i = 0 To resolved.Length - 1
                            a(i) = If(resolved(i).IsVector, resolved(i).Vector(k), resolved(i).Scalar)
                        Next
                        out(k) = engine.GetFunction(fname)(a)
                    Next
                    Return out
                End If
            Else
                Throw New Exception("未知函数: " & fname)
            End If
        End Function

        Private Function ResolveArg(argStr As String) As ArgValue
            Dim name = argStr.Trim()
            If vars.ContainsKey(name) Then
                If TypeOf vars(name) Is Double() Then
                    Return New ArgValue With {.IsVector = True, .Vector = DirectCast(vars(name), Double())}
                Else
                    Return New ArgValue With {.IsVector = False, .Scalar = CDbl(vars(name))}
                End If
            End If
            BindScalars(0)
            Return New ArgValue With {.IsVector = False, .Scalar = engine.Evaluate(name)}
        End Function

        Private Function VectorLength(syms As List(Of String)) As Integer
            For Each s In syms
                If vars.ContainsKey(s) AndAlso TypeOf vars(s) Is Double() Then
                    Return DirectCast(vars(s), Double()).Length
                End If
            Next
            Return 0
        End Function

        Private Sub BindScalars(k As Integer)
            For Each kv In vars
                If TypeOf kv.Value Is Double() Then
                    Dim v = DirectCast(kv.Value, Double())
                    engine.SetSymbol(kv.Key, If(k >= 0 AndAlso k < v.Length, v(k), 0))
                Else
                    engine.SetSymbol(kv.Key, CDbl(kv.Value))
                End If
            Next
        End Sub

        ' ===================== axis 内置 =====================

        Private Function MakeAxis(args As List(Of String)) As Double()
            Dim positional As New List(Of String)()
            Dim named As New Dictionary(Of String, String)(StringComparer.OrdinalIgnoreCase)
            For Each a In args
                Dim t = a.Trim()
                If t.Length = 0 Then Continue For
                Dim ei = t.IndexOf("="c)
                If ei >= 0 Then
                    named(t.Substring(0, ei).Trim().ToLower()) = t.Substring(ei + 1).Trim()
                Else
                    positional.Add(t)
                End If
            Next
            If positional.Count < 2 Then Throw New Exception("axis 需要至少两个参数: min, max")
            Dim mn = CDbl(positional(0))
            Dim mx = CDbl(positional(1))
            Dim n As Integer? = Nothing
            Dim stepVal As Double? = Nothing
            If named.ContainsKey("n") Then n = CInt(Math.Round(CDbl(named("n"))))
            If named.ContainsKey("step") Then stepVal = CDbl(named("step"))

            If n.HasValue AndAlso n.Value > 1 Then
                Dim arr(n.Value - 1) As Double
                For i = 0 To n.Value - 1
                    arr(i) = mn + (mx - mn) * i / (n.Value - 1)
                Next
                Return arr
            ElseIf stepVal.HasValue AndAlso stepVal.Value > 0 Then
                Dim list As New List(Of Double)()
                Dim v = mn
                Dim guard As Integer = 0
                Do While v <= mx + stepVal.Value * 0.5 AndAlso guard < 1000000
                    list.Add(v)
                    v += stepVal.Value
                    guard += 1
                Loop
                If list.Count = 0 Then list.Add(mn)
                Return list.ToArray()
            Else
                Dim arr(999) As Double
                For i = 0 To 999
                    arr(i) = mn + (mx - mn) * i / 999
                Next
                Return arr
            End If
        End Function

        ' ===================== 绘图指令 =====================

        Private Sub HandlePlot(kind As String, inner As String, result As ScriptResult)
            Dim rawArgs = ParseArgs(inner) _
                .Select(Function(s) s.Trim()) _
                .Where(Function(s) s.Length > 0) _
                .ToList()

            ' ctx：已知向量变量，供函数/表达式按参数名匹配
            Dim ctx As New Dictionary(Of String, Double())()
            For Each a In rawArgs
                If vars.ContainsKey(a) AndAlso TypeOf vars(a) Is Double() Then
                    ctx(a) = DirectCast(vars(a), Double())
                End If
            Next

            If kind = "surface" Then
                If rawArgs.Count < 3 Then Throw New Exception("surface 需要三个参数: x, y, z")
                Dim x = ResolveArgVector(rawArgs(0), ctx)
                Dim y = ResolveArgVector(rawArgs(1), ctx)
                Dim z = ResolveSurfaceZ(rawArgs(2), x, y)
                result.Commands.Add(New PlotCommand With {
                    .Kind = PlotKind.Surface, .X = x, .Y = y, .ZGrid = z, .Scheme = "viridis"})
            Else
                Dim knd = If(kind = "scatter", PlotKind.Scatter, PlotKind.Line)
                If rawArgs.Count < 2 Then Throw New Exception(kind & " 至少需要两个参数: x, y")
                Dim x = ResolveArgVector(rawArgs(0), ctx)
                Dim y = ResolveArgVector(rawArgs(1), ctx)
                Dim z As Double() = Nothing
                If rawArgs.Count >= 3 Then z = ResolveArgVector(rawArgs(2), ctx)
                result.Commands.Add(New PlotCommand With {
                    .Kind = knd, .X = x, .Y = y, .Z = z, .Scheme = "viridis"})
            End If
        End Sub

        Private Function ResolveArgVector(name As String, ctx As Dictionary(Of String, Double())) As Double()
            ' 1) 直接向量变量
            If vars.ContainsKey(name) AndAlso TypeOf vars(name) Is Double() Then
                Return DirectCast(vars(name), Double())
            End If

            ' 2) 用户函数（按参数名匹配 ctx 向量）
            If funcs.ContainsKey(name) Then
                Dim f = funcs(name)
                Dim argVecs(f.params.Length - 1) As Double()
                For pi = 0 To f.params.Length - 1
                    argVecs(pi) = If(ctx.ContainsKey(f.params(pi)), ctx(f.params(pi)), Nothing)
                Next
                If argVecs.All(Function(v) v IsNot Nothing) Then
                    Return ApplyUserFuncVector(name, f, argVecs)
                End If
            End If

            ' 3) 表达式（如 sin(x)、x*x、x+y）：在 ctx 向量上逐元素求值
            If name.Contains("("c) OrElse ExprReferencesVector(name, ctx) Then
                Return EvalExprVector(name, ctx)
            End If

            Throw New Exception("未找到向量变量或无法解析: " & name)
        End Function

        Private Function ApplyUserFuncVector(name As String, f As UserFunction, argVecs As Double()()) As Double()
            Dim len = argVecs(0).Length
            Dim out(len - 1) As Double
            For k = 0 To len - 1
                BindScalars(k)
                Dim a(f.params.Length - 1) As Double
                For pi = 0 To f.params.Length - 1 : a(pi) = argVecs(pi)(k) : Next
                out(k) = EvalUserFunc(name, a)
            Next
            Return out
        End Function

        Private Function EvalExprVector(exprStr As String, ctx As Dictionary(Of String, Double())) As Double()
            Dim expr = Expression.Parse(exprStr)
            Dim len = -1
            For Each kv In ctx
                If len < 0 Then len = kv.Value.Length Else len = Math.Min(len, kv.Value.Length)
            Next
            If len < 0 Then len = 1
            Dim out(len - 1) As Double
            For k = 0 To len - 1
                BindScalars(k)
                out(k) = engine.Evaluate(expr)
            Next
            Return out
        End Function

        Private Function ExprReferencesVector(exprStr As String, ctx As Dictionary(Of String, Double())) As Boolean
            Try
                Dim syms = Expression.Parse(exprStr).GetVariableSymbols()
                Return syms.Any(Function(s) ctx.ContainsKey(s))
            Catch
                Return False
            End Try
        End Function

        Private Function ResolveVector(name As String) As Double()
            If vars.ContainsKey(name) AndAlso TypeOf vars(name) Is Double() Then
                Return DirectCast(vars(name), Double())
            End If
            Throw New Exception("未找到向量变量: " & name)
        End Function

        Private Function ResolveSurfaceZ(name As String, x As Double(), y As Double()) As Double()()
            ' 情况1：双参数用户函数 -> 计算网格
            If funcs.ContainsKey(name) AndAlso funcs(name).params.Length = 2 Then
                Dim f = funcs(name)
                Dim grid(y.Length - 1)() As Double
                For i = 0 To y.Length - 1
                    grid(i) = New Double(x.Length - 1) {}
                    For j = 0 To x.Length - 1
                        BindScalars(0)
                        engine.SetSymbol(f.params(0), x(j))
                        engine.SetSymbol(f.params(1), y(i))
                        grid(i)(j) = engine.Evaluate(f.body)
                    Next
                Next
                Return grid
            End If

            ' 情况2：向量变量，长度 = x*y，按行优先重塑
            If vars.ContainsKey(name) AndAlso TypeOf vars(name) Is Double() Then
                Dim v = DirectCast(vars(name), Double())
                If v.Length = x.Length * y.Length Then
                    Dim grid(y.Length - 1)() As Double
                    For i = 0 To y.Length - 1
                        grid(i) = New Double(x.Length - 1) {}
                        For j = 0 To x.Length - 1
                            grid(i)(j) = v(i * x.Length + j)
                        Next
                    Next
                    Return grid
                End If
            End If

            Throw New Exception("surface 的 z 必须是双参数函数或长度为 " & (x.Length * y.Length) & " 的向量: " & name)
        End Function

        ' ===================== 工具 =====================

        Private Function ParseArgs(inner As String) As List(Of String)
            Dim list As New List(Of String)()
            Dim depth As Integer = 0
            Dim cur As New System.Text.StringBuilder()
            For Each ch In inner
                If ch = "("c Then depth += 1
                If ch = ")"c Then depth -= 1
                If ch = ","c AndAlso depth = 0 Then
                    list.Add(cur.ToString())
                    cur.Clear()
                Else
                    cur.Append(ch)
                End If
            Next
            If cur.Length > 0 Then list.Add(cur.ToString())
            Return list
        End Function

    End Class

End Namespace
