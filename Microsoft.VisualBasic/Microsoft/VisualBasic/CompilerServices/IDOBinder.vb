Imports System
Imports System.Dynamic
Imports System.Linq.Expressions
Imports System.Runtime.CompilerServices
Imports System.Runtime.InteropServices

Namespace Microsoft.VisualBasic.CompilerServices
    Friend Class IDOBinder
        ' Methods
        Private Sub New()
            Throw New InternalErrorException
        End Sub

        Friend Shared Function GetCopyBack() As Boolean()
            Return SaveCopyBack.GetCopyBack
        End Function

        Friend Shared Function IDOCall(Instance As IDynamicMetaObjectProvider, MemberName As String, Arguments As Object(), ArgumentNames As String(), CopyBack As Boolean(), IgnoreReturn As Boolean) As Object
            Dim obj2 As Object
            Using New SaveCopyBack(CopyBack)
                Dim callInfo As CallInfo = Nothing
                Dim packedArgs As Object() = Nothing
                IDOUtils.PackArguments(0, ArgumentNames, Arguments, packedArgs, callInfo)
                Try
                    obj2 = IDOUtils.CreateRefCallSiteAndInvoke(New VBCallBinder(MemberName, callInfo, IgnoreReturn), Instance, packedArgs)
                Finally
                    IDOUtils.CopyBackArguments(callInfo, packedArgs, Arguments)
                End Try
            End Using
            Return obj2
        End Function

        Friend Shared Function IDOFallbackInvokeDefault(Instance As IDynamicMetaObjectProvider, Arguments As Object(), ArgumentNames As String(), ReportErrors As Boolean, CopyBack As Boolean()) As Object
            Dim obj2 As Object
            Using New SaveCopyBack(CopyBack)
                Dim packedArgs As Object() = Nothing
                Dim callInfo As CallInfo = Nothing
                IDOUtils.PackArguments(0, ArgumentNames, Arguments, packedArgs, callInfo)
                Try
                    obj2 = IDOUtils.CreateRefCallSiteAndInvoke(New VBInvokeDefaultFallbackBinder(callInfo, ReportErrors), Instance, packedArgs)
                Finally
                    IDOUtils.CopyBackArguments(callInfo, packedArgs, Arguments)
                End Try
            End Using
            Return obj2
        End Function

        Friend Shared Function IDOGet(Instance As IDynamicMetaObjectProvider, MemberName As String, Arguments As Object(), ArgumentNames As String(), CopyBack As Boolean()) As Object
            Dim obj2 As Object
            Using New SaveCopyBack(CopyBack)
                Dim packedArgs As Object() = Nothing
                Dim callInfo As CallInfo = Nothing
                IDOUtils.PackArguments(0, ArgumentNames, Arguments, packedArgs, callInfo)
                Try
                    obj2 = IDOUtils.CreateRefCallSiteAndInvoke(New VBGetBinder(MemberName, callInfo), Instance, packedArgs)
                Finally
                    IDOUtils.CopyBackArguments(callInfo, packedArgs, Arguments)
                End Try
            End Using
            Return obj2
        End Function

        Friend Shared Sub IDOIndexSet(Instance As IDynamicMetaObjectProvider, Arguments As Object(), ArgumentNames As String())
            Using New SaveCopyBack(Nothing)
                Dim packedArgs As Object() = Nothing
                Dim callInfo As CallInfo = Nothing
                IDOUtils.PackArguments(1, ArgumentNames, Arguments, packedArgs, callInfo)
                IDOUtils.CreateFuncCallSiteAndInvoke(New VBIndexSetBinder(callInfo), Instance, packedArgs)
            End Using
        End Sub

        Friend Shared Sub IDOIndexSetComplex(Instance As IDynamicMetaObjectProvider, Arguments As Object(), ArgumentNames As String(), OptimisticSet As Boolean, RValueBase As Boolean)
            Using New SaveCopyBack(Nothing)
                Dim packedArgs As Object() = Nothing
                Dim callInfo As CallInfo = Nothing
                IDOUtils.PackArguments(1, ArgumentNames, Arguments, packedArgs, callInfo)
                IDOUtils.CreateFuncCallSiteAndInvoke(New VBIndexSetComplexBinder(callInfo, OptimisticSet, RValueBase), Instance, packedArgs)
            End Using
        End Sub

        Friend Shared Function IDOInvokeDefault(Instance As IDynamicMetaObjectProvider, Arguments As Object(), ArgumentNames As String(), ReportErrors As Boolean, CopyBack As Boolean()) As Object
            Dim obj2 As Object
            Using New SaveCopyBack(CopyBack)
                Dim packedArgs As Object() = Nothing
                Dim callInfo As CallInfo = Nothing
                IDOUtils.PackArguments(0, ArgumentNames, Arguments, packedArgs, callInfo)
                Try
                    obj2 = IDOUtils.CreateRefCallSiteAndInvoke(New VBInvokeDefaultBinder(callInfo, ReportErrors), Instance, packedArgs)
                Finally
                    IDOUtils.CopyBackArguments(callInfo, packedArgs, Arguments)
                End Try
            End Using
            Return obj2
        End Function

        Friend Shared Sub IDOSet(Instance As IDynamicMetaObjectProvider, MemberName As String, ArgumentNames As String(), Arguments As Object())
            Using New SaveCopyBack(Nothing)
                If (Arguments.Length = 1) Then
                    IDOUtils.CreateFuncCallSiteAndInvoke(New VBSetBinder(MemberName), Instance, Arguments)
                Else
                    Dim instance As Object = IDOUtils.CreateFuncCallSiteAndInvoke(New VBGetMemberBinder(MemberName), instance, Symbols.NoArguments)
                    If (instance Is IDOBinder.missingMemberSentinel) Then
                        NewLateBinding.ObjectLateSet(instance, Nothing, MemberName, Arguments, ArgumentNames, Symbols.NoTypeArguments)
                    Else
                        NewLateBinding.LateIndexSet(instance, Arguments, ArgumentNames)
                    End If
                End If
            End Using
        End Sub

        Friend Shared Sub IDOSetComplex(Instance As IDynamicMetaObjectProvider, MemberName As String, Arguments As Object(), ArgumentNames As String(), OptimisticSet As Boolean, RValueBase As Boolean)
            Using New SaveCopyBack(Nothing)
                If (Arguments.Length = 1) Then
                    IDOUtils.CreateFuncCallSiteAndInvoke(New VBSetComplexBinder(MemberName, OptimisticSet, RValueBase), Instance, Arguments)
                Else
                    Dim instance As Object = IDOUtils.CreateFuncCallSiteAndInvoke(New VBGetMemberBinder(MemberName), instance, Symbols.NoArguments)
                    If (instance Is IDOBinder.missingMemberSentinel) Then
                        NewLateBinding.ObjectLateSetComplex(instance, Nothing, MemberName, Arguments, ArgumentNames, Symbols.NoTypeArguments, OptimisticSet, RValueBase)
                    Else
                        NewLateBinding.LateIndexSetComplex(instance, Arguments, ArgumentNames, OptimisticSet, RValueBase)
                    End If
                End If
            End Using
        End Sub

        Friend Shared Function InvokeUserDefinedOperator(Op As UserDefinedOperator, Arguments As Object()) As Object
            Dim binder As CallSiteBinder
            Dim nullable As ExpressionType? = IDOUtils.LinqOperator(Op)
            If Not nullable.HasValue Then
                Return Operators.InvokeObjectUserDefinedOperator(Op, arguments)
            End If
            Dim linqOp As ExpressionType = nullable.Value
            If (arguments.Length = 1) Then
                binder = New VBUnaryOperatorBinder(Op, linqOp)
            Else
                binder = New VBBinaryOperatorBinder(Op, linqOp)
            End If
            Dim instance As Object = arguments(0)
            Dim arguments As Object() = If((arguments.Length = 1), Symbols.NoArguments, New Object() {arguments(1)})
            Return IDOUtils.CreateFuncCallSiteAndInvoke(binder, instance, arguments)
        End Function

        Friend Shared Function UserDefinedConversion(Expression As IDynamicMetaObjectProvider, TargetType As Type) As Object
            Return IDOUtils.CreateConvertCallSiteAndInvoke(New VBConversionBinder(TargetType), Expression)
        End Function


        ' Fields
        Friend Shared ReadOnly missingMemberSentinel As Object = New Object

        ' Nested Types
        <StructLayout(LayoutKind.Sequential)>
        Private Structure SaveCopyBack
            Implements IDisposable
            <ThreadStatic>
            Private Shared SavedCopyBack As Boolean()
            Private oldCopyBack As Boolean()
            Public Sub New(copyBack As Boolean())
                Me = New SaveCopyBack
                Me.oldCopyBack = SaveCopyBack.SavedCopyBack
                SaveCopyBack.SavedCopyBack = copyBack
            End Sub

            Public Sub Dispose() Implements IDisposable.Dispose
                SaveCopyBack.SavedCopyBack = Me.oldCopyBack
            End Sub

            Friend Shared Function GetCopyBack() As Boolean()
                Return SaveCopyBack.SavedCopyBack
            End Function
        End Structure
    End Class
End Namespace

