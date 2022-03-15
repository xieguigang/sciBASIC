#Region "Microsoft.VisualBasic::0959fa9c3b208ff19cb3c79b5ffb3b6d, sciBASIC#\vs_solutions\dev\VisualStudio\sourceMap\sourceMap.vb"

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

    '   Total Lines: 21
    '    Code Lines: 10
    ' Comment Lines: 8
    '   Blank Lines: 3
    '     File Size: 569.00 B


    '     Class sourceMap
    ' 
    '         Properties: file, mappings, names, sourceRoot, sources
    '                     version
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace SourceMap

    Public Class sourceMap

        Public Property version As Integer
        Public Property file As String
        Public Property sourceRoot As String
        ''' <summary>
        ''' the source file path
        ''' </summary>
        ''' <returns></returns>
        Public Property sources As String()
        ''' <summary>
        ''' the symbol names
        ''' </summary>
        ''' <returns></returns>
        Public Property names As String()
        Public Property mappings As String

    End Class
End Namespace
