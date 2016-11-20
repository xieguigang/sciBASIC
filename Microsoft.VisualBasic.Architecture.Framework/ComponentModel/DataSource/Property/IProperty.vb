#Region "Microsoft.VisualBasic::faf8d8d87803f5da3911f172d5fc2a64, ..\sciBASIC#\Microsoft.VisualBasic.Architecture.Framework\ComponentModel\DataSource\Property\IProperty.vb"

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

Namespace ComponentModel.DataSourceModel

    Public Interface IProperty : Inherits IReadOnlyId

        ''' <summary>
        ''' Gets property value from <paramref name="target"/> object.
        ''' </summary>
        ''' <param name="target"></param>
        ''' <returns></returns>
        Function GetValue(target As Object) As Object

        ''' <summary>
        ''' Set <paramref name="value"/> to the property of <paramref name="target"/> object.
        ''' </summary>
        ''' <param name="target"></param>
        ''' <param name="value"></param>
        Sub SetValue(target As Object, value As Object)
    End Interface
End Namespace
