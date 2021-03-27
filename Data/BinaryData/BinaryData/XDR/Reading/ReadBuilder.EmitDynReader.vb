#Region "Microsoft.VisualBasic::e7ca7ca1c9f2cd669436be7e5ed4e188, Data\BinaryData\BinaryData\XDR\Reading\ReadBuilder.EmitDynReader.vb"

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

    '     Class ReadBuilder
    ' 
    '         Function: EmitDynReader
    ' 
    '         Sub: EmitOverride_ReadTMany, EmitOverride_ReadTOne
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Reflection
Imports System.Reflection.Emit
Imports Microsoft.VisualBasic.Data.IO.Xdr.Emit

Namespace Xdr
    Public NotInheritable Partial Class ReadBuilder
        Private Function EmitDynReader() As Type
            Dim typeBuilder = _modBuilder.DefineType("DynReader", TypeAttributes.NotPublic Or TypeAttributes.Class Or TypeAttributes.Sealed, GetType(Reader))
            Dim fb_mapperInstance = typeBuilder.DefineField("Mapper", GetType(ReadMapper), FieldAttributes.Public Or FieldAttributes.Static)
            Dim ctor = typeBuilder.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, New Type() {GetType(IByteReader)})
            Dim ilCtor As ILGenerator = ctor.GetILGenerator()
            ilCtor.Emit(OpCodes.Ldarg_0)
            ilCtor.Emit(OpCodes.Ldarg_1) ' reader
            ilCtor.Emit(OpCodes.Call, GetType(Reader).GetConstructor(BindingFlags.Instance Or BindingFlags.NonPublic, Nothing, New Type() {GetType(IByteReader)}, Nothing))
            ilCtor.Emit(OpCodes.Ret)
            EmitOverride_ReadTOne(typeBuilder, fb_mapperInstance)
            EmitOverride_ReadTMany(typeBuilder, "CacheReadFix", _fixCacheDescription, fb_mapperInstance)
            EmitOverride_ReadTMany(typeBuilder, "CacheReadVar", _varCacheDescription, fb_mapperInstance)
            Return typeBuilder.CreateType()
        End Function

        Private Sub EmitOverride_ReadTOne(typeBuilder As TypeBuilder, mapperInstance As FieldInfo)
            Dim miDeclaration = GetType(Reader).GetMethod("CacheRead", BindingFlags.NonPublic Or BindingFlags.Instance)
            Dim mb = typeBuilder.DefineMethod("CacheRead", MethodAttributes.Family Or MethodAttributes.Virtual)
            Dim genTypeParam = mb.DefineGenericParameters("T")(0)
            mb.SetReturnType(genTypeParam)
            typeBuilder.DefineMethodOverride(mb, miDeclaration)
            Dim fi = TypeBuilder.GetField(_oneCacheDescription.Result.MakeGenericType(genTypeParam), _oneCacheDescription.Result.GetField("Instance"))
            Dim il As ILGenerator = mb.GetILGenerator()
            Dim noBuild As Label = il.DefineLabel()
            il.Emit(OpCodes.Ldsfld, fi)
            il.Emit(OpCodes.Brtrue, noBuild)
            il.Emit(OpCodes.Ldsfld, mapperInstance)
            il.Emit(OpCodes.Call, GetType(ReadMapper).GetMethod("BuildCaches", BindingFlags.Public Or BindingFlags.Instance))
            il.MarkLabel(noBuild)
            il.Emit(OpCodes.Ldsfld, fi)
            il.Emit(OpCodes.Ldarg_0) ' this reader
            Dim miInvoke = TypeBuilder.GetMethod(GetType(ReadOneDelegate(Of)).MakeGenericType(genTypeParam), GetType(ReadOneDelegate(Of)).GetMethod("Invoke"))
            il.Emit(OpCodes.Callvirt, miInvoke)
            il.Emit(OpCodes.Ret)
        End Sub

        Private Shared Sub EmitOverride_ReadTMany(tb As TypeBuilder, name As String, readManyCacheDesc As StaticCacheDescription, mapperInstance As FieldInfo)
            Dim miDeclaration = GetType(Reader).GetMethod(name, BindingFlags.NonPublic Or BindingFlags.Instance)
            Dim mb = tb.DefineMethod(name, MethodAttributes.Family Or MethodAttributes.Virtual)
            Dim genTypeParam = mb.DefineGenericParameters("T")(0)
            mb.SetReturnType(genTypeParam)
            mb.SetParameters(GetType(UInteger))
            tb.DefineMethodOverride(mb, miDeclaration)
            Dim fi = readManyCacheDesc.Instance(genTypeParam)
            Dim il As ILGenerator = mb.GetILGenerator()
            Dim noBuild As Label = il.DefineLabel()
            il.Emit(OpCodes.Ldsfld, fi)
            il.Emit(OpCodes.Brtrue, noBuild)
            il.Emit(OpCodes.Ldsfld, mapperInstance)
            il.Emit(OpCodes.Call, GetType(ReadMapper).GetMethod("BuildCaches", BindingFlags.Public Or BindingFlags.Instance))
            il.MarkLabel(noBuild)
            il.Emit(OpCodes.Ldsfld, fi)
            il.Emit(OpCodes.Ldarg_0)  ' this reader
            il.Emit(OpCodes.Ldarg_1) ' len or max
            Dim miInvoke = TypeBuilder.GetMethod(GetType(ReadManyDelegate(Of)).MakeGenericType(genTypeParam), GetType(ReadManyDelegate(Of)).GetMethod("Invoke"))
            il.Emit(OpCodes.Callvirt, miInvoke)
            il.Emit(OpCodes.Ret)
        End Sub
    End Class
End Namespace

