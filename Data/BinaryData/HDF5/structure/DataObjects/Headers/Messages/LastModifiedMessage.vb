#Region "Microsoft.VisualBasic::5cf994ca2e676741dc2d9e377dd4f737, Data\BinaryData\HDF5\structure\DataObjects\Headers\Messages\LastModifiedMessage.vb"

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

    '   Total Lines: 44
    '    Code Lines: 25
    ' Comment Lines: 6
    '   Blank Lines: 13
    '     File Size: 1.28 KB


    '     Class LastModifiedMessage
    ' 
    '         Properties: seconds, version
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: ToString
    ' 
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

Imports System.IO
Imports BinaryReader = Microsoft.VisualBasic.Data.IO.HDF5.device.BinaryReader

Namespace struct.messages

    Public Class LastModifiedMessage : Inherits Message

        Public ReadOnly Property version() As Integer
        Public ReadOnly Property seconds() As Integer

        Public Sub New([in] As BinaryReader, sb As Superblock, address As Long)
            Call MyBase.New(address)

            [in].offset = address

            Me.version = [in].readByte()

            [in].skipBytes(3)

            Me.seconds = [in].readInt()
        End Sub

        Public Overrides Function ToString() As String
            Return $"{MyBase.ToString} {seconds}"
        End Function

        Protected Friend Overrides Sub printValues(console As TextWriter)
            console.WriteLine("LastModifiedMessage >>>")
            console.WriteLine("address : " & Me.m_address)
            console.WriteLine("version : " & Me.version)
            console.WriteLine("seconds : " & Me.seconds)

            console.WriteLine("LastModifiedMessage <<<")
        End Sub
    End Class

End Namespace
