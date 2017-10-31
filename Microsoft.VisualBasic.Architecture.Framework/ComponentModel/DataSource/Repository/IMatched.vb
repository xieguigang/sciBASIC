#Region "Microsoft.VisualBasic::613c372cbc3eb1ee22d3a68bfa97e5d4, ..\sciBASIC#\Microsoft.VisualBasic.Architecture.Framework\ComponentModel\DataSource\IMatched.vb"

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

Namespace ComponentModel.DataSourceModel.Repository

    ''' <summary>
    ''' The object implements on this interface can be matched with some rules.
    ''' </summary>
    Public Interface IMatched

        ''' <summary>
        ''' Is this object matched the condition?
        ''' </summary>
        ''' <returns></returns>
        ReadOnly Property IsMatched As Boolean
    End Interface
End Namespace
