#Region "Microsoft.VisualBasic::ccaa480beac6ebfb2a2aad8c5825a47d, ..\visualbasic_App\Data\DataFrame\StorageProvider\Reflection\Attributes\Collection.vb"

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

Imports Microsoft.VisualBasic.Language

Namespace StorageProvider.Reflection

    ''' <summary>
    ''' This property is a array data type object.(并不建议使用本Csv属性来储存大量的文本字符串，极容易出错)
    ''' </summary>
    ''' <remarks></remarks>
    <AttributeUsage(AttributeTargets.Property, AllowMultiple:=False, Inherited:=False)>
    Public Class CollectionAttribute : Inherits Csv.StorageProvider.Reflection.ColumnAttribute
        Implements Reflection.IAttributeComponent

        Public ReadOnly Property Delimiter As String

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="Name"></param>
        ''' <param name="Delimiter">由于受正则表达式的解析速度的影响，因为CSV文件是使用逗号进行分隔的，假若使用逗号的话，正则表达式的解析速度会比较低，故在这里优先考虑使用分号来作为分隔符</param>
        Sub New(Name As String, Optional Delimiter As String = "; ")
            Call MyBase.New(Name)
            Me._Delimiter = Delimiter
        End Sub

        Public Overrides Function ToString() As String
            Return Me.Name
        End Function

        Protected Friend Shared Shadows ReadOnly Property TypeInfo As System.Type = GetType(CollectionAttribute)

        Public Function CreateObject(cellData As String) As String()
            Dim Tokens As String() = Strings.Split(cellData, Me._Delimiter)
            Return Tokens
        End Function

        Public Overrides ReadOnly Property ProviderId As ProviderIds Implements IAttributeComponent.ProviderId
            Get
                Return ProviderIds.CollectionColumn
            End Get
        End Property

        ''' <summary>
        ''' Collection of object into a cell string content.
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="source"></param>
        ''' <returns></returns>
        Public Function CreateObject(Of T)(source As IEnumerable(Of T)) As String
            Return CreateObject(source, Delimiter)
        End Function

        Public Shared Function CreateObject(Of T)(source As IEnumerable(Of T), delimiter As String) As String
            If source.IsNullOrEmpty Then
                Return ""
            End If

            Dim s As String() = LinqAPI.Exec(Of String) <=
 _
                From x As T
                In source
                Where Not x Is Nothing
                Select _create = x.ToString

            Return String.Join(delimiter, s)
        End Function
    End Class
End Namespace
