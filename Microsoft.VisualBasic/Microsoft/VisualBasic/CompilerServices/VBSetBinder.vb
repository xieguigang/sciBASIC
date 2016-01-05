Imports System
Imports System.Dynamic
Imports System.Linq.Expressions

Namespace Microsoft.VisualBasic.CompilerServices
    Friend Class VBSetBinder
        Inherits SetMemberBinder
        ' Methods
        Public Sub New(MemberName As String)
            MyBase.New(MemberName, True)
        End Sub

        Public Overrides Function Equals(_other As Object) As Boolean
            Dim binder As VBSetBinder = TryCast(_other, VBSetBinder)
            Return ((Not binder Is Nothing) AndAlso String.Equals(MyBase.Name, binder.Name))
        End Function

        Public Overrides Function FallbackSetMember(target As DynamicMetaObject, value As DynamicMetaObject, errorSuggestion As DynamicMetaObject) As DynamicMetaObject
            If IDOUtils.NeedsDeferral(target, Nothing, value) Then
                Dim args As DynamicMetaObject() = New DynamicMetaObject() {value}
                Return MyBase.Defer(target, args)
            End If
            If ((Not errorSuggestion Is Nothing) AndAlso Not NewLateBinding.CanBindSet(target.Value, MyBase.Name, value.Value, False, False)) Then
                Return errorSuggestion
            End If
            Dim expression As Expression = IDOUtils.ConvertToObject(value.Expression)
            Dim initializers As Expression() = New Expression() {expression}
            Return New DynamicMetaObject(Expression.Block(Expression.Call(GetType(NewLateBinding).GetMethod("FallbackSet"), target.Expression, Expression.Constant(MyBase.Name), Expression.NewArrayInit(GetType(Object), initializers)), expression), IDOUtils.CreateRestrictions(target, Nothing, value))
        End Function

        Public Overrides Function GetHashCode() As Integer
            Return (VBSetBinder._hash Xor MyBase.Name.GetHashCode)
        End Function


        ' Fields
        Private Shared ReadOnly _hash As Integer = GetType(VBSetBinder).GetHashCode
    End Class
End Namespace

