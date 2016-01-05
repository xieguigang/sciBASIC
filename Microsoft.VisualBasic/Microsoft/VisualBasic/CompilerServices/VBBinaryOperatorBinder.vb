Imports System
Imports System.Dynamic
Imports System.Linq.Expressions

Namespace Microsoft.VisualBasic.CompilerServices
    Friend Class VBBinaryOperatorBinder
        Inherits BinaryOperationBinder
        ' Methods
        Public Sub New(Op As UserDefinedOperator, LinqOp As ExpressionType)
            MyBase.New(LinqOp)
            Me._Op = Op
        End Sub

        Public Overrides Function Equals(_other As Object) As Boolean
            Dim binder As VBBinaryOperatorBinder = TryCast(_other, VBBinaryOperatorBinder)
            Return (((Not binder Is Nothing) AndAlso (Me._Op = binder._Op)) AndAlso (MyBase.Operation = binder.Operation))
        End Function

        Public Overrides Function FallbackBinaryOperation(target As DynamicMetaObject, arg As DynamicMetaObject, errorSuggestion As DynamicMetaObject) As DynamicMetaObject
            If IDOUtils.NeedsDeferral(target, Nothing, arg) Then
                Dim args As DynamicMetaObject() = New DynamicMetaObject() {arg}
                Return MyBase.Defer(target, args)
            End If
            If (Not errorSuggestion Is Nothing) Then
                Dim arguments As Object() = New Object() {target.Value, arg.Value}
                If (Operators.GetCallableUserDefinedOperator(Me._Op, arguments) Is Nothing) Then
                    Return errorSuggestion
                End If
            End If
            Dim initializers As Expression() = New Expression() {IDOUtils.ConvertToObject(target.Expression), IDOUtils.ConvertToObject(arg.Expression)}
            Return New DynamicMetaObject(Expression.Call(GetType(Operators).GetMethod("FallbackInvokeUserDefinedOperator"), Expression.Constant(Me._Op, GetType(Object)), Expression.NewArrayInit(GetType(Object), initializers)), IDOUtils.CreateRestrictions(target, Nothing, arg))
        End Function

        Public Overrides Function GetHashCode() As Integer
            Return ((VBBinaryOperatorBinder._hash Xor Me._Op.GetHashCode) Xor MyBase.Operation.GetHashCode)
        End Function


        ' Fields
        Private Shared ReadOnly _hash As Integer = GetType(VBBinaryOperatorBinder).GetHashCode
        Private ReadOnly _Op As UserDefinedOperator
    End Class
End Namespace

