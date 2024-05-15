#Region "Microsoft.VisualBasic::407d1c0bfd6a401fbaae6ebc8bf416c1, Microsoft.VisualBasic.Core\src\Language\Language\C\File.vb"

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

    '   Total Lines: 26
    '    Code Lines: 13
    ' Comment Lines: 9
    '   Blank Lines: 4
    '     File Size: 895 B


    '     Module File
    ' 
    '         Sub: fprintf
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Runtime.CompilerServices

Namespace Language.C

    Public Module File

        ''' <summary>
        ''' Specifies the beginning of a stream.(文件开头)
        ''' </summary>
        Public Const SEEK_SET As SeekOrigin = SeekOrigin.Begin
        ''' <summary>
        ''' Specifies the current position within a stream.(当前位置)
        ''' </summary>
        Public Const SEEK_CUR As SeekOrigin = SeekOrigin.Current
        ''' <summary>
        ''' Specifies the end of a stream.(文件结束)
        ''' </summary>
        Public Const SEEK_END As SeekOrigin = SeekOrigin.End

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub fprintf(fp As StreamWriter, format As String, ParamArray args As Object())
            Call fp.Write(sprintf(format, args))
        End Sub
    End Module
End Namespace
