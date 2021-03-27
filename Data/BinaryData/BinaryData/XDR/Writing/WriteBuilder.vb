#Region "Microsoft.VisualBasic::e686ed45971518f5d34a3e3fd2cb38b7, Data\BinaryData\BinaryData\XDR\Writing\WriteBuilder.vb"

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

    '     Class WriteBuilder
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
    Public NotInheritable Partial Class WriteBuilder
        Private _wm As WriteMapper
        Private _creater As Func(Of IByteWriter, Writer)
        Private _modBuilder As ModuleBuilder
        Private _buildBinderDescription As BuildBinderDescription
        Private _oneCacheDescription As StaticCacheDescription
        Private _varCacheDescription As StaticCacheDescription
        Private _fixCacheDescription As StaticCacheDescription

        Public Sub New()
            Dim name = "DynamicXdrWriteMapper"
            Dim asmName As AssemblyName = New AssemblyName(name)
            Dim asmBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(asmName, AssemblyBuilderAccess.RunAndSave)
            _modBuilder = asmBuilder.DefineDynamicModule(name & ".dll", name & ".dll")
            _buildBinderDescription = New BuildBinderDescription(_modBuilder)
            _oneCacheDescription = New StaticCacheDescription(_modBuilder, _buildBinderDescription, "OneCache", False, OpaqueType.One)
            _fixCacheDescription = New StaticCacheDescription(_modBuilder, _buildBinderDescription, "FixCache", False, OpaqueType.Fix)
            _varCacheDescription = New StaticCacheDescription(_modBuilder, _buildBinderDescription, "VarCache", False, OpaqueType.Var)
            Dim dynWriteMapperType As Type = EmitDynWriteMapper()
            _wm = CType(Activator.CreateInstance(dynWriteMapperType), WriteMapper)
            Dim dynWriterType As Type = EmitDynWriter()
            Dim mapperInstance = dynWriterType.GetField("Mapper", BindingFlags.Public Or BindingFlags.Static)
            mapperInstance.SetValue(Nothing, _wm)
            _creater = EmitCreater(dynWriterType.GetConstructor(New Type() {GetType(IByteWriter)}))
        End Sub

        Private Shared Function EmitCreater(ci As ConstructorInfo) As Func(Of IByteWriter, Writer)
            Dim dm = New DynamicMethod("DynCreateWriter", GetType(Writer), New Type() {GetType(IByteWriter)}, GetType(WriteBuilder), True)
            Dim il = dm.GetILGenerator()
            il.Emit(OpCodes.Ldarg_0)
            il.Emit(OpCodes.Newobj, ci)
            il.Emit(OpCodes.Ret)
            Return CType(dm.CreateDelegate(GetType(Func(Of IByteWriter, Writer))), Func(Of IByteWriter, Writer))
        End Function

        Public Function Map(Of T)(writer As WriteOneDelegate(Of T)) As WriteBuilder
            _wm.AppendMethod(GetType(T), OpaqueType.One, writer)
            Return Me
        End Function

        Public Function MapFix(Of T)(writer As WriteManyDelegate(Of T)) As WriteBuilder
            _wm.AppendMethod(GetType(T), OpaqueType.Fix, writer)
            Return Me
        End Function

        Public Function MapVar(Of T)(writer As WriteManyDelegate(Of T)) As WriteBuilder
            _wm.AppendMethod(GetType(T), OpaqueType.Var, writer)
            Return Me
        End Function

        Public Function Create(writer As IByteWriter) As Writer
            Return _creater(writer)
        End Function
    End Class
End Namespace

