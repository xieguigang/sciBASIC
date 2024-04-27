﻿#Region "Microsoft.VisualBasic::d7887a3c36cfe36d63fc0431a8fd9a0d, G:/GCModeller/src/runtime/sciBASIC#/Data/BinaryData/netCDF//Data/VLen.vb"

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

    '   Total Lines: 19
    '    Code Lines: 8
    ' Comment Lines: 6
    '   Blank Lines: 5
    '     File Size: 366 B


    '     Structure vlen_t
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.InteropServices

Namespace Data

    <StructLayout(LayoutKind.Sequential)>
    Public Structure vlen_t

        ''' <summary>
        ''' size_t
        ''' </summary>
        Public len As Integer
        ''' <summary>
        ''' void *
        ''' </summary>
        Public data As IntPtr

    End Structure

End Namespace
