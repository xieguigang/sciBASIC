' ---------------------------------------------------------------------------
' .NET 类型 / 运算符 / 数学函数 -> CUDA C 的映射表
'
' 这是整条流水线里唯一"知道 CUDA"的类型层：
'   * Single -> float，Double -> double，Integer -> int ...
'   * System.Math / System.MathF 的静态方法 -> sqrtf / fmaxf / powf ...
'     （单精度加 f 后缀，与 GPU 上的 float 语义一致，避免隐式双精度化）
'   * 字面量按 C 语法格式化（float 必须带 f 后缀，否则会被当成 double）
'
' NVRTC 内置了 cuda_device_runtime_api 与数学函数，不需要 #include。
' ---------------------------------------------------------------------------

Imports IL
Imports System.Globalization

Namespace IL2Cuda

    Public Module CudaTypeMap

        ''' <summary>.NET 类型 -> CUDA C 类型名；不支持的类型返回 Nothing</summary>
        Public Function CudaType(t As System.Type) As String
            If t Is Nothing Then Return Nothing

            If t = GetType(Single) Then Return "float"
            If t = GetType(Double) Then Return "double"
            If t = GetType(Integer) Then Return "int"
            If t = GetType(Long) Then Return "long long"
            If t = GetType(Short) Then Return "short"
            If t = GetType(UShort) Then Return "unsigned short"
            If t = GetType(Byte) Then Return "unsigned char"
            If t = GetType(SByte) Then Return "char"
            If t = GetType(UInteger) Then Return "unsigned int"
            If t = GetType(ULong) Then Return "unsigned long long"
            If t = GetType(Boolean) Then Return "int"
            If t = GetType(IntPtr) Then Return "long long"

            Return Nothing
        End Function

        ''' <summary>数组类型取元素类型；非数组原样返回</summary>
        Public Function ElementType(t As System.Type) As System.Type
            If t Is Nothing Then Return Nothing
            Return If(t.IsArray, t.GetElementType(), t)
        End Function

        ''' <summary>参数在内核/设备函数里的 C 类型（数组退化为指针）</summary>
        Public Function ParameterCudaType(t As System.Type, Optional isPointer As Boolean = False) As String
            Dim baseType = CudaType(ElementType(t))

            If baseType Is Nothing Then
                Throw New NotSupportedException($"类型 {t.FullName} 无法映射为 CUDA 类型")
            End If

            Return If(isPointer OrElse t.IsArray, baseType & "*", baseType)
        End Function

        ' ------------------------------------------------------------------
        ' 数学函数
        ' ------------------------------------------------------------------

        ''' <summary>
        ''' 把 System.Math / System.MathF 的静态方法映射成 CUDA 设备函数。
        ''' </summary>
        ''' <param name="declaringType">声明类型全名</param>
        ''' <param name="methodName">方法名</param>
        ''' <param name="isSingle">是否走单精度版本（加 f 后缀）</param>
        Public Function TryMapMathFunction(declaringType As String, methodName As String,
                                           isSingle As Boolean, ByRef cudaName As String) As Boolean
            cudaName = Nothing

            If declaringType <> "System.Math" AndAlso declaringType <> "System.MathF" Then
                Return False
            End If

            Dim suffix = If(isSingle, "f", "")

            Select Case methodName
                Case "Sqrt" : cudaName = "sqrt" & suffix
                Case "Abs" : cudaName = If(isSingle, "fabsf", "fabs")
                Case "Pow" : cudaName = "pow" & suffix
                Case "Exp" : cudaName = "exp" & suffix
                Case "Log" : cudaName = "log" & suffix
                Case "Log10" : cudaName = "log10" & suffix
                Case "Min" : cudaName = "fmin" & suffix
                Case "Max" : cudaName = "fmax" & suffix
                Case "Floor" : cudaName = "floor" & suffix
                Case "Ceiling" : cudaName = "ceil" & suffix
                Case "Round" : cudaName = "round" & suffix
                Case "Truncate" : cudaName = "trunc" & suffix
                Case "Sin" : cudaName = "sin" & suffix
                Case "Cos" : cudaName = "cos" & suffix
                Case "Tan" : cudaName = "tan" & suffix
                Case "Asin" : cudaName = "asin" & suffix
                Case "Acos" : cudaName = "acos" & suffix
                Case "Atan" : cudaName = "atan" & suffix
                Case "Atan2" : cudaName = "atan2" & suffix
                Case "Sinh" : cudaName = "sinh" & suffix
                Case "Cosh" : cudaName = "cosh" & suffix
                Case "Tanh" : cudaName = "tanh" & suffix
                Case Else
                    Return False
            End Select

            Return True
        End Function

        ''' <summary>该调用是否走单精度版本</summary>
        Public Function PrefersSingle(callNode As CallExpression) As Boolean
            If callNode Is Nothing Then Return False
            If callNode.Type IsNot Nothing AndAlso callNode.Type = GetType(Single) Then Return True

            For Each arg In callNode.AllArguments
                If arg IsNot Nothing AndAlso arg.Type Is GetType(Single) Then Return True
            Next

            Return False
        End Function

        ' ------------------------------------------------------------------
        ' 字面量
        ' ------------------------------------------------------------------

        Public Function FormatLiteral(value As Object) As String
            If value Is Nothing Then Return "0"

            If TypeOf value Is Single Then
                Dim s = CSng(value)

                If Single.IsNaN(s) Then Return "nanf("""")"
                If Single.IsPositiveInfinity(s) Then Return "INFINITY"
                If Single.IsNegativeInfinity(s) Then Return "-INFINITY"

                Return s.ToString("0.0########", CultureInfo.InvariantCulture) & "f"
            End If

            If TypeOf value Is Double Then
                Dim d = CDbl(value)

                If Double.IsNaN(d) Then Return "nan("""")"
                If Double.IsPositiveInfinity(d) Then Return "INFINITY"
                If Double.IsNegativeInfinity(d) Then Return "-INFINITY"

                Return d.ToString("0.0#############", CultureInfo.InvariantCulture)
            End If

            If TypeOf value Is Boolean Then Return If(CBool(value), "1", "0")

            If TypeOf value Is Long OrElse TypeOf value Is ULong Then
                Return System.Convert.ToString(value, CultureInfo.InvariantCulture) & "LL"
            End If

            Return System.Convert.ToString(value, CultureInfo.InvariantCulture)
        End Function

        ' ------------------------------------------------------------------
        ' 运算符
        ' ------------------------------------------------------------------

        Public Function OperatorText(op As BinaryOperator) As String
            Select Case op
                Case BinaryOperator.Add : Return "+"
                Case BinaryOperator.Subtract : Return "-"
                Case BinaryOperator.Multiply : Return "*"
                Case BinaryOperator.Divide : Return "/"
                Case BinaryOperator.Modulo : Return "%"
                Case BinaryOperator.BitwiseAnd : Return "&"
                Case BinaryOperator.BitwiseOr : Return "|"
                Case BinaryOperator.BitwiseXor : Return "^"
                Case BinaryOperator.ShiftLeft : Return "<<"
                Case BinaryOperator.ShiftRight : Return ">>"
                Case BinaryOperator.Equal : Return "=="
                Case BinaryOperator.NotEqual : Return "!="
                Case BinaryOperator.LessThan : Return "<"
                Case BinaryOperator.LessThanOrEqual : Return "<="
                Case BinaryOperator.GreaterThan : Return ">"
                Case BinaryOperator.GreaterThanOrEqual : Return ">="
                Case BinaryOperator.ShortCircuitAnd : Return "&&"
                Case BinaryOperator.ShortCircuitOr : Return "||"
                Case Else
                    Throw New NotSupportedException($"运算符 {op} 无法映射为 CUDA 运算符")
            End Select
        End Function

        ''' <summary>C 标识符安全化（SSA 名字本身已合法，这里只兜底）</summary>
        Public Function SafeName(name As String) As String
            If String.IsNullOrEmpty(name) Then Return "v"

            Dim buffer As New System.Text.StringBuilder()

            For Each ch As Char In name
                If Char.IsLetterOrDigit(ch) OrElse ch = "_"c Then
                    buffer.Append(ch)
                Else
                    buffer.Append("_"c)
                End If
            Next

            Dim text = buffer.ToString()

            If Char.IsDigit(text(0)) Then text = "v_" & text

            Return text
        End Function
    End Module
End Namespace
