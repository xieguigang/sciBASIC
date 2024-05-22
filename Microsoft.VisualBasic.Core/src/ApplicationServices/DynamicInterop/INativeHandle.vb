#Region "Microsoft.VisualBasic::17120ab45e80b2aa98596ee30e6ebc7d, Microsoft.VisualBasic.Core\src\ApplicationServices\DynamicInterop\INativeHandle.vb"

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

    '   Total Lines: 20
    '    Code Lines: 7 (35.00%)
    ' Comment Lines: 9 (45.00%)
    '    - Xml Docs: 88.89%
    ' 
    '   Blank Lines: 4 (20.00%)
    '     File Size: 800 B


    '     Interface INativeHandle
    ' 
    '         Function: GetHandle
    ' 
    '         Sub: AddRef, Release
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace ApplicationServices.DynamicInterop

    ''' <summary> Interface for native handle.</summary>
    ''' <remarks>This is similar in intent to the BCL SafeHandle, but with release 
    '''          behaviors that are more desirable in particular circumstances.
    '''          </remarks>
    Public Interface INativeHandle : Inherits IDisposable

        ''' <summary> Returns the value of the handle.</summary>
        '''
        ''' <returns> The handle.</returns>
        Function GetHandle() As IntPtr

        ''' <summary>Manually increments the reference counter</summary>
        Sub AddRef()

        ''' <summary>Manually decrements the reference counter. Triggers disposal if count reaches is zero.</summary>
        Sub Release()
    End Interface
End Namespace
