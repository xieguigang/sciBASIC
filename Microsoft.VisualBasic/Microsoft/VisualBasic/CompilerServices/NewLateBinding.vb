Imports System.Collections.Generic
Imports System.ComponentModel
Imports System.Diagnostics
Imports System.Dynamic
Imports System.Reflection
Imports Microsoft.VisualBasic.CompilerServices.OverloadResolution
Imports Microsoft.VisualBasic.CompilerServices.Symbols

Namespace Microsoft.VisualBasic.CompilerServices

    <EditorBrowsable(EditorBrowsableState.Never), DynamicallyInvokableAttribute>
    Public NotInheritable Class NewLateBinding

        ' Methods
        Private Sub New()
        End Sub

        Private Shared Function CallMethod(BaseReference As Container, MethodName As String, Arguments As Object(), ArgumentNames As String(), TypeArguments As Type(), CopyBack As Boolean(), InvocationFlags As BindingFlags, ReportErrors As Boolean, ByRef Failure As ResolutionFailure) As Object
            Failure = ResolutionFailure.None
            If ((ArgumentNames.Length > Arguments.Length) OrElse ((Not CopyBack Is Nothing) AndAlso (CopyBack.Length <> Arguments.Length))) Then
                Failure = ResolutionFailure.InvalidArgument
                If ReportErrors Then
                    Throw New ArgumentException(Utils.GetResourceString("Argument_InvalidValue"))
                End If
                Return Nothing
            End If
            If (Symbols.HasFlag(InvocationFlags, BindingFlags.SetProperty) AndAlso (Arguments.Length < 1)) Then
                Failure = ResolutionFailure.InvalidArgument
                If ReportErrors Then
                    Throw New ArgumentException(Utils.GetResourceString("Argument_InvalidValue"))
                End If
                Return Nothing
            End If
            Dim members As MemberInfo() = BaseReference.GetMembers(MethodName, ReportErrors)
            If ((members Is Nothing) OrElse (members.Length = 0)) Then
                Failure = ResolutionFailure.MissingMember
                If ReportErrors Then
                    members = BaseReference.GetMembers(MethodName, True)
                End If
                Return Nothing
            End If
            Dim targetProcedure As Method = NewLateBinding.ResolveCall(BaseReference, MethodName, members, Arguments, ArgumentNames, TypeArguments, InvocationFlags, ReportErrors, Failure)
            If (Failure Is ResolutionFailure.None) Then
                Return BaseReference.InvokeMethod(targetProcedure, Arguments, CopyBack, InvocationFlags)
            End If
            Return Nothing
        End Function

        Friend Shared Function CanBindCall(Instance As Object, MemberName As String, Arguments As Object(), ArgumentNames As String(), IgnoreReturn As Boolean) As Boolean
            Dim failure As ResolutionFailure
            Dim baseReference As New Container(Instance)
            Dim lookupFlags As BindingFlags = (BindingFlags.GetProperty Or BindingFlags.InvokeMethod)
            If IgnoreReturn Then
                lookupFlags = (lookupFlags Or BindingFlags.IgnoreReturn)
            End If
            Dim members As MemberInfo() = baseReference.GetMembers(MemberName, False)
            If ((members Is Nothing) OrElse (members.Length = 0)) Then
                Return False
            End If
            NewLateBinding.ResolveCall(baseReference, MemberName, members, Arguments, ArgumentNames, Symbols.NoTypeArguments, lookupFlags, False, failure)
            Return (failure = ResolutionFailure.None)
        End Function

        Friend Shared Function CanBindGet(Instance As Object, MemberName As String, Arguments As Object(), ArgumentNames As String()) As Boolean
            Dim baseReference As New Container(Instance)
            Dim lookupFlags As BindingFlags = (BindingFlags.GetProperty Or BindingFlags.InvokeMethod)
            Dim members As MemberInfo() = baseReference.GetMembers(MemberName, False)
            If ((Not members Is Nothing) AndAlso (members.Length <> 0)) Then
                Dim failure As ResolutionFailure
                If (members(0).MemberType = MemberTypes.Field) Then
                    Return True
                End If
                NewLateBinding.ResolveCall(baseReference, MemberName, members, Arguments, ArgumentNames, Symbols.NoTypeArguments, lookupFlags, False, failure)
                If (failure = ResolutionFailure.None) Then
                    Return True
                End If
                If (((Arguments.Length > 0) AndAlso (members.Length = 1)) AndAlso NewLateBinding.IsZeroArgumentCall(members(0))) Then
                    NewLateBinding.ResolveCall(baseReference, MemberName, members, Symbols.NoArguments, Symbols.NoArgumentNames, Symbols.NoTypeArguments, lookupFlags, False, failure)
                    If (failure = ResolutionFailure.None) Then
                        Return True
                    End If
                End If
            End If
            Return False
        End Function

        Friend Shared Function CanBindInvokeDefault(Instance As Object, Arguments As Object(), ArgumentNames As String(), ReportErrors As Boolean) As Boolean
            Dim container As New Container(Instance)
            ReportErrors = ((ReportErrors OrElse (Arguments.Length <> 0)) OrElse container.IsArray)
            If Not ReportErrors Then
                Return True
            End If
            If container.IsArray Then
                Return (ArgumentNames.Length = 0)
            End If
            Return NewLateBinding.CanBindCall(Instance, "", Arguments, ArgumentNames, False)
        End Function

        Friend Shared Function CanBindSet(Instance As Object, MemberName As String, Value As Object, OptimisticSet As Boolean, RValueBase As Boolean) As Boolean
            Dim failure As ResolutionFailure
            Dim baseReference As New Container(Instance)
            Dim arguments As Object() = New Object() {Value}
            Dim members As MemberInfo() = baseReference.GetMembers(MemberName, False)
            If ((members Is Nothing) OrElse (members.Length = 0)) Then
                Return False
            End If
            If (members(0).MemberType = MemberTypes.Field) Then
                If (((arguments.Length = 1) AndAlso RValueBase) AndAlso baseReference.IsValueType) Then
                    Return False
                End If
                If DirectCast(members(0), FieldInfo).IsInitOnly Then
                    Return False
                End If
                Return True
            End If
            NewLateBinding.ResolveCall(baseReference, MemberName, members, arguments, Symbols.NoArgumentNames, Symbols.NoTypeArguments, BindingFlags.SetProperty, False, failure)
            If (failure = ResolutionFailure.None) Then
                If (RValueBase AndAlso baseReference.IsValueType) Then
                    Return False
                End If
                Return True
            End If
            Dim lookupFlags As BindingFlags = (BindingFlags.GetProperty Or BindingFlags.InvokeMethod)
            If (failure = ResolutionFailure.MissingMember) Then
                NewLateBinding.ResolveCall(baseReference, MemberName, members, Symbols.NoArguments, Symbols.NoArgumentNames, Symbols.NoTypeArguments, lookupFlags, False, failure)
                If (failure = ResolutionFailure.None) Then
                    Return True
                End If
            End If
            Return OptimisticSet
        End Function

        Friend Shared Function CanIndexSetComplex(Instance As Object, Arguments As Object(), ArgumentNames As String(), OptimisticSet As Boolean, RValueBase As Boolean) As Boolean
            Dim failure As ResolutionFailure
            Dim baseReference As New Container(Instance)
            If baseReference.IsArray Then
                Return (ArgumentNames.Length = 0)
            End If
            Dim memberName As String = ""
            Dim setProperty As BindingFlags = BindingFlags.SetProperty
            Dim members As MemberInfo() = baseReference.GetMembers(memberName, False)
            If ((members Is Nothing) OrElse (members.Length = 0)) Then
                Return False
            End If
            NewLateBinding.ResolveCall(baseReference, memberName, members, Arguments, ArgumentNames, Symbols.NoTypeArguments, setProperty, False, failure)
            If (failure = ResolutionFailure.None) Then
                If (RValueBase AndAlso baseReference.IsValueType) Then
                    Return False
                End If
                Return True
            End If
            Return OptimisticSet
        End Function

        Friend Shared Function ConstructCallArguments(TargetProcedure As Method, Arguments As Object(), LookupFlags As BindingFlags) As Object()
            Dim parameters As ParameterInfo() = NewLateBinding.GetCallTarget(TargetProcedure, LookupFlags).GetParameters
            Dim matchedArguments As Object() = New Object(((parameters.Length - 1) + 1) - 1) {}
            Dim length As Integer = Arguments.Length
            Dim argument As Object = Nothing
            If Symbols.HasFlag(LookupFlags, BindingFlags.SetProperty) Then
                Dim sourceArray As Object() = Arguments
                Arguments = New Object(((length - 2) + 1) - 1) {}
                Array.Copy(sourceArray, Arguments, Arguments.Length)
                argument = sourceArray((length - 1))
            End If
            OverloadResolution.MatchArguments(TargetProcedure, Arguments, matchedArguments)
            If Symbols.HasFlag(LookupFlags, BindingFlags.SetProperty) Then
                Dim parameter As ParameterInfo = parameters((parameters.Length - 1))
                matchedArguments((parameters.Length - 1)) = OverloadResolution.PassToParameter(argument, parameter, parameter.ParameterType)
            End If
            Return matchedArguments
        End Function

        <Obsolete("do not use this method", True), EditorBrowsable(EditorBrowsableState.Never), DebuggerHidden, DebuggerStepThrough, DynamicallyInvokableAttribute>
        Public Shared Function FallbackCall(Instance As Object, MemberName As String, Arguments As Object(), ArgumentNames As String(), IgnoreReturn As Boolean) As Object
            Return NewLateBinding.ObjectLateCall(Instance, Nothing, MemberName, Arguments, ArgumentNames, Symbols.NoTypeArguments, IDOBinder.GetCopyBack, IgnoreReturn)
        End Function

        <Obsolete("do not use this method", True), EditorBrowsable(EditorBrowsableState.Never), DebuggerHidden, DebuggerStepThrough, DynamicallyInvokableAttribute>
        Public Shared Function FallbackGet(Instance As Object, MemberName As String, Arguments As Object(), ArgumentNames As String()) As Object
            Return NewLateBinding.ObjectLateGet(Instance, Nothing, MemberName, Arguments, ArgumentNames, Symbols.NoTypeArguments, IDOBinder.GetCopyBack)
        End Function

        <Obsolete("do not use this method", True), EditorBrowsable(EditorBrowsableState.Never), DebuggerHidden, DebuggerStepThrough, DynamicallyInvokableAttribute>
        Public Shared Sub FallbackIndexSet(Instance As Object, Arguments As Object(), ArgumentNames As String())
            NewLateBinding.ObjectLateIndexSet(Instance, Arguments, ArgumentNames)
        End Sub

        <Obsolete("do not use this method", True), EditorBrowsable(EditorBrowsableState.Never), DebuggerHidden, DebuggerStepThrough, DynamicallyInvokableAttribute>
        Public Shared Sub FallbackIndexSetComplex(Instance As Object, Arguments As Object(), ArgumentNames As String(), OptimisticSet As Boolean, RValueBase As Boolean)
            NewLateBinding.ObjectLateIndexSetComplex(Instance, Arguments, ArgumentNames, OptimisticSet, RValueBase)
        End Sub

        <Obsolete("do not use this method", True), EditorBrowsable(EditorBrowsableState.Never), DebuggerHidden, DebuggerStepThrough, DynamicallyInvokableAttribute>
        Public Shared Function FallbackInvokeDefault1(Instance As Object, Arguments As Object(), ArgumentNames As String(), ReportErrors As Boolean) As Object
            Return IDOBinder.IDOFallbackInvokeDefault(DirectCast(Instance, IDynamicMetaObjectProvider), Arguments, ArgumentNames, ReportErrors, IDOBinder.GetCopyBack)
        End Function

        <Obsolete("do not use this method", True), EditorBrowsable(EditorBrowsableState.Never), DebuggerHidden, DebuggerStepThrough, DynamicallyInvokableAttribute>
        Public Shared Function FallbackInvokeDefault2(Instance As Object, Arguments As Object(), ArgumentNames As String(), ReportErrors As Boolean) As Object
            Return NewLateBinding.ObjectLateInvokeDefault(Instance, Arguments, ArgumentNames, ReportErrors, IDOBinder.GetCopyBack)
        End Function

        <Obsolete("do not use this method", True), EditorBrowsable(EditorBrowsableState.Never), DebuggerHidden, DebuggerStepThrough, DynamicallyInvokableAttribute>
        Public Shared Sub FallbackSet(Instance As Object, MemberName As String, Arguments As Object())
            NewLateBinding.ObjectLateSet(Instance, Nothing, MemberName, Arguments, Symbols.NoArgumentNames, Symbols.NoTypeArguments)
        End Sub

        <Obsolete("do not use this method", True), EditorBrowsable(EditorBrowsableState.Never), DebuggerHidden, DebuggerStepThrough, DynamicallyInvokableAttribute>
        Public Shared Sub FallbackSetComplex(Instance As Object, MemberName As String, Arguments As Object(), OptimisticSet As Boolean, RValueBase As Boolean)
            NewLateBinding.ObjectLateSetComplex(Instance, Nothing, MemberName, Arguments, New String(0 - 1) {}, Symbols.NoTypeArguments, OptimisticSet, RValueBase)
        End Sub

        Friend Shared Function GetCallTarget(TargetProcedure As Method, Flags As BindingFlags) As MethodBase
            If TargetProcedure.IsMethod Then
                Return TargetProcedure.AsMethod
            End If
            If TargetProcedure.IsProperty Then
                Return NewLateBinding.MatchesPropertyRequirements(TargetProcedure, Flags)
            End If
            Return Nothing
        End Function

        Private Shared Function InternalLateIndexGet(Instance As Object, Arguments As Object(), ArgumentNames As String(), ReportErrors As Boolean, ByRef Failure As ResolutionFailure, CopyBack As Boolean()) As Object
            Failure = ResolutionFailure.None
            If (Arguments Is Nothing) Then
                Arguments = Symbols.NoArguments
            End If
            If (ArgumentNames Is Nothing) Then
                ArgumentNames = Symbols.NoArgumentNames
            End If
            Dim baseReference As New Container(Instance)
            If (baseReference.IsCOMObject AndAlso Not baseReference.IsWindowsRuntimeObject) Then
                Return LateBinding.LateIndexGet(Instance, Arguments, ArgumentNames)
            End If
            If baseReference.IsArray Then
                If (ArgumentNames.Length > 0) Then
                    Failure = ResolutionFailure.InvalidArgument
                    If ReportErrors Then
                        Throw New ArgumentException(Utils.GetResourceString("Argument_InvalidNamedArgs"))
                    End If
                    Return Nothing
                End If
                NewLateBinding.ResetCopyback(CopyBack)
                Return baseReference.GetArrayValue(Arguments)
            End If
            Return NewLateBinding.CallMethod(baseReference, "", Arguments, ArgumentNames, Symbols.NoTypeArguments, CopyBack, (BindingFlags.GetProperty Or BindingFlags.InvokeMethod), ReportErrors, Failure)
        End Function

        Private Shared Function InternalLateInvokeDefault(Instance As Object, Arguments As Object(), ArgumentNames As String(), ReportErrors As Boolean, CopyBack As Boolean()) As Object
            Dim instance As IDynamicMetaObjectProvider = IDOUtils.TryCastToIDMOP(instance)
            If (Not instance Is Nothing) Then
                Return IDOBinder.IDOInvokeDefault(instance, Arguments, ArgumentNames, ReportErrors, CopyBack)
            End If
            Return NewLateBinding.ObjectLateInvokeDefault(instance, Arguments, ArgumentNames, ReportErrors, CopyBack)
        End Function

        Friend Shared Function IsZeroArgumentCall(Member As MemberInfo) As Boolean
            Return (((Member.MemberType = MemberTypes.Method) AndAlso (DirectCast(Member, MethodInfo).GetParameters.Length = 0)) OrElse ((Member.MemberType = MemberTypes.Property) AndAlso (DirectCast(Member, PropertyInfo).GetIndexParameters.Length = 0)))
        End Function

        <DebuggerHidden, DebuggerStepThrough, DynamicallyInvokableAttribute>
        Public Shared Function LateCall(Instance As Object, Type As Type, MemberName As String, Arguments As Object(), ArgumentNames As String(), TypeArguments As Type(), CopyBack As Boolean(), IgnoreReturn As Boolean) As Object
            Dim container As Container
            If (Arguments Is Nothing) Then
                Arguments = Symbols.NoArguments
            End If
            If (ArgumentNames Is Nothing) Then
                ArgumentNames = Symbols.NoArgumentNames
            End If
            If (TypeArguments Is Nothing) Then
                TypeArguments = Symbols.NoTypeArguments
            End If
            If (Not Type Is Nothing) Then
                container = New Container(Type)
            Else
                container = New Container(instance)
            End If
            If (container.IsCOMObject AndAlso Not container.IsWindowsRuntimeObject) Then
                Return LateBinding.InternalLateCall(instance, Type, MemberName, Arguments, ArgumentNames, CopyBack, IgnoreReturn)
            End If
            Dim instance As IDynamicMetaObjectProvider = IDOUtils.TryCastToIDMOP(instance)
            If ((Not instance Is Nothing) AndAlso (TypeArguments Is Symbols.NoTypeArguments)) Then
                Return IDOBinder.IDOCall(instance, MemberName, Arguments, ArgumentNames, CopyBack, IgnoreReturn)
            End If
            Return NewLateBinding.ObjectLateCall(instance, Type, MemberName, Arguments, ArgumentNames, TypeArguments, CopyBack, IgnoreReturn)
        End Function

        <EditorBrowsable(EditorBrowsableState.Never), DebuggerHidden, DebuggerStepThrough>
        Public Shared Function LateCallInvokeDefault(Instance As Object, Arguments As Object(), ArgumentNames As String(), ReportErrors As Boolean) As Object
            Return NewLateBinding.InternalLateInvokeDefault(Instance, Arguments, ArgumentNames, ReportErrors, IDOBinder.GetCopyBack)
        End Function

        <DebuggerHidden, DebuggerStepThrough>
        Public Shared Function LateCanEvaluate(instance As Object, type As Type, memberName As String, arguments As Object(), allowFunctionEvaluation As Boolean, allowPropertyEvaluation As Boolean) As Boolean
            Dim container As Container
            If (Not type Is Nothing) Then
                container = New Container(type)
            Else
                container = New Container(instance)
            End If
            Dim members As MemberInfo() = container.GetMembers(memberName, False)
            If (members.Length <> 0) Then
                If (members(0).MemberType = MemberTypes.Field) Then
                    If (arguments.Length = 0) Then
                        Return True
                    End If
                    container = New Container(container.GetFieldValue(DirectCast(members(0), FieldInfo)))
                    Return (container.IsArray OrElse allowPropertyEvaluation)
                End If
                If (members(0).MemberType = MemberTypes.Method) Then
                    Return allowFunctionEvaluation
                End If
                If (members(0).MemberType = MemberTypes.Property) Then
                    Return allowPropertyEvaluation
                End If
            End If
            Return True
        End Function

        <DebuggerHidden, DebuggerStepThrough, DynamicallyInvokableAttribute>
        Public Shared Function LateGet(Instance As Object, Type As Type, MemberName As String, Arguments As Object(), ArgumentNames As String(), TypeArguments As Type(), CopyBack As Boolean()) As Object
            Dim container As Container
            If (Arguments Is Nothing) Then
                Arguments = Symbols.NoArguments
            End If
            If (ArgumentNames Is Nothing) Then
                ArgumentNames = Symbols.NoArgumentNames
            End If
            If (TypeArguments Is Nothing) Then
                TypeArguments = Symbols.NoTypeArguments
            End If
            If (Not Type Is Nothing) Then
                container = New Container(Type)
            Else
                container = New Container(instance)
            End If
            If (container.IsCOMObject AndAlso Not container.IsWindowsRuntimeObject) Then
                Return LateBinding.LateGet(instance, Type, MemberName, Arguments, ArgumentNames, CopyBack)
            End If
            Dim instance As IDynamicMetaObjectProvider = IDOUtils.TryCastToIDMOP(instance)
            If ((Not instance Is Nothing) AndAlso (TypeArguments Is Symbols.NoTypeArguments)) Then
                Return IDOBinder.IDOGet(instance, MemberName, Arguments, ArgumentNames, CopyBack)
            End If
            Return NewLateBinding.ObjectLateGet(instance, Type, MemberName, Arguments, ArgumentNames, TypeArguments, CopyBack)
        End Function

        <EditorBrowsable(EditorBrowsableState.Never), DebuggerHidden, DebuggerStepThrough>
        Public Shared Function LateGetInvokeDefault(Instance As Object, Arguments As Object(), ArgumentNames As String(), ReportErrors As Boolean) As Object
            If ((Not IDOUtils.TryCastToIDMOP(Instance) Is Nothing) OrElse ((Not Arguments Is Nothing) AndAlso (Arguments.Length > 0))) Then
                Return NewLateBinding.InternalLateInvokeDefault(Instance, Arguments, ArgumentNames, ReportErrors, IDOBinder.GetCopyBack)
            End If
            Return Instance
        End Function

        <DebuggerHidden, DebuggerStepThrough, DynamicallyInvokableAttribute>
        Public Shared Function LateIndexGet(Instance As Object, Arguments As Object(), ArgumentNames As String()) As Object
            Return NewLateBinding.InternalLateInvokeDefault(Instance, Arguments, ArgumentNames, True, Nothing)
        End Function

        Private Shared Function LateIndexGet(Instance As Object, Arguments As Object(), ArgumentNames As String(), CopyBack As Boolean()) As Object
            Return NewLateBinding.InternalLateInvokeDefault(Instance, Arguments, ArgumentNames, True, CopyBack)
        End Function

        <DebuggerHidden, DebuggerStepThrough, DynamicallyInvokableAttribute>
        Public Shared Sub LateIndexSet(Instance As Object, Arguments As Object(), ArgumentNames As String())
            Dim instance As IDynamicMetaObjectProvider = IDOUtils.TryCastToIDMOP(instance)
            If (Not instance Is Nothing) Then
                IDOBinder.IDOIndexSet(instance, Arguments, ArgumentNames)
            Else
                NewLateBinding.ObjectLateIndexSet(instance, Arguments, ArgumentNames)
            End If
        End Sub

        <DebuggerHidden, DebuggerStepThrough, DynamicallyInvokableAttribute>
        Public Shared Sub LateIndexSetComplex(Instance As Object, Arguments As Object(), ArgumentNames As String(), OptimisticSet As Boolean, RValueBase As Boolean)
            Dim instance As IDynamicMetaObjectProvider = IDOUtils.TryCastToIDMOP(instance)
            If (Not instance Is Nothing) Then
                IDOBinder.IDOIndexSetComplex(instance, Arguments, ArgumentNames, OptimisticSet, RValueBase)
            Else
                NewLateBinding.ObjectLateIndexSetComplex(instance, Arguments, ArgumentNames, OptimisticSet, RValueBase)
            End If
        End Sub

        <DebuggerHidden, DebuggerStepThrough, DynamicallyInvokableAttribute>
        Public Shared Sub LateSet(Instance As Object, Type As Type, MemberName As String, Arguments As Object(), ArgumentNames As String(), TypeArguments As Type())
            Dim instance As IDynamicMetaObjectProvider = IDOUtils.TryCastToIDMOP(instance)
            If ((Not instance Is Nothing) AndAlso (TypeArguments Is Nothing)) Then
                IDOBinder.IDOSet(instance, MemberName, ArgumentNames, Arguments)
            Else
                NewLateBinding.ObjectLateSet(instance, Type, MemberName, Arguments, ArgumentNames, TypeArguments)
            End If
        End Sub

        <DebuggerHidden, DebuggerStepThrough, DynamicallyInvokableAttribute>
        Public Shared Sub LateSet(Instance As Object, Type As Type, MemberName As String, Arguments As Object(), ArgumentNames As String(), TypeArguments As Type(), OptimisticSet As Boolean, RValueBase As Boolean, CallType As CallType)
            Dim container As Container
            If (Arguments Is Nothing) Then
                Arguments = Symbols.NoArguments
            End If
            If (ArgumentNames Is Nothing) Then
                ArgumentNames = Symbols.NoArgumentNames
            End If
            If (TypeArguments Is Nothing) Then
                TypeArguments = Symbols.NoTypeArguments
            End If
            If (Not Type Is Nothing) Then
                container = New Container(Type)
            Else
                container = New Container(Instance)
            End If
            If (container.IsCOMObject AndAlso Not container.IsWindowsRuntimeObject) Then
                Try
                    LateBinding.InternalLateSet(Instance, Type, MemberName, Arguments, ArgumentNames, OptimisticSet, CallType)
                    If (RValueBase AndAlso Type.IsValueType) Then
                        Dim args As String() = New String() {Utils.VBFriendlyName(Type, Instance), Utils.VBFriendlyName(Type, Instance)}
                        Throw New Exception(Utils.GetResourceString("RValueBaseForValueType", args))
                    End If
                Catch obj1 As Object When (?)
                End Try
            Else
                Dim members As MemberInfo() = container.GetMembers(MemberName, Not OptimisticSet)
                If Not ((members.Length = 0) And OptimisticSet) Then
                    If (members(0).MemberType = MemberTypes.Field) Then
                        If (TypeArguments.Length > 0) Then
                            Throw New ArgumentException(Utils.GetResourceString("Argument_InvalidValue"))
                        End If
                        If (Arguments.Length = 1) Then
                            If (RValueBase AndAlso container.IsValueType) Then
                                Dim textArray2 As String() = New String() {container.VBFriendlyName, container.VBFriendlyName}
                                Throw New Exception(Utils.GetResourceString("RValueBaseForValueType", textArray2))
                            End If
                            container.SetFieldValue(DirectCast(members(0), FieldInfo), Arguments(0))
                        Else
                            NewLateBinding.LateIndexSetComplex(container.GetFieldValue(DirectCast(members(0), FieldInfo)), Arguments, ArgumentNames, OptimisticSet, True)
                        End If
                    Else
                        Dim failure As ResolutionFailure
                        Dim method As Method
                        Dim setProperty As BindingFlags = BindingFlags.SetProperty
                        If (ArgumentNames.Length > Arguments.Length) Then
                            Throw New ArgumentException(Utils.GetResourceString("Argument_InvalidValue"))
                        End If
                        If (TypeArguments.Length = 0) Then
                            method = NewLateBinding.ResolveCall(container, MemberName, members, Arguments, ArgumentNames, Symbols.NoTypeArguments, setProperty, False, failure)
                            If (failure = ResolutionFailure.None) Then
                                If (RValueBase AndAlso container.IsValueType) Then
                                    Dim textArray3 As String() = New String() {container.VBFriendlyName, container.VBFriendlyName}
                                    Throw New Exception(Utils.GetResourceString("RValueBaseForValueType", textArray3))
                                End If
                                container.InvokeMethod(method, Arguments, Nothing, setProperty)
                                Return
                            End If
                        End If
                        Dim lookupFlags As BindingFlags = (BindingFlags.GetProperty Or BindingFlags.InvokeMethod)
                        Select Case failure
                            Case ResolutionFailure.None, ResolutionFailure.MissingMember
                                method = NewLateBinding.ResolveCall(container, MemberName, members, Symbols.NoArguments, Symbols.NoArgumentNames, TypeArguments, lookupFlags, False, failure)
                                If (failure = ResolutionFailure.None) Then
                                    Dim instance As Object = container.InvokeMethod(method, Symbols.NoArguments, Nothing, lookupFlags)
                                    If (instance Is Nothing) Then
                                        Dim textArray4 As String() = New String() {method.ToString, container.VBFriendlyName}
                                        Throw New MissingMemberException(Utils.GetResourceString("IntermediateLateBoundNothingResult1", textArray4))
                                    End If
                                    NewLateBinding.LateIndexSetComplex(instance, Arguments, ArgumentNames, OptimisticSet, True)
                                    Return
                                End If
                                Exit Select
                        End Select
                        If Not OptimisticSet Then
                            If (TypeArguments.Length = 0) Then
                                NewLateBinding.ResolveCall(container, MemberName, members, Arguments, ArgumentNames, TypeArguments, setProperty, True, failure)
                            Else
                                NewLateBinding.ResolveCall(container, MemberName, members, Symbols.NoArguments, Symbols.NoArgumentNames, TypeArguments, lookupFlags, True, failure)
                            End If
                            Throw New InternalErrorException
                        End If
                    End If
                End If
            End If
        End Sub

        <DebuggerHidden, DebuggerStepThrough, DynamicallyInvokableAttribute>
        Public Shared Sub LateSetComplex(Instance As Object, Type As Type, MemberName As String, Arguments As Object(), ArgumentNames As String(), TypeArguments As Type(), OptimisticSet As Boolean, RValueBase As Boolean)
            Dim instance As IDynamicMetaObjectProvider = IDOUtils.TryCastToIDMOP(instance)
            If ((Not instance Is Nothing) AndAlso (TypeArguments Is Nothing)) Then
                IDOBinder.IDOSetComplex(instance, MemberName, Arguments, ArgumentNames, OptimisticSet, RValueBase)
            Else
                NewLateBinding.ObjectLateSetComplex(instance, Type, MemberName, Arguments, ArgumentNames, TypeArguments, OptimisticSet, RValueBase)
            End If
        End Sub

        Friend Shared Function MatchesPropertyRequirements(TargetProcedure As Method, Flags As BindingFlags) As MethodInfo
            If Symbols.HasFlag(Flags, BindingFlags.SetProperty) Then
                Return TargetProcedure.AsProperty.GetSetMethod
            End If
            Return TargetProcedure.AsProperty.GetGetMethod
        End Function

        <DebuggerHidden, DebuggerStepThrough>
        Private Shared Function ObjectLateCall(Instance As Object, Type As Type, MemberName As String, Arguments As Object(), ArgumentNames As String(), TypeArguments As Type(), CopyBack As Boolean(), IgnoreReturn As Boolean) As Object
            Dim container As Container
            Dim failure As ResolutionFailure
            If (Not Type Is Nothing) Then
                container = New Container(Type)
            Else
                container = New Container(Instance)
            End If
            Dim invocationFlags As BindingFlags = (BindingFlags.GetProperty Or BindingFlags.InvokeMethod)
            If IgnoreReturn Then
                invocationFlags = (invocationFlags Or BindingFlags.IgnoreReturn)
            End If
            Return NewLateBinding.CallMethod(container, MemberName, Arguments, ArgumentNames, TypeArguments, CopyBack, invocationFlags, True, failure)
        End Function

        <DebuggerHidden, DebuggerStepThrough>
        Private Shared Function ObjectLateGet(Instance As Object, Type As Type, MemberName As String, Arguments As Object(), ArgumentNames As String(), TypeArguments As Type(), CopyBack As Boolean()) As Object
            Dim container As Container
            Dim failure As ResolutionFailure
            If (Not Type Is Nothing) Then
                container = New Container(Type)
            Else
                container = New Container(Instance)
            End If
            Dim lookupFlags As BindingFlags = (BindingFlags.GetProperty Or BindingFlags.InvokeMethod)
            Dim members As MemberInfo() = container.GetMembers(MemberName, True)
            If (members(0).MemberType = MemberTypes.Field) Then
                If (TypeArguments.Length > 0) Then
                    Throw New ArgumentException(Utils.GetResourceString("Argument_InvalidValue"))
                End If
                Dim fieldValue As Object = container.GetFieldValue(DirectCast(members(0), FieldInfo))
                If (Arguments.Length = 0) Then
                    Return fieldValue
                End If
                Return NewLateBinding.LateIndexGet(fieldValue, Arguments, ArgumentNames, CopyBack)
            End If
            If ((ArgumentNames.Length > Arguments.Length) OrElse ((Not CopyBack Is Nothing) AndAlso (CopyBack.Length <> Arguments.Length))) Then
                Throw New ArgumentException(Utils.GetResourceString("Argument_InvalidValue"))
            End If
            Dim targetProcedure As Method = NewLateBinding.ResolveCall(container, MemberName, members, Arguments, ArgumentNames, TypeArguments, lookupFlags, False, failure)
            If (failure = ResolutionFailure.None) Then
                Return container.InvokeMethod(targetProcedure, Arguments, CopyBack, lookupFlags)
            End If
            If (((Arguments.Length > 0) AndAlso (members.Length = 1)) AndAlso NewLateBinding.IsZeroArgumentCall(members(0))) Then
                targetProcedure = NewLateBinding.ResolveCall(container, MemberName, members, Symbols.NoArguments, Symbols.NoArgumentNames, TypeArguments, lookupFlags, False, failure)
                If (failure = ResolutionFailure.None) Then
                    Dim instance As Object = container.InvokeMethod(targetProcedure, Symbols.NoArguments, Nothing, lookupFlags)
                    If (instance Is Nothing) Then
                        Dim args As String() = New String() {targetProcedure.ToString, container.VBFriendlyName}
                        Throw New MissingMemberException(Utils.GetResourceString("IntermediateLateBoundNothingResult1", args))
                    End If
                    instance = NewLateBinding.InternalLateIndexGet(instance, Arguments, ArgumentNames, False, failure, CopyBack)
                    If (failure = ResolutionFailure.None) Then
                        Return instance
                    End If
                End If
            End If
            NewLateBinding.ResolveCall(container, MemberName, members, Arguments, ArgumentNames, TypeArguments, lookupFlags, True, failure)
            Throw New InternalErrorException
        End Function

        <DebuggerHidden, DebuggerStepThrough>
        Private Shared Sub ObjectLateIndexSet(Instance As Object, Arguments As Object(), ArgumentNames As String())
            NewLateBinding.ObjectLateIndexSetComplex(Instance, Arguments, ArgumentNames, False, False)
        End Sub

        <DebuggerHidden, DebuggerStepThrough>
        Friend Shared Sub ObjectLateIndexSetComplex(Instance As Object, Arguments As Object(), ArgumentNames As String(), OptimisticSet As Boolean, RValueBase As Boolean)
            If (Arguments Is Nothing) Then
                Arguments = Symbols.NoArguments
            End If
            If (ArgumentNames Is Nothing) Then
                ArgumentNames = Symbols.NoArgumentNames
            End If
            Dim baseReference As New Container(Instance)
            If baseReference.IsArray Then
                If (ArgumentNames.Length > 0) Then
                    Throw New ArgumentException(Utils.GetResourceString("Argument_InvalidNamedArgs"))
                End If
                baseReference.SetArrayValue(Arguments)
            Else
                If (ArgumentNames.Length > Arguments.Length) Then
                    Throw New ArgumentException(Utils.GetResourceString("Argument_InvalidValue"))
                End If
                If (Arguments.Length < 1) Then
                    Throw New ArgumentException(Utils.GetResourceString("Argument_InvalidValue"))
                End If
                Dim memberName As String = ""
                If (baseReference.IsCOMObject AndAlso Not baseReference.IsWindowsRuntimeObject) Then
                    LateBinding.LateIndexSetComplex(Instance, Arguments, ArgumentNames, OptimisticSet, RValueBase)
                Else
                    Dim failure As ResolutionFailure
                    Dim setProperty As BindingFlags = BindingFlags.SetProperty
                    Dim members As MemberInfo() = baseReference.GetMembers(memberName, True)
                    Dim targetProcedure As Method = NewLateBinding.ResolveCall(baseReference, memberName, members, Arguments, ArgumentNames, Symbols.NoTypeArguments, setProperty, False, failure)
                    If (failure = ResolutionFailure.None) Then
                        If (RValueBase AndAlso baseReference.IsValueType) Then
                            Dim args As String() = New String() {baseReference.VBFriendlyName, baseReference.VBFriendlyName}
                            Throw New Exception(Utils.GetResourceString("RValueBaseForValueType", args))
                        End If
                        baseReference.InvokeMethod(targetProcedure, Arguments, Nothing, setProperty)
                    ElseIf Not OptimisticSet Then
                        NewLateBinding.ResolveCall(baseReference, memberName, members, Arguments, ArgumentNames, Symbols.NoTypeArguments, setProperty, True, failure)
                        Throw New InternalErrorException
                    End If
                End If
            End If
        End Sub

        <DebuggerHidden, DebuggerStepThrough>
        Private Shared Function ObjectLateInvokeDefault(Instance As Object, Arguments As Object(), ArgumentNames As String(), ReportErrors As Boolean, CopyBack As Boolean()) As Object
            Dim failure As ResolutionFailure
            Dim container As New Container(Instance)
            Dim obj2 As Object = NewLateBinding.InternalLateIndexGet(Instance, Arguments, ArgumentNames, ((ReportErrors OrElse (Arguments.Length <> 0)) OrElse container.IsArray), failure, CopyBack)
            If (failure <> ResolutionFailure.None) Then
                Return Instance
            End If
            Return obj2
        End Function

        Friend Shared Sub ObjectLateSet(Instance As Object, Type As Type, MemberName As String, Arguments As Object(), ArgumentNames As String(), TypeArguments As Type())
            NewLateBinding.LateSet(Instance, Type, MemberName, Arguments, ArgumentNames, TypeArguments, False, False, DirectCast(0, CallType))
        End Sub

        <DebuggerHidden, DebuggerStepThrough>
        Friend Shared Sub ObjectLateSetComplex(Instance As Object, Type As Type, MemberName As String, Arguments As Object(), ArgumentNames As String(), TypeArguments As Type(), OptimisticSet As Boolean, RValueBase As Boolean)
            NewLateBinding.LateSet(Instance, Type, MemberName, Arguments, ArgumentNames, TypeArguments, OptimisticSet, RValueBase, DirectCast(0, CallType))
        End Sub

        Friend Shared Function ReportPropertyMismatch(TargetProcedure As Method, Flags As BindingFlags) As Exception
            If Symbols.HasFlag(Flags, BindingFlags.SetProperty) Then
                Dim textArray1 As String() = New String() {TargetProcedure.AsProperty.Name}
                Return New MissingMemberException(Utils.GetResourceString("NoSetProperty1", textArray1))
            End If
            Dim args As String() = New String() {TargetProcedure.AsProperty.Name}
            Return New MissingMemberException(Utils.GetResourceString("NoGetProperty1", args))
        End Function

        Friend Shared Sub ResetCopyback(CopyBack As Boolean())
            If (Not CopyBack Is Nothing) Then
                Dim num As Integer = (CopyBack.Length - 1)
                Dim i As Integer = 0
                Do While (i <= num)
                    CopyBack(i) = False
                    i += 1
                Loop
            End If
        End Sub

        Friend Shared Function ResolveCall(BaseReference As Container, MethodName As String, Members As MemberInfo(), Arguments As Object(), ArgumentNames As String(), TypeArguments As Type(), LookupFlags As BindingFlags, ReportErrors As Boolean, ByRef Failure As ResolutionFailure) As Method
            Failure = ResolutionFailure.None
            If ((Members(0).MemberType <> MemberTypes.Method) AndAlso (Members(0).MemberType <> MemberTypes.Property)) Then
                Failure = ResolutionFailure.InvalidTarget
                If ReportErrors Then
                    Dim args As String() = New String() {MethodName, BaseReference.VBFriendlyName}
                    Throw New ArgumentException(Utils.GetResourceString("ExpressionNotProcedure", args))
                End If
                Return Nothing
            End If
            Dim length As Integer = Arguments.Length
            Dim argument As Object = Nothing
            If Symbols.HasFlag(LookupFlags, BindingFlags.SetProperty) Then
                If (Arguments.Length = 0) Then
                    Failure = ResolutionFailure.InvalidArgument
                    If ReportErrors Then
                        Dim textArray2 As String() = New String() {MethodName}
                        Throw New InvalidCastException(Utils.GetResourceString("PropertySetMissingArgument1", textArray2))
                    End If
                    Return Nothing
                End If
                Dim sourceArray As Object() = Arguments
                Arguments = New Object(((length - 2) + 1) - 1) {}
                Array.Copy(sourceArray, Arguments, Arguments.Length)
                argument = sourceArray((length - 1))
            End If
            Dim targetProcedure As Method = OverloadResolution.ResolveOverloadedCall(MethodName, Members, Arguments, ArgumentNames, TypeArguments, LookupFlags, ReportErrors, Failure, BaseReference)
            If (Failure Is ResolutionFailure.None) Then
                If (Not targetProcedure.ArgumentsValidated AndAlso Not OverloadResolution.CanMatchArguments(targetProcedure, Arguments, ArgumentNames, TypeArguments, False, Nothing)) Then
                    Failure = ResolutionFailure.InvalidArgument
                    If ReportErrors Then
                        Dim str As String = ""
                        Dim errors As New List(Of String)
                        OverloadResolution.CanMatchArguments(targetProcedure, Arguments, ArgumentNames, TypeArguments, False, errors)
                        Dim str2 As String
                        For Each str2 In errors
                            str = (str & ChrW(13) & ChrW(10) & "    " & str2)
                        Next
                        Dim textArray3 As String() = New String() {targetProcedure.ToString, str}
                        Throw New InvalidCastException(Utils.GetResourceString("MatchArgumentFailure2", textArray3))
                    End If
                    Return Nothing
                End If
                If targetProcedure.IsProperty Then
                    If (NewLateBinding.MatchesPropertyRequirements(targetProcedure, LookupFlags) Is Nothing) Then
                        Failure = ResolutionFailure.InvalidTarget
                        If ReportErrors Then
                            Throw NewLateBinding.ReportPropertyMismatch(targetProcedure, LookupFlags)
                        End If
                        Return Nothing
                    End If
                ElseIf Symbols.HasFlag(LookupFlags, BindingFlags.SetProperty) Then
                    Failure = ResolutionFailure.InvalidTarget
                    If ReportErrors Then
                        Dim textArray4 As String() = New String() {targetProcedure.AsMethod.Name}
                        Throw New MissingMemberException(Utils.GetResourceString("MethodAssignment1", textArray4))
                    End If
                    Return Nothing
                End If
                If Not Symbols.HasFlag(LookupFlags, BindingFlags.SetProperty) Then
                    Return targetProcedure
                End If
                Dim parameters As ParameterInfo() = NewLateBinding.GetCallTarget(targetProcedure, LookupFlags).GetParameters
                Dim parameter As ParameterInfo = parameters((parameters.Length - 1))
                Dim requiresNarrowingConversion As Boolean = False
                Dim allNarrowingIsFromObject As Boolean = False
                If OverloadResolution.CanPassToParameter(targetProcedure, argument, parameter, False, False, Nothing, requiresNarrowingConversion, allNarrowingIsFromObject) Then
                    Return targetProcedure
                End If
                Failure = ResolutionFailure.InvalidArgument
                If ReportErrors Then
                    Dim str3 As String = ""
                    Dim list2 As New List(Of String)
                    allNarrowingIsFromObject = False
                    requiresNarrowingConversion = False
                    OverloadResolution.CanPassToParameter(targetProcedure, argument, parameter, False, False, list2, allNarrowingIsFromObject, requiresNarrowingConversion)
                    Dim str4 As String
                    For Each str4 In list2
                        str3 = (str3 & ChrW(13) & ChrW(10) & "    " & str4)
                    Next
                    Dim textArray5 As String() = New String() {targetProcedure.ToString, str3}
                    Throw New InvalidCastException(Utils.GetResourceString("MatchArgumentFailure2", textArray5))
                End If
            End If
            Return Nothing
        End Function

    End Class
End Namespace

