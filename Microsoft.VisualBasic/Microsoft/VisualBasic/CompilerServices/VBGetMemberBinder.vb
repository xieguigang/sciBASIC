Imports System
Imports System.Dynamic
Imports System.Linq.Expressions

Namespace Microsoft.VisualBasic.CompilerServices
    Friend Class VBGetMemberBinder
        Inherits GetMemberBinder
        ' Methods
        Public Sub New(name As String)
            MyBase.New(name, True)
        End Sub

        Public Overrides Function Equals(_other As Object) As Boolean
            Dim binder As VBGetMemberBinder = TryCast(_other, VBGetMemberBinder)
            Return ((Not binder Is Nothing) AndAlso String.Equals(MyBase.Name, binder.Name))
        End Function

        Public Overrides Function FallbackGetMember(target As DynamicMetaObject, errorSuggestion As DynamicMetaObject) As DynamicMetaObject
            If (Not errorSuggestion Is Nothing) Then
                Return errorSuggestion
            End If
            Return New DynamicMetaObject(Expression.Constant(IDOBinder.missingMemberSentinel), IDOUtils.CreateRestrictions(target, Nothing, Nothing))
        End Function

        Public Overrides Function GetHashCode() As Integer
            Return (VBGetMemberBinder._hash Xor MyBase.Name.GetHashCode)
        End Function


        ' Fields
        Private Shared ReadOnly _hash As Integer = GetType(VBGetMemberBinder).GetHashCode
    End Class
End Namespace

