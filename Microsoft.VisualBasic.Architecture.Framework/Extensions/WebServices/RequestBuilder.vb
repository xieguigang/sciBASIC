#Region "Microsoft.VisualBasic::5d5960f86bbe566247a2e6b7fb182f6a, ..\visualbasic_App\Microsoft.VisualBasic.Architecture.Framework\Extensions\WebServices\RequestBuilder.vb"

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

            For Each p In ps.Values.Where(Function(o) IsPrimitive(o.Property.PropertyType))
                Dim value As Object = p.GetValue(x)

                If Not value Is Nothing Then
                    Dim s As String = If(
                        value.GetType.IsEnum,
                        DirectCast(value, [Enum]).Description,
                        value.ToString)

                    args += New NamedValue(Of String) With {
                        .x = s,
                        .Name = p.Field.Name
                    }
                End If
            Next

            Dim param As String = args _
                .ToArray(Function(o) $"{o.Name}={o.x.UrlEncode}") _
                .JoinBy("&")
            Return param
        End Function
    End Module
End Namespace
