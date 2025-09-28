#Region "Microsoft.VisualBasic::256988e892ff2a55e2b537338e1140ad, Microsoft.VisualBasic.Core\src\Extensions\IO\FileModeDescriptor.vb"

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
    '    Code Lines: 10 (20.83%)
    ' Comment Lines: 32 (66.67%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 6 (12.50%)
    '     File Size: 2.58 KB


    '     Enum FileModeDescriptor
    ' 
    ' 
    '  
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.ComponentModel
Imports System.IO

Namespace FileIO

    ''' <summary>
    ''' Specifies how the operating system should open a file.
    ''' </summary>
    Public Enum FileModeDescriptor

        ''' <summary>
        ''' (r) Specifies that the operating system should open an existing file. The ability
        '''     to open the file is dependent on the value specified by the System.IO.FileAccess
        '''     enumeration. A System.IO.FileNotFoundException exception is thrown if the file
        '''     does not exist.
        ''' </summary>
        <Description("r")> Open = FileMode.Open

        ''' <summary>
        ''' (w) Specifies that the operating system should open a file if it exists; otherwise,
        '''     a new file should be created. If the file is opened with FileAccess.Read, System.Security.Permissions.FileIOPermissionAccess.Read
        '''     permission is required. If the file access is FileAccess.Write, System.Security.Permissions.FileIOPermissionAccess.Write
        '''     permission is required. If the file is opened with FileAccess.ReadWrite, both
        '''     System.Security.Permissions.FileIOPermissionAccess.Read and System.Security.Permissions.FileIOPermissionAccess.Write
        '''     permissions are required.
        ''' </summary>
        <Description("w")> OpenOrCreate = FileMode.OpenOrCreate

        ''' <summary>
        ''' (w+) Specifies that the operating system should open an existing file. When the file
        '''     is opened, it should be truncated so that its size is zero bytes. This requires
        '''     System.Security.Permissions.FileIOPermissionAccess.Write permission. Attempts
        '''     to read from a file opened with FileMode.Truncate cause an System.ArgumentException
        '''     exception.
        ''' </summary>
        <Description("w+")> Truncate = FileMode.Truncate

        ''' <summary>
        ''' (a+) Opens the file if it exists and seeks to the end of the file, or creates a new
        '''     file. This requires System.Security.Permissions.FileIOPermissionAccess.Append
        '''     permission. FileMode.Append can be used only in conjunction with FileAccess.Write.
        '''     Trying to seek to a position before the end of the file throws an System.IO.IOException
        '''     exception, and any attempt to read fails and throws a System.NotSupportedException
        '''     exception.
        ''' </summary>
        <Description("a+")> Append = FileMode.Append
    End Enum
End Namespace
