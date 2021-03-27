#Region "Microsoft.VisualBasic::e8ea3985ba52d2f47bc6f5004d9aa066, Data\BinaryData\BinaryData\XDR\Writing\Writer.vb"

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

    '     Class Writer
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Sub: Write, WriteFix, WriteOption, WriteVar
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System

Namespace Xdr
    Public MustInherit Class Writer
        Public ReadOnly ByteWriter As IByteWriter

        Protected Sub New(writer As IByteWriter)
            ByteWriter = writer
        End Sub

        Public Sub Write(Of T)(item As T)
            Try
                CacheWrite(item)
            Catch ex As SystemException
                Throw MapException.WriteOne(GetType(T), ex)
            End Try
        End Sub

        Protected MustOverride Sub CacheWrite(Of T)(item As T)

        Public Sub WriteFix(Of T)(len As UInteger, item As T)
            Try
                CacheWriteFix(len, item)
            Catch ex As SystemException
                Throw MapException.WriteFix(GetType(T), len, ex)
            End Try
        End Sub

        Protected MustOverride Sub CacheWriteFix(Of T)(len As UInteger, item As T)

        Public Sub WriteVar(Of T)(max As UInteger, item As T)
            Try
                CacheWriteVar(max, item)
            Catch ex As SystemException
                Throw MapException.WriteVar(GetType(T), max, ex)
            End Try
        End Sub

        Protected MustOverride Sub CacheWriteVar(Of T)(max As UInteger, item As T)

        Public Sub WriteOption(Of T As Class)(item As T)
            If item Is Nothing Then
                Write(False)
            Else
                Write(True)
                Write(item)
            End If
        End Sub
    End Class
End Namespace

