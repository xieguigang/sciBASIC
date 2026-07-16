Imports Microsoft.VisualBasic.Math.Scripting
Imports Microsoft.VisualBasic.Math.Scripting.MathExpression
Imports Microsoft.VisualBasic.Math.Scripting.MathExpression.Impl
Imports Microsoft.VisualBasic.Linq

Module Program

    Dim pass As Integer = 0
    Dim fail As Integer = 0

    Sub Main()
        Console.WriteLine("=== Scripting 表达式解析器修复验证测试 ===")
        Console.WriteLine()

        TestFactorial()
        TestIntegerDivision()
        TestNotEqualOperator()
        TestNotEqualWithSpaces()
        TestUDF_SetFunction()
        TestUDF_NestedCall()
        TestParseExpressionThrowEx()
        TestParseExpressionNotThrow()
        TestProcessOperatorsBoundary()
        TestMeanArrayParam()
        TestPowMultiArg()
        TestLogMultiArg()
        TestSymbolFactorial()
        TestExpressionEvaluation()

        Console.WriteLine()
        Console.WriteLine($"========================================")
        Console.WriteLine($"测试完成: {pass} 通过, {fail} 失败, 共 {pass + fail} 项")
        Console.WriteLine($"========================================")

        If fail > 0 Then
            Environment.ExitCode = 1
        End If
    End Sub

    Sub AssertEqual(name As String, expected As Object, actual As Object)
        If expected Is Nothing AndAlso actual Is Nothing Then
            pass += 1
            Console.WriteLine($"  [PASS] {name}: Nothing")
        ElseIf expected Is Nothing OrElse actual Is Nothing Then
            fail += 1
            Console.WriteLine($"  [FAIL] {name}: 期望 {If(expected Is Nothing, "Nothing", expected.ToString)}, 实际 {If(actual Is Nothing, "Nothing", actual.ToString)}")
        ElseIf expected.Equals(actual) Then
            pass += 1
            Console.WriteLine($"  [PASS] {name}: {expected}")
        Else
            fail += 1
            Console.WriteLine($"  [FAIL] {name}: 期望 {expected}, 实际 {actual}")
        End If
    End Sub

    Sub AssertThrows(name As String, action As Action)
        Try
            action()
            fail += 1
            Console.WriteLine($"  [FAIL] {name}: 未抛出预期的异常")
        Catch ex As Exception
            pass += 1
            Console.WriteLine($"  [PASS] {name}: 正确抛出异常 ({ex.GetType.Name})")
        End Try
    End Sub

    ' ================================================================
    ' 测试 1: 阶乘 5! —— 不应无限递归/栈溢出
    ' ================================================================
    Sub TestFactorial()
        Console.WriteLine("--- 测试: 阶乘 5! / 带空格表达式 ---")
        Dim engine As New ExpressionEngine

        ' 5! = 120
        Dim result1 = engine.Evaluate("5!")
        AssertEqual("5! = 120", 120.0, result1)

        ' 1! = 1
        Dim result2 = engine.Evaluate("1!")
        AssertEqual("1! = 1", 1.0, result2)

        ' 3! + 2 = 6 + 2 = 8
        Dim result3 = engine.Evaluate("3! + 2")
        AssertEqual("3! + 2 = 8", 8.0, result3)

        Console.WriteLine()
    End Sub

    ' ================================================================
    ' 测试 1b: 符号阶乘 x! —— 变量后接阶乘
    ' ================================================================
    Sub TestSymbolFactorial()
        Console.WriteLine("--- 测试: 符号阶乘 x! ---")
        Dim engine As New ExpressionEngine

        ' n = 5, n! = 120
        engine.SetSymbol("n", 5.0)
        Dim result = engine.Evaluate("n!")
        AssertEqual("n! (n=5) = 120", 120.0, result)

        Console.WriteLine()
    End Sub

    ' ================================================================
    ' 测试 2: 整数除法 \ —— 词法解析与求值
    ' ================================================================
    Sub TestIntegerDivision()
        Console.WriteLine("--- 测试: 整数除法 \ ---")
        Dim engine As New ExpressionEngine

        ' 5 \ 2 = 2
        Dim result1 = engine.Evaluate("5\2")
        AssertEqual("5 \ 2 = 2", 2.0, result1)

        ' 10 \ 3 = 3
        Dim result2 = engine.Evaluate("10\3")
        AssertEqual("10 \ 3 = 3", 3.0, result2)

        ' 7 \ 3 + 1 = 3
        Dim result3 = engine.Evaluate("7\3+1")
        AssertEqual("7 \ 3 + 1 = 3", 3.0, result3)

        Console.WriteLine()
    End Sub

    ' ================================================================
    ' 测试 3: 不等于运算符 <> 求值
    ' ================================================================
    Sub TestNotEqualOperator()
        Console.WriteLine("--- 测试: 不等于运算符 <> ---")
        Dim engine As New ExpressionEngine

        ' 3 <> 4 = 1 (True)
        Dim result1 = engine.Evaluate("3<>4")
        AssertEqual("3 <> 4 = 1", 1.0, result1)

        ' 3 <> 3 = 0 (False)
        Dim result2 = engine.Evaluate("3<>3")
        AssertEqual("3 <> 3 = 0", 0.0, result2)

        ' (3 <> 3) + 5 = 5 —— 注意 <> 优先级最低，需显式括号
        Dim result3 = engine.Evaluate("(3<>3)+5")
        AssertEqual("(3 <> 3) + 5 = 5", 5.0, result3)

        Console.WriteLine()
    End Sub

    ' ================================================================
    ' 测试 4: <> 带空格 —— mergeNotEqual 合并相邻 < >
    ' ================================================================
    Sub TestNotEqualWithSpaces()
        Console.WriteLine("--- 测试: <> 带空格 ---")
        Dim engine As New ExpressionEngine

        ' a <> b 带空格
        Dim result1 = engine.Evaluate(" 3   <>  4 ")
        AssertEqual("带空格 3 <> 4 = 1", 1.0, result1)

        Console.WriteLine()
    End Sub

    ' ================================================================
    ' 测试 5: UDF 用户自定义函数 SetFunction 截取 lambda 正确
    ' ================================================================
    Sub TestUDF_SetFunction()
        Console.WriteLine("--- 测试: UDF SetFunction (func add(x,y) x+y) ---")
        Dim engine As New ExpressionEngine

        ' 使用 ScriptEngine.SetFunction 注册 UDF
        ' 注意: SetFunction 的 run 参数应为去掉 "func " 前缀后的内容
        ' (正式调用路径由 Shell → setFunction，已剥离 func 关键字)
        ScriptEngine.SetFunction(engine, "add(x, y) x+y")
        Dim result = engine.Evaluate("add(2, 3)")
        AssertEqual("add(2, 3) = 5", 5.0, result)

        ' 注册另一个函数
        ScriptEngine.SetFunction(engine, "mul(a, b) a*b")
        Dim result2 = engine.Evaluate("mul(4, 5)")
        AssertEqual("mul(4, 5) = 20", 20.0, result2)

        Console.WriteLine()
    End Sub

    ' ================================================================
    ' 测试 6: UDF 嵌套调用 —— addFunction 内部 env 拷贝 functions
    '         使用纯算术表达式体来隔离混合解析的预存问题
    ' ================================================================
    Sub TestUDF_NestedCall()
        Console.WriteLine("--- 测试: UDF 嵌套调用 ---")
        Dim engine As New ExpressionEngine

        ' 注册 inner 函数
        ScriptEngine.SetFunction(engine, "square(x) x*x")
        ' 注册 outer 函数，lambda 体仅为调用 square（纯函数调用，无运算符混合）
        ScriptEngine.SetFunction(engine, "f(y) square(y)")
        Dim result = engine.Evaluate("f(3)")
        ' f(3) = square(3) = 9
        AssertEqual("f(3) = square(3) = 9", 9.0, result)

        Console.WriteLine()
    End Sub

    ' ================================================================
    ' 测试 7: ParseExpression throwEx = True 时重抛异常
    ' ================================================================
    Sub TestParseExpressionThrowEx()
        Console.WriteLine("--- 测试: ParseExpression throwEx=True 抛异常 ---")

        AssertThrows("非法表达式 throwEx=True 应抛异常",
            Sub()
                ScriptEngine.ParseExpression("abc @@@ xyz", throwEx:=True)
            End Sub)

        Console.WriteLine()
    End Sub

    ' ================================================================
    ' 测试 8: ParseExpression throwEx = False 返回 Nothing
    ' ================================================================
    Sub TestParseExpressionNotThrow()
        Console.WriteLine("--- 测试: ParseExpression throwEx=False 返回 Nothing ---")

        Dim result = ScriptEngine.ParseExpression("abc @@@ xyz", throwEx:=False)
        AssertEqual("非法表达式 throwEx=False 返回 Nothing", Nothing, result)

        Console.WriteLine()
    End Sub

    ' ================================================================
    ' 测试 9: processOperators 边界判断 —— 复杂表达式不崩溃
    ' ================================================================
    Sub TestProcessOperatorsBoundary()
        Console.WriteLine("--- 测试: processOperators 边界判断 ---")
        Dim engine As New ExpressionEngine

        ' 多运算符嵌套: 括号 + 混合优先级 + <>
        Dim result = engine.Evaluate("(1+2)*3")
        AssertEqual("(1+2)*3 = 9", 9.0, result)

        ' 多运算符链: ^ 和 */ 和 +-
        Dim result2 = engine.Evaluate("2^3+4*5-6/3")
        AssertEqual("2^3+4*5-6/3 = 8+20-2 = 26", 26.0, result2)

        ' 幂运算按优先级从左往右计算: 2^3^2 = (2^3)^2 = 8^2 = 64
        Dim result3 = engine.Evaluate("2^3^2")
        AssertEqual("2^3^2 = (2^3)^2 = 64", 64.0, result3)

        ' 若需右结合，使用括号: 2^(3^2) = 512
        Dim result4 = engine.Evaluate("2^(3^2)")
        AssertEqual("2^(3^2) = 512", 512.0, result4)

        Console.WriteLine()
    End Sub

    ' ================================================================
    ' 测试 10: R mean 数组参数 —— 直接传入 Double() 解包
    ' ================================================================
    Sub TestMeanArrayParam()
        Console.WriteLine("--- 测试: R mean 数组参数 ---")

        ' 测试 BasicR.base.mean 本身的行为（不在 ExpressionEngine.functions 中）
        Dim meanResult = BasicR.base.mean(1, 2, 3, 4, 5)
        AssertEqual("mean(1,2,3,4,5) = 3", 3.0, meanResult)

        ' 传入数组参数
        Dim arr As Double() = {10, 20, 30, 40, 50}
        Dim meanResult2 = BasicR.base.mean(arr)
        AssertEqual("mean({10,20,30,40,50}) = 30", 30.0, meanResult2)

        Console.WriteLine()
    End Sub

    ' ================================================================
    ' 测试 11: pow 多参数 —— 编译器级别测试
    ' ================================================================
    Sub TestPowMultiArg()
        Console.WriteLine("--- 测试: pow 多参数求值 ---")
        Dim engine As New ExpressionEngine

        ' pow(2, 3) = 8
        Dim result = engine.Evaluate("pow(2, 3)")
        AssertEqual("pow(2, 3) = 8", 8.0, result)

        ' pow(2, 10) = 1024
        Dim result2 = engine.Evaluate("pow(2, 10)")
        AssertEqual("pow(2, 10) = 1024", 1024.0, result2)

        Console.WriteLine()
    End Sub

    ' ================================================================
    ' 测试 12: log 多参数 —— log(value, base)
    ' ================================================================
    Sub TestLogMultiArg()
        Console.WriteLine("--- 测试: log 多参数求值 ---")
        Dim engine As New ExpressionEngine

        ' log(100, 10) = 2
        Dim result = engine.Evaluate("log(100, 10)")
        AssertEqual("log(100, 10) ≈ 2", 2.0, Math.Round(result, 10))

        ' log(8, 2) = 3
        Dim result2 = engine.Evaluate("log(8, 2)")
        AssertEqual("log(8, 2) = 3", 3.0, Math.Round(result2, 10))

        Console.WriteLine()
    End Sub

    ' ================================================================
    ' 测试 13: 综合表达式求值 —— 覆盖多种运算符组合
    ' ================================================================
    Sub TestExpressionEvaluation()
        Console.WriteLine("--- 测试: 综合表达式求值 ---")
        Dim engine As New ExpressionEngine

        ' 基本四则运算
        AssertEqual("2+3*4 = 14", 14.0, engine.Evaluate("2+3*4"))
        AssertEqual("(2+3)*4 = 20", 20.0, engine.Evaluate("(2+3)*4"))

        ' 幂运算
        AssertEqual("2^10 = 1024", 1024.0, engine.Evaluate("2^10"))

        ' 内置函数
        AssertEqual("sqrt(16) = 4", 4.0, engine.Evaluate("sqrt(16)"))
        AssertEqual("sin(0) = 0", 0.0, Math.Round(engine.Evaluate("sin(0)"), 10))
        AssertEqual("abs(-5) = 5", 5.0, engine.Evaluate("abs(-5)"))

        ' 常量
        AssertEqual("PI 符号存在", 1, If(engine.Evaluate("PI") > 3.14, 1, 0))

        Console.WriteLine()
    End Sub

End Module
