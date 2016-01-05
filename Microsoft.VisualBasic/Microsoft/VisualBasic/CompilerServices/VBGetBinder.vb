Imports System
Imports System.Dynamic
Imports System.Linq.Expressions

Namespace Microsoft.VisualBasic.CompilerServices
    Friend Class VBGetBinder
        Inherits InvokeMemberBinder
        ' Methods
        Public Sub New(MemberName As String, CallInfo As CallInfo)
            MyBase.New(MemberName, True, CallInfo)
        End Sub

        Public Overrides Function Equals(_other As Object) As Boolean
            Dim binder As VBGetBinder = TryCast(_other, VBGetBinder)
            Return (((Not binder Is Nothing) AndAlso String.Equals(MyBase.Name, binder.Name)) AndAlso MyBase.CallInfo.Equals(binder.CallInfo))
        End Function

        Public Overrides Function FallbackInvoke(target As DynamicMetaObject, packedArgs As DynamicMetaObject(), errorSuggestion As DynamicMetaObject) As DynamicMetaObject
            Return New VBInvokeBinder(MyBase.CallInfo, False).FallbackInvoke(target, packedArgs, errorSuggestion)
        End Function

        Public Overrides Function FallbackInvokeMember(target As DynamicMetaObject, packedArgs As DynamicMetaObject(), errorSuggestion As DynamicMetaObject) As DynamicMetaObject
            If IDOUtils.NeedsDeferral(target, packedArgs, Nothing) Then
                Return MyBase.Defer(target, packedArgs)
            End If
            Dim args As Expression() = Nothing
            Dim argNames As String() = Nothing
            Dim argValues As Object() = Nothing
            IDOUtils.UnpackArguments(packedArgs, MyBase.CallInfo, args, argNames, argValues)
            If ((Not errorSuggestion Is Nothing) AndAlso Not NewLateBinding.CanBindGet(target.Value, MyBase.Name, argValues, argNames)) Then
                Return errorSuggestion
            End If
            Dim left As ParameterExpression = Expression.Variable(GetType(Object), "result")
            Dim expression2 As ParameterExpression = Expression.Variable(GetType(Object()), "array")
            Dim right As Expression = Expression.Call(GetType(NewLateBinding).GetMethod("FallbackGet"), target.Expression, Expression.Constant(MyBase.Name), Expression.Assign(expression2, Expression.NewArrayInit(GetType(Object), args)), Expression.Constant(argNames, GetType(String())))
            Dim variables As ParameterExpression() = New ParameterExpression() {left, expression2}
            Dim expressions As Expression() = New Expression() {Expression.Assign(left, right), IDOUtils.GetWriteBack(args, expression2), left}
            Return New DynamicMetaObject(Expression.Block(variables, expressions), IDOUtils.CreateRestrictions(target, packedArgs, Nothing))
        End Function

        Public Overrides Function GetHashCode() As Integer
            Return ((VBGetBinder._hash Xor MyBase.Name.GetHashCode) Xor MyBase.CallInfo.GetHashCode)
        End Function


        ' Fields
        Private Shared ReadOnly _hash As Integer = GetType(VBGetBinder).GetHashCode
    End Class
End Namespace

