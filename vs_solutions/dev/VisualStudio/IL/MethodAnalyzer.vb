Imports System.Reflection
Imports System.Reflection.Emit
Imports Microsoft.VisualBasic.Serialization.JSON

Public Class MethodReference

    Public Property func As String
    Public Property parameter As String()
    Public Property body As String()
    Public Property [return] As String

    Public Overrides Function ToString() As String
        Return Me.GetJson
    End Function

End Class

Public Class MethodAnalyzer

    Private Shared ReadOnly OpCodeDict As Dictionary(Of UShort, OpCode)

    Shared Sub New()
        OpCodeDict = New Dictionary(Of UShort, OpCode)()
        Dim fields = GetType(OpCodes).GetFields(BindingFlags.Public Or BindingFlags.Static)
        For Each field In fields
            If field.FieldType = GetType(OpCode) Then
                Dim opCode = DirectCast(field.GetValue(Nothing), OpCode)
                OpCodeDict.Add(CUShort(opCode.Value), opCode)
            End If
        Next
    End Sub

    Private Function GetNormalizedTypeName(type As Type) As String
        If type Is Nothing Then Return String.Empty

        Dim name = type.FullName?.ToLowerInvariant()
        If type.IsGenericType Then
            name = type.GetGenericTypeDefinition().FullName?.ToLowerInvariant()
        End If
        Return If(name, String.Empty)
    End Function

    Public Function AnalyzeMethod(method As MethodInfo) As MethodReference
        ' 获取参数类型列表
        Dim parameters = method.GetParameters()
        Dim paramTypes = parameters.Select(Function(p) GetNormalizedTypeName(p.ParameterType)).ToList()
        ' 获取返回类型
        Dim returnType = GetNormalizedTypeName(method.ReturnType)

        ' 收集函数体内引用的类型
        Dim bodyTypes As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)

        ' 解析IL代码
        Dim methodBody = method.GetMethodBody()
        If methodBody IsNot Nothing Then
            Dim ilBytes = methodBody.GetILAsByteArray()
            Dim offset As Integer = 0
            Dim [module] = method.Module
            Dim currentAssembly = method.DeclaringType?.Assembly.GetName().Name

            While offset < ilBytes.Length
                Dim opCode = GetOpCode(ilBytes, offset)
                offset += opCode.Size

                ' 处理类型转换指令
                HandleConversionOpCodes(opCode, bodyTypes)

                ' 处理元数据令牌
                If opCode.OperandType = OperandType.InlineMethod OrElse
                   opCode.OperandType = OperandType.InlineType OrElse
                   opCode.OperandType = OperandType.InlineField OrElse
                   opCode.OperandType = OperandType.InlineTok Then

                    Dim token = BitConverter.ToInt32(ilBytes, offset)
                    offset += 4

                    Try
                        Select Case opCode.OperandType
                            Case OperandType.InlineMethod
                                Dim resolvedMethod = [module].ResolveMethod(token)
                                ProcessMethod(resolvedMethod, bodyTypes, currentAssembly)

                            Case OperandType.InlineType
                                Dim resolvedType = [module].ResolveType(token)
                                AddCustomType(resolvedType, bodyTypes, currentAssembly)

                            Case OperandType.InlineField
                                Dim resolvedField = [module].ResolveField(token)
                                AddCustomType(resolvedField.FieldType, bodyTypes, currentAssembly)
                                AddCustomType(resolvedField.DeclaringType, bodyTypes, currentAssembly)

                            Case OperandType.InlineTok
                                Dim member = [module].ResolveMember(token)
                                Select Case True
                                    Case TypeOf member Is Type
                                        AddCustomType(DirectCast(member, Type), bodyTypes, currentAssembly)
                                    Case TypeOf member Is MethodBase
                                        ProcessMethod(DirectCast(member, MethodBase), bodyTypes, currentAssembly)
                                    Case TypeOf member Is FieldInfo
                                        Dim f = DirectCast(member, FieldInfo)
                                        AddCustomType(f.FieldType, bodyTypes, currentAssembly)
                                        AddCustomType(f.DeclaringType, bodyTypes, currentAssembly)
                                End Select
                        End Select
                    Catch ex As Exception
                        ' 记录解析错误日志
                    End Try
                End If
            End While
        End If

        ' 过滤参数和返回类型
        bodyTypes.ExceptWith(paramTypes)
        bodyTypes.Remove(returnType)

        ' 构建结果对象
        Dim result As New MethodReference With {
            .func = method.Name,
            .body = bodyTypes.ToArray,
            .parameter = paramTypes.ToArray,
            .[return] = returnType
        }

        Return result
    End Function

    Private Sub AddCustomType(type As Type, ByRef types As HashSet(Of String), currentAssembly As String)
        If type Is Nothing Then Return

        ' 处理泛型类型
        If type.IsGenericType Then
            For Each genericArg In type.GetGenericArguments()
                AddCustomType(genericArg, types, currentAssembly)
            Next
        End If

        Dim typeName = GetNormalizedTypeName(type)
        If Not type.Assembly.GlobalAssemblyCache AndAlso
           type.Assembly.GetName().Name = currentAssembly Then
            types.Add(typeName)
        End If
    End Sub

    Private Sub HandleConversionOpCodes(opCode As OpCode, ByRef types As HashSet(Of String))
        Select Case opCode
            Case OpCodes.Conv_I1 : types.Add("system.sbyte")
            Case OpCodes.Conv_I2 : types.Add("system.int16")
            Case OpCodes.Conv_I4 : types.Add("system.int32")
            Case OpCodes.Conv_I8 : types.Add("system.int64")
            Case OpCodes.Conv_R4 : types.Add("system.single")
            Case OpCodes.Conv_R8 : types.Add("system.double")
            Case OpCodes.Conv_U1 : types.Add("system.byte")
            Case OpCodes.Conv_U2 : types.Add("system.uint16")
            Case OpCodes.Conv_U4 : types.Add("system.uint32")
            Case OpCodes.Conv_U8 : types.Add("system.uint64")
        End Select
    End Sub

    Private Shared Function GetOpCode(ilBytes As Byte(), ByRef position As Integer) As OpCode
        Dim opCodeValue As UShort = ilBytes(position)

        ' 处理双字节操作码
        If ilBytes(position) = &HFE Then
            If position + 1 >= ilBytes.Length Then
                Throw New InvalidOperationException("Invalid two-byte opcode")
            End If
            opCodeValue = CUShort((&HFE << 8) Or ilBytes(position + 1))
            position += 2
        Else
            position += 1
        End If

        Dim OpCode As OpCode

        If OpCodeDict.TryGetValue(opCodeValue, OpCode) Then
            Return OpCode
        Else
            Throw New InvalidOperationException($"Unknown opcode: 0x{opCodeValue:X2}")
        End If
    End Function

    Private Sub ProcessMethod(method As MethodBase, ByRef types As HashSet(Of String), currentAssembly As String)
        If method Is Nothing Then Return

        ' 添加方法声明类型
        AddCustomType(method.DeclaringType, types, currentAssembly)

        ' 处理返回类型
        If TypeOf method Is MethodInfo Then
            AddCustomType(DirectCast(method, MethodInfo).ReturnType, types, currentAssembly)
        End If

        ' 处理参数类型
        For Each param In method.GetParameters()
            AddCustomType(param.ParameterType, types, currentAssembly)
        Next
    End Sub

    Private Function GetOperandSize(operandType As OperandType) As Integer
        Select Case operandType
            Case OperandType.InlineNone
                Return 0
            Case OperandType.InlineI8, OperandType.InlineR
                Return 8
            Case OperandType.InlineBrTarget, OperandType.InlineField,
                 OperandType.InlineI, OperandType.InlineMethod,
                 OperandType.InlineSig, OperandType.InlineString,
                 OperandType.InlineTok, OperandType.InlineType,
                 OperandType.ShortInlineR
                Return 4
            Case OperandType.InlineVar
                Return 2
            Case OperandType.ShortInlineBrTarget,
                 OperandType.ShortInlineI, OperandType.ShortInlineVar
                Return 1
            Case Else
                Return 0
        End Select
    End Function
End Class