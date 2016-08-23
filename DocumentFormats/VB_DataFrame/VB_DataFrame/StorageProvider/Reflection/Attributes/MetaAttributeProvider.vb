#Region "Microsoft.VisualBasic::6c089df00d1865fac5d0d9e21307d106, ..\visualbasic_App\DocumentFormats\VB_DataFrame\VB_DataFrame\StorageProvider\Reflection\Attributes\MetaAttributeProvider.vb"

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

Imports System.Text

Namespace StorageProvider.Reflection

    ''' <summary>
    ''' 在执行解析操作的时候，所有的没有被序列化的属性都将会被看作为字典元素，该字典元素的数据都存储在这个属性值之中
    ''' </summary>
    <AttributeUsage(AttributeTargets.Property, AllowMultiple:=False, Inherited:=False)>
    Public Class MetaAttribute : Inherits Attribute
        Implements Reflection.IAttributeComponent

        ''' <summary>
        ''' The value type of the value slot in the meta attribute dictionary.(被序列化的对象之中的元数据的字典的值的类型)
        ''' </summary>
        ''' <returns></returns>
        Public Overloads ReadOnly Property TypeId As System.Type

        Public ReadOnly Property ProviderId As ProviderIds Implements IAttributeComponent.ProviderId
            Get
                Return ProviderIds.MetaAttribute
            End Get
        End Property

        ''' <summary>
        ''' 在执行解析操作的时候，所有的没有被序列化的属性都将会被看作为字典元素，该字典元素的数据都存储在这个属性值之中
        ''' </summary>
        ''' <param name="Type">The value type of the value slot in the meta attribute dictionary.(被序列化的对象之中的元数据的字典的值的类型)</param>
        Sub New(Optional Type As System.Type = Nothing)
            TypeId = If(Type Is Nothing, GetType(String), Type)
        End Sub

        Public Overrides Function ToString() As String
            Return TypeId.FullName
        End Function
    End Class
End Namespace
