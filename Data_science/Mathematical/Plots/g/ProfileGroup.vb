#Region "Microsoft.VisualBasic::18193899bcf15f450fd47ceec4995c47, ..\sciBASIC#\Data_science\Mathematical\Plots\g\ProfileGroup.vb"

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

Imports System.Drawing
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Serialization.JSON

''' <summary>
''' The plot data group
''' </summary>
Public MustInherit Class ProfileGroup

    ''' <summary>
    ''' The color profile of the plot elements
    ''' </summary>
    ''' <returns></returns>
    Public Overridable Property Serials As NamedValue(Of Color)()

    Public Overrides Function ToString() As String
        Return MyClass.GetJson
    End Function
End Class
