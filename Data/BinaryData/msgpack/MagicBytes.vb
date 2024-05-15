#Region "Microsoft.VisualBasic::b9454b6b28d428b6a9a3a76ef6a80360, Data\BinaryData\msgpack\MagicBytes.vb"

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

    '   Total Lines: 21
    '    Code Lines: 18
    ' Comment Lines: 0
    '   Blank Lines: 3
    '     File Size: 689 B


    ' Module MagicBytes
    ' 
    '     Function: IsArray, TypeOfMagic
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Data.IO.MessagePack.Constants

Public Module MagicBytes

    Public Function IsArray(magic As Byte) As Boolean
        If magic > FixedArray.MIN AndAlso magic <= FixedArray.MIN + 15 Then
            Return True
        Else
            Throw New NotImplementedException
        End If
    End Function

    Public Function TypeOfMagic(magic As Byte) As Type
        Select Case magic
            Case MsgPackFormats.DOUBLE : Return GetType(Double)
            Case MsgPackFormats.FLOAT_32 : Return GetType(Single)
            Case Else
                Throw New NotImplementedException(magic)
        End Select
    End Function
End Module
