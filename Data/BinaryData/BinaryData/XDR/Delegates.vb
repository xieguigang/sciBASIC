#Region "Microsoft.VisualBasic::f212c520101918e343e9cc664a31306a, Data\BinaryData\BinaryData\XDR\Delegates.vb"

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

    '     Delegate Function
    ' 
    ' 
    '     Delegate Function
    ' 
    ' 
    '     Delegate Sub
    ' 
    ' 
    '     Delegate Sub
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region


Namespace Xdr
    Public Delegate Function ReadOneDelegate(Of T)(reader As Reader) As T
    Public Delegate Function ReadManyDelegate(Of T)(reader As Reader, len As UInteger) As T
    Public Delegate Sub WriteOneDelegate(Of T)(writer As Writer, item As T)
    Public Delegate Sub WriteManyDelegate(Of T)(writer As Writer, len As UInteger, item As T)
End Namespace

