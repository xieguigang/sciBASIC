#Region "Microsoft.VisualBasic::c4dcd1085592febe3d3a7f0547fa6c19, vs_solutions\dev\VisualStudio\VBProject\CodeDOM\Syntax\VBStatement.vb"

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

    '   Total Lines: 36
    '    Code Lines: 10 (27.78%)
    ' Comment Lines: 19 (52.78%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 7 (19.44%)
    '     File Size: 1.31 KB


    '     Class VBStatement
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace VBProj.CodeDOM.Syntax

    ''' <summary>
    ''' a logical (already line-continued) source line together with the
    ''' xml documentation comment that immediately precedes it.
    ''' </summary>
    Public Class VBStatement

        ''' <summary>
        ''' the physical line (1-based, as shown in an editor) where the
        ''' declaration keyword of this statement starts.
        ''' </summary>
        Public Line As Integer

        ''' <summary>
        ''' the last physical line (1-based) covered by this logical statement
        ''' after line continuation (trailing underscore) merging. Equal to
        ''' <see cref="Line"/> when the statement is a single physical line.
        ''' </summary>
        Public EndLine As Integer

        ''' <summary>
        ''' the earliest physical line (1-based) of the xml documentation
        ''' comment (''') / standalone attribute block (&lt;...&gt;) that
        ''' immediately precedes this statement. Falls back to <see cref="Line"/>
        ''' when there is no leading comment or attribute.
        ''' </summary>
        Public LeadingLine As Integer

        Public Tokens As List(Of Token)
        Public XmlDoc As String
        Public Attributes As New List(Of String)

    End Class

End Namespace
