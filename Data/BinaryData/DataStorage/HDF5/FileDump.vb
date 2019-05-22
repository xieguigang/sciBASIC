#Region "Microsoft.VisualBasic::c9e97eaf70b6e4ebe19dd5238d04f05e, Data\BinaryData\DataStorage\HDF5\FileDump.vb"

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

Imports System.Runtime.CompilerServices

Namespace HDF5

    <HideModuleName> Public Module FileDump

        <Extension>
        Public Sub CreateFileDump(reader As HDF5Reader, out As System.IO.StringWriter)
            Call DirectCast(reader, IFileDump).printValues(out)
        End Sub
    End Module

    Public Interface IFileDump

        Sub printValues(console As System.IO.StringWriter)
    End Interface
End Namespace
