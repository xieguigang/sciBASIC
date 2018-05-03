Imports System
Imports System.ComponentModel
Imports System.Reflection
Imports System.Threading
Imports Microsoft.VisualBasic.CompilerServices.ConversionResolution
Imports Microsoft.VisualBasic.CompilerServices.Symbols

Namespace Microsoft.VisualBasic.CompilerServices
    <EditorBrowsable(EditorBrowsableState.Never), DynamicallyInvokableAttribute> _
    Public NotInheritable Class ObjectFlowControl
        ' Methods
        Private Sub New()
        End Sub

        <DynamicallyInvokableAttribute>
        Public Shared Sub CheckForSyncLockOnValueType(Expression As Object)
            If ((Not Expression Is Nothing) AndAlso Expression.GetType.IsValueType) Then
                Dim args As String() = New String() {Utils.VBFriendlyName(Expression.GetType)}
                Throw New ArgumentException(Utils.GetResourceString("SyncLockRequiresReferenceType1", args))
            End If
        End Sub


        ' Nested Types
        <EditorBrowsable(EditorBrowsableState.Never), DynamicallyInvokableAttribute>
        Public NotInheritable Class ForLoopControl
            ' Methods
            Private Sub New()
            End Sub

            Private Shared Function CheckContinueLoop(LoopFor As ForLoopControl) As Boolean
                If Not LoopFor.UseUserDefinedOperators Then
                    Try
                        Dim num As Integer = DirectCast(LoopFor.Counter, IComparable).CompareTo(LoopFor.Limit)
                        If LoopFor.PositiveStep Then
                            Return (num <= 0)
                        End If
                        Return (num >= 0)
                    Catch exception As InvalidCastException
                        Dim args As String() = New String() {"loop control variable", Utils.VBFriendlyName(LoopFor.Counter)}
                        Throw New ArgumentException(Utils.GetResourceString("Argument_IComparable2", args))
                    End Try
                End If
                If LoopFor.PositiveStep Then
                    Dim objArray1 As Object() = New Object() {LoopFor.Counter, LoopFor.Limit}
                    Return Conversions.ToBoolean(Operators.InvokeUserDefinedOperator(LoopFor.OperatorLessEqual, True, objArray1))
                End If
                Dim arguments As Object() = New Object() {LoopFor.Counter, LoopFor.Limit}
                Return Conversions.ToBoolean(Operators.InvokeUserDefinedOperator(LoopFor.OperatorGreaterEqual, True, arguments))
            End Function

            Private Shared Function ConvertLoopElement(ElementName As String, Value As Object, SourceType As Type, TargetType As Type) As Object
                Dim obj2 As Object
                Try
                    obj2 = Conversions.ChangeType(Value, TargetType)
                Catch exception As AccessViolationException
                    Throw exception
                Catch exception2 As StackOverflowException
                    Throw exception2
                Catch exception3 As OutOfMemoryException
                    Throw exception3
                Catch exception4 As ThreadAbortException
                    Throw exception4
                Catch exception8 As Exception
                    Dim args As String() = New String() {ElementName, Utils.VBFriendlyName(SourceType), Utils.VBFriendlyName(TargetType)}
                    Throw New ArgumentException(Utils.GetResourceString("ForLoop_ConvertToType3", args))
                End Try
                Return obj2
            End Function

            <DynamicallyInvokableAttribute>
            Public Shared Function ForLoopInitObj(Counter As Object, Start As Object, Limit As Object, StepValue As Object, ByRef LoopForResult As Object, ByRef CounterResult As Object) As Boolean
                If (Start Is Nothing) Then
                    Dim args As String() = New String() {"Start"}
                    Throw New ArgumentException(Utils.GetResourceString("Argument_InvalidNullValue1", args))
                End If
                If (Limit Is Nothing) Then
                    Dim textArray2 As String() = New String() {"Limit"}
                    Throw New ArgumentException(Utils.GetResourceString("Argument_InvalidNullValue1", textArray2))
                End If
                If (StepValue Is Nothing) Then
                    Dim textArray3 As String() = New String() {"Step"}
                    Throw New ArgumentException(Utils.GetResourceString("Argument_InvalidNullValue1", textArray3))
                End If
                Dim type As Type = Start.GetType
                Dim type2 As Type = Limit.GetType
                Dim type3 As Type = StepValue.GetType
                Dim type4 As Type = ForLoopControl.GetWidestType(type3, type, type2)
                If (type4 Is Nothing) Then
                    Dim textArray4 As String() = New String() {Utils.VBFriendlyName(type), Utils.VBFriendlyName(type2), Utils.VBFriendlyName(StepValue)}
                    Throw New ArgumentException(Utils.GetResourceString("ForLoop_CommonType3", textArray4))
                End If
                Dim loopFor As New ForLoopControl
                Dim typeCode As TypeCode = Symbols.GetTypeCode(type4)
                Select Case typeCode
                    Case TypeCode.Object
                        loopFor.UseUserDefinedOperators = True
                        Exit Select
                    Case TypeCode.String
                        typeCode = TypeCode.Double
                        Exit Select
                End Select
                Dim code2 As TypeCode = Type.GetTypeCode(type)
                Dim code3 As TypeCode = Type.GetTypeCode(type3)
                Dim type5 As Type = Nothing
                If ((code2 = typeCode) AndAlso type.IsEnum) Then
                    type5 = type
                End If
                If ((Type.GetTypeCode(type2) = typeCode) AndAlso type2.IsEnum) Then
                    If ((Not type5 Is Nothing) AndAlso (Not type5 Is type2)) Then
                        type5 = Nothing
                        GoTo Label_0142
                    End If
                    type5 = type2
                End If
                If ((code3 = typeCode) AndAlso type3.IsEnum) Then
                    If ((Not type5 Is Nothing) AndAlso (Not type5 Is type3)) Then
                        type5 = Nothing
                    Else
                        type5 = type3
                    End If
                End If
