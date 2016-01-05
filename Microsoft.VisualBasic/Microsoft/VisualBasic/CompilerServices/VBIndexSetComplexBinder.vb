Imports System
Imports System.Dynamic
Imports System.Linq.Expressions

Namespace Microsoft.VisualBasic.CompilerServices
    Friend Class VBIndexSetComplexBinder
        Inherits SetIndexBinder
        ' Methods
        Public Sub New(CallInfo As CallInfo, OptimisticSet As Boolean, RValueBase As Boolean)
            MyBase.New(CallInfo)
            Me._optimisticSet = OptimisticSet
            Me._rValueBase = RValueBase
        End Sub

        Public Overrides Function Equals(_other As Object) As Boolean
            Dim binder As VBIndexSetComplexBinder = TryCast(_other, VBIndexSetComplexBinder)
            Return ((((Not binder Is Nothing) AndAlso MyBase.CallInfo.Equals(binder.CallInfo)) AndAlso (Me._optimisticSet = binder._optimisticSet)) AndAlso (Me._rValueBase = binder._rValueBase))
        End Function

        Public Overrides Function FallbackSetIndex(target As DynamicMetaObject, packedIndexes As DynamicMetaObject(), value As DynamicMetaObject, errorSuggestion As DynamicMetaObject) As DynamicMetaObject
            If IDOUtils.NeedsDeferral(target, packedIndexes, value) Then
                array.Resize(Of DynamicMetaObject)(packedIndexes, (packedIndexes.Length + 1))
                packedIndexes((packedIndexes.Length - 1)) = value
                Return MyBase.Defer(target, packedIndexes)
            End If
            Dim argNames As String() = Nothing
            Dim args As Expression() = Nothing
            Dim argValues As Object() = Nothing
            IDOUtils.UnpackArguments(packedIndexes, MyBase.CallInfo, args, argNames, argValues)
            Dim array As Object() = New Object((argValues.Length + 1) - 1) {}
            argValues.CopyTo(array, 0)
            array(argValues.Length) = value.Value
            If ((Not errorSuggestion Is Nothing) AndAlso Not NewLateBinding.CanIndexSetComplex(target.Value, array, argNames, Me._optimisticSet, Me._rValueBase)) Then
                Return errorSuggestion
            End If
            Dim expression As Expression = IDOUtils.ConvertToObject(value.Expression)
            Dim expressionArray2 As Expression() = New Expression((args.Length + 1) - 1) {}
            args.CopyTo(expressionArray2, 0)
            expressionArray2(args.Length) = expression
            Return New DynamicMetaObject(Expression.Block(Expression.Call(GetType(NewLateBinding).GetMethod("FallbackIndexSetComplex"), target.Expression, Expression.NewArrayInit(GetType(Object), expressionArray2), Expression.Constant(argNames, GetType(String())), Expression.Constant(Me._optimisticSet), Expression.Constant(Me._rValueBase)), expression), IDOUtils.CreateRestrictions(target, packedIndexes, value))
        End Function

        Public Overrides Function GetHashCode() As Integer
            Return (((VBIndexSetComplexBinder._hash Xor MyBase.CallInfo.GetHashCode) Xor Me._optimisticSet.GetHashCode) Xor Me._rValueBase.GetHashCode)
        End Function


        ' Fields
        Private Shared ReadOnly _hash As Integer = GetType(VBIndexSetComplexBinder).GetHashCode
        Private ReadOnly _optimisticSet As Boolean
        Private ReadOnly _rValueBase As Boolean
    End Class
End Namespace

