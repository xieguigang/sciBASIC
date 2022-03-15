#Region "Microsoft.VisualBasic::79b2798fa052d1d45c811bd4bea65f3b, sciBASIC#\Data\SearchEngine\SearchEngine\QueryArgument.vb"

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

    '   Total Lines: 35
    '    Code Lines: 13
    ' Comment Lines: 17
    '   Blank Lines: 5
    '     File Size: 920.00 B


    ' Class QueryArgument
    ' 
    '     Properties: Data, Expression, Name
    ' 
    '     Function: Compile, ToString
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.Serialization.JSON

Public Class QueryArgument : Implements INamedValue

    Public Property Name As String Implements INamedValue.Key
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
