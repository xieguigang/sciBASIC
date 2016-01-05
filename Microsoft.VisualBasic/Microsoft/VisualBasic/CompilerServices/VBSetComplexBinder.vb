Imports System
Imports System.Dynamic
Imports System.Linq.Expressions

Namespace Microsoft.VisualBasic.CompilerServices
    Friend Class VBSetComplexBinder
        Inherits SetMemberBinder
        ' Methods
        Public Sub New(MemberName As String, OptimisticSet As Boolean, RValueBase As Boolean)
            MyBase.New(MemberName, True)
            Me._optimisticSet = OptimisticSet
            Me._rValueBase = RValueBase
        End Sub

        Public Overrides Function Equals(_other As Object) As Boolean
            Dim binder As VBSetComplexBinder = TryCast(_other, VBSetComplexBinder)
            Return ((((Not binder Is Nothing) AndAlso String.Equals(MyBase.Name, binder.Name)) AndAlso (Me._optimisticSet = binder._optimisticSet)) AndAlso (Me._rValueBase = binder._rValueBase))
        End Function

        Public Overrides Function FallbackSetMember(target As DynamicMetaObject, value As DynamicMetaObject, errorSuggestion As DynamicMetaObject) As DynamicMetaObject
            If IDOUtils.NeedsDeferral(target, Nothing, value) Then
                Dim args As DynamicMetaObject() = New DynamicMetaObject() {value}
                Return MyBase.Defer(target, args)
            End If
            If ((Not errorSuggestion Is Nothing) AndAlso Not NewLateBinding.CanBindSet(target.Value, MyBase.Name, value.Value, Me._optimisticSet, Me._rValueBase)) Then
                Return errorSuggestion
            End If
            Dim expression As Expression = IDOUtils.ConvertToObject(value.Expression)
            Dim initializers As Expression() = New Expression() {expression}
            Return New DynamicMetaObject(Expression.Block(Expression.Call(GetType(NewLateBinding).GetMethod("FallbackSetComplex"), target.Expression, Expression.Constant(MyBase.Name), Expression.NewArrayInit(GetType(Object), initializers), Expression.Constant(Me._optimisticSet), Expression.Constant(Me._rValueBase)), expression), IDOUtils.CreateRestrictions(target, Nothing, value))
        End Function

        Public Overrides Function GetHashCode() As Integer
            Return (((VBSetComplexBinder._hash Xor MyBase.Name.GetHashCode) Xor Me._optimisticSet.GetHashCode) Xor Me._rValueBase.GetHashCode)
        End Function


        ' Fields
        Private Shared ReadOnly _hash As Integer = GetType(VBSetComplexBinder).GetHashCode
        Private ReadOnly _optimisticSet As Boolean
        Private ReadOnly _rValueBase As Boolean
    End Class
End Namespace

