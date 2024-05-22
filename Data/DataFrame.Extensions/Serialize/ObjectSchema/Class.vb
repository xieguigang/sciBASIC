#Region "Microsoft.VisualBasic::492fb063c19575c675b8ca1a0a0dac43, Data\DataFrame.Extensions\Serialize\ObjectSchema\Class.vb"

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

    '   Total Lines: 100
    '    Code Lines: 62 (62.00%)
    ' Comment Lines: 22 (22.00%)
    '    - Xml Docs: 90.91%
    ' 
    '   Blank Lines: 16 (16.00%)
    '     File Size: 3.42 KB


    '     Class [Class]
    ' 
    '         Properties: Fields, Stack, Type
    ' 
    '         Function: GetEnumerator, GetField, (+2 Overloads) GetSchema, IEnumerable_GetEnumerator, ToString
    ' 
    '         Sub: Remove
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Reflection
Imports Microsoft.VisualBasic.Data.csv.StorageProvider.ComponentModels
Imports Microsoft.VisualBasic.Data.csv.StorageProvider.Reflection
Imports Microsoft.VisualBasic.Language

Namespace Serialize.ObjectSchema

    ''' <summary>
    ''' Class object schema
    ''' </summary>
    Public Class [Class] : Implements IEnumerable(Of Field)

        ''' <summary>
        ''' Properties in the class type
        ''' </summary>
        ''' <returns></returns>
        Public Property Fields As Dictionary(Of String, Field)
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

        Public Function GetField(name$) As Field
            If Fields.ContainsKey(name) Then
                Return Fields(name)
            Else
                Return Nothing
            End If
        End Function

        Public Sub Remove(name$)
            If Fields.ContainsKey(name) Then
                Call Fields.Remove(name)
            End If
        End Sub

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
                    sp = Column.CreateObject(New ColumnAttribute(prop.Name), prop)
                Else
                    ' 简单类型，不需要再做额外域的处理
                End If

                fields += New Field With {
                    .Binding = sp,
                    .InnerClass = cls
                }
            Next

            Return New [Class] With {
                .Fields = fields.ToDictionary(Function(k) k.Name),
                .Type = type,
                .Stack = stack
            }
        End Function

        Public Iterator Function GetEnumerator() As IEnumerator(Of Field) Implements IEnumerable(Of Field).GetEnumerator
            For Each f As Field In Fields.Values
                Yield f
            Next
        End Function

        Private Iterator Function IEnumerable_GetEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
            Yield GetEnumerator()
        End Function
    End Class
End Namespace
