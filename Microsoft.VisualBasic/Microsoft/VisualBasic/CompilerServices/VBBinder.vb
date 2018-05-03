Imports Microsoft.VisualBasic
Imports System
Imports System.ComponentModel
Imports System.Diagnostics
Imports System.Globalization
Imports System.Reflection
Imports System.Runtime.Remoting
Imports System.Security
Imports System.Security.Permissions

Namespace Microsoft.VisualBasic.CompilerServices
    Friend NotInheritable Class VBBinder
        Inherits Binder
        ' Methods
        Public Sub New(CopyBack As Boolean())
            Me.m_ByRefFlags = CopyBack
        End Sub

        Private Function BindingScore(Parameters As ParameterInfo(), paramOrder As Integer(), ArgTypes As Type(), IsPropertySet As Boolean, ParamArrayIndex As Integer) As BindScore
            Dim exact As BindScore = BindScore.Exact
            Dim upperBound As Integer = ArgTypes.GetUpperBound(0)
            Dim num4 As Integer = Parameters.GetUpperBound(0)
            If IsPropertySet Then
                num4 -= 1
                upperBound -= 1
            End If
            Dim num5 As Integer = Math.Max(upperBound, num4)
            Dim i As Integer = 0
            Do While (i <= num5)
                Dim type As Type
                Dim num As Integer
                If (paramOrder Is Nothing) Then
                    num = i
                Else
                    num = paramOrder(i)
                End If
                If (num = -1) Then
                    type = Nothing
                Else
                    type = ArgTypes(num)
                End If
                If (Not type Is Nothing) Then
                    Dim parameterType As Type
                    If (i > num4) Then
                        parameterType = Parameters(ParamArrayIndex).ParameterType
                    Else
                        parameterType = Parameters(i).ParameterType
                    End If
                    If (((i <> ParamArrayIndex) OrElse Not type.IsArray) OrElse (Not parameterType Is type)) Then
                        If (((i = ParamArrayIndex) AndAlso type.IsArray) AndAlso (((Me.m_state.m_OriginalArgs Is Nothing) OrElse (Me.m_state.m_OriginalArgs(num) Is Nothing)) OrElse parameterType.IsInstanceOfType(Me.m_state.m_OriginalArgs(num)))) Then
                            If (exact < BindScore.Widening1) Then
                                exact = BindScore.Widening1
                            End If
                        Else
                            If (((ParamArrayIndex <> -1) AndAlso (i >= ParamArrayIndex)) OrElse parameterType.IsByRef) Then
                                parameterType = parameterType.GetElementType
                            End If
                            If (Not type Is parameterType) Then
                                If ObjectType.IsWideningConversion(type, parameterType) Then
                                    If (exact < BindScore.Widening1) Then
                                        exact = BindScore.Widening1
                                    End If
                                ElseIf (type.IsArray AndAlso (((Me.m_state.m_OriginalArgs Is Nothing) OrElse (Me.m_state.m_OriginalArgs(num) Is Nothing)) OrElse parameterType.IsInstanceOfType(Me.m_state.m_OriginalArgs(num)))) Then
                                    If (exact < BindScore.Widening1) Then
                                        exact = BindScore.Widening1
                                    End If
                                Else
                                    exact = BindScore.Narrowing
                                End If
                            End If
                        End If
                    End If
                End If
                i += 1
            Loop
            Return exact
        End Function

        Public Overrides Function BindToField(bindingAttr As BindingFlags, match As FieldInfo(), value As Object, culture As CultureInfo) As FieldInfo
            If (((Not Me.m_CachedMember Is Nothing) AndAlso (Me.m_CachedMember.MemberType = MemberTypes.Field)) AndAlso ((Not match(0) Is Nothing) AndAlso (match(0).Name = Me.m_CachedMember.Name))) Then
                Return DirectCast(Me.m_CachedMember, FieldInfo)
            End If
            Dim info As FieldInfo = match(0)
            Dim upperBound As Integer = match.GetUpperBound(0)
            Dim i As Integer = 1
            Do While (i <= upperBound)
                If match(i).DeclaringType.IsSubclassOf(info.DeclaringType) Then
                    info = match(i)
                End If
                i += 1
            Loop
            Return info
        End Function

        Public Overrides Function BindToMethod(bindingAttr As BindingFlags, match As MethodBase(), ByRef args As Object(), modifiers As ParameterModifier(), culture As CultureInfo, names As String(), ByRef ObjState As Object) As MethodBase
            Dim flag As Boolean
            Dim num2 As Integer
            Dim base4 As MethodBase
            Dim info As ParameterInfo
            Dim num3 As Integer
            Dim num5 As Integer
            Dim num6 As Integer
            Dim infoArray As ParameterInfo()
            Dim type As Type = Nothing
            Dim parameterType As Type = Nothing
            Dim elementType As Type = Nothing
            Dim num7 As Integer
            Dim num8 As Integer
            Dim num9 As Integer
            Dim numArray2 As Integer()
            If ((match Is Nothing) OrElse (match.Length = 0)) Then
                Throw ExceptionUtils.VbMakeException(&H1B6)
            End If
            If (((Not Me.m_CachedMember Is Nothing) AndAlso (Me.m_CachedMember.MemberType = MemberTypes.Method)) AndAlso ((Not match(0) Is Nothing) AndAlso (match(0).Name = Me.m_CachedMember.Name))) Then
                Return DirectCast(Me.m_CachedMember, MethodBase)
            End If
            Dim isPropertySet As Boolean = ((bindingAttr And BindingFlags.SetProperty) > BindingFlags.Default)
            If ((Not names Is Nothing) AndAlso (names.Length = 0)) Then
                names = Nothing
            End If
            Dim length As Integer = match.Length
            If (length > 1) Then
                Dim num12 As Integer = match.GetUpperBound(0)
                num3 = 0
                Do While (num3 <= num12)
                    base4 = match(num3)
                    If ((Not base4 Is Nothing) AndAlso Not base4.IsHideBySig) Then
                        If base4.IsVirtual Then
                            If ((base4.Attributes And MethodAttributes.NewSlot) <> MethodAttributes.PrivateScope) Then
                                Dim num14 As Integer = match.GetUpperBound(0)
                                Dim i As Integer = 0
                                Do While (i <= num14)
                                    If (((num3 <> i) AndAlso (Not match(i) Is Nothing)) AndAlso base4.DeclaringType.IsSubclassOf(match(i).DeclaringType)) Then
                                        match(i) = Nothing
                                        length -= 1
                                    End If
                                    i += 1
                                Loop
                            End If
                        Else
                            Dim num16 As Integer = match.GetUpperBound(0)
                            Dim j As Integer = 0
                            Do While (j <= num16)
                                If (((num3 <> j) AndAlso (Not match(j) Is Nothing)) AndAlso base4.DeclaringType.IsSubclassOf(match(j).DeclaringType)) Then
                                    match(j) = Nothing
                                    length -= 1
                                End If
                                j += 1
                            Loop
                        End If
                    End If
                    num3 += 1
                Loop
            End If
            Dim num10 As Integer = length
            If (Not names Is Nothing) Then
                Dim num17 As Integer = match.GetUpperBound(0)
                num3 = 0
                Do While (num3 <= num17)
                    base4 = match(num3)
                    If (Not base4 Is Nothing) Then
                        infoArray = base4.GetParameters
                        num9 = infoArray.GetUpperBound(0)
                        If isPropertySet Then
                            num9 -= 1
                        End If
                        If (num9 >= 0) Then
                            info = infoArray(num9)
                            num7 = -1
                            If info.ParameterType.IsArray Then
                                Dim customAttributes As Object() = info.GetCustomAttributes(GetType(ParamArrayAttribute), False)
                                If ((Not customAttributes Is Nothing) AndAlso (customAttributes.Length > 0)) Then
                                    num7 = num9
                                Else
                                    num7 = -1
                                End If
                            End If
                        End If
                        Dim num18 As Integer = names.GetUpperBound(0)
                        Dim k As Integer = 0
                        Do While (k <= num18)
                            Dim num19 As Integer = num9
                            num5 = 0
                            Do While (num5 <= num19)
                                If (Strings.StrComp(names(k), infoArray(num5).Name, CompareMethod.Text) = 0) Then
                                    If ((num5 = num7) AndAlso (length = 1)) Then
                                        Throw ExceptionUtils.VbMakeExceptionEx(&H1BE, Utils.GetResourceString("NamedArgumentOnParamArray"))
                                    End If
                                    If (num5 = num7) Then
                                        num5 = (num9 + 1)
                                    End If
                                    Exit Do
                                End If
                                num5 += 1
                            Loop
                            If (num5 > num9) Then
                                If (length = 1) Then
                                    Dim textArray1 As String() = New String() {names(k), Me.CalledMethodName}
                                    Throw New MissingMemberException(Utils.GetResourceString("Argument_InvalidNamedArg2", textArray1))
                                End If
                                match(num3) = Nothing
                                length -= 1
                                Exit Do
                            End If
                            k += 1
                        Loop
                    End If
                    num3 += 1
                Loop
            End If
            Dim numArray As Integer() = New Integer(((match.Length - 1) + 1) - 1) {}
            Dim upperBound As Integer = match.GetUpperBound(0)
            num3 = 0
            Do While (num3 <= upperBound)
                base4 = match(num3)
                If (Not base4 Is Nothing) Then
                    num7 = -1
                    infoArray = base4.GetParameters
                    num9 = infoArray.GetUpperBound(0)
                    If isPropertySet Then
                        num9 -= 1
                    End If
                    If (num9 >= 0) Then
                        info = infoArray(num9)
                        If info.ParameterType.IsArray Then
                            Dim objArray3 As Object() = info.GetCustomAttributes(GetType(ParamArrayAttribute), False)
                            If ((Not objArray3 Is Nothing) AndAlso (objArray3.Length > 0)) Then
                                num7 = num9
                            End If
                        End If
                    End If
                    numArray(num3) = num7
                    If ((num7 = -1) AndAlso (args.Length > infoArray.Length)) Then
                        If (length = 1) Then
                            Dim textArray2 As String() = New String() {Me.CalledMethodName, Conversions.ToString(Me.GetPropArgCount(args, isPropertySet))}
                            Throw New MissingMemberException(Utils.GetResourceString("NoMethodTakingXArguments2", textArray2))
                        End If
                        match(num3) = Nothing
                        length -= 1
                    End If
                    Dim num21 As Integer = num9
                    If (num7 <> -1) Then
                        num21 -= 1
                    End If
                    If (args.Length < num21) Then
                        Dim num23 As Integer = (num21 - 1)
                        Dim index As Integer = args.Length
                        Do While (index <= num23)
                            If (infoArray(index).DefaultValue Is DBNull.Value) Then
                                Exit Do
                            End If
                            index += 1
                        Loop
                        If (index <> num21) Then
                            If (length = 1) Then
                                Dim textArray3 As String() = New String() {Me.CalledMethodName, Conversions.ToString(Me.GetPropArgCount(args, isPropertySet))}
                                Throw New MissingMemberException(Utils.GetResourceString("NoMethodTakingXArguments2", textArray3))
                            End If
                            match(num3) = Nothing
                            length -= 1
                        End If
                    End If
                End If
                num3 += 1
            Loop
            Dim paramOrder As Object() = New Object(((match.Length - 1) + 1) - 1) {}
            Dim num24 As Integer = match.GetUpperBound(0)
            num3 = 0
            Do While (num3 <= num24)
                base4 = match(num3)
                If (Not base4 Is Nothing) Then
                    infoArray = base4.GetParameters
                    If (args.Length > infoArray.Length) Then
                        numArray2 = New Integer(((args.Length - 1) + 1) - 1) {}
                    Else
                        numArray2 = New Integer(((infoArray.Length - 1) + 1) - 1) {}
                    End If
                    paramOrder(num3) = numArray2
                    If (names Is Nothing) Then
                        Dim num25 As Integer = args.GetUpperBound(0)
                        If isPropertySet Then
                            num25 -= 1
                        End If
                        Dim num26 As Integer = num25
                        num6 = 0
                        Do While (num6 <= num26)
                            If (TypeOf args(num6) Is Missing AndAlso ((num6 > infoArray.GetUpperBound(0)) OrElse infoArray(num6).IsOptional)) Then
                                numArray2(num6) = -1
                            Else
                                numArray2(num6) = num6
                            End If
                            num6 += 1
                        Loop
                        num25 = numArray2.GetUpperBound(0)
                        Dim num27 As Integer = num25
                        num6 = num6
                        Do While (num6 <= num27)
                            numArray2(num6) = -1
                            num6 += 1
                        Loop
                        If isPropertySet Then
                            numArray2(num25) = args.GetUpperBound(0)
                        End If
                    Else
                        Dim exception As Exception = Me.CreateParamOrder(isPropertySet, numArray2, base4.GetParameters, args, names)
                        If (Not exception Is Nothing) Then
                            If (length = 1) Then
                                Throw exception
                            End If
                            match(num3) = Nothing
                            length -= 1
                        End If
                    End If
                End If
                num3 += 1
            Loop
            Dim argTypes As Type() = New Type(((args.Length - 1) + 1) - 1) {}
            Dim num28 As Integer = args.GetUpperBound(0)
            num6 = 0
            Do While (num6 <= num28)
                If (Not args(num6) Is Nothing) Then
                    argTypes(num6) = args(num6).GetType
                End If
                num6 += 1
            Loop
            Dim num29 As Integer = match.GetUpperBound(0)
            num3 = 0
            Do While (num3 <= num29)
                base4 = match(num3)
                If (Not base4 Is Nothing) Then
                    infoArray = base4.GetParameters
                    numArray2 = DirectCast(paramOrder(num3), Integer())
                    num9 = numArray2.GetUpperBound(0)
                    If isPropertySet Then
                        num9 -= 1
                    End If
                    num7 = numArray(num3)
                    If (num7 <> -1) Then
                        elementType = infoArray(num7).ParameterType.GetElementType
                    ElseIf (numArray2.Length > infoArray.Length) Then
                        GoTo Label_08AB
                    End If
                    Dim num30 As Integer = num9
                    num5 = 0
                    Do While (num5 <= num30)
                        Dim empty As TypeCode
                        num6 = numArray2(num5)
                        If (num6 = -1) Then
                            If (Not infoArray(num5).IsOptional AndAlso (num5 <> numArray(num3))) Then
                                If (length = 1) Then
                                    Dim textArray4 As String() = New String() {Me.CalledMethodName, Conversions.ToString(Me.GetPropArgCount(args, isPropertySet))}
                                    Throw New MissingMemberException(Utils.GetResourceString("NoMethodTakingXArguments2", textArray4))
                                End If
                                GoTo Label_08AB
                            End If
                        Else
                            type = argTypes(num6)
                            If (Not type Is Nothing) Then
                                If ((num7 <> -1) AndAlso (num5 > num7)) Then
                                    parameterType = infoArray(num7).ParameterType.GetElementType
                                Else
                                    parameterType = infoArray(num5).ParameterType
                                    If parameterType.IsByRef Then
                                        parameterType = parameterType.GetElementType
                                    End If
                                    If (num5 = num7) Then
                                        If (parameterType.IsInstanceOfType(args(num6)) AndAlso (num5 = num9)) Then
                                            GoTo Label_089A
                                        End If
                                        parameterType = elementType
                                    End If
                                End If
                                If ((Not parameterType Is type) AndAlso ((Not type Is Type.Missing) OrElse Not infoArray(num5).IsOptional)) Then
                                    If (args(num6) Is Missing.Value) Then
                                        GoTo Label_08AB
                                    End If
                                    If ((Not parameterType Is GetType(Object)) AndAlso Not parameterType.IsInstanceOfType(args(num6))) Then
                                        Dim typeCode As TypeCode = Type.GetTypeCode(parameterType)
                                        If (type Is Nothing) Then
                                            empty = TypeCode.Empty
                                        Else
                                            empty = Type.GetTypeCode(type)
                                        End If
                                        Select Case typeCode
                                            Case TypeCode.Boolean, TypeCode.Byte, TypeCode.Int16, TypeCode.Int32, TypeCode.Int64, TypeCode.Single, TypeCode.Double, TypeCode.Decimal
                                                Select Case empty
                                                    Case TypeCode.Boolean, TypeCode.Byte, TypeCode.Int16, TypeCode.Int32, TypeCode.Int64, TypeCode.Single, TypeCode.Double, TypeCode.Decimal, TypeCode.String
                                                        GoTo Label_089A
                                                End Select
                                                GoTo Label_08AB
                                            Case TypeCode.Char
                                                If (empty = TypeCode.String) Then
                                                    GoTo Label_089A
                                                End If
                                                GoTo Label_08AB
                                            Case TypeCode.DateTime
                                                If (empty = TypeCode.String) Then
                                                    GoTo Label_089A
                                                End If
                                                GoTo Label_08AB
                                            Case TypeCode.String
                                                Exit Select
                                            Case Else
                                                GoTo Label_0871
                                        End Select
                                        Select Case empty
                                            Case TypeCode.Empty, TypeCode.Boolean, TypeCode.Char, TypeCode.Byte, TypeCode.Int16, TypeCode.Int32, TypeCode.Int64, TypeCode.Single, TypeCode.Double, TypeCode.Decimal, TypeCode.String
                                                GoTo Label_089A
                                        End Select
                                        If (Not type Is GetType(Char())) Then
                                            GoTo Label_08AB
                                        End If
                                    End If
                                End If
                            End If
                        End If
                        GoTo Label_089A
