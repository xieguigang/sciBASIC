#Region "Microsoft.VisualBasic::cb4d03ae3d9df43b866eb0745a0fa120, ..\VisualBasic_AppFramework\DocumentFormats\VB_DataFrame\VB_DataFrame\StorageProvider\Reflection\Attributes\ColumnAttributes.vb"

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

Imports System.Text

Namespace StorageProvider.Reflection

    ''' <summary>
    ''' This property will not write into the csv document file.
    ''' </summary>
    ''' <remarks></remarks>
    <AttributeUsage(AttributeTargets.Property, AllowMultiple:=False, Inherited:=False)>
    Public Class Ignored : Inherits Attribute

        Public Shared ReadOnly Property TypeInfo As Type = GetType(Ignored)

        Public Overrides Function ToString() As String
            Return "This property is ignored for the reflection operation..."
        End Function
    End Class

    Public Interface IAttributeComponent
        ReadOnly Property ProviderId As ProviderIds
    End Interface

    Public Enum ProviderIds As Integer
        NullMask = -100

        Column = 0
        CollectionColumn
        [Enum]
        MetaAttribute
        ''' <summary>
        ''' 在写入Csv文件之后是以键值对的形式出现的： Name:=value  (例如： GeneId:=XC_1184)
        ''' </summary>
        KeyValuePair
    End Enum
End Namespace
