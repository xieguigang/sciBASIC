#Region "Microsoft.VisualBasic::1652edaed64c8f628f14f8f55ab5c9d1, gr\Microsoft.VisualBasic.Imaging\Drivers\IElementCommentWriter.vb"

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

    '   Total Lines: 12
    '    Code Lines: 5 (41.67%)
    ' Comment Lines: 4 (33.33%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 3 (25.00%)
    '     File Size: 297 B


    '     Interface IElementCommentWriter
    ' 
    '         Sub: SetLastComment
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Driver

    Public Interface IElementCommentWriter

        ''' <summary>
        ''' set comment text to the last graphics element
        ''' </summary>
        ''' <param name="comment"></param>
        Sub SetLastComment(comment As String)

    End Interface
End Namespace
