Imports System
Imports System.Reflection.Emit
Imports System.Reflection

Namespace Xdr.Emit
    Public Class BuildBinderDescription
        Public ReadOnly Result As Type
        Public ReadOnly BuildRequest As FieldInfo

        Public Sub New(modBuilder As ModuleBuilder)
            Dim typeBuilder = modBuilder.DefineType("BuildBinder", TypeAttributes.Public Or TypeAttributes.Class Or TypeAttributes.Abstract Or TypeAttributes.Sealed)
            Dim fb_request = typeBuilder.DefineField("Request", GetType(Action(Of Type, OpaqueType)), FieldAttributes.Public Or FieldAttributes.Static)
            Result = typeBuilder.CreateType()
            BuildRequest = fb_request
        End Sub
    End Class
End Namespace
