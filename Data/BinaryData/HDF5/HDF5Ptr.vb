#Region "Microsoft.VisualBasic::a6f7f854d7cbdee1880d40c00279c069, Data\BinaryData\HDF5\HDF5Ptr.vb"

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

    '   Total Lines: 33
    '    Code Lines: 16
    ' Comment Lines: 10
    '   Blank Lines: 7
    '     File Size: 860 B


    ' Class HDF5Ptr
    ' 
    '     Properties: address
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Function: ToString
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO

''' <summary>
''' A internal pointer in a hdf5 binary data file.
''' </summary>
Public MustInherit Class HDF5Ptr : Implements IFileDump

    ''' <summary>
    ''' 当前的这个对象在文件之中的起始位置
    ''' </summary>
    Protected m_address&

    ''' <summary>
    ''' 获取当前的这个对象在文件之中的起始位置
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property address As Long
        Get
            Return m_address
        End Get
    End Property

    Sub New(address&)
        Me.m_address = address
    End Sub

    Public Overrides Function ToString() As String
        Return $"&{address} {Me.GetType.Name}"
    End Function

    Protected Friend MustOverride Sub printValues(console As TextWriter) Implements IFileDump.printValues

End Class
