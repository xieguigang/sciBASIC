Imports System
Imports System.Dynamic
Imports System.Linq.Expressions

Namespace Microsoft.VisualBasic.CompilerServices
    Friend Class VBConversionBinder
        Inherits ConvertBinder
        ' Methods
        Public Sub New(T As Type)
            MyBase.New(T, True)
        End Sub

        Public Overrides Function Equals(_other As Object) As Boolean
            Dim binder As VBConversionBinder = TryCast(_other, VBConversionBinder)
            Return ((Not binder Is Nothing) AndAlso (MyBase.Type Is binder.Type))
        End Function

        Public Overrides Function FallbackConvert(target As DynamicMetaObject, errorSuggestion As DynamicMetaObject) As DynamicMetaObject
            If IDOUtils.NeedsDeferral(target, Nothing, Nothing) Then
                Return MyBase.Defer(target, New DynamicMetaObject(0 - 1) {})
            End If
            If ((Not errorSuggestion Is Nothing) AndAlso Not Conversions.CanUserDefinedConvert(target.Value, MyBase.Type)) Then
                Return errorSuggestion
            End If
            Return New DynamicMetaObject(Expression.Convert(Expression.Call(GetType(Conversions).GetMethod("FallbackUserDefinedConversion"), target.Expression, Expression.Constant(MyBase.Type, GetType(Type))), Me.ReturnType), IDOUtils.CreateRestrictions(target, Nothing, Nothing))
        End Function

        Public Overrides Function GetHashCode() As Integer
            Return (VBConversionBinder._hash Xor MyBase.Type.GetHashCode)
        End Function


        ' Fields
        Private Shared ReadOnly _hash As Integer = GetType(VBConversionBinder).GetHashCode
    End Class
End Namespace

