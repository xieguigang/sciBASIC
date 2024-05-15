#Region "Microsoft.VisualBasic::ef2b84ea515f6e67950286caee101be0, Data\BinaryData\HDF5\structure\DataObject.vb"

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

    '   Total Lines: 64
    '    Code Lines: 35
    ' Comment Lines: 14
    '   Blank Lines: 15
    '     File Size: 2.04 KB


    '     Class DataObject
    ' 
    '         Properties: groupMessage, messages
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


Imports System.IO
Imports Microsoft.VisualBasic.Data.IO.HDF5.struct.messages
Imports BinaryReader = Microsoft.VisualBasic.Data.IO.HDF5.device.BinaryReader

Namespace struct

    ''' <summary>
    ''' 一个数据块对象
    ''' </summary>
    Public Class DataObject : Inherits HDF5Ptr

        Dim objectHeader As ObjectHeader

        ''' <summary>
        ''' For instance, a group is an object header that contains a message that points to a 
        ''' local heap and to a B-tree which points to symbol nodes.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property groupMessage As GroupMessage

        Public ReadOnly Property messages As List(Of ObjectHeaderMessage)
            Get
                If Me.objectHeader IsNot Nothing Then
                    Return Me.objectHeader.headerMessages
                End If
                Return Nothing
            End Get
        End Property

        Public Sub New(sb As Superblock, address As Long)
            Call MyBase.New(address)

            Dim [in] As BinaryReader = sb.FileReader(address)

            Me.objectHeader = New ObjectHeader(sb, address)

            For Each msg As ObjectHeaderMessage In Me.objectHeader.headerMessages
                If msg.headerMessageType Is ObjectHeaderMessageType.Group Then
                    Me.groupMessage = msg.groupMessage
                End If
            Next
        End Sub

        Protected Friend Overrides Sub printValues(console As TextWriter)
            Call console.WriteLine("DataObject >>>")
            Call console.WriteLine("address : " & Me.m_address)

            If objectHeader IsNot Nothing Then
                Call objectHeader.printValues(console)
            End If

            Call console.WriteLine("DataObject <<<")
        End Sub
    End Class

End Namespace
