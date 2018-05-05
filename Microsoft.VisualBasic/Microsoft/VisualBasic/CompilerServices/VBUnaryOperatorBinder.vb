Imports System.Dynamic
Imports System.Linq.Expressions
Imports Microsoft.VisualBasic.CompilerServices.Symbols

Namespace Microsoft.VisualBasic.CompilerServices

    Friend Class VBUnaryOperatorBinder
        Inherits UnaryOperationBinder
        ' Methods
        Public Sub New(Op As UserDefinedOperator, LinqOp As ExpressionType)
            MyBase.New(LinqOp)
            Me._Op = Op
        End Sub

        Public Overrides Function Equals(_other As Object) As Boolean
            Dim binder As VBUnaryOperatorBinder = TryCast(_other, VBUnaryOperatorBinder)
            Return (((Not binder Is Nothing) AndAlso (Me._Op = binder._Op)) AndAlso (MyBase.Operation = binder.Operation))
        End Function

        Public Overrides Function FallbackUnaryOperation(target As DynamicMetaObject, errorSuggestion As DynamicMetaObject) As DynamicMetaObject
            If IDOUtils.NeedsDeferral(target, Nothing, Nothing) Then
                Return MyBase.Defer(target, New DynamicMetaObject(0 - 1) {})
            End If
            If (Not errorSuggestion Is Nothing) Then
                Dim arguments As Object() = New Object() {target.Value}
                If (Operators.GetCallableUserDefinedOperator(Me._Op, arguments) Is Nothing) Then
                    Return errorSuggestion
                End If
            End If
            Dim initializers As Expression() = New Expression() {IDOUtils.ConvertToObject(target.Expression)}
            Return New DynamicMetaObject(Expression.Call(GetType(Operators).GetMethod("FallbackInvokeUserDefinedOperator"), Expression.Constant(Me._Op, GetType(Object)), Expression.NewArrayInit(GetType(Object), initializers)), IDOUtils.CreateRestrictions(target, Nothing, Nothing))
        End Function

        Public Overrides Function GetHashCode() As Integer
            Return ((VBUnaryOperatorBinder._hash Xor Me._Op.GetHashCode) Xor MyBase.Operation.GetHashCode)
        End Function


        ' Fields
        Private Shared ReadOnly _hash As Integer = GetType(VBUnaryOperatorBinder).GetHashCode
        Private ReadOnly _Op As UserDefinedOperator
    End Class
End Namespace

