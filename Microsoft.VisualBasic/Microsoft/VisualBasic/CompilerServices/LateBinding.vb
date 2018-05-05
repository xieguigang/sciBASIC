Imports System.ComponentModel
Imports System.Diagnostics
Imports System.Reflection
Imports System.Runtime.InteropServices
Imports System.Runtime.Remoting
Imports System.Threading

Namespace Microsoft.VisualBasic.CompilerServices
    <EditorBrowsable(EditorBrowsableState.Never)> _
    Public NotInheritable Class LateBinding
        ' Methods
        Private Sub New()
        End Sub

        Private Shared Sub CheckForClassExtendingCOMClass(objType As Type)
            If ((objType.IsCOMObject AndAlso (objType.FullName <> "System.__ComObject")) AndAlso (objType.BaseType.FullName <> "System.__ComObject")) Then
                Throw New InvalidOperationException(Utils.GetResourceString("LateboundCallToInheritedComClass"))
            End If
        End Sub

        Friend Shared Function DoesTargetObjectMatch(Value As Object, Member As MemberInfo) As Boolean
            Return ((Value Is Nothing) OrElse Member.DeclaringType.IsAssignableFrom(Value.GetType))
        End Function

        <DebuggerHidden, DebuggerStepThrough>
        Private Shared Function FastCall(o As Object, method As MethodBase, Parameters As ParameterInfo(), args As Object(), objType As Type, objIReflect As IReflect) As Object
            Dim upperBound As Integer = args.GetUpperBound(0)
            Dim i As Integer = 0
            Do While (i <= upperBound)
                Dim info As ParameterInfo = Parameters(i)
                Dim defaultValue As Object = args(i)
                If (TypeOf defaultValue Is Missing AndAlso info.IsOptional) Then
                    defaultValue = info.DefaultValue
                End If
                args(i) = ObjectType.CTypeHelper(defaultValue, info.ParameterType)
                i += 1
            Loop
            VBBinder.SecurityCheckForLateboundCalls(method, objType, objIReflect)
            If (((objType Is objIReflect) OrElse method.IsStatic) OrElse LateBinding.DoesTargetObjectMatch(o, method)) Then
                LateBinding.VerifyObjRefPresentForInstanceCall(o, method)
                Return method.Invoke(o, args)
            End If
            Return LateBinding.InvokeMemberOnIReflect(objIReflect, method, BindingFlags.InvokeMethod, o, args)
        End Function

        Private Shared Function GetCorrectIReflect(o As Object, objType As Type) As IReflect
            If (((Not o Is Nothing) AndAlso Not objType.IsCOMObject) AndAlso (Not RemotingServices.IsTransparentProxy(o) AndAlso Not TypeOf o Is Type)) Then
                Dim reflect2 As IReflect = TryCast(o, IReflect)
                If (Not reflect2 Is Nothing) Then
                    Return reflect2
                End If
            End If
            Return objType
        End Function

        Private Shared Function GetDefaultMembers(typ As Type, objIReflect As IReflect, ByRef DefaultName As String) As MemberInfo()
            Dim nonGenericMembers As MemberInfo()
            If (typ Is objIReflect) Then
                Do
                    Dim customAttributes As Object() = typ.GetCustomAttributes(GetType(DefaultMemberAttribute), False)
                    If ((Not customAttributes Is Nothing) AndAlso (customAttributes.Length <> 0)) Then
                        DefaultName = DirectCast(customAttributes(0), DefaultMemberAttribute).MemberName
                        nonGenericMembers = LateBinding.GetNonGenericMembers(typ.GetMember(DefaultName, (BindingFlags.FlattenHierarchy Or (BindingFlags.Public Or (BindingFlags.Static Or (BindingFlags.Instance Or BindingFlags.IgnoreCase))))))
                        If ((Not nonGenericMembers Is Nothing) AndAlso (nonGenericMembers.Length <> 0)) Then
                            Return nonGenericMembers
                        End If
                        DefaultName = ""
                        Return Nothing
                    End If
                    typ = typ.BaseType
                Loop While (Not typ Is Nothing)
                DefaultName = ""
                Return Nothing
            End If
            nonGenericMembers = LateBinding.GetNonGenericMembers(objIReflect.GetMember("", (BindingFlags.FlattenHierarchy Or (BindingFlags.Public Or (BindingFlags.Static Or (BindingFlags.Instance Or BindingFlags.IgnoreCase))))))
            If ((nonGenericMembers Is Nothing) OrElse (nonGenericMembers.Length = 0)) Then
                DefaultName = ""
                Return Nothing
            End If
            DefaultName = nonGenericMembers(0).Name
            Return nonGenericMembers
        End Function

        Private Shared Function GetMembersByName(objIReflect As IReflect, name As String, flags As BindingFlags) As MemberInfo()
            Dim nonGenericMembers As MemberInfo() = LateBinding.GetNonGenericMembers(objIReflect.GetMember(name, flags))
            If ((Not nonGenericMembers Is Nothing) AndAlso (nonGenericMembers.Length = 0)) Then
                nonGenericMembers = Nothing
            End If
            Return nonGenericMembers
        End Function

        Private Shared Function GetMostDerivedMemberInfo(objIReflect As IReflect, name As String, flags As BindingFlags) As MemberInfo
            Dim nonGenericMembers As MemberInfo() = LateBinding.GetNonGenericMembers(objIReflect.GetMember(name, flags))
            If ((nonGenericMembers Is Nothing) OrElse (nonGenericMembers.Length = 0)) Then
                Return Nothing
            End If
            Dim info2 As MemberInfo = nonGenericMembers(0)
            Dim upperBound As Integer = nonGenericMembers.GetUpperBound(0)
            Dim i As Integer = 1
            Do While (i <= upperBound)
                If nonGenericMembers(i).DeclaringType.IsSubclassOf(info2.DeclaringType) Then
                    info2 = nonGenericMembers(i)
                End If
                i += 1
            Loop
            Return info2
        End Function

        Friend Shared Function GetNonGenericMembers(Members As MemberInfo()) As MemberInfo()
            If ((Not Members Is Nothing) AndAlso (Members.Length > 0)) Then
                Dim num As Integer = 0
                Dim upperBound As Integer = Members.GetUpperBound(0)
                Dim i As Integer = 0
                Do While (i <= upperBound)
                    If LateBinding.LegacyIsGeneric(Members(i)) Then
                        Members(i) = Nothing
                    Else
                        num += 1
                    End If
                    i += 1
                Loop
                If (num = (Members.GetUpperBound(0) + 1)) Then
                    Return Members
                End If
                If (num > 0) Then
                    Dim infoArray2 As MemberInfo() = New MemberInfo(((num - 1) + 1) - 1) {}
                    Dim index As Integer = 0
                    Dim num5 As Integer = Members.GetUpperBound(0)
                    Dim j As Integer = 0
                    Do While (j <= num5)
                        If (Not Members(j) Is Nothing) Then
                            infoArray2(index) = Members(j)
                            index += 1
                        End If
                        j += 1
                    Loop
                    Return infoArray2
                End If
            End If
            Return Nothing
        End Function

        Private Shared Function GetPropertyPutFlags(NewValue As Object) As BindingFlags
            If (NewValue Is Nothing) Then
                Return BindingFlags.SetProperty
            End If
            If (((TypeOf NewValue Is ValueType OrElse TypeOf NewValue Is String) OrElse (TypeOf NewValue Is DBNull OrElse TypeOf NewValue Is Missing)) OrElse (TypeOf NewValue Is Array OrElse TypeOf NewValue Is CurrencyWrapper)) Then
                Return BindingFlags.PutDispProperty
            End If
            Return BindingFlags.PutRefDispProperty
        End Function

        <DebuggerHidden, DebuggerStepThrough>
        Friend Shared Function InternalLateCall(o As Object, objType As Type, name As String, args As Object(), paramnames As String(), CopyBack As Boolean(), IgnoreReturn As Boolean) As Object
            Dim obj2 As Object
            Dim flags As BindingFlags = (BindingFlags.OptionalParamBinding Or (BindingFlags.InvokeMethod Or (BindingFlags.FlattenHierarchy Or (BindingFlags.Public Or (BindingFlags.Static Or (BindingFlags.Instance Or BindingFlags.IgnoreCase))))))
            If IgnoreReturn Then
                flags = (flags Or BindingFlags.IgnoreReturn)
            End If
            If (objType Is Nothing) Then
                If (o Is Nothing) Then
                    Throw ExceptionUtils.VbMakeException(&H5B)
                End If
                objType = o.GetType
            End If
            Dim correctIReflect As IReflect = LateBinding.GetCorrectIReflect(o, objType)
            If objType.IsCOMObject Then
                LateBinding.CheckForClassExtendingCOMClass(objType)
            End If
            If (name Is Nothing) Then
                name = ""
            End If
            Dim binder As New VBBinder(CopyBack)
            If Not objType.IsCOMObject Then
                Dim mi As MemberInfo() = LateBinding.GetMembersByName(correctIReflect, name, flags)
                If ((mi Is Nothing) OrElse (mi.Length = 0)) Then
                    Dim textArray1 As String() = New String() {name, Utils.VBFriendlyName(objType, o)}
                    Throw New MissingMemberException(Utils.GetResourceString("MissingMember_MemberNotFoundOnType2", textArray1))
                End If
                If LateBinding.MemberIsField(mi) Then
                    Dim textArray2 As String() = New String() {name, Utils.VBFriendlyName(objType, o)}
                    Throw New ArgumentException(Utils.GetResourceString("ExpressionNotProcedure", textArray2))
                End If
                If ((mi.Length = 1) AndAlso ((paramnames Is Nothing) OrElse (paramnames.Length = 0))) Then
                    Dim getMethod As MemberInfo = mi(0)
                    If (getMethod.MemberType = MemberTypes.Property) Then
                        getMethod = DirectCast(getMethod, PropertyInfo).GetGetMethod
                        If (getMethod Is Nothing) Then
                            Dim textArray3 As String() = New String() {name, Utils.VBFriendlyName(objType, o)}
                            Throw New MissingMemberException(Utils.GetResourceString("MissingMember_MemberNotFoundOnType2", textArray3))
                        End If
                    End If
                    Dim method As MethodBase = DirectCast(getMethod, MethodBase)
                    Dim parameters As ParameterInfo() = method.GetParameters
                    Dim length As Integer = args.Length
                    Dim num2 As Integer = parameters.Length
                    If (num2 = length) Then
                        If (num2 = 0) Then
                            Return LateBinding.FastCall(o, method, parameters, args, objType, correctIReflect)
                        End If
                        If ((CopyBack Is Nothing) AndAlso LateBinding.NoByrefs(parameters)) Then
                            Dim info2 As ParameterInfo = parameters((num2 - 1))
                            If info2.ParameterType.IsArray Then
                                Dim customAttributes As Object() = info2.GetCustomAttributes(GetType(ParamArrayAttribute), False)
                                If ((customAttributes Is Nothing) OrElse (customAttributes.Length = 0)) Then
                                    Return LateBinding.FastCall(o, method, parameters, args, objType, correctIReflect)
                                End If
                            Else
                                Return LateBinding.FastCall(o, method, parameters, args, objType, correctIReflect)
                            End If
                        End If
                    End If
                End If
            End If
            Try
                obj2 = binder.InvokeMember(name, flags, objType, correctIReflect, o, args, paramnames)
            Catch exception As MissingMemberException
                Throw
            Catch obj1 As Object When (?)
                Dim textArray4 As String() = New String() {name, Utils.VBFriendlyName(objType, o)}
                Throw New MissingMemberException(Utils.GetResourceString("MissingMember_MemberNotFoundOnType2", textArray4))
            Catch exception3 As TargetInvocationException
                Throw exception3.InnerException
            End Try
            Return obj2
        End Function

        <DebuggerHidden, DebuggerStepThrough>
        Friend Shared Sub InternalLateSet(o As Object, ByRef objType As Type, name As String, args As Object(), paramnames As String(), OptimisticSet As Boolean, UseCallType As CallType)
            Dim invokeAttr As BindingFlags = (BindingFlags.OptionalParamBinding Or (BindingFlags.FlattenHierarchy Or (BindingFlags.Public Or (BindingFlags.Static Or (BindingFlags.Instance Or BindingFlags.IgnoreCase)))))
            If (objType Is Nothing) Then
                If (o Is Nothing) Then
                    Throw ExceptionUtils.VbMakeException(&H5B)
                End If
                objType = o.GetType
            End If
            Dim correctIReflect As IReflect = LateBinding.GetCorrectIReflect(o, objType)
            If (name Is Nothing) Then
                name = ""
            End If
            If objType.IsCOMObject Then
                LateBinding.CheckForClassExtendingCOMClass(objType)
                If (UseCallType = CallType.Set) Then
                    invokeAttr = (invokeAttr Or BindingFlags.PutRefDispProperty)
                    If (args(args.GetUpperBound(0)) Is Nothing) Then
                        args(args.GetUpperBound(0)) = New DispatchWrapper(Nothing)
                    End If
                ElseIf (UseCallType = CallType.Let) Then
                    invokeAttr = (invokeAttr Or BindingFlags.PutDispProperty)
                Else
                    invokeAttr = (invokeAttr Or LateBinding.GetPropertyPutFlags(args(args.GetUpperBound(0))))
                End If
            Else
                invokeAttr = (invokeAttr Or BindingFlags.SetProperty)
                Dim member As MemberInfo = LateBinding.GetMostDerivedMemberInfo(correctIReflect, name, (invokeAttr Or BindingFlags.SetField))
                If ((Not member Is Nothing) AndAlso (member.MemberType = MemberTypes.Field)) Then
                    Dim info2 As FieldInfo = DirectCast(member, FieldInfo)
                    If info2.IsInitOnly Then
                        Dim textArray1 As String() = New String() {name, Utils.VBFriendlyName(objType, o)}
                        Throw New MissingMemberException(Utils.GetResourceString("MissingMember_ReadOnlyField2", textArray1))
                    End If
                    If ((args Is Nothing) OrElse (args.Length = 0)) Then
                        Dim textArray2 As String() = New String() {name, Utils.VBFriendlyName(objType, o)}
                        Throw New MissingMemberException(Utils.GetResourceString("MissingMember_MemberNotFoundOnType2", textArray2))
                    End If
                    If (args.Length = 1) Then
                        Dim obj2 As Object
                        VBBinder.SecurityCheckForLateboundCalls(info2, objType, correctIReflect)
                        If (args(0) Is Nothing) Then
                            obj2 = Nothing
                        Else
                            obj2 = ObjectType.CTypeHelper(args(0), info2.FieldType)
                        End If
                        If (((objType Is correctIReflect) OrElse info2.IsStatic) OrElse LateBinding.DoesTargetObjectMatch(o, info2)) Then
                            LateBinding.VerifyObjRefPresentForInstanceCall(o, info2)
                            info2.SetValue(o, obj2)
                            Return
                        End If
                        Dim objArray1 As Object() = New Object() {obj2}
                        LateBinding.InvokeMemberOnIReflect(correctIReflect, info2, BindingFlags.SetField, o, objArray1)
                        Return
                    End If
                    If (args.Length > 1) Then
                        VBBinder.SecurityCheckForLateboundCalls(member, objType, correctIReflect)
                        Dim obj3 As Object = Nothing
                        If (((objType Is correctIReflect) OrElse DirectCast(member, FieldInfo).IsStatic) OrElse LateBinding.DoesTargetObjectMatch(o, member)) Then
                            LateBinding.VerifyObjRefPresentForInstanceCall(o, member)
                            obj3 = DirectCast(member, FieldInfo).GetValue(o)
                        Else
                            Dim objArray2 As Object() = New Object() {obj3}
                            obj3 = LateBinding.InvokeMemberOnIReflect(correctIReflect, member, BindingFlags.GetField, o, objArray2)
                        End If
                        LateBinding.LateIndexSet(obj3, args, paramnames)
                        Return
                    End If
                End If
            End If
            Dim binder As New VBBinder(Nothing)
            If (OptimisticSet AndAlso (args.GetUpperBound(0) > 0)) Then
                Dim bindingAttr As BindingFlags = (BindingFlags.OptionalParamBinding Or (BindingFlags.GetProperty Or (BindingFlags.FlattenHierarchy Or (BindingFlags.Public Or (BindingFlags.Static Or (BindingFlags.Instance Or BindingFlags.IgnoreCase))))))
                Dim types As Type() = New Type(((args.GetUpperBound(0) - 1) + 1) - 1) {}
                Dim upperBound As Integer = types.GetUpperBound(0)
                Dim i As Integer = 0
                Do While (i <= upperBound)
                    Dim obj4 As Object = args(i)
                    If (obj4 Is Nothing) Then
                        types(i) = Nothing
                    Else
                        types(i) = obj4.GetType
                    End If
                    i += 1
                Loop
                Try
                    Dim info3 As PropertyInfo = correctIReflect.GetProperty(name, bindingAttr, binder, GetType(Integer), types, Nothing)
                    If ((Not info3 Is Nothing) AndAlso info3.CanWrite) Then
                        GoTo Label_029F
                    End If
                Catch exception As MissingMemberException
                End Try
                Return
            End If
