﻿#Region "Microsoft.VisualBasic::51c68e0dfb729482b05636aef2a39119, vs_solutions\dev\VisualStudio\CodeSign\CodeStatistics.vb"

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

    '   Total Lines: 49
    '    Code Lines: 36 (73.47%)
    ' Comment Lines: 5 (10.20%)
    '    - Xml Docs: 80.00%
    ' 
    '   Blank Lines: 8 (16.33%)
    '     File Size: 1.75 KB


    '     Class CodeStatics
    ' 
    '         Properties: [function], [method], [operator], blankLines, classes
    '                     commentLines, lineOfCodes, properties, size, totalLines
    '                     xml_comments
    ' 
    '         Function: StatVB
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Text
Imports Microsoft.VisualBasic.Text

Namespace CodeSign

    Public Class CodeStatics

        Public Property totalLines As Integer
        Public Property commentLines As Integer
        Public Property xml_comments As Integer
        Public Property blankLines As Integer
        Public Property size As Double

        Public ReadOnly Property lineOfCodes As Integer
            Get
                Return totalLines - commentLines - blankLines
            End Get
        End Property

        Public Property classes As Integer
        Public Property [method] As Integer
        Public Property [operator] As Integer
        Public Property [function] As Integer
        Public Property properties As Integer

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="code"></param>
        ''' <returns></returns>
        Public Shared Function StatVB(code As String) As CodeStatics
            Dim lines = code.LineTokens.Select(Function(str) str.Trim(" "c, ASCII.TAB)).ToArray
            Dim stat As New CodeStatics With {
                .totalLines = lines.Length,
                .blankLines = lines.Where(Function(str) str.StringEmpty).Count,
                .commentLines = lines.Where(Function(str) Not str.StringEmpty AndAlso str.StartsWith("'")).Count,
                .size = Encoding.UTF8.GetBytes(code).Length,
                .xml_comments = lines _
                    .Where(Function(str)
                               Return Not str.StringEmpty AndAlso str.StartsWith("''' ")
                           End Function) _
                    .Count
            }

            Return stat
        End Function

    End Class
End Namespace
