#Region "Microsoft.VisualBasic::3271e9eeeb902e1fdc2ec9be3a8c3df6, Data\BinaryData\BinaryData\XDR\Reading\Reader.vb"

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

    '     Class Reader
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: Read, ReadFix, ReadOption, ReadVar
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System

Namespace Xdr
    Public MustInherit Class Reader
        Public ReadOnly ByteReader As IByteReader

        Protected Sub New(reader As IByteReader)
            ByteReader = reader
        End Sub

        Public Function Read(Of T)() As T
            Try
                Return CacheRead(Of T)()
            Catch ex As SystemException
                Throw MapException.ReadOne(GetType(T), ex)
            End Try
        End Function

        Protected MustOverride Function CacheRead(Of T)() As T

        Public Function ReadFix(Of T)(len As UInteger) As T
            Try
                Return CacheReadFix(Of T)(len)
            Catch ex As SystemException
                Throw MapException.ReadFix(GetType(T), len, ex)
            End Try
        End Function

        Protected MustOverride Function CacheReadFix(Of T)(len As UInteger) As T

        Public Function ReadVar(Of T)(max As UInteger) As T
            Try
                Return CacheReadVar(Of T)(max)
            Catch ex As SystemException
                Throw MapException.ReadVar(GetType(T), max, ex)
            End Try
        End Function

        Protected MustOverride Function CacheReadVar(Of T)(max As UInteger) As T

        Public Function ReadOption(Of T As Class)() As T
            If Read(Of Boolean)() Then
                Return Read(Of T)()
            Else
                Return Nothing
            End If
        End Function
    End Class
End Namespace

