#Region "Microsoft.VisualBasic::147cee868ddb3bd494a062b1867e99e0, ..\VisualBasic_AppFramework\DocumentFormats\VB_DataFrame\VB_DataFrame.Extensions\Field.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
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

Imports System.IO
Imports System.Reflection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel.SchemaMaps
Imports Microsoft.VisualBasic.DocumentFormat.Csv.DocumentStream
Imports Microsoft.VisualBasic.DocumentFormat.Csv.StorageProvider
Imports Microsoft.VisualBasic.DocumentFormat.Csv.StorageProvider.ComponentModels
Imports Microsoft.VisualBasic.DocumentFormat.Csv.StorageProvider.Reflection
Imports Microsoft.VisualBasic.Language

''' <summary>
''' + ``#`` uid;
''' + ``[FiledName]`` This field links to a external file, and id is point to the ``#`` uid field in the external file.
''' </summary>
Public Class Field

    ''' <summary>
    ''' Field Name
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property Name As String
        Get
            Return Binding.Name
        End Get
    End Property

    ''' <summary>
    ''' 首先DirectCast为<see cref="IAttributeComponent"/>类型
    ''' </summary>
    ''' <returns></returns>
    Public Property Binding As ComponentModels.StorageProvider
    ''' <summary>
    ''' 假若这个为Nothing，则说明当前的域是简单类型
    ''' </summary>
    ''' <returns></returns>
    Public Property InnerClass As [Class]

    Public Function GetValue(x As Object) As Object
        Return Binding.BindProperty.GetValue(x, Nothing)
    End Function

    Public Overrides Function ToString() As String
        Return Binding.ToString
    End Function
End Class

''' <summary>
''' Class object schema
''' </summary>
Public Class [Class]

    ''' <summary>
    ''' Properties in the class type
    ''' </summary>
    ''' <returns></returns>
    Public Property Fields As Field()
    ''' <summary>
    ''' raw
    ''' </summary>
    ''' <returns></returns>
    Public Property Type As Type

    Public Overrides Function ToString() As String
        Return "Public Class " & Type.FullName
    End Function

    Public Shared Function GetSchema(Of T)() As [Class]
        Return GetSchema(GetType(T))
    End Function

    ''' <summary>
    ''' Property stack
    ''' </summary>
    ''' <returns></returns>
    Public Property Stack As String

    Friend __writer As Writer

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="type"></param>
    ''' <param name="stack">Top stack using identifier ``#``</param>
    ''' <returns></returns>
    Public Shared Function GetSchema(type As Type, Optional stack As String = "#") As [Class]
        Dim props As PropertyInfo() =
            type.GetProperties(BindingFlags.Public + BindingFlags.Instance)
        Dim fields As New List(Of Field)

        For Each prop As PropertyInfo In props
            Dim sp = TypeSchemaProvider.GetInterfaces(prop, False, False)
            Dim cls As [Class] = Nothing

            If sp Is Nothing Then  ' 复杂类型，需要建立外部文件的连接
                Dim pType As Type = prop.PropertyType
                cls = GetSchema(pType, stack & "::" & prop.Name)
                sp = New Column(New ColumnAttribute(prop.Name), prop)
            Else
                ' 简单类型，不需要再做额外域的处理
            End If

            fields += New Field With {
                .Binding = sp,
                .InnerClass = cls
            }
        Next

        Return New [Class] With {
            .Fields = fields,
            .Type = type,
            .Stack = stack
        }
    End Function
End Class
