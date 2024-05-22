#Region "Microsoft.VisualBasic::22817113f43af04039fb4db22e8a29ab, Data\BinaryData\msgpack\ObjectExtensions.vb"

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
    '    Code Lines: 12 (85.71%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 2 (14.29%)
    '     File Size: 409 B


    ' Module ObjectExtensions
    ' 
    '     Function: ToMsgPack
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices

<HideModuleName>
Public Module ObjectExtensions

    <Extension()>
    Public Function ToMsgPack(Of T)(obj As T) As Byte()
        If obj Is Nothing Then
            Throw New ArgumentException("Can't serialize null references", NameOf(obj))
        Else
            Return MsgPackSerializer.SerializeObject(obj)
        End If
    End Function
End Module
