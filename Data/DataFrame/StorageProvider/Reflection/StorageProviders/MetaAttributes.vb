#Region "Microsoft.VisualBasic::070ae830566ae8dd763b2959b59f62e5, Data\DataFrame\StorageProvider\Reflection\StorageProviders\MetaAttributes.vb"

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


    ' Code Statistics:

    '   Total Lines: 81
    '    Code Lines: 53 (65.43%)
    ' Comment Lines: 16 (19.75%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 12 (14.81%)
    '     File Size: 3.66 KB


    '     Module MetaAttributeParser
    ' 
    '         Function: GetEntry, LoadData, MakeDictionaryType
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Reflection
Imports Microsoft.VisualBasic.Data.Framework.StorageProvider.ComponentModels
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq

Namespace StorageProvider.Reflection

    ''' <summary>
    ''' 解析出字典域标记信息
    ''' </summary>
    Public Module MetaAttributeParser

        Public Function GetEntry(type As Type) As ComponentModels.MetaAttribute
            Dim attrEntry As Type = GetType(Reflection.MetaAttribute)
            Dim metaAttr = LinqAPI.DefaultFirst(Of ComponentModels.MetaAttribute) _
 _
                () <= From prop As PropertyInfo
                      In type.GetProperties(BindingFlags.Public Or BindingFlags.Instance)
                      Let attrs As Object() = prop.GetCustomAttributes(attrEntry, inherit:=True)
                      Where Not attrs.IsNullOrEmpty
                      Let mattr As MetaAttribute = DirectCast(attrs.First, MetaAttribute)
                      Select New ComponentModels.MetaAttribute(mattr, prop)

            Return metaAttr
        End Function

        ''' <summary>
        ''' 将csv文档里面的数据加载进入对象数组的字典属性之中
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="buf"></param>
        ''' <param name="DataSource"></param>
        ''' <param name="Schema"></param>
        ''' <returns></returns>
        Public Function LoadData(Of T As Class)(buf As IEnumerable(Of T), DataSource As DynamicObjectLoader(), Schema As SchemaProvider) As IEnumerable(Of T)
            If Not Schema.HasMetaAttributes Then
                Return buf
            End If

            Dim MetaEntry As ComponentModels.MetaAttribute = Schema.MetaAttributes
            Dim type As Type = MetaEntry.MetaAttribute.TypeId
            Dim typeCast As Scripting.LoadObject = Scripting.CasterString(type)
            Dim attrs = LinqAPI.Exec(Of KeyValuePair(Of String, Integer)) _
 _
                () <= From Name As KeyValuePair(Of String, Integer)
                      In DataSource.First.Schema
                      Where Not String.IsNullOrEmpty(Name.Key) AndAlso
                          Not Schema.ContainsField(Name.Key)
                      Select Name

            Dim hashType As Type = MakeDictionaryType(type)
            Dim o As Object

            For Each x As SeqValue(Of T) In buf.SeqIterator
                Dim hash As IDictionary =
                    DirectCast(Activator.CreateInstance(hashType), IDictionary)
                Dim source As DynamicObjectLoader = DataSource(x.i)

                For Each attrName As KeyValuePair(Of String, Integer) In attrs
                    o = typeCast(source.GetValue(attrName.Value))
                    Call hash.Add(attrName.Key, o)
                Next

                Call MetaEntry.BindProperty.SetValue(x.value, hash, Nothing)
            Next

            Return buf
        End Function

        ''' <summary>
        ''' Function returns type of <see cref="Dictionary(Of String, ValueType)"/>
        ''' </summary>
        ''' <param name="ValueType">Type of the value in the dictionary, and the key type is <see cref="String"/></param>
        ''' <returns></returns>
        Public Function MakeDictionaryType(ValueType As Type) As Type
            Dim GenericType As Type = GetType(Dictionary(Of ,)) ' Type.GetType("System.Collections.Generic.IEnumerable")
            GenericType = GenericType.MakeGenericType({GetType(String), ValueType})
            Return GenericType
        End Function
    End Module
End Namespace
