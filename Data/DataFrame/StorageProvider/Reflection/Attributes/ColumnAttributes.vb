#Region "Microsoft.VisualBasic::97664aef72bfb5e9495eb5bdeb8b3752, Data\DataFrame\StorageProvider\Reflection\Attributes\ColumnAttributes.vb"

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

    '   Total Lines: 33
    '    Code Lines: 20 (60.61%)
    ' Comment Lines: 7 (21.21%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 6 (18.18%)
    '     File Size: 1.04 KB


    '     Class Ignored
    ' 
    '         Properties: TypeInfo
    ' 
    '         Function: ToString
    ' 
    '     Interface IAttributeComponent
    ' 
    '         Properties: ProviderId
    ' 
    '     Enum ProviderIds
    ' 
    '         [Enum], CollectionColumn, KeyValuePair, MetaAttribute
    ' 
    '  
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

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
