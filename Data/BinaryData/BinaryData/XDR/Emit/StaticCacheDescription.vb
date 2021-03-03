Imports System
Imports System.Reflection.Emit
Imports System.Reflection

Namespace Xdr.Emit
    Public Class StaticCacheDescription
        Public ReadOnly Result As Type

        Public Sub New(modBuilder As ModuleBuilder, delegCacheDesc As BuildBinderDescription, name As String, read As Boolean, mType As OpaqueType)
            Dim typeBuilder = modBuilder.DefineType(name, TypeAttributes.Public Or TypeAttributes.Class Or TypeAttributes.Abstract Or TypeAttributes.Sealed)
            Dim genTypeParam = typeBuilder.DefineGenericParameters("T")(0)
            Dim instanceType As Type

            If read Then
                If mType = OpaqueType.One Then
                    instanceType = GetType(ReadOneDelegate(Of))
                Else
                    instanceType = GetType(ReadManyDelegate(Of))
                End If
            Else

                If mType = OpaqueType.One Then
                    instanceType = GetType(WriteOneDelegate(Of))
                Else
                    instanceType = GetType(WriteManyDelegate(Of))
                End If
            End If

            typeBuilder.DefineField("Instance", instanceType.MakeGenericType(genTypeParam), FieldAttributes.Public Or FieldAttributes.Static)
            Dim ctor = typeBuilder.DefineConstructor(MethodAttributes.Static, CallingConventions.Standard, New Type(-1) {})
            Dim il As ILGenerator = ctor.GetILGenerator()
            il.Emit(OpCodes.Ldsfld, delegCacheDesc.BuildRequest)
            il.Emit(OpCodes.Ldtoken, genTypeParam)
            il.Emit(OpCodes.Call, GetType(Type).GetMethod("GetTypeFromHandle"))
            il.Emit(OpCodes.Ldc_I4_S, mType)
            il.Emit(OpCodes.Callvirt, GetType(Action(Of Type, OpaqueType)).GetMethod("Invoke"))
            il.Emit(OpCodes.Ret)
            Result = typeBuilder.CreateType()
        End Sub

        Public Function Instance(genType As Type) As FieldInfo
            Return TypeBuilder.GetField(Result.MakeGenericType(genType), Result.GetField("Instance"))
        End Function
    End Class
End Namespace
