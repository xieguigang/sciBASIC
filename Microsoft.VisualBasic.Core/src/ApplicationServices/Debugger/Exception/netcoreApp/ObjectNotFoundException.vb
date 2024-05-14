#Region "Microsoft.VisualBasic::5b2ddc726abf201ae278e00a4d4a0d21, Microsoft.VisualBasic.Core\src\ApplicationServices\Debugger\Exception\netcoreApp\ObjectNotFoundException.vb"

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

    '   Total Lines: 9
    '    Code Lines: 7
    ' Comment Lines: 0
    '   Blank Lines: 2
    '     File Size: 239 B


    '     Class ObjectNotFoundException
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace ApplicationServices.Debugging.Diagnostics

    Public Class ObjectNotFoundException : Inherits Exception

        Sub New(message As String)
            Call MyBase.New(message)
        End Sub
    End Class
End Namespace
