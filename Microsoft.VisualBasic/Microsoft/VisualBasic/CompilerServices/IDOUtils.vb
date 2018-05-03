Imports System.Collections.Generic
Imports System.Dynamic
Imports System.Linq.Expressions
Imports System.Reflection
Imports System.Reflection.Emit
Imports System.Runtime.CompilerServices
Imports System.Runtime.Remoting
Imports Microsoft.VisualBasic.CompilerServices.Symbols

Namespace Microsoft.VisualBasic.CompilerServices
    Friend Class IDOUtils
        ' Methods
        Private Sub New()
            Throw New InternalErrorException
        End Sub

        Public Shared Function ConvertToObject(valueExpression As Expression) As Expression
            If Not valueExpression.Type.Equals(GetType(Object)) Then
                Return Expression.Convert(valueExpression, GetType(Object))
            End If
            Return valueExpression
        End Function

        Public Shared Sub CopyBackArguments(callInfo As CallInfo, packedArgs As Object(), args As Object())
            If (Not packedArgs Is args) Then
                Dim argumentCount As Integer = callInfo.ArgumentCount
                Dim length As Integer = packedArgs.Length
                Dim num2 As Integer = (length - callInfo.ArgumentNames.Count)
                Dim num3 As Integer = (length - 1)
                Dim i As Integer = 0
                Do While (i <= num3)
                    args(i) = packedArgs(If((i < argumentCount), ((i + num2) Mod argumentCount), i))
                    i += 1
                Loop
            End If
        End Sub

        Public Shared Function CreateConvertCallSiteAndInvoke(Action As ConvertBinder, Instance As Object) As Object
            Dim obj2 As Object
            Dim typeArgs As Type() = New Type() {GetType(CallSite), GetType(Object), Action.Type}
            Dim site As CallSite = CallSite.Create(Expression.GetFuncType(typeArgs), IDOUtils.GetCachedBinder(Action))
            Dim args As Object() = New Object() {site, Instance}
            Dim delegate2 As Delegate = DirectCast(site.GetType.GetField("Target").GetValue(site), Delegate)
            Try
                obj2 = delegate2.DynamicInvoke(args)
            Catch exception As TargetInvocationException
                Throw exception.InnerException
            End Try
            Return obj2
        End Function

        Public Shared Function CreateFuncCallSiteAndInvoke(Action As CallSiteBinder, Instance As Object, Arguments As Object()) As Object
            Dim obj2 As Object
            Action = IDOUtils.GetCachedBinder(Action)
            Select Case Arguments.Length
                Case 0
                    Dim site As CallSite(Of Func(Of CallSite, Object, Object)) = CallSite(Of Func(Of CallSite, Object, Object)).Create(Action)
                    Return site.Target.Invoke(site, Instance)
                Case 1
                    Dim site2 As CallSite(Of Func(Of CallSite, Object, Object, Object)) = CallSite(Of Func(Of CallSite, Object, Object, Object)).Create(Action)
                    Return site2.Target.Invoke(site2, Instance, Arguments(0))
                Case 2
                    Dim site3 As CallSite(Of Func(Of CallSite, Object, Object, Object, Object)) = CallSite(Of Func(Of CallSite, Object, Object, Object, Object)).Create(Action)
                    Return site3.Target.Invoke(site3, Instance, Arguments(0), Arguments(1))
                Case 3
                    Dim site4 As CallSite(Of Func(Of CallSite, Object, Object, Object, Object, Object)) = CallSite(Of Func(Of CallSite, Object, Object, Object, Object, Object)).Create(Action)
                    Return site4.Target.Invoke(site4, Instance, Arguments(0), Arguments(1), Arguments(2))
                Case 4
                    Dim site5 As CallSite(Of Func(Of CallSite, Object, Object, Object, Object, Object, Object)) = CallSite(Of Func(Of CallSite, Object, Object, Object, Object, Object, Object)).Create(Action)
                    Return site5.Target.Invoke(site5, Instance, Arguments(0), Arguments(1), Arguments(2), Arguments(3))
                Case 5
                    Dim site6 As CallSite(Of Func(Of CallSite, Object, Object, Object, Object, Object, Object, Object)) = CallSite(Of Func(Of CallSite, Object, Object, Object, Object, Object, Object, Object)).Create(Action)
                    Return site6.Target.Invoke(site6, Instance, Arguments(0), Arguments(1), Arguments(2), Arguments(3), Arguments(4))
                Case 6
                    Dim site7 As CallSite(Of Func(Of CallSite, Object, Object, Object, Object, Object, Object, Object, Object)) = CallSite(Of Func(Of CallSite, Object, Object, Object, Object, Object, Object, Object, Object)).Create(Action)
                    Return site7.Target.Invoke(site7, Instance, Arguments(0), Arguments(1), Arguments(2), Arguments(3), Arguments(4), Arguments(5))
                Case 7
                    Dim site8 As CallSite(Of Func(Of CallSite, Object, Object, Object, Object, Object, Object, Object, Object, Object)) = CallSite(Of Func(Of CallSite, Object, Object, Object, Object, Object, Object, Object, Object, Object)).Create(Action)
                    Return site8.Target.Invoke(site8, Instance, Arguments(0), Arguments(1), Arguments(2), Arguments(3), Arguments(4), Arguments(5), Arguments(6))
            End Select
            Dim typeArgs As Type() = New Type(((Arguments.Length + 2) + 1) - 1) {}
            typeArgs(0) = GetType(CallSite)
            Dim num2 As Integer = (typeArgs.Length - 1)
            Dim i As Integer = 1
            Do While (i <= num2)
                typeArgs(i) = GetType(Object)
                i += 1
            Loop
            Dim site9 As CallSite = CallSite.Create(Expression.GetDelegateType(typeArgs), Action)
            Dim array As Object() = New Object(((Arguments.Length + 1) + 1) - 1) {}
            array(0) = site9
            array(1) = Instance
            Arguments.CopyTo(array, 2)
            Dim delegate2 As Delegate = DirectCast(site9.GetType.GetField("Target").GetValue(site9), Delegate)
            Try
                obj2 = delegate2.DynamicInvoke(array)
            Catch exception As TargetInvocationException
                Throw exception.InnerException
            End Try
            Return obj2
        End Function

        Private Shared Function CreateInvoker(ArgLength As Integer) As Func(Of CallSiteBinder, Object, Object(), Object)
            Dim returnType As Type = GetType(Object)
            Dim type2 As Type = returnType.MakeByRefType
            Dim type3 As Type = GetType(CallSiteBinder)
            Dim typeArgs As Type() = New Type(((ArgLength + 2) + 1) - 1) {}
            typeArgs(0) = GetType(CallSite)
            typeArgs(1) = returnType
            Dim num As Integer = (typeArgs.Length - 2)
            Dim i As Integer = 2
            Do While (i <= num)
                typeArgs(i) = type2
                i += 1
            Loop
            typeArgs((typeArgs.Length - 1)) = returnType
            Dim delegateType As Type = Expression.GetDelegateType(typeArgs)
            Dim typeArguments As Type() = New Type() {delegateType}
            Dim localType As Type = GetType(CallSite(Of )).MakeGenericType(typeArguments)
            Dim parameterTypes As Type() = New Type() {type3, returnType, GetType(Object())}
            Dim method As New DynamicMethod("Invoker", returnType, parameterTypes, True)
            Dim iLGenerator As ILGenerator = method.GetILGenerator
            Dim local As LocalBuilder = iLGenerator.DeclareLocal(localType)
            iLGenerator.Emit(OpCodes.Ldarg_0)
            Dim types As Type() = New Type() {type3}
            iLGenerator.Emit(OpCodes.Call, localType.GetMethod("Create", types))
            iLGenerator.Emit(OpCodes.Stloc, local)
            iLGenerator.Emit(OpCodes.Ldloc, local)
            iLGenerator.Emit(OpCodes.Ldfld, localType.GetField("Target"))
            iLGenerator.Emit(OpCodes.Ldloc, local)
            iLGenerator.Emit(OpCodes.Ldarg_1)
            Dim num3 As Integer = (ArgLength - 1)
            Dim j As Integer = 0
            Do While (j <= num3)
                iLGenerator.Emit(OpCodes.Ldarg_2)
                iLGenerator.Emit(OpCodes.Ldc_I4, j)
                iLGenerator.Emit(OpCodes.Ldelema, returnType)
                j += 1
            Loop
            iLGenerator.Emit(OpCodes.Callvirt, delegateType.GetMethod("Invoke"))
            iLGenerator.Emit(OpCodes.Ret)
            Return DirectCast(method.CreateDelegate(GetType(Func(Of CallSiteBinder, Object, Object(), Object))), Func(Of CallSiteBinder, Object, Object(), Object))
        End Function

        Public Shared Function CreateRefCallSiteAndInvoke(Action As CallSiteBinder, Instance As Object, Arguments As Object()) As Object
            Action = IDOUtils.GetCachedBinder(Action)
            Dim func As Func(Of CallSiteBinder, Object, Object(), Object) = Nothing
            Dim invokers As Object = IDOUtils.Invokers
            SyncLock invokers
                If Not IDOUtils.Invokers.TryGetValue(Arguments.Length, func) Then
                    func = IDOUtils.CreateInvoker(Arguments.Length)
                    IDOUtils.Invokers.Add(Arguments.Length, func)
                End If
            End SyncLock
            Return func.Invoke(Action, Instance, Arguments)
        End Function

        Private Shared Function CreateRestriction(metaObject As DynamicMetaObject) As BindingRestrictions
            If (metaObject.Value Is Nothing) Then
                Return metaObject.Restrictions.Merge(BindingRestrictions.GetInstanceRestriction(metaObject.Expression, Nothing))
            End If
            Return metaObject.Restrictions.Merge(BindingRestrictions.GetTypeRestriction(metaObject.Expression, metaObject.LimitType))
        End Function

        Friend Shared Function CreateRestrictions(target As DynamicMetaObject, Optional args As DynamicMetaObject() = Nothing, Optional value As DynamicMetaObject = Nothing) As BindingRestrictions
            Dim restrictions As BindingRestrictions = IDOUtils.CreateRestriction(target)
            If (Not args Is Nothing) Then
                Dim obj2 As DynamicMetaObject
                For Each obj2 In args
                    restrictions = restrictions.Merge(IDOUtils.CreateRestriction(obj2))
                Next
            End If
            If (Not value Is Nothing) Then
                restrictions = restrictions.Merge(IDOUtils.CreateRestriction(value))
            End If
            Return restrictions
        End Function

        Private Shared Function GetCachedBinder(Action As CallSiteBinder) As CallSiteBinder
            Return IDOUtils.binderCache.GetExistingOrAdd(Action)
        End Function

        Public Shared Function GetWriteBack(arguments As Expression(), array As ParameterExpression) As Expression
            Dim expressions As New List(Of Expression)
            Dim num As Integer = (arguments.Length - 1)
            Dim i As Integer = 0
            Do While (i <= num)
                Dim left As ParameterExpression = TryCast(arguments(i), ParameterExpression)
                If ((Not left Is Nothing) AndAlso left.IsByRef) Then
                    expressions.Add(Expression.Assign(left, Expression.ArrayIndex(array, Expression.Constant(i))))
                End If
                i += 1
            Loop
            Select Case expressions.Count
                Case 0
                    Return Expression.Empty
                Case 1
                    Return expressions.Item(0)
            End Select
            Return Expression.Block(expressions)
        End Function

        Friend Shared Function LinqOperator(vbOperator As UserDefinedOperator) As ExpressionType?
            Select Case vbOperator
                Case UserDefinedOperator.Negate
                    Return New ExpressionType?(ExpressionType.Negate)
                Case UserDefinedOperator.Not
                    Return New ExpressionType?(ExpressionType.Not)
                Case UserDefinedOperator.UnaryPlus
                    Return New ExpressionType?(ExpressionType.UnaryPlus)
                Case UserDefinedOperator.Plus
                    Return New ExpressionType?(ExpressionType.Add)
                Case UserDefinedOperator.Minus
                    Return New ExpressionType?(ExpressionType.Subtract)
                Case UserDefinedOperator.Multiply
                    Return New ExpressionType?(ExpressionType.Multiply)
                Case UserDefinedOperator.Divide
                    Return New ExpressionType?(ExpressionType.Divide)
                Case UserDefinedOperator.Power
                    Return New ExpressionType?(ExpressionType.Power)
                Case UserDefinedOperator.ShiftLeft
                    Return New ExpressionType?(ExpressionType.LeftShift)
                Case UserDefinedOperator.ShiftRight
                    Return New ExpressionType?(ExpressionType.RightShift)
                Case UserDefinedOperator.Modulus
                    Return New ExpressionType?(ExpressionType.Modulo)
                Case UserDefinedOperator.Or
                    Return New ExpressionType?(ExpressionType.Or)
                Case UserDefinedOperator.Xor
                    Return New ExpressionType?(ExpressionType.ExclusiveOr)
                Case UserDefinedOperator.And
                    Return New ExpressionType?(ExpressionType.And)
                Case UserDefinedOperator.Equal
                    Return New ExpressionType?(ExpressionType.Equal)
                Case UserDefinedOperator.NotEqual
                    Return New ExpressionType?(ExpressionType.NotEqual)
                Case UserDefinedOperator.Less
                    Return New ExpressionType?(ExpressionType.LessThan)
                Case UserDefinedOperator.LessEqual
                    Return New ExpressionType?(ExpressionType.LessThanOrEqual)
                Case UserDefinedOperator.GreaterEqual
                    Return New ExpressionType?(ExpressionType.GreaterThanOrEqual)
                Case UserDefinedOperator.Greater
                    Return New ExpressionType?(ExpressionType.GreaterThan)
            End Select
            Return Nothing
        End Function

        Friend Shared Function NeedsDeferral(target As DynamicMetaObject, Optional args As DynamicMetaObject() = Nothing, Optional value As DynamicMetaObject = Nothing) As Boolean
            If Not target.HasValue Then
                Return True
            End If
            If ((Not value Is Nothing) AndAlso Not value.HasValue) Then
                Return True
            End If
            If (Not args Is Nothing) Then
                Dim objArray As DynamicMetaObject() = args
                Dim i As Integer
                For i = 0 To objArray.Length - 1
                    If Not objArray(i).HasValue Then
                        Return True
                    End If
                Next i
            End If
            Return False
        End Function

        Public Shared Sub PackArguments(valueArgs As Integer, argNames As String(), args As Object(), ByRef packedArgs As Object(), ByRef callInfo As CallInfo)
            If (argNames Is Nothing) Then
                argNames = New String(0 - 1) {}
            End If
            callInfo = New CallInfo((args.Length - valueArgs), argNames)
            If (argNames.Length > 0) Then
                packedArgs = New Object(((args.Length - 1) + 1) - 1) {}
                Dim num As Integer = (args.Length - valueArgs)
                Dim num2 As Integer = (num - 1)
                Dim i As Integer = 0
                Do While (i <= num2)
                    packedArgs(i) = args(((i + argNames.Length) Mod num))
                    i += 1
                Loop
                Dim num4 As Integer = (args.Length - 1)
                Dim j As Integer = num
                Do While (j <= num4)
                    packedArgs(j) = args(j)
                    j += 1
                Loop
            Else
                packedArgs = args
            End If
        End Sub

        Friend Shared Function TryCastToIDMOP(o As Object) As IDynamicMetaObjectProvider
            Dim provider2 As IDynamicMetaObjectProvider = TryCast(o, IDynamicMetaObjectProvider)
            If ((Not provider2 Is Nothing) AndAlso Not RemotingServices.IsObjectOutOfAppDomain(o)) Then
                Return provider2
            End If
            Return Nothing
        End Function

        Public Shared Sub UnpackArguments(packedArgs As DynamicMetaObject(), callInfo As CallInfo, ByRef args As Expression(), ByRef argNames As String(), ByRef argValues As Object())
            Dim length As Integer = packedArgs.Length
            Dim argumentCount As Integer = callInfo.ArgumentCount
            args = New Expression(((length - 1) + 1) - 1) {}
            argValues = New Object(((length - 1) + 1) - 1) {}
            Dim count As Integer = callInfo.ArgumentNames.Count
            Dim num4 As Integer = (length - count)
            Dim num5 As Integer = (argumentCount - 1)
            Dim i As Integer = 0
            Do While (i <= num5)
                Dim obj2 As DynamicMetaObject = packedArgs(((i + num4) Mod argumentCount))
                args(i) = obj2.Expression
                argValues(i) = obj2.Value
                i += 1
            Loop
            Dim num7 As Integer = (length - 1)
            Dim j As Integer = argumentCount
            Do While (j <= num7)
                Dim obj3 As DynamicMetaObject = packedArgs(j)
                args(j) = obj3.Expression
                argValues(j) = obj3.Value
                j += 1
            Loop
            argNames = New String(((count - 1) + 1) - 1) {}
            callInfo.ArgumentNames.CopyTo(argNames, 0)
        End Sub


        ' Fields
        Private Shared binderCache As CacheSet(Of CallSiteBinder) = New CacheSet(Of CallSiteBinder)(&H40)
        Private Shared Invokers As CacheDict(Of Integer, Func(Of CallSiteBinder, Object, Object(), Object)) = New CacheDict(Of Integer, Func(Of CallSiteBinder, Object, Object(), Object))(&H10)
    End Class
End Namespace

