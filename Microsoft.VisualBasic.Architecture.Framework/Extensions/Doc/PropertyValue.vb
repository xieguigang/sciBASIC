#Region "Microsoft.VisualBasic::dc3204c4c2dfef51279ed179b7e1a80b, ..\sciBASIC#\Microsoft.VisualBasic.Architecture.Framework\Extensions\Doc\PropertyValue.vb"

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

Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel.Repository
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Serialization.JSON
Imports Microsoft.VisualBasic.Text

Public Interface IPropertyValue : Inherits INamedValue, Value(Of String).IValueOf
    Property [Property] As String
End Interface

Public Class PropertyValue
    Implements INamedValue
    Implements IPropertyValue

    Public Property Key As String Implements IKeyedEntity(Of String).Key
    Public Property [Property] As String Implements IPropertyValue.Property
    Public Property Value As String Implements IPropertyValue.value

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