Label_0871:
                        If (Not parameterType Is GetType(Char())) Then
                            GoTo Label_08AB
                        End If
                        If (empty <> TypeCode.Object) Then
                            If (empty = TypeCode.String) Then
                                GoTo Label_089A
                            End If
                            GoTo Label_08AB
                        End If
                        If (Not type Is GetType(Char())) Then
                            GoTo Label_08AB
                        End If
Label_089A:
                        num5 += 1
                    Loop
                End If
                Continue Do
Label_08AB:
                If (length = 1) Then
                    If (num10 <> 1) Then
                        Dim textArray5 As String() = New String() {Me.CalledMethodName}
                        Throw New AmbiguousMatchException(Utils.GetResourceString("AmbiguousMatch_NarrowingConversion1", textArray5))
                    End If
                    Me.ThrowInvalidCast(type, parameterType, num5)
                End If
                match(num3) = Nothing
                length -= 1
                num3 += 1
            Loop
            length = 0
            Dim num31 As Integer = match.GetUpperBound(0)
            num3 = 0
            Do While (num3 <= num31)
                base4 = match(num3)
                If (Not base4 Is Nothing) Then
                    numArray2 = DirectCast(paramOrder(num3), Integer())
                    infoArray = base4.GetParameters
                    Dim flag2 As Boolean = False
                    num9 = infoArray.GetUpperBound(0)
                    If isPropertySet Then
                        num9 -= 1
                    End If
                    num8 = args.GetUpperBound(0)
                    If isPropertySet Then
                        num8 -= 1
                    End If
                    num7 = numArray(num3)
                    If (num7 <> -1) Then
                        elementType = infoArray(num9).ParameterType.GetElementType
                    End If
                    Dim num32 As Integer = num9
                    num5 = 0
                    Do While (num5 <= num32)
                        Dim code4 As TypeCode
                        If (num5 = num7) Then
                            parameterType = elementType
                        Else
                            parameterType = infoArray(num5).ParameterType
                        End If
                        If parameterType.IsByRef Then
                            flag2 = True
                            parameterType = parameterType.GetElementType
                        End If
                        num6 = numArray2(num5)
                        If (((num6 <> -1) OrElse Not infoArray(num5).IsOptional) AndAlso (num5 <> numArray(num3))) Then
                            type = argTypes(num6)
                            If (((Not type Is Nothing) AndAlso ((Not type Is Type.Missing) OrElse Not infoArray(num5).IsOptional)) AndAlso ((Not parameterType Is type) AndAlso (Not parameterType Is GetType(Object)))) Then
                                Dim code3 As TypeCode = Type.GetTypeCode(parameterType)
                                If (type Is Nothing) Then
                                    code4 = TypeCode.Empty
                                Else
                                    code4 = Type.GetTypeCode(type)
                                End If
                                Select Case code3
                                    Case TypeCode.Boolean, TypeCode.Byte, TypeCode.Int16, TypeCode.Int32, TypeCode.Int64, TypeCode.Single, TypeCode.Double, TypeCode.Decimal
                                        GoTo Label_0A7A
                                End Select
                            End If
                        End If
                        GoTo Label_0AD2
