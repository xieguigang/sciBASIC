#Region "Microsoft.VisualBasic::04495aeb2ec95bc163bff7ad36f3acb8, Data\BinaryData\BinaryData\XDR\Reading\ReadBuilder.vb"

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
    '         Constructor: (+1 Overloads) Sub New
    '         Function: Create, EmitCreater, Map, MapFix, MapVar
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Reflection
Imports System.Reflection.Emit
Imports Microsoft.VisualBasic.Data.IO.Xdr.Emit

Namespace Xdr
    Public NotInheritable Partial Class ReadBuilder
        Private _rm As ReadMapper
        Private _creater As Func(Of IByteReader, Reader)
        Private _modBuilder As ModuleBuilder
        Private _buildBinderDescription As BuildBinderDescription
        Private _oneCacheDescription As StaticCacheDescription
        Private _varCacheDescription As StaticCacheDescription
        Private _fixCacheDescription As StaticCacheDescription

        Public Sub New()
            Dim name = "DynamicXdrReadMapper"
            Dim asmName As AssemblyName = New AssemblyName(name)
            Dim asmBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(asmName, AssemblyBuilderAccess.RunAndSave)
            _modBuilder = asmBuilder.DefineDynamicModule(name & ".dll", name & ".dll")
            _buildBinderDescription = New BuildBinderDescription(_modBuilder)
            _oneCacheDescription = New StaticCacheDescription(_modBuilder, _buildBinderDescription, "OneCache", True, OpaqueType.One)
            _fixCacheDescription = New StaticCacheDescription(_modBuilder, _buildBinderDescription, "FixCache", True, OpaqueType.Fix)
            _varCacheDescription = New StaticCacheDescription(_modBuilder, _buildBinderDescription, "VarCache", True, OpaqueType.Var)
            Dim dynReadMapperType As Type = EmitDynReadMapper()
            _rm = CType(Activator.CreateInstance(dynReadMapperType), ReadMapper)
            Dim dynReaderType As Type = EmitDynReader()
            Dim mapperInstance = dynReaderType.GetField("Mapper", BindingFlags.Public Or BindingFlags.Static)
            mapperInstance.SetValue(Nothing, _rm)
            _creater = EmitCreater(dynReaderType.GetConstructor(New Type() {GetType(IByteReader)}))
        End Sub

        Private Shared Function EmitCreater(ci As ConstructorInfo) As Func(Of IByteReader, Reader)
            Dim dm = New DynamicMethod("DynCreateReader", GetType(Reader), New Type() {GetType(IByteReader)}, GetType(ReadBuilder), True)
            Dim il = dm.GetILGenerator()
            il.Emit(OpCodes.Ldarg_0)
            il.Emit(OpCodes.Newobj, ci)
            il.Emit(OpCodes.Ret)
            Return CType(dm.CreateDelegate(GetType(Func(Of IByteReader, Reader))), Func(Of IByteReader, Reader))
        End Function

        Public Function Map(Of T)(reader As ReadOneDelegate(Of T)) As ReadBuilder
            _rm.AppendMethod(GetType(T), OpaqueType.One, reader)
            Return Me
        End Function

        Public Function MapFix(Of T)(reader As ReadManyDelegate(Of T)) As ReadBuilder
            _rm.AppendMethod(GetType(T), OpaqueType.Fix, reader)
            Return Me
        End Function

        Public Function MapVar(Of T)(reader As ReadManyDelegate(Of T)) As ReadBuilder
            _rm.AppendMethod(GetType(T), OpaqueType.Var, reader)
            Return Me
        End Function

        Public Function Create(reader As IByteReader) As Reader
            Return _creater(reader)
        End Function
    End Class
End Namespace

