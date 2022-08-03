#Region "Microsoft.VisualBasic::f1b87093e3dae4b826410dffe724fc66, sciBASIC#\Data\BinaryData\DataStorage\HDF5\FileDump.vb"

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
    '    Code Lines: 14
    ' Comment Lines: 7
    '   Blank Lines: 5
    '     File Size: 751 B


    '     Module FileDump
    ' 
    '         Sub: CreateFileDump
    ' 
    '     Interface IFileDump
    ' 
    '         Sub: printValues
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Runtime.CompilerServices
Imports System.Text

Namespace HDF5

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
End Namespace
