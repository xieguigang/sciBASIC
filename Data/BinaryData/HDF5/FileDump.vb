#Region "Microsoft.VisualBasic::7ba0e559571e994ae0de9f8fb1ae7235, Data\BinaryData\HDF5\FileDump.vb"

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

    '   Total Lines: 23
    '    Code Lines: 12 (52.17%)
    ' Comment Lines: 7 (30.43%)
    '    - Xml Docs: 85.71%
    ' 
    '   Blank Lines: 4 (17.39%)
    '     File Size: 654 B


    ' Module FileDump
    ' 
    '     Sub: CreateFileDump
    ' 
    ' Interface IFileDump
    ' 
    '     Sub: printValues
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Runtime.CompilerServices
Imports System.Text

<HideModuleName> Public Module FileDump

    <Extension>
    Public Sub CreateFileDump(obj As IFileDump, out As TextWriter)
        Call obj.printValues(out)
    End Sub
End Module

Public Interface IFileDump

    ''' <summary>
    ''' 可以通过下面的两种方法构建出所需要的<paramref name="console"/>参数
    ''' 
    ''' + <see cref="StringBuilder"/> => new <see cref="TextWriter"/>
    ''' + <see cref="StreamWriter"/>
    ''' </summary>
    ''' <param name="console"></param>
    Sub printValues(console As TextWriter)
End Interface
