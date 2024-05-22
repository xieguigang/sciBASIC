#Region "Microsoft.VisualBasic::a4890cf57fde5e00cded4c2f639b3a07, Data\BinaryData\msgpack\Serialization\SchemaProvider.vb"

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

    '   Total Lines: 63
    '    Code Lines: 31 (49.21%)
    ' Comment Lines: 23 (36.51%)
    '    - Xml Docs: 91.30%
    ' 
    '   Blank Lines: 9 (14.29%)
    '     File Size: 2.31 KB


    '     Class SchemaProvider
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: ReadFile, Write
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Linq
Imports System.Reflection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel

Namespace Serialization

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <remarks>
    ''' 这个模块是为了处理元素类型定义信息和序列化代码调用模块之间没有实际的引用关系的情况
    ''' 例如模块A没有引用messagepack模块，则没有办法添加<see cref="MessagePackMemberAttribute"/>
    ''' 来完成序列化，则这个时候会需要通过这个模块来提供这样的映射关系
    ''' </remarks>
    Public MustInherit Class SchemaProvider(Of T)

        Shared ReadOnly slotList As Dictionary(Of String, PropertyInfo) = DataFramework.Schema(Of T)(
            flag:=PropertyAccess.ReadWrite,
            nonIndex:=True,
            primitive:=False,
            binds:=PublicProperty
        )

        ''' <summary>
        ''' provides a schema table for base object for generates 
        ''' a sequence of <see cref="MessagePackMemberAttribute"/>
        ''' </summary>
        ''' <returns></returns>
        Protected Friend MustOverride Iterator Function GetObjectSchema() As IEnumerable(Of (obj As Type, schema As Dictionary(Of String, NilImplication)))

        Sub New()
            Call MsgPackSerializer.DefaultContext.RegisterSerializer(Me)
        End Sub

        Public Shared Function ReadFile(file As Stream) As T()
            Return MsgPackSerializer.Deserialize(Of T())(file)
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="items"></param>
        ''' <param name="file"></param>
        ''' <returns></returns>
        ''' <remarks>
        ''' data will be auto flush to <paramref name="file"/>.
        ''' </remarks>
        Public Shared Function Write(items As IEnumerable(Of T), file As Stream) As Boolean
            Try
                Call MsgPackSerializer.SerializeObject(items.ToArray, file)
                Call file.Flush()
            Catch ex As Exception
                Call App.LogException(ex)
                Return False
            End Try

            Return True
        End Function

    End Class
End Namespace
