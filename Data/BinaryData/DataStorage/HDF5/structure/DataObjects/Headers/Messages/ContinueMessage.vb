#Region "Microsoft.VisualBasic::3471a655df03a64dbafa7b1a7b49aa82, Data\BinaryData\DataStorage\HDF5\structure\DataObjects\Headers\Messages\ContinueMessage.vb"

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

    '     Class ContinueMessage
    ' 
    '         Properties: length, offset, totalObjectHeaderMessageContinueSize
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Sub: printValues
    ' 
    ' 
    ' /********************************************************************************/

#End Region

'
' * Mostly copied from NETCDF4 source code.
' * refer : http://www.unidata.ucar.edu
' * 
' * Modified by iychoi@email.arizona.edu
' 


Imports Microsoft.VisualBasic.Data.IO.HDF5.IO

Namespace HDF5.[Structure]


    Public Class ContinueMessage : Inherits Message

        Public ReadOnly Property offset As Long
        Public ReadOnly Property length As Long
        Public ReadOnly Property totalObjectHeaderMessageContinueSize As Integer

        Public Sub New([in] As BinaryReader, sb As Superblock, address As Long)
            Call MyBase.New(address)

            [in].offset = address

            Me.offset = ReadHelper.readO([in], sb)
            Me.length = ReadHelper.readL([in], sb)
            Me.totalObjectHeaderMessageContinueSize = sb.sizeOfOffsets + sb.sizeOfLengths
        End Sub

        Public Overridable Sub printValues()
            Console.WriteLine("ObjectHeaderMessageContinue >>>")
            Console.WriteLine("address : " & Me.m_address)
            Console.WriteLine("offset : " & Me.offset)
            Console.WriteLine("length : " & Me.length)
            Console.WriteLine("total header message continue size : " & Me.totalObjectHeaderMessageContinueSize)
            Console.WriteLine("ObjectHeaderMessageContinue <<<")
        End Sub
    End Class

End Namespace
