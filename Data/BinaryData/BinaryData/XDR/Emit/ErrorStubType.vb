#Region "Microsoft.VisualBasic::aca7ad23e0d93dd7158815a5910f7457, Data\BinaryData\BinaryData\XDR\Emit\ErrorStubType.vb"

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

    '     Class ErrorStubType
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: ReadMany, ReadOne
    ' 
    '         Sub: WriteMany, WriteOne
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System

Namespace Xdr
    Friend NotInheritable Class ErrorStubType(Of T)
        Public ReadOnly [Error] As Exception

        Public Sub New(ex As Exception)
            [Error] = ex
        End Sub

        Public Function ReadOne(reader As Reader) As T
            Throw [Error]
        End Function

        Public Function ReadMany(reader As Reader, len As UInteger) As T
            Throw [Error]
        End Function

        Public Sub WriteOne(writer As Writer, v As T)
            Throw [Error]
        End Sub

        Public Sub WriteMany(writer As Writer, len As UInteger, v As T)
            Throw [Error]
        End Sub
    End Class
End Namespace

