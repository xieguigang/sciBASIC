#Region "Microsoft.VisualBasic::41081eac9d8b2576f3b6f8cc89c82140, Data\BinaryData\BinaryData\Stream\BinaryDateTimeFormat.vb"

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

    '   Total Lines: 14
    '    Code Lines: 4 (28.57%)
    ' Comment Lines: 9 (64.29%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 1 (7.14%)
    '     File Size: 423 B


    ' Enum BinaryDateTimeFormat
    ' 
    '     	CTime, 	NetTicks
    ' 
    '  
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

''' <summary>
''' Represents the set of formats of binary date and time encodings.
''' </summary>
Public Enum BinaryDateTimeFormat
	''' <summary>
	''' The <see cref="System.DateTime"/> has the time_t format of the C library.
	''' </summary>
	CTime

	''' <summary>
	''' The <see cref="System.DateTime"/> is stored as the ticks of a .NET <see cref="System.DateTime"/> instance.
	''' </summary>
	NetTicks
End Enum
