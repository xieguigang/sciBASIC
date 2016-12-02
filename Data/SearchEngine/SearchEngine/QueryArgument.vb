#Region "Microsoft.VisualBasic::0f23770ef2e93298742f271153280c50, ..\sciBASIC#\Data\SearchEngine\SearchEngine\QueryArgument.vb"

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
Imports Microsoft.VisualBasic.Serialization.JSON

Public Class QueryArgument : Implements sIdEnumerable

    Public Property Name As String Implements sIdEnumerable.Identifier
    ''' <summary>
    ''' 
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks>
    ''' Example: 
    ''' 
    ''' ```
    ''' #\d+
    ''' #\d+ AND Name:'#\s{3,}' TOP 100
    ''' value:'#[a-zA-Z]{5,}' LIMIT 10
    ''' ```
    ''' </remarks>
    Public Property Expression As String

    ''' <summary>
    ''' The additional extension data.
    ''' </summary>
    ''' <returns></returns>
    Public Property Data As Dictionary(Of String, String)

    Public Function Compile() As Expression
        Return Build(Expression)
    End Function

    Public Overrides Function ToString() As String
        Return Me.GetJson
    End Function
End Class
