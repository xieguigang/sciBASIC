#Region "Microsoft.VisualBasic::6b4087e1862e5a0af5c252092ad6880a, Data\BinaryData\BinaryData\XDR\Emit\StaticCacheDescription.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



    ' /********************************************************************************/

    ' Summaries:

    '     Class StaticCacheDescription
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: Instance
    ' 
    ' 
    ' /********************************************************************************/

#End Region

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