Label_0142:
                loopFor.EnumType = type5
                If Not loopFor.UseUserDefinedOperators Then
                    loopFor.WidestType = Symbols.MapTypeCodeToType(typeCode)
                Else
                    loopFor.WidestType = type4
                End If
                loopFor.WidestTypeCode = typeCode
                loopFor.Counter = ForLoopControl.ConvertLoopElement("Start", Start, type, loopFor.WidestType)
                loopFor.Limit = ForLoopControl.ConvertLoopElement("Limit", Limit, type2, loopFor.WidestType)
                loopFor.StepValue = ForLoopControl.ConvertLoopElement("Step", StepValue, type3, loopFor.WidestType)
                If loopFor.UseUserDefinedOperators Then
                    loopFor.OperatorPlus = ForLoopControl.VerifyForLoopOperator(UserDefinedOperator.Plus, loopFor.Counter, loopFor.WidestType)
                    ForLoopControl.VerifyForLoopOperator(UserDefinedOperator.Minus, loopFor.Counter, loopFor.WidestType)
                    loopFor.OperatorLessEqual = ForLoopControl.VerifyForLoopOperator(UserDefinedOperator.LessEqual, loopFor.Counter, loopFor.WidestType)
                    loopFor.OperatorGreaterEqual = ForLoopControl.VerifyForLoopOperator(UserDefinedOperator.GreaterEqual, loopFor.Counter, loopFor.WidestType)
                End If
                loopFor.PositiveStep = Operators.ConditionalCompareObjectGreaterEqual(loopFor.StepValue, Operators.SubtractObject(loopFor.StepValue, loopFor.StepValue), False)
                LoopForResult = loopFor
                If (Not loopFor.EnumType Is Nothing) Then
                    CounterResult = [Enum].ToObject(loopFor.EnumType, loopFor.Counter)
                Else
                    CounterResult = loopFor.Counter
                End If
                Return ForLoopControl.CheckContinueLoop(loopFor)
            End Function

            <DynamicallyInvokableAttribute>
            Public Shared Function ForNextCheckDec(count As Decimal, limit As Decimal, StepValue As Decimal) As Boolean
                If (Decimal.Compare(StepValue, Decimal.Zero) >= 0) Then
                    Return (Decimal.Compare(count, limit) <= 0)
                End If
                Return (Decimal.Compare(count, limit) >= 0)
            End Function

            <DynamicallyInvokableAttribute>
            Public Shared Function ForNextCheckObj(Counter As Object, LoopObj As Object, ByRef CounterResult As Object) As Boolean
                If (LoopObj Is Nothing) Then
                    Throw ExceptionUtils.VbMakeException(&H5C)
                End If
                If (Counter Is Nothing) Then
                    Dim args As String() = New String() {"Counter"}
                    Throw New NullReferenceException(Utils.GetResourceString("Argument_InvalidNullValue1", args))
                End If
                Dim loopFor As ForLoopControl = DirectCast(LoopObj, ForLoopControl)
                Dim flag2 As Boolean = False
                If Not loopFor.UseUserDefinedOperators Then
                    Dim typeCode As TypeCode = DirectCast(Counter, IConvertible).GetTypeCode
                    If ((typeCode <> loopFor.WidestTypeCode) OrElse (typeCode = TypeCode.String)) Then
                        If (typeCode = TypeCode.Object) Then
                            Dim textArray2 As String() = New String() {Utils.VBFriendlyName(Symbols.MapTypeCodeToType(typeCode)), Utils.VBFriendlyName(loopFor.WidestType)}
                            Throw New ArgumentException(Utils.GetResourceString("ForLoop_CommonType2", textArray2))
                        End If
                        Dim code2 As TypeCode = Symbols.GetTypeCode(ForLoopControl.GetWidestType(Symbols.MapTypeCodeToType(typeCode), loopFor.WidestType))
                        If (code2 = TypeCode.String) Then
                            code2 = TypeCode.Double
                        End If
                        loopFor.WidestTypeCode = code2
                        loopFor.WidestType = Symbols.MapTypeCodeToType(code2)
                        flag2 = True
                    End If
                End If
                If (flag2 OrElse loopFor.UseUserDefinedOperators) Then
                    Counter = ForLoopControl.ConvertLoopElement("Start", Counter, Counter.GetType, loopFor.WidestType)
                    If Not loopFor.UseUserDefinedOperators Then
                        loopFor.Limit = ForLoopControl.ConvertLoopElement("Limit", loopFor.Limit, loopFor.Limit.GetType, loopFor.WidestType)
                        loopFor.StepValue = ForLoopControl.ConvertLoopElement("Step", loopFor.StepValue, loopFor.StepValue.GetType, loopFor.WidestType)
                    End If
                End If
                If Not loopFor.UseUserDefinedOperators Then
                    loopFor.Counter = Operators.AddObject(Counter, loopFor.StepValue)
                    Dim code3 As TypeCode = DirectCast(loopFor.Counter, IConvertible).GetTypeCode
                    If (Not loopFor.EnumType Is Nothing) Then
                        CounterResult = [Enum].ToObject(loopFor.EnumType, loopFor.Counter)
                    Else
                        CounterResult = loopFor.Counter
                    End If
                    If (code3 <> loopFor.WidestTypeCode) Then
                        loopFor.Limit = Conversions.ChangeType(loopFor.Limit, Symbols.MapTypeCodeToType(code3))
                        loopFor.StepValue = Conversions.ChangeType(loopFor.StepValue, Symbols.MapTypeCodeToType(code3))
                        Return False
                    End If
                Else
                    Dim arguments As Object() = New Object() {Counter, loopFor.StepValue}
                    loopFor.Counter = Operators.InvokeUserDefinedOperator(loopFor.OperatorPlus, True, arguments)
                    If (Not loopFor.Counter.GetType Is loopFor.WidestType) Then
                        loopFor.Counter = ForLoopControl.ConvertLoopElement("Start", loopFor.Counter, loopFor.Counter.GetType, loopFor.WidestType)
                    End If
                    CounterResult = loopFor.Counter
                End If
                Return ForLoopControl.CheckContinueLoop(loopFor)
            End Function

            <DynamicallyInvokableAttribute>
            Public Shared Function ForNextCheckR4(count As Single, limit As Single, StepValue As Single) As Boolean
                If (StepValue >= 0!) Then
                    Return (count <= limit)
                End If
                Return (count >= limit)
            End Function

            <DynamicallyInvokableAttribute>
            Public Shared Function ForNextCheckR8(count As Double, limit As Double, StepValue As Double) As Boolean
                If (StepValue >= 0) Then
                    Return (count <= limit)
                End If
                Return (count >= limit)
            End Function

            Private Shared Function GetWidestType(Type1 As Type, Type2 As Type) As Type
                If ((Not Type1 Is Nothing) AndAlso (Not Type2 Is Nothing)) Then
                    If (Not Type1.IsEnum AndAlso Not Type2.IsEnum) Then
                        Dim typeCode As TypeCode = Symbols.GetTypeCode(Type1)
                        Dim code2 As TypeCode = Symbols.GetTypeCode(Type2)
                        If (Symbols.IsNumericType(typeCode) AndAlso Symbols.IsNumericType(code2)) Then
                            Return Symbols.MapTypeCodeToType(ConversionResolution.ForLoopWidestTypeCode(CInt(typeCode))(CInt(code2)))
                        End If
                    End If
                    Dim operatorMethod As Method = Nothing
                    Select Case ConversionResolution.ClassifyConversion(Type2, Type1, operatorMethod)
                        Case ConversionClass.Identity, ConversionClass.Widening
                            Return Type2
                    End Select
                    operatorMethod = Nothing
                    If (ConversionResolution.ClassifyConversion(Type1, Type2, operatorMethod) = ConversionClass.Widening) Then
                        Return Type1
                    End If
                End If
                Return Nothing
            End Function

            Private Shared Function GetWidestType(Type1 As Type, Type2 As Type, Type3 As Type) As Type
                Return ForLoopControl.GetWidestType(Type1, ForLoopControl.GetWidestType(Type2, Type3))
            End Function

            Private Shared Function VerifyForLoopOperator(Op As UserDefinedOperator, ForLoopArgument As Object, ForLoopArgumentType As Type) As Method
                Dim arguments As Object() = New Object() {ForLoopArgument, ForLoopArgument}
                Dim callableUserDefinedOperator As Method = Operators.GetCallableUserDefinedOperator(Op, arguments)
                If (callableUserDefinedOperator Is Nothing) Then
                    Dim args As String() = New String() {Utils.VBFriendlyNameOfType(ForLoopArgumentType, True), Symbols.OperatorNames(CInt(Op))}
                    Throw New ArgumentException(Utils.GetResourceString("ForLoop_OperatorRequired2", args))
                End If
                Dim info As MethodInfo = TryCast(callableUserDefinedOperator.AsMethod, MethodInfo)
                Dim parameters As ParameterInfo() = info.GetParameters
                Dim [operator] As UserDefinedOperator = Op
                If ([operator] <= UserDefinedOperator.Minus) Then
                    Select Case [operator]
                        Case UserDefinedOperator.Plus, UserDefinedOperator.Minus
                            If (((parameters.Length = 2) AndAlso (parameters(0).ParameterType Is ForLoopArgumentType)) AndAlso ((parameters(1).ParameterType Is ForLoopArgumentType) AndAlso (info.ReturnType Is ForLoopArgumentType))) Then
                                Return callableUserDefinedOperator
                            End If
                            Dim textArray2 As String() = New String() {callableUserDefinedOperator.ToString, Utils.VBFriendlyNameOfType(ForLoopArgumentType, True)}
                            Throw New ArgumentException(Utils.GetResourceString("ForLoop_UnacceptableOperator2", textArray2))
                    End Select
                    Return callableUserDefinedOperator
                End If
                Select Case [operator]
                    Case UserDefinedOperator.LessEqual, UserDefinedOperator.GreaterEqual
                        If (((parameters.Length = 2) AndAlso (parameters(0).ParameterType Is ForLoopArgumentType)) AndAlso (parameters(1).ParameterType Is ForLoopArgumentType)) Then
                            Return callableUserDefinedOperator
                        End If
                        Dim textArray3 As String() = New String() {callableUserDefinedOperator.ToString, Utils.VBFriendlyNameOfType(ForLoopArgumentType, True)}
                        Throw New ArgumentException(Utils.GetResourceString("ForLoop_UnacceptableRelOperator2", textArray3))
                End Select
                Return callableUserDefinedOperator
            End Function


            ' Fields
            Private Counter As Object
            Private EnumType As Type
            Private Limit As Object
            Private OperatorGreaterEqual As Method
            Private OperatorLessEqual As Method
            Private OperatorPlus As Method
            Private PositiveStep As Boolean
            Private StepValue As Object
            Private UseUserDefinedOperators As Boolean
            Private WidestType As Type
            Private WidestTypeCode As TypeCode
        End Class
    End Class
End Namespace