Label_0A7A:
                        Select Case code4
                            Case TypeCode.Boolean, TypeCode.Byte, TypeCode.Int16, TypeCode.Int32, TypeCode.Int64, TypeCode.Single, TypeCode.Double, TypeCode.Decimal, TypeCode.String
                                Exit Select
                            Case Else
                                If (length = 0) Then
                                    Me.ThrowInvalidCast(type, parameterType, num5)
                                End If
                                Exit Select
                        End Select
Label_0AD2:
                        num5 += 1
                    Loop
                    If (num5 > num9) Then
                        If (num3 <> length) Then
                            match(length) = match(num3)
                            paramOrder(length) = paramOrder(num3)
                            numArray(length) = numArray(num3)
                            match(num3) = Nothing
                        End If
                        length += 1
                        If flag2 Then
                            flag = True
                        End If
                    Else
                        match(num3) = Nothing
                    End If
                End If
                num3 += 1
            Loop
            If (length = 0) Then
                Dim textArray6 As String() = New String() {Me.CalledMethodName, Conversions.ToString(Me.GetPropArgCount(args, isPropertySet))}
                Throw New MissingMemberException(Utils.GetResourceString("NoMethodTakingXArguments2", textArray6))
            End If
            Dim state As New VBBinderState
            Me.m_state = state
            ObjState = state
            state.m_OriginalArgs = args
            If (length = 1) Then
                num2 = 0
            Else
                num2 = 0
                Dim unknown As BindScore = BindScore.Unknown
                Dim num33 As Integer = 0
                Dim num34 As Integer = (length - 1)
                num3 = 0
                Do While (num3 <= num34)
                    base4 = match(num3)
                    If (Not base4 Is Nothing) Then
                        numArray2 = DirectCast(paramOrder(num3), Integer())
                        Dim score As BindScore = Me.BindingScore(base4.GetParameters, numArray2, argTypes, isPropertySet, numArray(num3))
                        If (score < unknown) Then
                            If (num3 <> 0) Then
                                match(0) = match(num3)
                                paramOrder(0) = paramOrder(num3)
                                numArray(0) = numArray(num3)
                                match(num3) = Nothing
                            End If
                            num33 = 1
                            unknown = score
                            Continue Do
                        End If
                        If (score = unknown) Then
                            If ((score = BindScore.Exact) OrElse (score = BindScore.Widening1)) Then
                                Dim flag4 As Boolean
                                Select Case Me.GetMostSpecific(match(0), base4, numArray2, paramOrder, isPropertySet, numArray(0), numArray(num3), args)
                                    Case -1
                                        If (num33 <> num3) Then
                                            match(num33) = match(num3)
                                            paramOrder(num33) = paramOrder(num3)
                                            numArray(num33) = numArray(num3)
                                            match(num3) = Nothing
                                        End If
                                        num33 += 1
                                        Continue Do
                                    Case 0
                                        Continue Do
                                    Case Else
                                        flag4 = True
                                        Dim num35 As Integer = (num33 - 1)
                                        Dim m As Integer = 1
                                        Do While (m <= num35)
                                            If (Me.GetMostSpecific(match(m), base4, numArray2, paramOrder, isPropertySet, numArray(m), numArray(num3), args) <> 1) Then
                                                flag4 = False
                                                Exit Do
                                            End If
                                            m += 1
                                        Loop
                                        Exit Select
                                End Select
                                If flag4 Then
                                    num33 = 0
                                End If
                                If (num3 <> num33) Then
                                    match(num33) = match(num3)
                                    paramOrder(num33) = paramOrder(num3)
                                    numArray(num33) = numArray(num3)
                                    match(num3) = Nothing
                                End If
                                num33 += 1
                                Continue Do
                            End If
                            If (num33 <> num3) Then
                                match(num33) = match(num3)
                                paramOrder(num33) = paramOrder(num3)
                                numArray(num33) = numArray(num3)
                                match(num3) = Nothing
                            End If
                            num33 += 1
                            Continue Do
                        End If
                        match(num3) = Nothing
                    End If
                    num3 += 1
                Loop
                If (num33 > 1) Then
                    Dim num37 As Integer = match.GetUpperBound(0)
                    num3 = 0
                    Do While (num3 <= num37)
                        base4 = match(num3)
                        If (Not base4 Is Nothing) Then
                            Dim num39 As Integer = match.GetUpperBound(0)
                            Dim n As Integer = 0
                            Do While (n <= num39)
                                If (((num3 <> n) AndAlso (Not match(n) Is Nothing)) AndAlso ((base4 Is match(n)) OrElse (base4.DeclaringType.IsSubclassOf(match(n).DeclaringType) AndAlso Me.MethodsDifferOnlyByReturnType(base4, match(n))))) Then
                                    match(n) = Nothing
                                    num33 -= 1
                                End If
                                n += 1
                            Loop
                        End If
                        num3 += 1
                    Loop
                    Dim num40 As Integer = match.GetUpperBound(0)
                    num3 = 0
                    Do While (num3 <= num40)
                        If (match(num3) Is Nothing) Then
                            Dim num42 As Integer = match.GetUpperBound(0)
                            Dim num41 As Integer = (num3 + 1)
                            Do While (num41 <= num42)
                                Dim base5 As MethodBase = match(num41)
                                If (Not base5 Is Nothing) Then
                                    match(num3) = base5
                                    paramOrder(num3) = paramOrder(num41)
                                    numArray(num3) = numArray(num41)
                                    match(num41) = Nothing
                                End If
                                num41 += 1
                            Loop
                        End If
                        num3 += 1
                    Loop
                End If
                If (num33 > 1) Then
                    Dim str As String = (ChrW(13) & ChrW(10) & "    " & Utils.MethodToString(match(0)))
                    Dim num43 As Integer = (num33 - 1)
                    num3 = 1
                    Do While (num3 <= num43)
                        str = (str & ChrW(13) & ChrW(10) & "    " & Utils.MethodToString(match(num3)))
                        num3 += 1
                    Loop
                    Select Case unknown
                        Case BindScore.Exact
                            Dim textArray7 As String() = New String() {Me.CalledMethodName, str}
                            Throw New AmbiguousMatchException(Utils.GetResourceString("AmbiguousCall_ExactMatch2", textArray7))
                        Case BindScore.Widening0, BindScore.Widening1
                            Dim textArray8 As String() = New String() {Me.CalledMethodName, str}
                            Throw New AmbiguousMatchException(Utils.GetResourceString("AmbiguousCall_WideningConversion2", textArray8))
                    End Select
                    Dim textArray9 As String() = New String() {Me.CalledMethodName, str}
                    Throw New AmbiguousMatchException(Utils.GetResourceString("AmbiguousCall2", textArray9))
                End If
            End If
            Dim base3 As MethodBase = match(num2)
            numArray2 = DirectCast(paramOrder(num2), Integer())
            If (Not names Is Nothing) Then
                Me.ReorderParams(numArray2, args, state)
            End If
            Dim parameters As ParameterInfo() = base3.GetParameters
            If (args.Length > 0) Then
                state.m_ByRefFlags = New Boolean((args.GetUpperBound(0) + 1) - 1) {}
                flag = False
                Dim num44 As Integer = parameters.GetUpperBound(0)
                num5 = 0
                Do While (num5 <= num44)
                    If parameters(num5).ParameterType.IsByRef Then
                        If (state.m_OriginalParamOrder Is Nothing) Then
                            If (num5 < state.m_ByRefFlags.Length) Then
                                state.m_ByRefFlags(num5) = True
                            End If
                        ElseIf (num5 < state.m_OriginalParamOrder.Length) Then
                            Dim num45 As Integer = state.m_OriginalParamOrder(num5)
                            If (num45 >= 0) Then
                                state.m_ByRefFlags(num45) = True
                            End If
                        End If
                        flag = True
                    End If
                    num5 += 1
                Loop
                If Not flag Then
                    state.m_ByRefFlags = Nothing
                End If
            Else
                state.m_ByRefFlags = Nothing
            End If
            num7 = numArray(num2)
            If (num7 <> -1) Then
                num9 = parameters.GetUpperBound(0)
                If isPropertySet Then
                    num9 -= 1
                End If
                num8 = args.GetUpperBound(0)
                If isPropertySet Then
                    num8 -= 1
                End If
                Dim objArray4 As Object() = New Object(((parameters.Length - 1) + 1) - 1) {}
                Dim num46 As Integer = (Math.Min(num8, num7) - 1)
                num5 = 0
                Do While (num5 <= num46)
                    objArray4(num5) = ObjectType.CTypeHelper(args(num5), parameters(num5).ParameterType)
                    num5 += 1
                Loop
                If (num8 < num7) Then
                    Dim num47 As Integer = (num7 - 1)
                    num5 = (num8 + 1)
                    Do While (num5 <= num47)
                        objArray4(num5) = ObjectType.CTypeHelper(parameters(num5).DefaultValue, parameters(num5).ParameterType)
                        num5 += 1
                    Loop
                End If
                If isPropertySet Then
                    Dim num48 As Integer = objArray4.GetUpperBound(0)
                    objArray4(num48) = ObjectType.CTypeHelper(args(args.GetUpperBound(0)), parameters(num48).ParameterType)
                End If
                If (num8 = -1) Then
                    objArray4(num7) = Array.CreateInstance(elementType, 0)
                Else
                    elementType = parameters(num9).ParameterType.GetElementType
                    Dim num49 As Integer = ((args.Length - parameters.Length) + 1)
                    parameterType = parameters(num9).ParameterType
                    If (((num49 = 1) AndAlso parameterType.IsArray) AndAlso ((args(num7) Is Nothing) OrElse parameterType.IsInstanceOfType(args(num7)))) Then
                        objArray4(num7) = args(num7)
                    ElseIf (elementType Is GetType(Object)) Then
                        Dim objArray5 As Object() = New Object(((num49 - 1) + 1) - 1) {}
                        Dim num50 As Integer = (num49 - 1)
                        num6 = 0
                        Do While (num6 <= num50)
                            objArray5(num6) = ObjectType.CTypeHelper(args((num6 + num7)), elementType)
                            num6 += 1
                        Loop
                        objArray4(num7) = objArray5
                    Else
                        Dim array As Array = Array.CreateInstance(elementType, num49)
                        Dim num51 As Integer = (num49 - 1)
                        num6 = 0
                        Do While (num6 <= num51)
                            array.SetValue(ObjectType.CTypeHelper(args((num6 + num7)), elementType), num6)
                            num6 += 1
                        Loop
                        objArray4(num7) = array
                    End If
                End If
                args = objArray4
            Else
                Dim objArray6 As Object() = New Object(((parameters.Length - 1) + 1) - 1) {}
                Dim num53 As Integer = objArray6.GetUpperBound(0)
                num6 = 0
                Do While (num6 <= num53)
                    Dim num52 As Integer = numArray2(num6)
                    If ((num52 >= 0) AndAlso (num52 <= args.GetUpperBound(0))) Then
                        objArray6(num6) = ObjectType.CTypeHelper(args(num52), parameters(num6).ParameterType)
                    Else
                        objArray6(num6) = ObjectType.CTypeHelper(parameters(num6).DefaultValue, parameters(num6).ParameterType)
                    End If
                    num6 += 1
                Loop
                Dim num54 As Integer = parameters.GetUpperBound(0)
                num5 = num6
                Do While (num5 <= num54)
                    objArray6(num5) = ObjectType.CTypeHelper(parameters(num5).DefaultValue, parameters(num5).ParameterType)
                    num5 += 1
                Loop
                args = objArray6
            End If
            If (base3 Is Nothing) Then
                Dim textArray10 As String() = New String() {Me.CalledMethodName, Conversions.ToString(Me.GetPropArgCount(args, isPropertySet))}
                Throw New MissingMemberException(Utils.GetResourceString("NoMethodTakingXArguments2", textArray10))
            End If
            Return base3
        End Function

        Friend Sub CacheMember(member As MemberInfo)
            Me.m_CachedMember = member
        End Sub

        Friend Function CalledMethodName() As String
            Return (Me.m_objType.Name & "." & Me.m_BindToName)
        End Function

        Public Overrides Function ChangeType(value As Object, typ As Type, culture As CultureInfo) As Object
            Dim obj2 As Object
            Try
                If ((typ Is GetType(Object)) OrElse (typ.IsByRef AndAlso (typ.GetElementType Is GetType(Object)))) Then
                    Return value
                End If
                obj2 = ObjectType.CTypeHelper(value, typ)
            Catch exception As Exception
                Dim args As String() = New String() {Utils.VBFriendlyName(value), Utils.VBFriendlyName(typ)}
                Throw New InvalidCastException(Utils.GetResourceString("InvalidCast_FromTo", args))
            End Try
            Return obj2
        End Function

        Private Function CreateParamOrder(SetProp As Boolean, paramOrder As Integer(), pars As ParameterInfo(), args As Object(), names As String()) As Exception
            Dim num As Integer
            Dim flagArray As Boolean() = New Boolean(((pars.Length - 1) + 1) - 1) {}
            Dim num3 As Integer = ((args.Length - names.Length) - 1)
            Dim upperBound As Integer = pars.GetUpperBound(0)
            Dim num5 As Integer = pars.GetUpperBound(0)
            num = 0
            Do While (num <= num5)
                paramOrder(num) = -1
                num += 1
            Loop
            If SetProp Then
                paramOrder(pars.GetUpperBound(0)) = args.GetUpperBound(0)
                num3 -= 1
                upperBound -= 1
            End If
            Dim num6 As Integer = num3
            num = 0
            Do While (num <= num6)
                paramOrder(num) = (names.Length + num)
                num += 1
            Loop
            Dim num7 As Integer = names.GetUpperBound(0)
            num = 0
            Do While (num <= num7)
                Dim num8 As Integer = upperBound
                Dim index As Integer = 0
                Do While (index <= num8)
                    If (Strings.StrComp(names(num), pars(index).Name, CompareMethod.Text) = 0) Then
                        If (paramOrder(index) <> -1) Then
                            Dim textArray1 As String() = New String() {pars(index).Name}
                            Return New ArgumentException(Utils.GetResourceString("NamedArgumentAlreadyUsed1", textArray1))
                        End If
                        paramOrder(index) = num
                        flagArray(num) = True
                        Exit Do
                    End If
                    index += 1
                Loop
                If (index > upperBound) Then
                    Dim textArray2 As String() = New String() {names(num), Me.CalledMethodName}
                    Return New MissingMemberException(Utils.GetResourceString("Argument_InvalidNamedArg2", textArray2))
                End If
                num += 1
            Loop
            Return Nothing
        End Function

        Private Function GetDefaultMemberName(typ As Type) As String
            Do
                Dim customAttributes As Object() = typ.GetCustomAttributes(GetType(DefaultMemberAttribute), False)
                If ((Not customAttributes Is Nothing) AndAlso (customAttributes.Length <> 0)) Then
                    Return DirectCast(customAttributes(0), DefaultMemberAttribute).MemberName
                End If
                typ = typ.BaseType
            Loop While (Not typ Is Nothing)
            Return Nothing
        End Function

        Private Function GetMethodsByName(objType As Type, objIReflect As IReflect, name As String, invokeAttr As BindingFlags) As MethodBase()
            Dim num2 As Integer
            Dim nonGenericMembers As MemberInfo() = LateBinding.GetNonGenericMembers(objIReflect.GetMember(name, invokeAttr))
            If (nonGenericMembers Is Nothing) Then
                Return Nothing
            End If
            Dim upperBound As Integer = nonGenericMembers.GetUpperBound(0)
            Dim i As Integer = 0
            Do While (i <= upperBound)
                Dim info As MemberInfo = nonGenericMembers(i)
                If (Not info Is Nothing) Then
                    Dim declaringType As Type
                    If (info.MemberType = MemberTypes.Field) Then
                        declaringType = info.DeclaringType
                        Dim num6 As Integer = nonGenericMembers.GetUpperBound(0)
                        Dim k As Integer = 0
                        Do While (k <= num6)
                            If (((i <> k) AndAlso (Not nonGenericMembers(k) Is Nothing)) AndAlso declaringType.IsSubclassOf(nonGenericMembers(k).DeclaringType)) Then
                                nonGenericMembers(k) = Nothing
                                num2 += 1
                            End If
                            k += 1
                        Loop
                    Else
                        Dim getMethod As MethodInfo
                        If (info.MemberType = MemberTypes.Method) Then
                            getMethod = DirectCast(info, MethodInfo)
                            If (Not getMethod.IsHideBySig AndAlso ((Not getMethod.IsVirtual OrElse (getMethod.IsVirtual AndAlso ((getMethod.Attributes And MethodAttributes.NewSlot) <> MethodAttributes.PrivateScope))) OrElse (getMethod.IsVirtual AndAlso ((getMethod.GetBaseDefinition.Attributes And MethodAttributes.NewSlot) <> MethodAttributes.PrivateScope)))) Then
                                declaringType = info.DeclaringType
                                Dim num8 As Integer = nonGenericMembers.GetUpperBound(0)
                                Dim m As Integer = 0
                                Do While (m <= num8)
                                    If (((i <> m) AndAlso (Not nonGenericMembers(m) Is Nothing)) AndAlso declaringType.IsSubclassOf(nonGenericMembers(m).DeclaringType)) Then
                                        nonGenericMembers(m) = Nothing
                                        num2 += 1
                                    End If
                                    m += 1
                                Loop
                            End If
                        ElseIf (info.MemberType = MemberTypes.Property) Then
                            Dim info3 As PropertyInfo = DirectCast(info, PropertyInfo)
                            Dim num9 As Integer = 1
                            Do
                                If (num9 = 1) Then
                                    getMethod = info3.GetGetMethod
                                Else
                                    getMethod = info3.GetSetMethod
                                End If
                                If (((Not getMethod Is Nothing) AndAlso Not getMethod.IsHideBySig) AndAlso (Not getMethod.IsVirtual OrElse (getMethod.IsVirtual AndAlso ((getMethod.Attributes And MethodAttributes.NewSlot) <> MethodAttributes.PrivateScope)))) Then
                                    declaringType = info.DeclaringType
                                    Dim num11 As Integer = nonGenericMembers.GetUpperBound(0)
                                    Dim n As Integer = 0
                                    Do While (n <= num11)
                                        If (((i <> n) AndAlso (Not nonGenericMembers(n) Is Nothing)) AndAlso declaringType.IsSubclassOf(nonGenericMembers(n).DeclaringType)) Then
                                            nonGenericMembers(n) = Nothing
                                            num2 += 1
                                        End If
                                        n += 1
                                    Loop
                                End If
                                num9 += 1
                            Loop While (num9 <= 2)
                            If ((invokeAttr And BindingFlags.GetProperty) <> BindingFlags.Default) Then
                                getMethod = info3.GetGetMethod
                            ElseIf ((invokeAttr And BindingFlags.SetProperty) <> BindingFlags.Default) Then
                                getMethod = info3.GetSetMethod
                            Else
                                getMethod = Nothing
                            End If
                            If (getMethod Is Nothing) Then
                                num2 += 1
                            End If
                            nonGenericMembers(i) = getMethod
                        ElseIf (info.MemberType = MemberTypes.NestedType) Then
                            declaringType = info.DeclaringType
                            Dim num13 As Integer = nonGenericMembers.GetUpperBound(0)
                            Dim num12 As Integer = 0
                            Do While (num12 <= num13)
                                If (((i <> num12) AndAlso (Not nonGenericMembers(num12) Is Nothing)) AndAlso declaringType.IsSubclassOf(nonGenericMembers(num12).DeclaringType)) Then
                                    nonGenericMembers(num12) = Nothing
                                    num2 += 1
                                End If
                                num12 += 1
                            Loop
                            If (num2 = (nonGenericMembers.Length - 1)) Then
                                Dim args As String() = New String() {name, Utils.VBFriendlyName(objType)}
                                Throw New ArgumentException(Utils.GetResourceString("Argument_IllegalNestedType2", args))
                            End If
                            nonGenericMembers(i) = Nothing
                            num2 += 1
                        End If
                    End If
                End If
                i += 1
            Loop
            Dim baseArray2 As MethodBase() = New MethodBase((((nonGenericMembers.Length - num2) - 1) + 1) - 1) {}
            Dim index As Integer = 0
            Dim num14 As Integer = (nonGenericMembers.Length - 1)
            Dim j As Integer = 0
            Do While (j <= num14)
                If (Not nonGenericMembers(j) Is Nothing) Then
                    baseArray2(index) = DirectCast(nonGenericMembers(j), MethodBase)
                    index += 1
                End If
                j += 1
            Loop
            Return baseArray2
        End Function

        Private Function GetMostSpecific(match0 As MethodBase, ThisMethod As MethodBase, ArgIndexes As Integer(), ParamOrder As Object(), IsPropertySet As Boolean, ParamArrayIndex0 As Integer, ParamArrayIndex1 As Integer, args As Object()) As Integer
            Dim flag As Boolean
            Dim flag2 As Boolean
            Dim num As Integer = -1
            Dim fromType As Type = Nothing
            Dim toType As Type = Nothing
            Dim upperBound As Integer = args.GetUpperBound(0)
            Dim parameters As ParameterInfo() = ThisMethod.GetParameters
            Dim infoArray As ParameterInfo() = match0.GetParameters
            Dim numArray As Integer() = DirectCast(ParamOrder(0), Integer())
            num = -1
            Dim index As Integer = args.GetUpperBound(0)
            Dim num5 As Integer = infoArray.GetUpperBound(0)
            Dim num6 As Integer = parameters.GetUpperBound(0)
            If IsPropertySet Then
                num5 -= 1
                num6 -= 1
                index -= 1
                upperBound -= 1
            End If
            If (ParamArrayIndex0 = -1) Then
                flag = False
            Else
                fromType = infoArray(ParamArrayIndex0).ParameterType.GetElementType
                flag = True
                If ((index <> -1) AndAlso (index = num5)) Then
                    Dim o As Object = args(index)
                    If ((o Is Nothing) OrElse infoArray(num5).ParameterType.IsInstanceOfType(o)) Then
                        flag = False
                    End If
                End If
            End If
            If (ParamArrayIndex1 = -1) Then
                flag2 = False
            Else
                toType = parameters(ParamArrayIndex1).ParameterType.GetElementType
                flag2 = True
                If ((index <> -1) AndAlso (index = num6)) Then
                    Dim obj3 As Object = args(index)
                    If ((obj3 Is Nothing) OrElse parameters(num6).ParameterType.IsInstanceOfType(obj3)) Then
                        flag2 = False
                    End If
                End If
            End If
            Dim num9 As Integer = Math.Min(upperBound, Math.Max(num5, num6))
            Dim i As Integer = 0
            Do While (i <= num9)
                Dim num3 As Integer
                Dim num4 As Integer
                If (i <= num5) Then
                    num3 = numArray(i)
                Else
                    num3 = -1
                End If
                If (i <= num6) Then
                    num4 = ArgIndexes(i)
                Else
                    num4 = -1
                End If
                If ((num3 <> -1) OrElse (num4 <> -1)) Then
                    Dim parameterType As Type
                    Dim elementType As Type
                    If ((flag2 AndAlso (ParamArrayIndex1 <> -1)) AndAlso (i >= ParamArrayIndex1)) Then
                        If ((flag AndAlso (ParamArrayIndex0 <> -1)) AndAlso (i >= ParamArrayIndex0)) Then
                            parameterType = fromType
                        Else
                            parameterType = infoArray(num3).ParameterType
                            If parameterType.IsByRef Then
                                parameterType = parameterType.GetElementType
                            End If
                        End If
                        If (toType Is parameterType) Then
                            If (((num = -1) AndAlso (ParamArrayIndex0 = -1)) AndAlso ((i = num5) AndAlso (Not args(num5) Is Nothing))) Then
                                num = 0
                            End If
                            Continue Do
                        End If
                        If ObjectType.IsWideningConversion(parameterType, toType) Then
                            If (num <> 1) Then
                                num = 0
                                Continue Do
                            End If
                            num = -1
                        Else
                            If Not ObjectType.IsWideningConversion(toType, parameterType) Then
                                Continue Do
                            End If
                            If (num <> 0) Then
                                num = 1
                                Continue Do
                            End If
                            num = -1
                        End If
                        Exit Do
                    End If
                    If ((flag AndAlso (ParamArrayIndex0 <> -1)) AndAlso (i >= ParamArrayIndex0)) Then
                        If ((flag2 AndAlso (ParamArrayIndex1 <> -1)) AndAlso (i >= ParamArrayIndex1)) Then
                            elementType = toType
                        Else
                            elementType = parameters(num4).ParameterType
                            If elementType.IsByRef Then
                                elementType = elementType.GetElementType
                            End If
                        End If
                        If (fromType Is elementType) Then
                            If (((num = -1) AndAlso (ParamArrayIndex1 = -1)) AndAlso ((i = num6) AndAlso (Not args(num6) Is Nothing))) Then
                                num = 1
                            End If
                            Continue Do
                        End If
                        If ObjectType.IsWideningConversion(fromType, elementType) Then
                            If (num <> 1) Then
                                num = 0
                                Continue Do
                            End If
                            num = -1
                        Else
                            If Not ObjectType.IsWideningConversion(elementType, fromType) Then
                                Continue Do
                            End If
                            If (num <> 0) Then
                                num = 1
                                Continue Do
                            End If
                            num = -1
                        End If
                        Exit Do
                    End If
                    parameterType = infoArray(numArray(i)).ParameterType
                    elementType = parameters(ArgIndexes(i)).ParameterType
                    If (Not parameterType Is elementType) Then
                        If ObjectType.IsWideningConversion(parameterType, elementType) Then
                            If (num <> 1) Then
                                num = 0
                                Continue Do
                            End If
                            num = -1
                            Exit Do
                        End If
                        If ObjectType.IsWideningConversion(elementType, parameterType) Then
                            If (num <> 0) Then
                                num = 1
                                Continue Do
                            End If
                            num = -1
                            Exit Do
                        End If
                        If ObjectType.IsWiderNumeric(parameterType, elementType) Then
                            If (num <> 0) Then
                                num = 1
                                Continue Do
                            End If
                            num = -1
                            Exit Do
                        End If
                        If ObjectType.IsWiderNumeric(elementType, parameterType) Then
                            If (num <> 1) Then
                                num = 0
                                Continue Do
                            End If
                            num = -1
                            Exit Do
                        End If
                        num = -1
                    End If
                End If
                i += 1
            Loop
            If (num = -1) Then
                If (((ParamArrayIndex0 = -1) OrElse Not flag) AndAlso (ParamArrayIndex1 <> -1)) Then
                    If (flag2 AndAlso Me.MatchesParamArraySignature(infoArray, parameters, ParamArrayIndex1, IsPropertySet, upperBound)) Then
                        num = 0
                    End If
                    Return num
                End If
                If ((ParamArrayIndex1 <> -1) AndAlso flag2) Then
                    Return num
                End If
                If (((ParamArrayIndex0 <> -1) AndAlso flag) AndAlso Me.MatchesParamArraySignature(parameters, infoArray, ParamArrayIndex0, IsPropertySet, upperBound)) Then
                    num = 1
                End If
            End If
            Return num
        End Function

        Private Function GetPropArgCount(args As Object(), IsPropertySet As Boolean) As Integer
            If IsPropertySet Then
                Return (args.Length - 1)
            End If
            Return args.Length
        End Function

        <SecuritySafeCritical, DebuggerHidden, DebuggerStepThrough>
        Friend Function InvokeMember(name As String, invokeAttr As BindingFlags, objType As Type, objIReflect As IReflect, target As Object, args As Object(), namedParameters As String()) As Object
            Dim obj4 As Object
            If objType.IsCOMObject Then
                Dim modifiers As ParameterModifier() = Nothing
                If (((Not Me.m_ByRefFlags Is Nothing) AndAlso (Not target Is Nothing)) AndAlso Not RemotingServices.IsTransparentProxy(target)) Then
                    Dim modifier As New ParameterModifier(args.Length)
                    modifiers = New ParameterModifier() {modifier}
                    Dim obj5 As Object = Missing.Value
                    Dim upperBound As Integer = args.GetUpperBound(0)
                    Dim i As Integer = 0
                    Do While (i <= upperBound)
                        If (Not args(i) Is obj5) Then
                            modifier.Item(i) = Me.m_ByRefFlags(i)
                        End If
                        i += 1
                    Loop
                End If
                Try
                    Call New SecurityPermission(PermissionState.Unrestricted).Demand
                    Return objIReflect.InvokeMember(name, invokeAttr, Nothing, target, args, modifiers, Nothing, namedParameters)
                Catch exception As MissingMemberException
                    Dim textArray1 As String() = New String() {name, Utils.VBFriendlyName(objType)}
                    Throw New MissingMemberException(Utils.GetResourceString("MissingMember_MemberNotFoundOnType2", textArray1))
                End Try
            End If
            Me.m_BindToName = name
            Me.m_objType = objType
            If (name.Length = 0) Then
                If (objType Is objIReflect) Then
                    name = Me.GetDefaultMemberName(objType)
                    If (name Is Nothing) Then
                        Dim textArray2 As String() = New String() {Utils.VBFriendlyName(objType)}
                        Throw New MissingMemberException(Utils.GetResourceString("MissingMember_NoDefaultMemberFound1", textArray2))
                    End If
                Else
                    name = ""
                End If
            End If
            Dim match As MethodBase() = Me.GetMethodsByName(objType, objIReflect, name, invokeAttr)
            If (args Is Nothing) Then
                args = New Object(0 - 1) {}
            End If
            Dim objState As Object = Nothing
            Dim member As MethodBase = Me.BindToMethod(invokeAttr, match, args, Nothing, Nothing, namedParameters, objState)
            If (member Is Nothing) Then
                Dim textArray3 As String() = New String() {Me.CalledMethodName, Conversions.ToString(Me.GetPropArgCount(args, ((invokeAttr And BindingFlags.SetProperty) > BindingFlags.Default)))}
                Throw New MissingMemberException(Utils.GetResourceString("NoMethodTakingXArguments2", textArray3))
            End If
            VBBinder.SecurityCheckForLateboundCalls(member, objType, objIReflect)
            Dim info As MethodInfo = DirectCast(member, MethodInfo)
            If (((objType Is objIReflect) OrElse info.IsStatic) OrElse LateBinding.DoesTargetObjectMatch(target, info)) Then
                LateBinding.VerifyObjRefPresentForInstanceCall(target, info)
                obj4 = info.Invoke(target, args)
            Else
                obj4 = LateBinding.InvokeMemberOnIReflect(objIReflect, info, BindingFlags.InvokeMethod, target, args)
            End If
            If (Not objState Is Nothing) Then
                Me.ReorderArgumentArray(args, objState)
            End If
            Return obj4
        End Function

        Private Shared Function IsMemberPublic(Member As MemberInfo) As Boolean
            Select Case Member.MemberType
                Case MemberTypes.Constructor
                    Return DirectCast(Member, ConstructorInfo).IsPublic
                Case MemberTypes.Field
                    Return DirectCast(Member, FieldInfo).IsPublic
                Case MemberTypes.Method
                    Return DirectCast(Member, MethodInfo).IsPublic
                Case MemberTypes.Property
                    Return False
            End Select
            Return False
        End Function

        Private Function MatchesParamArraySignature(param0 As ParameterInfo(), param1 As ParameterInfo(), ParamArrayIndex1 As Integer, IsPropertySet As Boolean, ArgCountUpperBound As Integer) As Boolean
            Dim upperBound As Integer = param0.GetUpperBound(0)
            If IsPropertySet Then
                upperBound -= 1
            End If
            Dim num3 As Integer = Math.Min(upperBound, ArgCountUpperBound)
            Dim i As Integer = 0
            Do While (i <= num3)
                Dim elementType As Type
                Dim parameterType As Type = param0(i).ParameterType
                If parameterType.IsByRef Then
                    parameterType = parameterType.GetElementType
                End If
                If (i >= ParamArrayIndex1) Then
                    elementType = param1(ParamArrayIndex1).ParameterType.GetElementType
                Else
                    elementType = param1(i).ParameterType
                    If elementType.IsByRef Then
                        elementType = elementType.GetElementType
                    End If
                End If
                If (Not parameterType Is elementType) Then
                    Return False
                End If
                i += 1
            Loop
            Return True
        End Function

        Private Function MethodsDifferOnlyByReturnType(match1 As MethodBase, match2 As MethodBase) As Boolean
            Dim num As Integer
            Dim parameters As ParameterInfo() = match1.GetParameters
            Dim infoArray2 As ParameterInfo() = match2.GetParameters
            Dim num2 As Integer = Math.Min(parameters.GetUpperBound(0), infoArray2.GetUpperBound(0))
            Dim num3 As Integer = num2
            num = 0
            Do While (num <= num3)
                Dim parameterType As Type = parameters(num).ParameterType
                If parameterType.IsByRef Then
                    parameterType = parameterType.GetElementType
                End If
                Dim elementType As Type = infoArray2(num).ParameterType
                If elementType.IsByRef Then
                    elementType = elementType.GetElementType
                End If
                If (Not parameterType Is elementType) Then
                    Return False
                End If
                num += 1
            Loop
            If (parameters.Length > infoArray2.Length) Then
                Dim upperBound As Integer = infoArray2.GetUpperBound(0)
                num = (num2 + 1)
                Do While (num <= upperBound)
                    If Not parameters(num).IsOptional Then
                        Return False
                    End If
                    num += 1
                Loop
            ElseIf (infoArray2.Length > parameters.Length) Then
                Dim num5 As Integer = parameters.GetUpperBound(0)
                num = (num2 + 1)
                Do While (num <= num5)
                    If Not infoArray2(num).IsOptional Then
                        Return False
                    End If
                    num += 1
                Loop
            End If
            Return True
        End Function

        Public Overrides Sub ReorderArgumentArray(ByRef args As Object(), objState As Object)
            Dim state As VBBinderState = DirectCast(objState, VBBinderState)
            If ((Not args Is Nothing) AndAlso (Not state Is Nothing)) Then
                Dim num As Integer
                If (Not state.m_OriginalParamOrder Is Nothing) Then
                    If (Not Me.m_ByRefFlags Is Nothing) Then
                        If (state.m_ByRefFlags Is Nothing) Then
                            Dim upperBound As Integer = Me.m_ByRefFlags.GetUpperBound(0)
                            num = 0
                            Do While (num <= upperBound)
                                Me.m_ByRefFlags(num) = False
                                num += 1
                            Loop
                        Else
                            Dim num4 As Integer = state.m_OriginalParamOrder.GetUpperBound(0)
                            num = 0
                            Do While (num <= num4)
                                Dim index As Integer = state.m_OriginalParamOrder(num)
                                If ((index >= 0) AndAlso (index <= args.GetUpperBound(0))) Then
                                    Me.m_ByRefFlags(index) = state.m_ByRefFlags(index)
                                    state.m_OriginalArgs(index) = args(num)
                                End If
                                num += 1
                            Loop
                        End If
                    End If
                ElseIf (Not Me.m_ByRefFlags Is Nothing) Then
                    If (state.m_ByRefFlags Is Nothing) Then
                        Dim num5 As Integer = Me.m_ByRefFlags.GetUpperBound(0)
                        num = 0
                        Do While (num <= num5)
                            Me.m_ByRefFlags(num) = False
                            num += 1
                        Loop
                    Else
                        Dim num6 As Integer = Me.m_ByRefFlags.GetUpperBound(0)
                        num = 0
                        Do While (num <= num6)
                            If Me.m_ByRefFlags(num) Then
                                Dim flag As Boolean = state.m_ByRefFlags(num)
                                Me.m_ByRefFlags(num) = flag
                                If flag Then
                                    state.m_OriginalArgs(num) = args(num)
                                End If
                            End If
                            num += 1
                        Loop
                    End If
                End If
            End If
            If (Not state Is Nothing) Then
                state.m_OriginalParamOrder = Nothing
                state.m_ByRefFlags = Nothing
            End If
        End Sub

        Private Sub ReorderParams(paramOrder As Integer(), vars As Object(), state As VBBinderState)
            Dim num2 As Integer = Math.Max(vars.GetUpperBound(0), paramOrder.GetUpperBound(0))
            state.m_OriginalParamOrder = New Integer((num2 + 1) - 1) {}
            Dim num3 As Integer = num2
            Dim i As Integer = 0
            Do While (i <= num3)
                state.m_OriginalParamOrder(i) = paramOrder(i)
                i += 1
            Loop
        End Sub

        Friend Shared Sub SecurityCheckForLateboundCalls(member As MemberInfo, objType As Type, objIReflect As IReflect)
            If ((Not objType Is objIReflect) AndAlso Not VBBinder.IsMemberPublic(member)) Then
                Throw New MissingMethodException
            End If
            Dim declaringType As Type = member.DeclaringType
            If (Not declaringType.IsPublic AndAlso (declaringType.Assembly Is Utils.VBRuntimeAssembly)) Then
                Throw New MissingMethodException
            End If
        End Sub

        Public Overrides Function SelectMethod(bindingAttr As BindingFlags, match As MethodBase(), types As Type(), modifiers As ParameterModifier()) As MethodBase
            Throw New NotSupportedException
        End Function

        Public Overrides Function SelectProperty(bindingAttr As BindingFlags, match As PropertyInfo(), returnType As Type, indexes As Type(), modifiers As ParameterModifier()) As PropertyInfo
            Dim unknown As BindScore = BindScore.Unknown
            Dim index As Integer = 0
            Dim upperBound As Integer = match.GetUpperBound(0)
            Dim i As Integer = 0
            Do While (i <= upperBound)
                Dim info2 As PropertyInfo = match(i)
                If (Not info2 Is Nothing) Then
                    Dim score As BindScore = Me.BindingScore(info2.GetIndexParameters, Nothing, indexes, False, -1)
                    If (score < unknown) Then
                        If (i <> 0) Then
                            match(0) = match(i)
                            match(i) = Nothing
                        End If
                        index = 1
                        unknown = score
                        Continue Do
                    End If
                    If (score = unknown) Then
                        If (score = BindScore.Widening1) Then
                            Dim num5 As Integer = -1
                            Dim indexParameters As ParameterInfo() = info2.GetIndexParameters
                            Dim infoArray2 As ParameterInfo() = match(0).GetIndexParameters
                            num5 = -1
                            Dim num8 As Integer = indexParameters.GetUpperBound(0)
                            Dim j As Integer = 0
                            Do While (j <= num8)
                                Dim num6 As Integer = j
                                Dim num7 As Integer = j
                                If ((num6 <> -1) AndAlso (num7 <> -1)) Then
                                    Dim parameterType As Type = infoArray2(num6).ParameterType
                                    Dim toType As Type = indexParameters(num7).ParameterType
                                    If ObjectType.IsWideningConversion(parameterType, toType) Then
                                        If (num5 <> 1) Then
                                            num5 = 0
                                            Continue Do
                                        End If
                                        num5 = -1
                                        Exit Do
                                    End If
                                    If ObjectType.IsWideningConversion(toType, parameterType) Then
                                        If (num5 <> 0) Then
                                            num5 = 1
                                        Else
                                            num5 = -1
                                            Exit Do
                                        End If
                                    End If
                                End If
                                j += 1
                            Loop
                            If (num5 = -1) Then
                                If (index <> i) Then
                                    match(index) = match(i)
                                    match(i) = Nothing
                                End If
                                index += 1
                            ElseIf (num5 = 0) Then
                                index = 1
                            Else
                                If (i <> 0) Then
                                    match(0) = match(i)
                                    match(i) = Nothing
                                End If
                                index = 1
                            End If
                            Continue Do
                        End If
                        If (score = BindScore.Exact) Then
                            If info2.DeclaringType.IsSubclassOf(match(0).DeclaringType) Then
                                If (i <> 0) Then
                                    match(0) = match(i)
                                    match(i) = Nothing
                                End If
                                index = 1
                            ElseIf Not match(0).DeclaringType.IsSubclassOf(info2.DeclaringType) Then
                                If (index <> i) Then
                                    match(index) = match(i)
                                    match(i) = Nothing
                                End If
                                index += 1
                            End If
                        Else
                            If (index <> i) Then
                                match(index) = match(i)
                                match(i) = Nothing
                            End If
                            index += 1
                        End If
                        Continue Do
                    End If
                    match(i) = Nothing
                End If
                i += 1
            Loop
            If (index = 1) Then
                Return match(0)
            End If
            Return Nothing
        End Function

        Private Sub ThrowInvalidCast(ArgType As Type, ParmType As Type, ParmIndex As Integer)
            Dim args As String() = New String() {Me.CalledMethodName, Conversions.ToString(CInt((ParmIndex + 1))), Utils.VBFriendlyName(ArgType), Utils.VBFriendlyName(ParmType)}
            Throw New InvalidCastException(Utils.GetResourceString("InvalidCast_FromToArg4", args))
        End Sub


        ' Fields
        Private Const ARG_MISSING As Integer = -1
        Friend m_BindToName As String
        Private m_ByRefFlags As Boolean()
        Private m_CachedMember As MemberInfo
        Friend m_objType As Type
        Private m_state As VBBinderState
        Private Const PARAMARRAY_NONE As Integer = -1

        ' Nested Types
        <EditorBrowsable(EditorBrowsableState.Never)> _
        Public Enum BindScore
            ' Fields
            Exact = 0
            [Narrowing] = 3
            Unknown = 4
            Widening0 = 1
            Widening1 = 2
        End Enum

        <EditorBrowsable(EditorBrowsableState.Never)> _
        Friend NotInheritable Class VBBinderState
            ' Methods
            Friend Sub New()
            End Sub


            ' Fields
            Friend m_ByRefFlags As Boolean()
            Friend m_OriginalArgs As Object()
            Friend m_OriginalByRefFlags As Boolean()
            Friend m_OriginalParamOrder As Integer()
        End Class
    End Class
End Namespace

