Imports System
Imports System.Dynamic
Imports System.Linq.Expressions

Namespace Microsoft.VisualBasic.CompilerServices
    Friend Class VBIndexSetBinder
        Inherits SetIndexBinder
        ' Methods
        Public Sub New(CallInfo As CallInfo)
            MyBase.New(CallInfo)
        End Sub

        Public Overrides Function Equals(_other As Object) As Boolean
            Dim binder As VBIndexSetBinder = TryCast(_other, VBIndexSetBinder)
            Return ((Not binder Is Nothing) AndAlso MyBase.CallInfo.Equals(binder.CallInfo))
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
            If ((Not errorSuggestion Is Nothing) AndAlso Not NewLateBinding.CanIndexSetComplex(target.Value, array, argNames, False, False)) Then
                Return errorSuggestion
            End If
            Dim expression As Expression = IDOUtils.ConvertToObject(value.Expression)
            Dim expressionArray2 As Expression() = New Expression((args.Length + 1) - 1) {}
            args.CopyTo(expressionArray2, 0)
            expressionArray2(args.Length) = expression
            Return New DynamicMetaObject(Expression.Block(Expression.Call(GetType(NewLateBinding).GetMethod("FallbackIndexSet"), target.Expression, Expression.NewArrayInit(GetType(Object), expressionArray2), Expression.Constant(argNames, GetType(String()))), expression), IDOUtils.CreateRestrictions(target, packedIndexes, value))
        End Function

        Public Overrides Function GetHashCode() As Integer
            Return (VBIndexSetBinder._hash Xor MyBase.CallInfo.GetHashCode)
        End Function


        ' Fields
        Private Shared ReadOnly _hash As Integer = GetType(VBIndexSetBinder).GetHashCode
    End Class
End Namespace

