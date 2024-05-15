#Region "Microsoft.VisualBasic::313e3d556a3fb5524e6c53ff0045fef3, Microsoft.VisualBasic.Core\src\My\JavaScript\DataView.vb"

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

    '   Total Lines: 73
    '    Code Lines: 37
    ' Comment Lines: 22
    '   Blank Lines: 14
    '     File Size: 2.38 KB


    '     Class DataView
    ' 
    '         Properties: byteLength
    ' 
    '         Constructor: (+3 Overloads) Sub New
    '         Sub: (+2 Overloads) Dispose
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Runtime.CompilerServices

Namespace My.JavaScript

    Public Class DataView : Implements IDisposable

        ''' <summary>
        ''' The binary data is present in big endian.
        ''' 
        ''' (network byte order)
        ''' </summary>
        Public Const BIG_ENDIAN As UShort = &HFEFF

        ''' <summary>
        ''' The binary data is present in little endian.
        ''' </summary>
        Public Const LITTLE_ENDIAN As UShort = &HFFFE

        Protected stream As Stream

        ''' <summary>
        ''' the <see cref="System.IO.Stream.Length"/>
        ''' </summary>
        ''' <returns></returns>
        Public Overridable ReadOnly Property byteLength As Integer
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return stream.Length
            End Get
        End Property

        Sub New(bytes As Byte())
            stream = New MemoryStream(bytes)
        End Sub

        Sub New(bytes As SByte())
            Call Me.New(CType(CObj(bytes), Byte()))
        End Sub

        Sub New(bytes As Stream)
            Me.stream = bytes
        End Sub

        Private disposedValue As Boolean

        Protected Overridable Sub Dispose(disposing As Boolean)
            If Not disposedValue Then
                If disposing Then
                    ' TODO: dispose managed state (managed objects)
                    Call stream.Dispose()
                End If

                ' TODO: free unmanaged resources (unmanaged objects) and override finalizer
                ' TODO: set large fields to null
                disposedValue = True
            End If
        End Sub

        ' ' TODO: override finalizer only if 'Dispose(disposing As Boolean)' has code to free unmanaged resources
        ' Protected Overrides Sub Finalize()
        '     ' Do not change this code. Put cleanup code in 'Dispose(disposing As Boolean)' method
        '     Dispose(disposing:=False)
        '     MyBase.Finalize()
        ' End Sub

        Public Sub Dispose() Implements IDisposable.Dispose
            ' Do not change this code. Put cleanup code in 'Dispose(disposing As Boolean)' method
            Dispose(disposing:=True)
            GC.SuppressFinalize(Me)
        End Sub
    End Class
End Namespace
