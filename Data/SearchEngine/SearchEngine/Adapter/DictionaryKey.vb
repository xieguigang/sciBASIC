#Region "Microsoft.VisualBasic::a20517397c69a40615c8be491f50fc38, ..\sciBASIC#\Data\SearchEngine\SearchEngine\Adapter\DictionaryKey.vb"

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
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel

Public Structure DictionaryKey
    Implements IProperty

    Public ReadOnly Property Key As String Implements IReadOnlyId.Identity

    Sub New(name$)
        Key = name
    End Sub

    Public Sub SetValue(target As Object, value As Object) Implements IProperty.SetValue
        DirectCast(target, IDictionary)(Key) = value
    End Sub

    Public Function GetValue(target As Object) As Object Implements IProperty.GetValue
        Return DirectCast(target, IDictionary)(Key)
    End Function

    Public Overrides Function ToString() As String
        Return Key
    End Function
End Structure
