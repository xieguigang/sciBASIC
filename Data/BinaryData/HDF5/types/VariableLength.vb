#Region "Microsoft.VisualBasic::c02eb62e3a9a53ab6ccccb658126de28, Data\BinaryData\HDF5\types\VariableLength.vb"

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
    '    Code Lines: 16
    ' Comment Lines: 0
    '   Blank Lines: 5
    '     File Size: 592 B


    '     Class VariableLength
    ' 
    '         Properties: encoding, paddingType, type, TypeInfo
    ' 
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Text

Namespace type

    Public Class VariableLength : Inherits DataType

        Public Property type As Integer
        Public Property paddingType As Integer
        Public Property encoding As Encoding

        Public Overrides ReadOnly Property TypeInfo As System.Type
            Get
                Return GetType(String)
            End Get
        End Property

        Public Overrides Function ToString() As String
            Return $"({encoding.ToString} {Me.GetType.Name}) {TypeInfo.FullName}"
        End Function
    End Class
End Namespace
