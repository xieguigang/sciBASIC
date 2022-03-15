#Region "Microsoft.VisualBasic::a20517397c69a40615c8be491f50fc38, sciBASIC#\Data\SearchEngine\SearchEngine\Adapter\DictionaryKey.vb"

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

    '   Total Lines: 24
    '    Code Lines: 18
    ' Comment Lines: 0
    '   Blank Lines: 6
    '     File Size: 745.00 B


    ' Structure DictionaryKey
    ' 
    '     Properties: Key
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    '     Function: GetValue, ToString
    ' 
    '     Sub: SetValue
    ' 
    ' /********************************************************************************/

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
