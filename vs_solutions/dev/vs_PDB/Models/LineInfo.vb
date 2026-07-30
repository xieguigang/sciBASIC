#Region "Microsoft.VisualBasic::1fa15a85ecafffe4045997842b127906, vs_solutions\dev\vs_PDB\Models\LineInfo.vb"

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

    '   Total Lines: 48
    '    Code Lines: 14 (29.17%)
    ' Comment Lines: 25 (52.08%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 9 (18.75%)
    '     File Size: 1.67 KB


    '     Class LineInfo
    ' 
    '         Properties: Document, EndColumn, EndLine, MethodName, Offset
    '                     StartColumn, StartLine
    ' 
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Models

    ''' <summary>
    ''' A line-number / sequence-point mapping between a method and a source document.
    ''' </summary>
    Public Class LineInfo

        ''' <summary>
        ''' The source document this line range belongs to.
        ''' </summary>
        Public Property Document As SourceDocument

        ''' <summary>
        ''' IL / native offset at which this line range begins (best-effort, 0 when not available).
        ''' </summary>
        Public Property Offset As Long

        ''' <summary>
        ''' Method or function name this line range is part of (best-effort; may be empty for
        ''' classic PDBs that do not carry method names in the line stream).
        ''' </summary>
        Public Property MethodName As String

        ''' <summary>
        ''' 1-based start line in the source document.
        ''' </summary>
        Public Property StartLine As Integer

        ''' <summary>
        ''' 1-based end line in the source document.
        ''' </summary>
        Public Property EndLine As Integer

        ''' <summary>
        ''' Start column (0-based within the line), or 0 when not available.
        ''' </summary>
        Public Property StartColumn As Integer

        ''' <summary>
        ''' End column (0-based within the line), or 0 when not available.
        ''' </summary>
        Public Property EndColumn As Integer

        Public Overrides Function ToString() As String
            Return $"{If(Document?.FilePath, "?")}({StartLine},{StartColumn})-({EndLine},{EndColumn}) {If(MethodName, "")}"
        End Function
    End Class
End Namespace
