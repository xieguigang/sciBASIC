Imports System
Imports System.Collections
Imports System.Collections.Generic
Imports System.Collections.Specialized
Imports System.ComponentModel
Imports System.Reflection
Imports System.Runtime.Remoting
Imports System.Security
Imports System.Security.Permissions

Namespace Microsoft.VisualBasic.CompilerServices
    Friend Class Symbols
        ' Methods
        Shared Sub New()
            Symbols.OperatorCLSNames(1) = "op_Explicit"
            Symbols.OperatorCLSNames(2) = "op_Implicit"
            Symbols.OperatorCLSNames(3) = "op_True"
            Symbols.OperatorCLSNames(4) = "op_False"
            Symbols.OperatorCLSNames(5) = "op_UnaryNegation"
            Symbols.OperatorCLSNames(6) = "op_OnesComplement"
            Symbols.OperatorCLSNames(7) = "op_UnaryPlus"
            Symbols.OperatorCLSNames(8) = "op_Addition"
            Symbols.OperatorCLSNames(9) = "op_Subtraction"
            Symbols.OperatorCLSNames(10) = "op_Multiply"
            Symbols.OperatorCLSNames(11) = "op_Division"
            Symbols.OperatorCLSNames(12) = "op_Exponent"
            Symbols.OperatorCLSNames(13) = "op_IntegerDivision"
            Symbols.OperatorCLSNames(14) = "op_Concatenate"
            Symbols.OperatorCLSNames(15) = "op_LeftShift"
            Symbols.OperatorCLSNames(&H10) = "op_RightShift"
            Symbols.OperatorCLSNames(&H11) = "op_Modulus"
            Symbols.OperatorCLSNames(&H12) = "op_BitwiseOr"
            Symbols.OperatorCLSNames(&H13) = "op_ExclusiveOr"
            Symbols.OperatorCLSNames(20) = "op_BitwiseAnd"
            Symbols.OperatorCLSNames(&H15) = "op_Like"
            Symbols.OperatorCLSNames(&H16) = "op_Equality"
            Symbols.OperatorCLSNames(&H17) = "op_Inequality"
            Symbols.OperatorCLSNames(&H18) = "op_LessThan"
            Symbols.OperatorCLSNames(&H19) = "op_LessThanOrEqual"
            Symbols.OperatorCLSNames(&H1A) = "op_GreaterThanOrEqual"
            Symbols.OperatorCLSNames(&H1B) = "op_GreaterThan"
            Symbols.OperatorNames = New String(&H1C  - 1) {}
            Symbols.OperatorNames(1) = "CType"
            Symbols.OperatorNames(2) = "CType"
            Symbols.OperatorNames(3) = "IsTrue"
            Symbols.OperatorNames(4) = "IsFalse"
            Symbols.OperatorNames(5) = "-"
            Symbols.OperatorNames(6) = "Not"
            Symbols.OperatorNames(7) = "+"
            Symbols.OperatorNames(8) = "+"
            Symbols.OperatorNames(9) = "-"
            Symbols.OperatorNames(10) = "*"
            Symbols.OperatorNames(11) = "/"
            Symbols.OperatorNames(12) = "^"
            Symbols.OperatorNames(13) = "\"
            Symbols.OperatorNames(14) = "&"
            Symbols.OperatorNames(15) = "<<"
            Symbols.OperatorNames(&H10) = ">>"
            Symbols.OperatorNames(&H11) = "Mod"
            Symbols.OperatorNames(&H12) = "Or"
            Symbols.OperatorNames(&H13) = "Xor"
            Symbols.OperatorNames(20) = "And"
            Symbols.OperatorNames(&H15) = "Like"
            Symbols.OperatorNames(&H16) = "="
            Symbols.OperatorNames(&H17) = "<>"
            Symbols.OperatorNames(&H18) = "<"
            Symbols.OperatorNames(&H19) = "<="
            Symbols.OperatorNames(&H1A) = ">="
            Symbols.OperatorNames(&H1B) = ">"
        End Sub

        Private Sub New()
        End Sub

        Friend Shared Function AreGenericMethodDefsEqual(Method1 As MethodBase, Method2 As MethodBase) As Boolean
            If (Not Method1 Is Method2) Then
                Return (Method1.MetadataToken = Method2.MetadataToken)
            End If
            Return True
        End Function

        Friend Shared Function AreParametersAndReturnTypesValid(Parameters As ParameterInfo(), ReturnType As Type) As Boolean
            If ((Not ReturnType Is Nothing) AndAlso (ReturnType.IsPointer OrElse ReturnType.IsByRef)) Then
                Return False
            End If
            If (Not Parameters Is Nothing) Then
                Dim infoArray As ParameterInfo() = Parameters
                Dim i As Integer
                For i = 0 To infoArray.Length - 1
                    If infoArray(i).ParameterType.IsPointer Then
                        Return False
                    End If
                Next i
            End If
            Return True
        End Function

        Friend Shared Sub GetAllParameterCounts(Parameters As ParameterInfo(), ByRef RequiredParameterCount As Integer, ByRef MaximumParameterCount As Integer, ByRef ParamArrayIndex As Integer)
            MaximumParameterCount = Parameters.Length
            Dim i As Integer = (MaximumParameterCount - 1)
            Do While (i >= 0)
                If Not Parameters(i).IsOptional Then
                    RequiredParameterCount = (i + 1)
                    Exit Do
                End If
                i = (i + -1)
            Loop
            If ((Not MaximumParameterCount Is 0) AndAlso Symbols.IsParamArray(Parameters((MaximumParameterCount - 1)))) Then
                ParamArrayIndex = (MaximumParameterCount - 1)
                RequiredParameterCount -= 1
            End If
        End Sub

        Friend Shared Function GetClassConstraint(GenericParameter As Type) As Type
            Dim baseType As Type = GenericParameter.BaseType
            If Symbols.IsRootObjectType(baseType) Then
                Return Nothing
            End If
            Return baseType
        End Function

        Friend Shared Function GetElementType(Type As Type) As Type
            Return Type.GetElementType
        End Function

        Friend Shared Function GetInterfaceConstraints(GenericParameter As Type) As Type()
            Return GenericParameter.GetInterfaces
        End Function

        Friend Shared Function GetTypeArguments(Type As Type) As Type()
            Return Type.GetGenericArguments
        End Function

        Friend Shared Function GetTypeCode(Type As Type) As TypeCode
            Return Type.GetTypeCode(Type)
        End Function

        Friend Shared Function GetTypeParameters(Member As MemberInfo) As Type()
            Dim base2 As MethodBase = TryCast(Member, MethodBase)
            If (base2 Is Nothing) Then
                Return Symbols.NoTypeParameters
            End If
            Return base2.GetGenericArguments
        End Function

        Friend Shared Function GetTypeParameters(Type As Type) As Type()
            Return Type.GetGenericArguments
        End Function

        Friend Shared Function HasFlag(Flags As BindingFlags, FlagToTest As BindingFlags) As Boolean
            Return ((Flags And FlagToTest) > BindingFlags.Default)
        End Function

        Friend Shared Function [Implements](Implementor As Type, [Interface] As Type) As Boolean
            Dim type As Type
            For Each type In Implementor.GetInterfaces
                If ((type Is [Interface]) OrElse Symbols.IsEquivalentType(type, [Interface])) Then
                    Return True
                End If
            Next
            Return False
        End Function

        Friend Shared Function IndexIn(PossibleGenericParameter As Type, GenericMethodDef As MethodBase) As Integer
            If ((Symbols.IsGenericParameter(PossibleGenericParameter) AndAlso (Not PossibleGenericParameter.DeclaringMethod Is Nothing)) AndAlso Symbols.AreGenericMethodDefsEqual(PossibleGenericParameter.DeclaringMethod, GenericMethodDef)) Then
                Return PossibleGenericParameter.GenericParameterPosition
            End If
            Return -1
        End Function

        Friend Shared Function IsArrayType(Type As Type) As Boolean
            Return Type.IsArray
        End Function

        Friend Shared Function IsBinaryOperator(Op As UserDefinedOperator) As Boolean
            Select Case Op
                Case UserDefinedOperator.Plus, UserDefinedOperator.Minus, UserDefinedOperator.Multiply, UserDefinedOperator.Divide, UserDefinedOperator.Power, UserDefinedOperator.IntegralDivide, UserDefinedOperator.Concatenate, UserDefinedOperator.ShiftLeft, UserDefinedOperator.ShiftRight, UserDefinedOperator.Modulus, UserDefinedOperator.Or, UserDefinedOperator.Xor, UserDefinedOperator.And, UserDefinedOperator.Like, UserDefinedOperator.Equal, UserDefinedOperator.NotEqual, UserDefinedOperator.Less, UserDefinedOperator.LessEqual, UserDefinedOperator.GreaterEqual, UserDefinedOperator.Greater
                    Return True
            End Select
            Return False
        End Function

        Friend Shared Function IsCharArrayRankOne(Type As Type) As Boolean
            Return (Type Is GetType(Char()))
        End Function

        Friend Shared Function IsClass(Type As Type) As Boolean
            If Not Type.IsClass Then
                Return Symbols.IsRootEnumType(Type)
            End If
            Return True
        End Function

        Friend Shared Function IsClassOrInterface(Type As Type) As Boolean
            If Not Symbols.IsClass(Type) Then
                Return Symbols.IsInterface(Type)
            End If
            Return True
        End Function

        Friend Shared Function IsClassOrValueType(Type As Type) As Boolean
            If Not Symbols.IsValueType(Type) Then
                Return Symbols.IsClass(Type)
            End If
            Return True
        End Function

        Friend Shared Function IsCollectionInterface(Type As Type) As Boolean
            Return (Type.IsInterface AndAlso ((Type.IsGenericType AndAlso ((((Type.GetGenericTypeDefinition Is GetType(IList(Of ))) OrElse (Type.GetGenericTypeDefinition Is GetType(ICollection(Of )))) OrElse ((Type.GetGenericTypeDefinition Is GetType(IEnumerable(Of ))) OrElse (Type.GetGenericTypeDefinition Is GetType(IReadOnlyList(Of ))))) OrElse (((Type.GetGenericTypeDefinition Is GetType(IReadOnlyCollection(Of ))) OrElse (Type.GetGenericTypeDefinition Is GetType(IDictionary(Of ,)))) OrElse (Type.GetGenericTypeDefinition Is GetType(IReadOnlyDictionary(Of ,)))))) OrElse (((Type Is GetType(IList)) OrElse (Type Is GetType(ICollection))) OrElse (((Type Is GetType(IEnumerable)) OrElse (Type Is GetType(INotifyPropertyChanged))) OrElse (Type Is GetType(INotifyCollectionChanged))))))
        End Function

        Friend Shared Function IsEnum(Type As Type) As Boolean
            Return Type.IsEnum
        End Function

        Friend Shared Function IsEquivalentType(Left As Type, Right As Type) As Boolean
            If ((Symbols.IsInstantiatedGeneric(Left) AndAlso Not Left.IsInterface) AndAlso (Symbols.IsInstantiatedGeneric(Right) AndAlso Not Right.IsInterface)) Then
                If Not Symbols.IsEquivalentType(Left.GetGenericTypeDefinition, Right.GetGenericTypeDefinition) Then
                    Return False
                End If
                Dim genericArguments As Type() = Left.GetGenericArguments
                Dim typeArray2 As Type() = Right.GetGenericArguments
                If (genericArguments.Length <> typeArray2.Length) Then
                    Return False
                End If
                Dim num As Integer = (genericArguments.Length - 1)
                Dim i As Integer = 0
                Do While (i <= num)
                    If Not Symbols.IsEquivalentType(genericArguments(i), typeArray2(i)) Then
                        Return False
                    End If
                    i += 1
                Loop
                Return True
            End If
            Return Left.IsEquivalentTo(Right)
        End Function

        Friend Shared Function IsGeneric(Member As MemberInfo) As Boolean
            Dim method As MethodBase = TryCast(Member, MethodBase)
            If (method Is Nothing) Then
                Return False
            End If
            Return Symbols.IsGeneric(method)
        End Function

        Friend Shared Function IsGeneric(Method As MethodBase) As Boolean
            Return Method.IsGenericMethod
        End Function

        Friend Shared Function IsGeneric(Type As Type) As Boolean
            Return Type.IsGenericType
        End Function

        Friend Shared Function IsGenericParameter(Type As Type) As Boolean
            Return Type.IsGenericParameter
        End Function

        Friend Shared Function IsInstantiatedGeneric(Type As Type) As Boolean
            Return (Type.IsGenericType AndAlso Not Type.IsGenericTypeDefinition)
        End Function

        Friend Shared Function IsIntegralType(TypeCode As TypeCode) As Boolean
            Select Case TypeCode
                Case TypeCode.SByte, TypeCode.Byte, TypeCode.Int16, TypeCode.UInt16, TypeCode.Int32, TypeCode.UInt32, TypeCode.Int64, TypeCode.UInt64
                    Return True
            End Select
            Return False
        End Function

        Friend Shared Function IsInterface(Type As Type) As Boolean
            Return Type.IsInterface
        End Function

        Friend Shared Function IsIntrinsicType(Type As Type) As Boolean
            Return (Symbols.IsIntrinsicType(Symbols.GetTypeCode(Type)) AndAlso Not Symbols.IsEnum(Type))
        End Function

        Friend Shared Function IsIntrinsicType(TypeCode As TypeCode) As Boolean
            Select Case TypeCode
                Case TypeCode.Boolean, TypeCode.Char, TypeCode.SByte, TypeCode.Byte, TypeCode.Int16, TypeCode.UInt16, TypeCode.Int32, TypeCode.UInt32, TypeCode.Int64, TypeCode.UInt64, TypeCode.Single, TypeCode.Double, TypeCode.Decimal, TypeCode.DateTime, TypeCode.String
                    Return True
            End Select
            Return False
        End Function

        Friend Shared Function IsNarrowingConversionOperator(Method As MethodBase) As Boolean
            Return (Method.IsSpecialName AndAlso Method.Name.Equals(Symbols.OperatorCLSNames(1)))
        End Function

        Friend Shared Function IsNonPublicRuntimeMember(Member As MemberInfo) As Boolean
            Dim declaringType As Type = Member.DeclaringType
            Return (Not declaringType.IsPublic AndAlso (declaringType.Assembly Is Utils.VBRuntimeAssembly))
        End Function

        Friend Shared Function IsNumericType(Type As Type) As Boolean
            Return Symbols.IsNumericType(Symbols.GetTypeCode(Type))
        End Function

        Friend Shared Function IsNumericType(TypeCode As TypeCode) As Boolean
            Select Case TypeCode
                Case TypeCode.SByte, TypeCode.Byte, TypeCode.Int16, TypeCode.UInt16, TypeCode.Int32, TypeCode.UInt32, TypeCode.Int64, TypeCode.UInt64, TypeCode.Single, TypeCode.Double, TypeCode.Decimal
                    Return True
            End Select
            Return False
        End Function

        Friend Shared Function IsOrInheritsFrom(Derived As Type, Base As Type) As Boolean
            If (Derived Is Base) Then
                Return True
            End If
            If Derived.IsGenericParameter Then
                If ((Symbols.IsClass(Base) AndAlso ((Derived.GenericParameterAttributes And GenericParameterAttributes.NotNullableValueTypeConstraint) > GenericParameterAttributes.None)) AndAlso Symbols.IsOrInheritsFrom(GetType(ValueType), Base)) Then
                    Return True
                End If
                Dim genericParameterConstraints As Type() = Derived.GetGenericParameterConstraints
                Dim i As Integer
                For i = 0 To genericParameterConstraints.Length - 1
                    If Symbols.IsOrInheritsFrom(genericParameterConstraints(i), Base) Then
                        Return True
                    End If
                Next i
            ElseIf Symbols.IsInterface(Derived) Then
                If Symbols.IsInterface(Base) Then
                    Dim interfaces As Type() = Derived.GetInterfaces
                    Dim j As Integer
                    For j = 0 To interfaces.Length - 1
                        If (interfaces(j) Is Base) Then
                            Return True
                        End If
                    Next j
                End If
            ElseIf (Symbols.IsClass(Base) AndAlso Symbols.IsClassOrValueType(Derived)) Then
                Return Derived.IsSubclassOf(Base)
            End If
            Return False
        End Function

        Friend Shared Function IsParamArray(Parameter As ParameterInfo) As Boolean
            Return (Symbols.IsArrayType(Parameter.ParameterType) AndAlso Parameter.IsDefined(GetType(ParamArrayAttribute), False))
        End Function

        Friend Shared Function IsRawGeneric(Method As MethodBase) As Boolean
            Return (Method.IsGenericMethod AndAlso Method.IsGenericMethodDefinition)
        End Function

        Friend Shared Function IsReferenceType(Type As Type) As Boolean
            If Not Symbols.IsClass(Type) Then
                Return Symbols.IsInterface(Type)
            End If
            Return True
        End Function

        Friend Shared Function IsRootEnumType(Type As Type) As Boolean
            Return (Type Is GetType([Enum]))
        End Function

        Friend Shared Function IsRootObjectType(Type As Type) As Boolean
            Return (Type Is GetType(Object))
        End Function

        Friend Shared Function IsShadows(Method As MethodBase) As Boolean
            If Method.IsHideBySig Then
                Return False
            End If
            If ((Method.IsVirtual AndAlso ((Method.Attributes And MethodAttributes.NewSlot) = MethodAttributes.PrivateScope)) AndAlso ((DirectCast(Method, MethodInfo).GetBaseDefinition.Attributes And MethodAttributes.NewSlot) = MethodAttributes.PrivateScope)) Then
                Return False
            End If
            Return True
        End Function

        Friend Shared Function IsShared(Member As MemberInfo) As Boolean
            Select Case Member.MemberType
                Case MemberTypes.Constructor
                    Return DirectCast(Member, ConstructorInfo).IsStatic
                Case MemberTypes.Field
                    Return DirectCast(Member, FieldInfo).IsStatic
                Case MemberTypes.Method
                    Return DirectCast(Member, MethodInfo).IsStatic
                Case MemberTypes.Property
                    Return DirectCast(Member, PropertyInfo).GetGetMethod.IsStatic
            End Select
            Return False
        End Function

        Friend Shared Function IsStringType(Type As Type) As Boolean
            Return (Type Is GetType(String))
        End Function

        Friend Shared Function IsUnaryOperator(Op As UserDefinedOperator) As Boolean
            Select Case Op
                Case UserDefinedOperator.Narrow, UserDefinedOperator.Widen, UserDefinedOperator.IsTrue, UserDefinedOperator.IsFalse, UserDefinedOperator.Negate, UserDefinedOperator.Not, UserDefinedOperator.UnaryPlus
                    Return True
            End Select
            Return False
        End Function

        Friend Shared Function IsUserDefinedOperator(Method As MethodBase) As Boolean
            Return (Method.IsSpecialName AndAlso Method.Name.StartsWith("op_", StringComparison.Ordinal))
        End Function

        Friend Shared Function IsValueType(Type As Type) As Boolean
            Return Type.IsValueType
        End Function

        Friend Shared Function MapToUserDefinedOperator(Method As MethodBase) As UserDefinedOperator
            Dim index As Integer = 1
            Do
                If Method.Name.Equals(Symbols.OperatorCLSNames(index)) Then
                    Dim length As Integer = Method.GetParameters.Length
                    Dim op As UserDefinedOperator = DirectCast(CSByte(index), UserDefinedOperator)
                    If ((length = 1) AndAlso Symbols.IsUnaryOperator(op)) Then
                        Return op
                    End If
                    If ((length = 2) AndAlso Symbols.IsBinaryOperator(op)) Then
                        Return op
                    End If
                End If
                index += 1
            Loop While (index <= &H1B)
            Return UserDefinedOperator.UNDEF
        End Function

        Friend Shared Function MapTypeCodeToType(TypeCode As TypeCode) As Type
            Select Case TypeCode
                Case TypeCode.Object
                    Return GetType(Object)
                Case TypeCode.DBNull
                    Return GetType(DBNull)
                Case TypeCode.Boolean
                    Return GetType(Boolean)
                Case TypeCode.Char
                    Return GetType(Char)
                Case TypeCode.SByte
                    Return GetType(SByte)
                Case TypeCode.Byte
                    Return GetType(Byte)
                Case TypeCode.Int16
                    Return GetType(Short)
                Case TypeCode.UInt16
                    Return GetType(UInt16)
                Case TypeCode.Int32
                    Return GetType(Integer)
                Case TypeCode.UInt32
                    Return GetType(UInt32)
                Case TypeCode.Int64
                    Return GetType(Long)
                Case TypeCode.UInt64
                    Return GetType(UInt64)
                Case TypeCode.Single
                    Return GetType(Single)
                Case TypeCode.Double
                    Return GetType(Double)
                Case TypeCode.Decimal
                    Return GetType(Decimal)
                Case TypeCode.DateTime
                    Return GetType(DateTime)
                Case TypeCode.String
                    Return GetType(String)
            End Select
            Return Nothing
        End Function

        Friend Shared Function RefersToGenericParameter(ReferringType As Type, Method As MethodBase) As Boolean
            If Symbols.IsRawGeneric(Method) Then
                If ReferringType.IsByRef Then
                    ReferringType = Symbols.GetElementType(ReferringType)
                End If
                If Symbols.IsGenericParameter(ReferringType) Then
                    If Symbols.AreGenericMethodDefsEqual(ReferringType.DeclaringMethod, Method) Then
                        Return True
                    End If
                ElseIf Symbols.IsGeneric(ReferringType) Then
                    Dim typeArguments As Type() = Symbols.GetTypeArguments(ReferringType)
                    Dim i As Integer
                    For i = 0 To typeArguments.Length - 1
                        If Symbols.RefersToGenericParameter(typeArguments(i), Method) Then
                            Return True
                        End If
                    Next i
                ElseIf Symbols.IsArrayType(ReferringType) Then
                    Return Symbols.RefersToGenericParameter(ReferringType.GetElementType, Method)
                End If
            End If
            Return False
        End Function

        Friend Shared Function RefersToGenericParameterCLRSemantics(ReferringType As Type, Typ As Type) As Boolean
            If ReferringType.IsByRef Then
                ReferringType = Symbols.GetElementType(ReferringType)
            End If
            If Symbols.IsGenericParameter(ReferringType) Then
                If (ReferringType.DeclaringType Is Typ) Then
                    Return True
                End If
            ElseIf Symbols.IsGeneric(ReferringType) Then
                Dim typeArguments As Type() = Symbols.GetTypeArguments(ReferringType)
                Dim i As Integer
                For i = 0 To typeArguments.Length - 1
                    If Symbols.RefersToGenericParameterCLRSemantics(typeArguments(i), Typ) Then
                        Return True
                    End If
                Next i
            ElseIf Symbols.IsArrayType(ReferringType) Then
                Return Symbols.RefersToGenericParameterCLRSemantics(ReferringType.GetElementType, Typ)
            End If
            Return False
        End Function


        ' Fields
        Friend Shared ReadOnly NoArgumentNames As String() = New String(0 - 1) {}
        Friend Shared ReadOnly NoArguments As Object() = New Object(0 - 1) {}
        Friend Shared ReadOnly NoTypeArguments As Type() = New Type(0 - 1) {}
        Friend Shared ReadOnly NoTypeParameters As Type() = New Type(0 - 1) {}
        Friend Shared ReadOnly OperatorCLSNames As String() = New String(&H1C - 1) {}
        Friend Shared ReadOnly OperatorNames As String()

        ' Nested Types
        Friend NotInheritable Class Container
            ' Methods
            Friend Sub New(Instance As Object)
                If (Instance Is Nothing) Then
                    Throw ExceptionUtils.VbMakeException(&H5B)
                End If
                Me.m_Instance = Instance
                Me.m_Type = Instance.GetType
                Me.m_UseCustomReflection = False
                If ((Not Me.m_Type.IsCOMObject AndAlso Not RemotingServices.IsTransparentProxy(Instance)) AndAlso Not TypeOf Instance Is Type) Then
                    Me.m_IReflect = TryCast(Instance, IReflect)
                    If (Not Me.m_IReflect Is Nothing) Then
                        Me.m_UseCustomReflection = True
                    End If
                End If
                If Not Me.m_UseCustomReflection Then
                    Me.m_IReflect = Me.m_Type
                End If
                Me.CheckForClassExtendingCOMClass()
            End Sub

            Friend Sub New(Type As Type)
                If (Type Is Nothing) Then
                    Throw ExceptionUtils.VbMakeException(&H5B)
                End If
                Me.m_Instance = Nothing
                Me.m_Type = Type
                Me.m_IReflect = Type
                Me.m_UseCustomReflection = False
                Me.CheckForClassExtendingCOMClass()
            End Sub

            Private Sub CheckForClassExtendingCOMClass()
                If ((Me.IsCOMObject AndAlso Not Me.IsWindowsRuntimeObject) AndAlso ((Me.m_Type.FullName <> "System.__ComObject") AndAlso (Me.m_Type.BaseType.FullName <> "System.__ComObject"))) Then
                    Throw New InvalidOperationException(Utils.GetResourceString("LateboundCallToInheritedComClass"))
                End If
            End Sub

            Private Shared Function FilterInvalidMembers(Members As MemberInfo()) As MemberInfo()
                If ((Not Members Is Nothing) AndAlso (Members.Length <> 0)) Then
                    Dim num As Integer = 0
                    Dim index As Integer = 0
                    Dim num3 As Integer = (Members.Length - 1)
                    index = 0
                    Do While (index <= num3)
                        Dim destinationArray As ParameterInfo() = Nothing
                        Dim returnType As Type = Nothing
                        Dim flag As Boolean = True
                        Select Case Members(index).MemberType
                            Case MemberTypes.Method, MemberTypes.Constructor
                                Dim method As MethodInfo = DirectCast(Members(index), MethodInfo)
                                destinationArray = method.GetParameters
                                returnType = method.ReturnType
                                flag = Container.IsMethodDynamicallyInvokable(method)
                                Exit Select
                            Case MemberTypes.Property
                                Dim info As PropertyInfo = DirectCast(Members(index), PropertyInfo)
                                Dim getMethod As MethodInfo = info.GetGetMethod
                                If (Not getMethod Is Nothing) Then
                                    destinationArray = getMethod.GetParameters
                                Else
                                    Dim parameters As ParameterInfo() = info.GetSetMethod.GetParameters
                                    destinationArray = New ParameterInfo(((parameters.Length - 2) + 1) - 1) {}
                                    Array.Copy(parameters, destinationArray, destinationArray.Length)
                                End If
                                returnType = info.PropertyType
                                Exit Select
                            Case MemberTypes.Field
                                returnType = DirectCast(Members(index), FieldInfo).FieldType
                                Exit Select
                        End Select
                        If (Symbols.AreParametersAndReturnTypesValid(destinationArray, returnType) AndAlso flag) Then
                            num += 1
                        Else
                            Members(index) = Nothing
                        End If
                        index += 1
                    Loop
                    If (num = Members.Length) Then
                        Return Members
                    End If
                    If (num > 0) Then
                        Dim infoArray4 As MemberInfo() = New MemberInfo(((num - 1) + 1) - 1) {}
                        Dim num4 As Integer = 0
                        Dim num5 As Integer = (Members.Length - 1)
                        index = 0
                        Do While (index <= num5)
                            If (Not Members(index) Is Nothing) Then
                                infoArray4(num4) = Members(index)
                                num4 += 1
                            End If
                            index += 1
                        Loop
                        Return infoArray4
                    End If
                End If
                Return Nothing
            End Function

            Friend Function GetArrayValue(Indices As Object()) As Object
                Dim instance As Array = DirectCast(Me.m_Instance, Array)
                Dim rank As Integer = instance.Rank
                If (indices.Length <> rank) Then
                    Throw New RankException
                End If
                Dim index As Integer = CInt(Conversions.ChangeType(indices(0), GetType(Integer)))
                If (rank = 1) Then
                    Return instance.GetValue(index)
                End If
                Dim num3 As Integer = CInt(Conversions.ChangeType(indices(1), GetType(Integer)))
                If (rank = 2) Then
                    Return instance.GetValue(index, num3)
                End If
                Dim num4 As Integer = CInt(Conversions.ChangeType(indices(2), GetType(Integer)))
                If (rank = 3) Then
                    Return instance.GetValue(index, num3, num4)
                End If
                Dim indices As Integer() = New Integer(((rank - 1) + 1) - 1) {}
                indices(0) = index
                indices(1) = num3
                indices(2) = num4
                Dim num5 As Integer = (rank - 1)
                Dim i As Integer = 3
                Do While (i <= num5)
                    indices(i) = CInt(Conversions.ChangeType(indices(i), GetType(Integer)))
                    i += 1
                Loop
                Return instance.GetValue(indices)
            End Function

            Friend Function GetFieldValue(Field As FieldInfo) As Object
                If ((Me.m_Instance Is Nothing) AndAlso Not Symbols.IsShared(Field)) Then
                    Dim args As String() = New String() {Utils.FieldToString(Field)}
                    Throw New NullReferenceException(Utils.GetResourceString("NullReference_InstanceReqToAccessMember1", args))
                End If
                If Symbols.IsNonPublicRuntimeMember(Field) Then
                    Throw New MissingMemberException
                End If
                Return Field.GetValue(Me.m_Instance)
            End Function

            <SecuritySafeCritical, ReflectionPermission(SecurityAction.Assert, Flags:=ReflectionPermissionFlag.MemberAccess)>
            Public Shared Function GetIsInvokableDelegate() As Func(Of MethodBase, Boolean)
                Dim func As Func(Of MethodBase, Boolean) = Nothing
                Dim info As MethodInfo = GetType(MethodBase).GetMethod("get_IsDynamicallyInvokable", (BindingFlags.NonPublic Or (BindingFlags.Public Or BindingFlags.Instance)), Nothing, Type.EmptyTypes, Nothing)
                Dim currentDomain As AppDomain = AppDomain.CurrentDomain
                If (((Not info Is Nothing) AndAlso currentDomain.IsHomogenous) AndAlso currentDomain.IsFullyTrusted) Then
                    func = DirectCast(info.CreateDelegate(GetType(Func(Of MethodBase, Boolean))), Func(Of MethodBase, Boolean))
                End If
                Return func
            End Function

            Friend Function GetMembers(ByRef MemberName As String, ReportErrors As Boolean) As MemberInfo()
                Dim namedMembers As MemberInfo()
                If (MemberName Is Nothing) Then
                    MemberName = ""
                End If
                If (MemberName = "") Then
                    If Me.m_UseCustomReflection Then
                        namedMembers = Me.LookupNamedMembers(MemberName)
                    Else
                        namedMembers = Me.LookupDefaultMembers(MemberName, Me.m_Type)
                    End If
                    If Me.IsWindowsRuntimeObject Then
                        Dim winRTCollectionDefaultMembers As List(Of MemberInfo) = Me.LookupWinRTCollectionDefaultMembers(MemberName)
                        If (Not namedMembers Is Nothing) Then
                            winRTCollectionDefaultMembers.AddRange(namedMembers)
                        End If
                        namedMembers = winRTCollectionDefaultMembers.ToArray
                    End If
                    If (namedMembers.Length = 0) Then
                        If ReportErrors Then
                            Dim args As String() = New String() {Me.VBFriendlyName}
                            Throw New MissingMemberException(Utils.GetResourceString("MissingMember_NoDefaultMemberFound1", args))
                        End If
                        Return namedMembers
                    End If
                    If Me.m_UseCustomReflection Then
                        MemberName = namedMembers(0).Name
                    End If
                    Return namedMembers
                End If
                namedMembers = Me.LookupNamedMembers(MemberName)
                If (namedMembers.Length = 0) Then
                    If ReportErrors Then
                        Dim textArray2 As String() = New String() {MemberName, Me.VBFriendlyName}
                        Throw New MissingMemberException(Utils.GetResourceString("MissingMember_MemberNotFoundOnType2", textArray2))
                    End If
                    Return namedMembers
                End If
                Return namedMembers
            End Function

            Friend Function InvokeMethod(TargetProcedure As Method, Arguments As Object(), CopyBack As Boolean(), Flags As BindingFlags) As Object
                Dim obj2 As Object
                Dim callTarget As MethodBase = NewLateBinding.GetCallTarget(TargetProcedure, Flags)
                Dim parameters As Object() = NewLateBinding.ConstructCallArguments(TargetProcedure, Arguments, Flags)
                If ((Me.m_Instance Is Nothing) AndAlso Not Symbols.IsShared(callTarget)) Then
                    Dim args As String() = New String() {TargetProcedure.ToString}
                    Throw New NullReferenceException(Utils.GetResourceString("NullReference_InstanceReqToAccessMember1", args))
                End If
                If Symbols.IsNonPublicRuntimeMember(callTarget) Then
                    Throw New MissingMemberException
                End If
                Try
                    obj2 = callTarget.Invoke(Me.m_Instance, parameters)
                Catch obj1 As Object When (?)
                    Dim exception As TargetInvocationException
                    Throw exception.InnerException
                End Try
                OverloadResolution.ReorderArgumentArray(TargetProcedure, parameters, Arguments, CopyBack, Flags)
                Return obj2
            End Function

            Private Shared Function IsMethodDynamicallyInvokable(Method As MethodBase) As Boolean
                If (Not Container.s_IsInvokableDelegate Is Nothing) Then
                    Return Container.s_IsInvokableDelegate.Invoke(Method)
                End If
                Return True
            End Function

            Private Function LookupDefaultMembers(ByRef DefaultMemberName As String, SearchType As Type) As MemberInfo()
                Dim name As String = Nothing
                Dim baseType As Type = SearchType
                Do
                    Dim customAttributes As Object() = baseType.GetCustomAttributes(GetType(DefaultMemberAttribute), False)
                    If ((Not customAttributes Is Nothing) AndAlso (customAttributes.Length > 0)) Then
                        name = DirectCast(customAttributes(0), DefaultMemberAttribute).MemberName
                        Exit Do
                    End If
                    baseType = baseType.BaseType
                Loop While ((Not baseType Is Nothing) AndAlso Not Symbols.IsRootObjectType(baseType))
                If (Not name Is Nothing) Then
                    Dim array As MemberInfo() = Container.FilterInvalidMembers(baseType.GetMember(name, (BindingFlags.FlattenHierarchy Or (BindingFlags.Public Or (BindingFlags.Static Or (BindingFlags.Instance Or BindingFlags.IgnoreCase))))))
                    If (Not array Is Nothing) Then
                        DefaultMemberName = name
                        If (array.Length > 1) Then
                            array.Sort(array, InheritanceSorter.Instance)
                        End If
                        Return array
                    End If
                End If
                Return Container.NoMembers
            End Function

            Friend Function LookupNamedMembers(MemberName As String) As MemberInfo()
                Dim member As MemberInfo()
                If Symbols.IsGenericParameter(Me.m_Type) Then
                    Dim classConstraint As Type = Symbols.GetClassConstraint(Me.m_Type)
                    If (Not classConstraint Is Nothing) Then
                        member = classConstraint.GetMember(MemberName, (BindingFlags.FlattenHierarchy Or (BindingFlags.Public Or (BindingFlags.Static Or (BindingFlags.Instance Or BindingFlags.IgnoreCase)))))
                    Else
                        member = Nothing
                    End If
                Else
                    member = Me.m_IReflect.GetMember(MemberName, (BindingFlags.FlattenHierarchy Or (BindingFlags.Public Or (BindingFlags.Static Or (BindingFlags.Instance Or BindingFlags.IgnoreCase)))))
                End If
                If Me.IsWindowsRuntimeObject Then
                    Dim winRTCollectionInterfaceMembers As List(Of MemberInfo) = Me.LookupWinRTCollectionInterfaceMembers(MemberName)
                    If (Not member Is Nothing) Then
                        winRTCollectionInterfaceMembers.AddRange(member)
                    End If
                    member = winRTCollectionInterfaceMembers.ToArray
                End If
                member = Container.FilterInvalidMembers(member)
                If (member Is Nothing) Then
                    Return Container.NoMembers
                End If
                If (member.Length > 1) Then
                    Array.Sort(member, InheritanceSorter.Instance)
                End If
                Return member
            End Function

            Private Function LookupWinRTCollectionDefaultMembers(ByRef DefaultMemberName As String) As List(Of MemberInfo)
                Dim list As New List(Of MemberInfo)
                Dim type As Type
                For Each type In Me.m_Type.GetInterfaces
                    If Symbols.IsCollectionInterface(type) Then
                        Dim defaultMembers As MemberInfo() = Me.LookupDefaultMembers(DefaultMemberName, type)
                        If (Not defaultMembers Is Nothing) Then
                            list.AddRange(defaultMembers)
                        End If
                    End If
                Next
                Return list
            End Function

            Friend Function LookupWinRTCollectionInterfaceMembers(MemberName As String) As List(Of MemberInfo)
                Dim list As New List(Of MemberInfo)
                Dim type As Type
                For Each type In Me.m_Type.GetInterfaces
                    If Symbols.IsCollectionInterface(type) Then
                        Dim member As MemberInfo() = type.GetMember(MemberName, (BindingFlags.FlattenHierarchy Or (BindingFlags.Public Or (BindingFlags.Static Or (BindingFlags.Instance Or BindingFlags.IgnoreCase)))))
                        If (Not member Is Nothing) Then
                            list.AddRange(member)
                        End If
                    End If
                Next
                Return list
            End Function

            Friend Sub SetArrayValue(Arguments As Object())
                Dim instance As Array = DirectCast(Me.m_Instance, Array)
                Dim rank As Integer = instance.Rank
                If ((Arguments.Length - 1) <> rank) Then
                    Throw New RankException
                End If
                Dim expression As Object = Arguments((Arguments.Length - 1))
                Dim elementType As Type = Me.m_Type.GetElementType
                Dim index As Integer = CInt(Conversions.ChangeType(Arguments(0), GetType(Integer)))
                If (rank = 1) Then
                    instance.SetValue(Conversions.ChangeType(expression, elementType), index)
                Else
                    Dim num3 As Integer = CInt(Conversions.ChangeType(Arguments(1), GetType(Integer)))
                    If (rank = 2) Then
                        instance.SetValue(Conversions.ChangeType(expression, elementType), index, num3)
                    Else
                        Dim num4 As Integer = CInt(Conversions.ChangeType(Arguments(2), GetType(Integer)))
                        If (rank = 3) Then
                            instance.SetValue(Conversions.ChangeType(expression, elementType), index, num3, num4)
                        Else
                            Dim indices As Integer() = New Integer(((rank - 1) + 1) - 1) {}
                            indices(0) = index
                            indices(1) = num3
                            indices(2) = num4
                            Dim num5 As Integer = (rank - 1)
                            Dim i As Integer = 3
                            Do While (i <= num5)
                                indices(i) = CInt(Conversions.ChangeType(Arguments(i), GetType(Integer)))
                                i += 1
                            Loop
                            instance.SetValue(Conversions.ChangeType(expression, elementType), indices)
                        End If
                    End If
                End If
            End Sub

            Friend Sub SetFieldValue(Field As FieldInfo, Value As Object)
                If Field.IsInitOnly Then
                    Dim args As String() = New String() {Field.Name, Me.VBFriendlyName}
                    Throw New MissingMemberException(Utils.GetResourceString("MissingMember_ReadOnlyField2", args))
                End If
                If ((Me.m_Instance Is Nothing) AndAlso Not Symbols.IsShared(Field)) Then
                    Dim textArray2 As String() = New String() {Utils.FieldToString(Field)}
                    Throw New NullReferenceException(Utils.GetResourceString("NullReference_InstanceReqToAccessMember1", textArray2))
                End If
                If Symbols.IsNonPublicRuntimeMember(Field) Then
                    Throw New MissingMemberException
                End If
                Field.SetValue(Me.m_Instance, Conversions.ChangeType(Value, Field.FieldType))
            End Sub


            ' Properties
            Friend ReadOnly Property IsArray As Boolean
                Get
                    Return (Symbols.IsArrayType(Me.m_Type) AndAlso (Me.m_Instance IsNot Nothing))
                End Get
            End Property

            Friend ReadOnly Property IsCOMObject As Boolean
                Get
                    Return Me.m_Type.IsCOMObject
                End Get
            End Property

            Friend ReadOnly Property IsValueType As Boolean
                Get
                    Return (Symbols.IsValueType(Me.m_Type) AndAlso (Me.m_Instance IsNot Nothing))
                End Get
            End Property

            Friend ReadOnly Property IsWindowsRuntimeObject As Boolean
                Get
                    Dim type As Type = Me.m_Type
                    Do While (Not type Is Nothing)
                        If type.Attributes.HasFlag(TypeAttributes.WindowsRuntime) Then
                            Return True
                        End If
                        If type.Attributes.HasFlag(TypeAttributes.Import) Then
                            Return False
                        End If
                        type = type.BaseType
                    Loop
                    Return False
                End Get
            End Property

            Friend ReadOnly Property VBFriendlyName As String
                Get
                    Return Utils.VBFriendlyName(Me.m_Type, Me.m_Instance)
                End Get
            End Property


            ' Fields
            Private Const DefaultLookupFlags As BindingFlags = (BindingFlags.FlattenHierarchy Or (BindingFlags.Public Or (BindingFlags.Static Or (BindingFlags.Instance Or BindingFlags.IgnoreCase))))
            Private ReadOnly m_Instance As Object
            Private ReadOnly m_IReflect As IReflect
            Private ReadOnly m_Type As Type
            Private ReadOnly m_UseCustomReflection As Boolean
            Private Shared ReadOnly NoMembers As MemberInfo() = New MemberInfo(0 - 1) {}
            Private Shared s_IsInvokableDelegate As Func(Of MethodBase, Boolean) = Container.GetIsInvokableDelegate

            ' Nested Types
            Private Class InheritanceSorter
                Implements IComparer
                ' Methods
                Private Sub New()
                End Sub

                Private Function [Compare](Left As Object, Right As Object) As Integer Implements IComparer.Compare
                    Dim declaringType As Type = DirectCast(Left, MemberInfo).DeclaringType
                    Dim c As Type = DirectCast(Right, MemberInfo).DeclaringType
                    If (declaringType Is c) Then
                        Return 0
                    End If
                    If declaringType.IsSubclassOf(c) Then
                        Return -1
                    End If
                    Return 1
                End Function


                ' Fields
                Friend Shared ReadOnly Instance As InheritanceSorter = New InheritanceSorter
            End Class
        End Class

        Friend NotInheritable Class Method
            ' Methods
            Private Sub New(Parameters As ParameterInfo(), ParamArrayIndex As Integer, ParamArrayExpanded As Boolean)
                Me.m_Parameters = Parameters
                Me.m_RawParameters = Parameters
                Me.ParamArrayIndex = ParamArrayIndex
                Me.ParamArrayExpanded = ParamArrayExpanded
                Me.AllNarrowingIsFromObject = True
            End Sub

            Friend Sub New(Method As MethodBase, Parameters As ParameterInfo(), ParamArrayIndex As Integer, ParamArrayExpanded As Boolean)
                Me.New(Parameters, ParamArrayIndex, ParamArrayExpanded)
                Me.m_Item = Method
                Me.m_RawItem = Method
            End Sub

            Friend Sub New([Property] As PropertyInfo, Parameters As ParameterInfo(), ParamArrayIndex As Integer, ParamArrayExpanded As Boolean)
                Me.New(Parameters, ParamArrayIndex, ParamArrayExpanded)
                Me.m_Item = [Property]
            End Sub

            Friend Function AsMethod() As MethodBase
                Return TryCast(Me.m_Item, MethodBase)
            End Function

            Friend Function AsProperty() As PropertyInfo
                Return TryCast(Me.m_Item, PropertyInfo)
            End Function

            Friend Function BindGenericArguments() As Boolean
                Try
                    Me.m_Item = DirectCast(Me.m_RawItem, MethodInfo).MakeGenericMethod(Me.TypeArguments)
                    Me.m_Parameters = Me.AsMethod.GetParameters
                    Return True
                Catch exception As ArgumentException
                    Return False
                End Try
            End Function

            Public Shared Operator =(Left As Method, Right As Method) As Boolean
                Return (Left.m_Item Is Right.m_Item)
            End Operator

            Public Shared Operator =(Left As MemberInfo, Right As Method) As Boolean
                Return (Left Is Right.m_Item)
            End Operator

            Public Shared Operator <>(Left As Method, right As Method) As Boolean
                Return Not (Left.m_Item Is right.m_Item)
            End Operator

            Public Shared Operator <>(Left As MemberInfo, Right As Method) As Boolean
                Return Not (Left Is Right.m_Item)
            End Operator

            Public Overrides Function ToString() As String
                Return Utils.MemberToString(Me.m_Item)
            End Function


            ' Properties
            Friend ReadOnly Property DeclaringType As Type
                Get
                    Return Me.m_Item.DeclaringType
                End Get
            End Property

            Friend ReadOnly Property HasByRefParameter As Boolean
                Get
                    Dim parameters As ParameterInfo() = Me.Parameters
                    Dim i As Integer
                    For i = 0 To parameters.Length - 1
                        If parameters(i).ParameterType.IsByRef Then
                            Return True
                        End If
                    Next i
                    Return False
                End Get
            End Property

            Friend ReadOnly Property HasParamArray As Boolean
                Get
                    Return (Me.ParamArrayIndex > -1)
                End Get
            End Property

            Friend ReadOnly Property IsGeneric As Boolean
                Get
                    Return Symbols.IsGeneric(Me.m_Item)
                End Get
            End Property

            Friend ReadOnly Property IsMethod As Boolean
                Get
                    If (Me.m_Item.MemberType <> MemberTypes.Method) Then
                        Return (Me.m_Item.MemberType = MemberTypes.Constructor)
                    End If
                    Return True
                End Get
            End Property

            Friend ReadOnly Property IsProperty As Boolean
                Get
                    Return (Me.m_Item.MemberType = MemberTypes.Property)
                End Get
            End Property

            Friend ReadOnly Property Parameters As ParameterInfo()
                Get
                    Return Me.m_Parameters
                End Get
            End Property

            Friend ReadOnly Property RawDeclaringType As Type
                Get
                    If (Me.m_RawDeclaringType Is Nothing) Then
                        Dim declaringType As Type = Me.m_Item.DeclaringType
                        Dim metadataToken As Integer = declaringType.MetadataToken
                        Me.m_RawDeclaringType = declaringType.Module.ResolveType(metadataToken, Nothing, Nothing)
                    End If
                    Return Me.m_RawDeclaringType
                End Get
            End Property

            Friend ReadOnly Property RawParameters As ParameterInfo()
                Get
                    Return Me.m_RawParameters
                End Get
            End Property

            Friend ReadOnly Property RawParametersFromType As ParameterInfo()
                Get
                    If (Me.m_RawParametersFromType Is Nothing) Then
                        If Not Me.IsProperty Then
                            Dim metadataToken As Integer = Me.m_Item.MetadataToken
                            Me.m_RawParametersFromType = Me.m_Item.DeclaringType.Module.ResolveMethod(metadataToken, Nothing, Nothing).GetParameters
                        Else
                            Me.m_RawParametersFromType = Me.m_RawParameters
                        End If
                    End If
                    Return Me.m_RawParametersFromType
                End Get
            End Property

            Friend ReadOnly Property TypeParameters As Type()
                Get
                    Return Symbols.GetTypeParameters(Me.m_Item)
                End Get
            End Property


            ' Fields
            Friend AllNarrowingIsFromObject As Boolean
            Friend ArgumentMatchingDone As Boolean
            Friend ArgumentsValidated As Boolean
            Friend LessSpecific As Boolean
            Private m_Item As MemberInfo
            Private m_Parameters As ParameterInfo()
            Private m_RawDeclaringType As Type
            Private m_RawItem As MethodBase
            Private m_RawParameters As ParameterInfo()
            Private m_RawParametersFromType As ParameterInfo()
            Friend NamedArgumentMapping As Integer()
            Friend NotCallable As Boolean
            Friend ReadOnly ParamArrayExpanded As Boolean
            Friend ReadOnly ParamArrayIndex As Integer
            Friend RequiresNarrowingConversion As Boolean
            Friend TypeArguments As Type()
            Friend UsedDefaultForAnOptionalParameter As Boolean
        End Class

        Friend NotInheritable Class TypedNothing
            ' Methods
            Friend Sub New(Type As Type)
                Me.Type = Type
            End Sub


            ' Fields
            Friend ReadOnly Type As Type
        End Class

        Friend Enum UserDefinedOperator As SByte
            ' Fields
            [And] = 20
            Concatenate = 14
            Divide = 11
            Equal = &H16
            Greater = &H1B
            GreaterEqual = &H1A
            IntegralDivide = 13
            IsFalse = 4
            IsTrue = 3
            Less = &H18
            LessEqual = &H19
            [Like] = &H15
            MAX = &H1C
            Minus = 9
            Modulus = &H11
            Multiply = 10
            Narrow = 1
            Negate = 5
            [Not] = 6
            NotEqual = &H17
            [Or] = &H12
            Plus = 8
            Power = 12
            ShiftLeft = 15
            ShiftRight = &H10
            UnaryPlus = 7
            UNDEF = 0
            Widen = 2
            [Xor] = &H13
        End Enum
    End Class
End Namespace