Label_029F:
            Try
                binder.InvokeMember(name, invokeAttr, objType, correctIReflect, o, args, paramnames)
            Catch obj1 As Object When (?)
                If ((Not args Is Nothing) AndAlso (args.Length > 1)) Then
                    Dim exception2 As Exception
                    Dim obj5 As Object
                    invokeAttr = (BindingFlags.OptionalParamBinding Or (BindingFlags.GetProperty Or (BindingFlags.FlattenHierarchy Or (BindingFlags.Public Or (BindingFlags.Static Or (BindingFlags.Instance Or BindingFlags.IgnoreCase))))))
                    If Not objType.IsCOMObject Then
                        invokeAttr = (invokeAttr Or BindingFlags.GetField)
                    End If
                    Try
                        obj5 = binder.InvokeMember(name, invokeAttr, objType, correctIReflect, o, Nothing, Nothing)
                    Catch obj6 As Object When (?)
                        Throw exception2
                    Catch exception4 As AccessViolationException
                        Throw exception4
                    Catch exception5 As StackOverflowException
                        Throw exception5
                    Catch exception6 As OutOfMemoryException
                        Throw exception6
                    Catch exception7 As ThreadAbortException
                        Throw exception7
                    Catch exception8 As Exception
                        obj5 = Nothing
                    End Try
                    If (obj5 Is Nothing) Then
                        Dim textArray3 As String() = New String() {name, Utils.VBFriendlyName(objType, o)}
                        Throw New MissingMemberException(Utils.GetResourceString("MissingMember_MemberNotFoundOnType2", textArray3))
                    End If
                    Try
                        LateBinding.LateIndexSet(obj5, args, paramnames)
                        Return
                    Catch obj7 As Object When (?)
                        Throw exception2
                    End Try
                End If
                Dim textArray4 As String() = New String() {name, Utils.VBFriendlyName(objType, o)}
                Throw New MissingMemberException(Utils.GetResourceString("MissingMember_MemberNotFoundOnType2", textArray4))
            Catch exception10 As TargetInvocationException
                If (exception10.InnerException Is Nothing) Then
                    Throw exception10
                End If
                If Not TypeOf exception10.InnerException Is TargetParameterCountException Then
                    Throw exception10.InnerException
                End If
                If ((invokeAttr And BindingFlags.PutRefDispProperty) <> BindingFlags.Default) Then
                    Dim textArray5 As String() = New String() {name, Utils.VBFriendlyName(objType, o)}
                    Throw New MissingMemberException(Utils.GetResourceString("MissingMember_MemberSetNotFoundOnType2", textArray5))
                End If
                Dim textArray6 As String() = New String() {name, Utils.VBFriendlyName(objType, o)}
                Throw New MissingMemberException(Utils.GetResourceString("MissingMember_MemberLetNotFoundOnType2", textArray6))
            End Try
        End Sub

        Friend Shared Function InvokeMemberOnIReflect(objIReflect As IReflect, member As MemberInfo, flags As BindingFlags, target As Object, args As Object()) As Object
            Dim binder As New VBBinder(Nothing)
            binder.CacheMember(member)
            Return objIReflect.InvokeMember(member.Name, ((BindingFlags.OptionalParamBinding Or (BindingFlags.FlattenHierarchy Or (BindingFlags.Public Or (BindingFlags.Static Or (BindingFlags.Instance Or BindingFlags.IgnoreCase))))) Or flags), binder, target, args, Nothing, Nothing, Nothing)
        End Function

        Private Shared Function IsMissingMemberException(ex As Exception) As Boolean
            If TypeOf ex Is MissingMemberException Then
                Return True
            End If
            If TypeOf ex Is MemberAccessException Then
                Return True
            End If
            Dim exception As COMException = TryCast(ex, COMException)
            If (Not exception Is Nothing) Then
                If (exception.ErrorCode = -2147352570) Then
                    Return True
                End If
                If (exception.ErrorCode = -2146827850) Then
                    Return True
                End If
            ElseIf ((TypeOf ex Is TargetInvocationException AndAlso TypeOf ex.InnerException Is COMException) AndAlso (DirectCast(ex.InnerException, COMException).ErrorCode = -2147352559)) Then
                Return True
            End If
            Return False
        End Function

        <DebuggerHidden, DebuggerStepThrough>
        Public Shared Sub LateCall(o As Object, objType As Type, name As String, args As Object(), paramnames As String(), CopyBack As Boolean())
            LateBinding.InternalLateCall(o, objType, name, args, paramnames, CopyBack, True)
        End Sub

        <DebuggerHidden, DebuggerStepThrough>
        Public Shared Function LateGet(o As Object, objType As Type, name As String, args As Object(), paramnames As String(), CopyBack As Boolean()) As Object
            Dim obj2 As Object
            Dim invokeAttr As BindingFlags = (BindingFlags.OptionalParamBinding Or (BindingFlags.GetProperty Or (BindingFlags.InvokeMethod Or (BindingFlags.FlattenHierarchy Or (BindingFlags.Public Or (BindingFlags.Static Or (BindingFlags.Instance Or BindingFlags.IgnoreCase)))))))
            If (objType Is Nothing) Then
                If (o Is Nothing) Then
                    Throw ExceptionUtils.VbMakeException(&H5B)
                End If
                objType = o.GetType
            End If
            Dim correctIReflect As IReflect = LateBinding.GetCorrectIReflect(o, objType)
            If (name Is Nothing) Then
                name = ""
            End If
            If objType.IsCOMObject Then
                LateBinding.CheckForClassExtendingCOMClass(objType)
            Else
                Dim member As MemberInfo = LateBinding.GetMostDerivedMemberInfo(correctIReflect, name, (invokeAttr Or BindingFlags.GetField))
                If ((Not member Is Nothing) AndAlso (member.MemberType = MemberTypes.Field)) Then
                    Dim obj3 As Object
                    VBBinder.SecurityCheckForLateboundCalls(member, objType, correctIReflect)
                    If (((objType Is correctIReflect) OrElse DirectCast(member, FieldInfo).IsStatic) OrElse LateBinding.DoesTargetObjectMatch(o, member)) Then
                        LateBinding.VerifyObjRefPresentForInstanceCall(o, member)
                        obj3 = DirectCast(member, FieldInfo).GetValue(o)
                    Else
                        obj3 = LateBinding.InvokeMemberOnIReflect(correctIReflect, member, BindingFlags.GetField, o, Nothing)
                    End If
                    If ((args Is Nothing) OrElse (args.Length = 0)) Then
                        Return obj3
                    End If
                    Return LateBinding.LateIndexGet(obj3, args, paramnames)
                End If
            End If
            Dim binder As New VBBinder(CopyBack)
            Try
                obj2 = binder.InvokeMember(name, invokeAttr, objType, correctIReflect, o, args, paramnames)
            Catch obj1 As Object When (?)
                If (objType.IsCOMObject OrElse ((Not args Is Nothing) AndAlso (args.Length > 0))) Then
                    Dim obj4 As Object
                    invokeAttr = (BindingFlags.OptionalParamBinding Or (BindingFlags.GetProperty Or (BindingFlags.InvokeMethod Or (BindingFlags.FlattenHierarchy Or (BindingFlags.Public Or (BindingFlags.Static Or (BindingFlags.Instance Or BindingFlags.IgnoreCase)))))))
                    If Not objType.IsCOMObject Then
                        invokeAttr = (invokeAttr Or BindingFlags.GetField)
                    End If
                    Try
                        obj4 = binder.InvokeMember(name, invokeAttr, objType, correctIReflect, o, Nothing, Nothing)
                    Catch exception2 As AccessViolationException
                        Throw exception2
                    Catch exception3 As StackOverflowException
                        Throw exception3
                    Catch exception4 As OutOfMemoryException
                        Throw exception4
                    Catch exception5 As ThreadAbortException
                        Throw exception5
                    Catch exception11 As Exception
                        obj4 = Nothing
                    End Try
                    If (obj4 Is Nothing) Then
                        Dim textArray1 As String() = New String() {name, Utils.VBFriendlyName(objType, o)}
                        Throw New MissingMemberException(Utils.GetResourceString("MissingMember_MemberNotFoundOnType2", textArray1))
                    End If
                    Try
                        Return LateBinding.LateIndexGet(obj4, args, paramnames)
                    Catch obj5 As Object When (?)
                        Dim exception As Exception
                        Throw exception
                    End Try
                End If
                Dim textArray2 As String() = New String() {name, Utils.VBFriendlyName(objType, o)}
                Throw New MissingMemberException(Utils.GetResourceString("MissingMember_MemberNotFoundOnType2", textArray2))
            Catch exception7 As TargetInvocationException
                Throw exception7.InnerException
            End Try
            Return obj2
        End Function

        <DebuggerHidden, DebuggerStepThrough>
        Public Shared Function LateIndexGet(o As Object, args As Object(), paramnames As String()) As Object
            Dim obj2 As Object
            Dim defaultName As String = Nothing
            If (o Is Nothing) Then
                Throw ExceptionUtils.VbMakeException(&H5B)
            End If
            Dim objType As Type = o.GetType
            Dim correctIReflect As IReflect = LateBinding.GetCorrectIReflect(o, objType)
            If objType.IsArray Then
                If ((Not paramnames Is Nothing) AndAlso (paramnames.Length <> 0)) Then
                    Throw New ArgumentException(Utils.GetResourceString("Argument_InvalidNamedArgs"))
                End If
                Dim array As Array = DirectCast(o, Array)
                Dim length As Integer = args.Length
                If (length <> array.Rank) Then
                    Throw New RankException
                End If
                If (length = 1) Then
                    Return array.GetValue(Conversions.ToInteger(args(0)))
                End If
                If (length = 2) Then
                    Return array.GetValue(Conversions.ToInteger(args(0)), Conversions.ToInteger(args(1)))
                End If
                Dim indices As Integer() = New Integer(((length - 1) + 1) - 1) {}
                Dim num3 As Integer = (length - 1)
                Dim i As Integer = 0
                Do While (i <= num3)
                    indices(i) = Conversions.ToInteger(args(i))
                    i += 1
                Loop
                Return array.GetValue(indices)
            End If
            Dim match As MethodBase() = Nothing
            Dim invokeAttr As BindingFlags = (BindingFlags.OptionalParamBinding Or (BindingFlags.GetProperty Or (BindingFlags.InvokeMethod Or (BindingFlags.FlattenHierarchy Or (BindingFlags.Public Or (BindingFlags.Static Or (BindingFlags.Instance Or BindingFlags.IgnoreCase)))))))
            If Not objType.IsCOMObject Then
                Dim num4 As Integer
                Dim num5 As Integer
                If ((args Is Nothing) OrElse (args.Length = 0)) Then
                    invokeAttr = (invokeAttr Or BindingFlags.GetField)
                End If
                Dim infoArray As MemberInfo() = LateBinding.GetDefaultMembers(objType, correctIReflect, defaultName)
                If (Not infoArray Is Nothing) Then
                    Dim upperBound As Integer = infoArray.GetUpperBound(0)
                    num4 = 0
                    Do While (num4 <= upperBound)
                        Dim getMethod As MemberInfo = infoArray(num4)
                        If (getMethod.MemberType = MemberTypes.Property) Then
                            getMethod = DirectCast(getMethod, PropertyInfo).GetGetMethod
                        End If
                        If ((Not getMethod Is Nothing) AndAlso (getMethod.MemberType <> MemberTypes.Field)) Then
                            infoArray(num5) = getMethod
                            num5 += 1
                        End If
                        num4 += 1
                    Loop
                End If
                If ((infoArray Is Nothing) Or (num5 = 0)) Then
                    Dim textArray1 As String() = New String() {Utils.VBFriendlyName(objType, o)}
                    Throw New MissingMemberException(Utils.GetResourceString("MissingMember_NoDefaultMemberFound1", textArray1))
                End If
                match = New MethodBase(((num5 - 1) + 1) - 1) {}
                Dim num7 As Integer = (num5 - 1)
                num4 = 0
                Do While (num4 <= num7)
                    Try
                        match(num4) = DirectCast(infoArray(num4), MethodBase)
                    Catch exception As StackOverflowException
                        Throw exception
                    Catch exception2 As OutOfMemoryException
                        Throw exception2
                    Catch exception3 As ThreadAbortException
                        Throw exception3
                    Catch exception4 As Exception
                    End Try
                    num4 += 1
                Loop
            Else
                LateBinding.CheckForClassExtendingCOMClass(objType)
            End If
            Dim binder As New VBBinder(Nothing)
            Try
                Dim obj4 As Object
                If objType.IsCOMObject Then
                    Return binder.InvokeMember("", invokeAttr, objType, correctIReflect, o, args, paramnames)
                End If
                Dim objState As Object = Nothing
                binder.m_BindToName = defaultName
                binder.m_objType = objType
                Dim member As MethodBase = binder.BindToMethod(invokeAttr, match, args, Nothing, Nothing, paramnames, objState)
                VBBinder.SecurityCheckForLateboundCalls(member, objType, correctIReflect)
                If (((objType Is correctIReflect) OrElse member.IsStatic) OrElse LateBinding.DoesTargetObjectMatch(o, member)) Then
                    LateBinding.VerifyObjRefPresentForInstanceCall(o, member)
                    obj4 = member.Invoke(o, args)
                Else
                    obj4 = LateBinding.InvokeMemberOnIReflect(correctIReflect, member, BindingFlags.InvokeMethod, o, args)
                End If
                binder.ReorderArgumentArray(args, objState)
                obj2 = obj4
            Catch obj1 As Object When (?)
                Dim textArray2 As String() = New String() {Utils.VBFriendlyName(objType, o)}
                Throw New MissingMemberException(Utils.GetResourceString("MissingMember_NoDefaultMemberFound1", textArray2))
            Catch exception6 As TargetInvocationException
                Throw exception6.InnerException
            End Try
            Return obj2
        End Function

        <DebuggerHidden, DebuggerStepThrough>
        Public Shared Sub LateIndexSet(o As Object, args As Object(), paramnames As String())
            Dim defaultName As String = Nothing
            If (o Is Nothing) Then
                Throw ExceptionUtils.VbMakeException(&H5B)
            End If
            Dim objType As Type = o.GetType
            Dim correctIReflect As IReflect = LateBinding.GetCorrectIReflect(o, objType)
            If objType.IsArray Then
                If ((Not paramnames Is Nothing) AndAlso (paramnames.Length <> 0)) Then
                    Throw New ArgumentException(Utils.GetResourceString("Argument_InvalidNamedArgs"))
                End If
                Dim array As Array = DirectCast(o, Array)
                Dim index As Integer = (args.Length - 1)
                Dim obj2 As Object = args(index)
                If (Not obj2 Is Nothing) Then
                    Dim elementType As Type = objType.GetElementType
                    If (Not obj2.GetType Is elementType) Then
                        obj2 = ObjectType.CTypeHelper(obj2, elementType)
                    End If
                End If
                If (index <> array.Rank) Then
                    Throw New RankException
                End If
                If (index = 1) Then
                    array.SetValue(obj2, Conversions.ToInteger(args(0)))
                ElseIf (index = 2) Then
                    array.SetValue(obj2, Conversions.ToInteger(args(0)), Conversions.ToInteger(args(1)))
                Else
                    Dim indices As Integer() = New Integer(((index - 1) + 1) - 1) {}
                    Dim num3 As Integer = (index - 1)
                    Dim i As Integer = 0
                    Do While (i <= num3)
                        indices(i) = Conversions.ToInteger(args(i))
                        i += 1
                    Loop
                    array.SetValue(obj2, indices)
                End If
            Else
                Dim match As MethodBase() = Nothing
                Dim invokeAttr As BindingFlags = (BindingFlags.OptionalParamBinding Or (BindingFlags.FlattenHierarchy Or (BindingFlags.Public Or (BindingFlags.Static Or (BindingFlags.Instance Or BindingFlags.IgnoreCase)))))
                If objType.IsCOMObject Then
                    LateBinding.CheckForClassExtendingCOMClass(objType)
                    invokeAttr = (invokeAttr Or LateBinding.GetPropertyPutFlags(args(args.GetUpperBound(0))))
                Else
                    Dim num4 As Integer
                    Dim num5 As Integer
                    invokeAttr = (invokeAttr Or BindingFlags.SetProperty)
                    If (args.Length = 1) Then
                        invokeAttr = (invokeAttr Or BindingFlags.SetField)
                    End If
                    Dim infoArray As MemberInfo() = LateBinding.GetDefaultMembers(objType, correctIReflect, defaultName)
                    If (Not infoArray Is Nothing) Then
                        Dim upperBound As Integer = infoArray.GetUpperBound(0)
                        num4 = 0
                        Do While (num4 <= upperBound)
                            Dim setMethod As MemberInfo = infoArray(num4)
                            If (setMethod.MemberType = MemberTypes.Property) Then
                                setMethod = DirectCast(setMethod, PropertyInfo).GetSetMethod
                            End If
                            If ((Not setMethod Is Nothing) AndAlso (setMethod.MemberType <> MemberTypes.Field)) Then
                                infoArray(num5) = setMethod
                                num5 += 1
                            End If
                            num4 += 1
                        Loop
                    End If
                    If ((infoArray Is Nothing) Or (num5 = 0)) Then
                        Dim textArray1 As String() = New String() {Utils.VBFriendlyName(objType, o)}
                        Throw New MissingMemberException(Utils.GetResourceString("MissingMember_NoDefaultMemberFound1", textArray1))
                    End If
                    match = New MethodBase(((num5 - 1) + 1) - 1) {}
                    Dim num7 As Integer = (num5 - 1)
                    num4 = 0
                    Do While (num4 <= num7)
                        Try
                            match(num4) = DirectCast(infoArray(num4), MethodBase)
                        Catch exception As StackOverflowException
                            Throw exception
                        Catch exception2 As OutOfMemoryException
                            Throw exception2
                        Catch exception3 As ThreadAbortException
                            Throw exception3
                        Catch exception8 As Exception
                        End Try
                        num4 += 1
                    Loop
                End If
                Dim binder As New VBBinder(Nothing)
                Try
                    If objType.IsCOMObject Then
                        binder.InvokeMember("", invokeAttr, objType, correctIReflect, o, args, paramnames)
                    Else
                        Dim objState As Object = Nothing
                        binder.m_BindToName = defaultName
                        binder.m_objType = objType
                        Dim member As MethodBase = binder.BindToMethod(invokeAttr, match, args, Nothing, Nothing, paramnames, objState)
                        VBBinder.SecurityCheckForLateboundCalls(member, objType, correctIReflect)
                        If (((objType Is correctIReflect) OrElse member.IsStatic) OrElse LateBinding.DoesTargetObjectMatch(o, member)) Then
                            LateBinding.VerifyObjRefPresentForInstanceCall(o, member)
                            member.Invoke(o, args)
                        Else
                            LateBinding.InvokeMemberOnIReflect(correctIReflect, member, BindingFlags.InvokeMethod, o, args)
                        End If
                        binder.ReorderArgumentArray(args, objState)
                    End If
                Catch obj1 As Object When (?)
                    Dim textArray2 As String() = New String() {Utils.VBFriendlyName(objType, o)}
                    Throw New MissingMemberException(Utils.GetResourceString("MissingMember_NoDefaultMemberFound1", textArray2))
                Catch exception5 As TargetInvocationException
                    Throw exception5.InnerException
                End Try
            End If
        End Sub

        <DebuggerHidden, DebuggerStepThrough>
        Public Shared Sub LateIndexSetComplex(o As Object, args As Object(), paramnames As String(), OptimisticSet As Boolean, RValueBase As Boolean)
            Try
                LateBinding.LateIndexSet(o, args, paramnames)
                If (RValueBase AndAlso o.GetType.IsValueType) Then
                    Dim textArray1 As String() = New String() {o.GetType.Name, o.GetType.Name}
                    Throw New Exception(Utils.GetResourceString("RValueBaseForValueType", textArray1))
                End If
            Catch obj1 As Object When (?)
            End Try
        End Sub

        <DebuggerHidden, DebuggerStepThrough>
        Public Shared Sub LateSet(o As Object, objType As Type, name As String, args As Object(), paramnames As String())
            LateBinding.InternalLateSet(o, objType, name, args, paramnames, False, DirectCast(0, CallType))
        End Sub

        <DebuggerHidden, DebuggerStepThrough>
        Public Shared Sub LateSetComplex(o As Object, objType As Type, name As String, args As Object(), paramnames As String(), OptimisticSet As Boolean, RValueBase As Boolean)
            Try
                LateBinding.InternalLateSet(o, objType, name, args, paramnames, OptimisticSet, DirectCast(0, CallType))
                If (RValueBase AndAlso objType.IsValueType) Then
                    Dim textArray1 As String() = New String() {Utils.VBFriendlyName(objType, o), Utils.VBFriendlyName(objType, o)}
                    Throw New Exception(Utils.GetResourceString("RValueBaseForValueType", textArray1))
                End If
            Catch obj1 As Object When (?)
            End Try
        End Sub

        Friend Shared Function LegacyIsGeneric(Member As MemberInfo) As Boolean
            Dim base2 As MethodBase = TryCast(Member, MethodBase)
            If (base2 Is Nothing) Then
                Return False
            End If
            Return base2.IsGenericMethod
        End Function

        Private Shared Function MemberIsField(mi As MemberInfo()) As Boolean
            Dim upperBound As Integer = mi.GetUpperBound(0)
            Dim i As Integer = 0
            Do While (i <= upperBound)
                info = mi(i)
                If ((Not info Is Nothing) AndAlso (info.MemberType = MemberTypes.Field)) Then
                    Dim num4 As Integer = mi.GetUpperBound(0)
                    Dim j As Integer = 0
                    Do While (j <= num4)
                        If (((i <> j) AndAlso (Not mi(j) Is Nothing)) AndAlso info.DeclaringType.IsSubclassOf(mi(j).DeclaringType)) Then
                            mi(j) = Nothing
                        End If
                        j += 1
                    Loop
                End If
                i += 1
            Loop
            Dim info As MemberInfo
            For Each info In mi
                If ((Not info Is Nothing) AndAlso (info.MemberType <> MemberTypes.Field)) Then
                    Return False
                End If
            Next
            Return True
        End Function

        Private Shared Function NoByrefs(parameters As ParameterInfo()) As Boolean
            Dim num2 As Integer = (parameters.Length - 1)
            Dim i As Integer = 0
            Do While (i <= num2)
                If parameters(i).ParameterType.IsByRef Then
                    Return False
                End If
                i += 1
            Loop
            Return True
        End Function

        Friend Shared Sub VerifyObjRefPresentForInstanceCall(Value As Object, Member As MemberInfo)
            If (Value Is Nothing) Then
                Dim isStatic As Boolean = True
                Select Case Member.MemberType
                    Case MemberTypes.Constructor
                        isStatic = DirectCast(Member, ConstructorInfo).IsStatic
                        Exit Select
                    Case MemberTypes.Field
                        isStatic = DirectCast(Member, FieldInfo).IsStatic
                        Exit Select
                    Case MemberTypes.Method
                        isStatic = DirectCast(Member, MethodInfo).IsStatic
                        Exit Select
                End Select
                If Not isStatic Then
                    Dim args As String() = New String() {Utils.MemberToString(Member)}
                    Throw New NullReferenceException(Utils.GetResourceString("NullReference_InstanceReqToAccessMember1", args))
                End If
            End If
        End Sub


        ' Fields
        Private Const DefaultCallType As CallType = DirectCast(0, CallType)
        Private Const VBLateBindingFlags As BindingFlags = (BindingFlags.OptionalParamBinding Or (BindingFlags.FlattenHierarchy Or (BindingFlags.Public Or (BindingFlags.Static Or (BindingFlags.Instance Or BindingFlags.IgnoreCase)))))
    End Class
End Namespace

