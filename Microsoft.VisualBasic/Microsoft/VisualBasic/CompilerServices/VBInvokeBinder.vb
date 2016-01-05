Imports System
Imports System.Dynamic
Imports System.Linq.Expressions

Namespace Microsoft.VisualBasic.CompilerServices
    Friend Class VBInvokeBinder
        Inherits InvokeBinder
        ' Methods
        Public Sub New(CallInfo As CallInfo, LateCall As Boolean)
            MyBase.New(CallInfo)
            Me._lateCall = LateCall
        End Sub

        Public Overrides Function Equals(_other As Object) As Boolean
            Dim binder As VBInvokeBinder = TryCast(_other, VBInvokeBinder)
            Return (((Not binder Is Nothing) AndAlso MyBase.CallInfo.Equals(binder.CallInfo)) AndAlso Me._lateCall.Equals(binder._lateCall))
        End Function

        Public Overrides Function FallbackInvoke(target As DynamicMetaObject, packedArgs As DynamicMetaObject(), errorSuggestion As DynamicMetaObject) As DynamicMetaObject
            If IDOUtils.NeedsDeferral(target, packedArgs, Nothing) Then
                Return MyBase.Defer(target, packedArgs)
            End If
            Dim args As Expression() = Nothing
            Dim argNames As String() = Nothing
            Dim argValues As Object() = Nothing
            IDOUtils.UnpackArguments(packedArgs, MyBase.CallInfo, args, argNames, argValues)
            If ((Not errorSuggestion Is Nothing) AndAlso Not NewLateBinding.CanBindInvokeDefault(target.Value, argValues, argNames, Me._lateCall)) Then
                Return errorSuggestion
            End If
            Dim left As ParameterExpression = Expression.Variable(GetType(Object), "result")
            Dim expression2 As ParameterExpression = Expression.Variable(GetType(Object()), "array")
            Dim right As Expression = Expression.Call(GetType(NewLateBinding).GetMethod(If(Me._lateCall, "LateCallInvokeDefault", "LateGetInvokeDefault")), target.Expression, Expression.Assign(expression2, Expression.NewArrayInit(GetType(Object), args)), Expression.Constant(argNames, GetType(String())), Expression.Constant(Me._lateCall))
            Dim variables As ParameterExpression() = New ParameterExpression() {left, expression2}
            Dim expressions As Expression() = New Expression() {Expression.Assign(left, right), IDOUtils.GetWriteBack(args, expression2), left}
            Return New DynamicMetaObject(Expression.Block(variables, expressions), IDOUtils.CreateRestrictions(target, packedArgs, Nothing))
        End Function

        Public Overrides Function GetHashCode() As Integer
            Return ((VBInvokeBinder._hash Xor MyBase.CallInfo.GetHashCode) Xor Me._lateCall.GetHashCode)
        End Function


        ' Fields
        Private Shared ReadOnly _hash As Integer = GetType(VBGetBinder).GetHashCode
        Private ReadOnly _lateCall As Boolean
    End Class
End Namespace

