#Region "Microsoft.VisualBasic::3112c1a546f0f61f40ce49c58f192baa, ..\sciBASIC#\Microsoft.VisualBasic.Architecture.Framework\Extensions\WebServices\RequestBuilder.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region

Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel.SchemaMaps.DataFrameColumnAttribute
Imports Microsoft.VisualBasic.Language

Namespace Net.Http

    Public Module RequestBuilder

        ''' <summary>
        ''' 当前的这个函数操作里面已经包含有了URL的编码工作了
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="x"></param>
        ''' <returns></returns>
        <Extension>
        Public Function BuildParameters(Of T As Class)(x As T) As String
            Dim type As Type = GetType(T)
            Dim args As New List(Of NamedValue(Of String))
            Dim ps = LoadMapping(type, mapsAll:=True)

            For Each p In ps.Values.Where(Function(o) IsPrimitive(o.member.PropertyType))
                Dim value As Object = p.GetValue(x)

                If Not value Is Nothing Then
                    Dim s As String = If(
                        value.GetType.IsEnum,
                        DirectCast(value, [Enum]).Description,
                        value.ToString)

                    args += New NamedValue(Of String) With {
                        .Value = s,
                        .Name = p.Field.Name
                    }
                End If
            Next

            Dim param As String = args _
                .ToArray(Function(o) $"{o.Name}={o.Value.UrlEncode}") _
                .JoinBy("&")
            Return param
        End Function
    End Module
End Namespace
