#Region "Microsoft.VisualBasic::4ff408c5f945b9b02f8ac7e6ca3fd3f3, Microsoft.VisualBasic.Core\src\Text\Xml\Models\ValueTuples\PropertyValue.vb"

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

    '   Total Lines: 68
    '    Code Lines: 36 (52.94%)
    ' Comment Lines: 24 (35.29%)
    '    - Xml Docs: 95.83%
    ' 
    '   Blank Lines: 8 (11.76%)
    '     File Size: 2.43 KB


    '     Class PropertyValue
    ' 
    '         Properties: [Property], Key, Value
    ' 
    '         Function: ImportsLines, ImportsTsv, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel.Repository
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel.SchemaMaps
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace Text.Xml.Models

    ''' <summary>
    ''' 用于读写tsv/XML文件格式的键值对数据
    ''' </summary>
    Public Class PropertyValue
        Implements INamedValue, IPropertyValue

        ''' <summary>
        ''' ID
        ''' </summary>
        ''' <returns></returns>
        <XmlAttribute> Public Property Key As String Implements IKeyedEntity(Of String).Key
        ''' <summary>
        ''' Property Name
        ''' </summary>
        ''' <returns></returns>
        <XmlAttribute> Public Property [Property] As String Implements IPropertyValue.Property
        ''' <summary>
        ''' Value text
        ''' </summary>
        ''' <returns></returns>
        <XmlText> Public Property Value As String Implements IPropertyValue.Value

        Public Overrides Function ToString() As String
            Return Me.GetJson
        End Function

        ''' <summary>
        ''' Imports the tsv file like:
        ''' 
        ''' ```
        ''' &lt;ID>&lt;tab>&lt;PropertyName>&lt;tab>&lt;Value>
        ''' ```
        ''' </summary>
        ''' <param name="path$"></param>
        ''' <returns></returns>
        Public Shared Function ImportsTsv(path$, Optional header As Boolean = True) As PropertyValue()
            Dim lines$() = path.ReadAllLines

            If header Then
                lines = lines.Skip(1).ToArray
            End If

            Return ImportsLines(
                data:=lines,
                delimiter:=ASCII.TAB)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function ImportsLines(data As IEnumerable(Of String), Optional delimiter As Char = ASCII.TAB) As PropertyValue()
            Return data _
                .Select(Function(t) t.Split(delimiter)) _
                .Select(Function(row) New PropertyValue With {
                    .Key = row(0),
                    .Property = row(1),
                    .Value = row(2)
                }).ToArray
        End Function
    End Class
End Namespace
