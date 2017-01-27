#Region "Microsoft.VisualBasic::505abb825744496f1a191fc7a76cba98, ..\sciBASIC#\Microsoft.VisualBasic.Architecture.Framework\Extensions\Image\Wmf.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region

Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Drawing.Imaging

Namespace Imaging

    Public Class Wmf : Implements System.IDisposable

        ReadOnly curMetafile As Metafile
        ReadOnly gSource As Graphics
        ReadOnly gDrawing As Graphics
        ReadOnly hdc As IntPtr

        Public ReadOnly Property Graphics As Graphics
            Get
                Return gDrawing
            End Get
        End Property

        Public ReadOnly Property Size As Size
        Public ReadOnly Property FilePath As String

        Sub New(size As Size, Save As String)
            Dim bitmap As New Bitmap(size.Width, size.Height)
            gSource = Graphics.FromImage(bitmap)
            hdc = gSource.GetHdc()
            curMetafile = New Metafile(Save, hdc)
            gDrawing = Graphics.FromImage(curMetafile)
            gDrawing.SmoothingMode = SmoothingMode.HighQuality

            Me.Size = size
            Me.FilePath = Save
        End Sub

#Region "IDisposable Support"
        Private disposedValue As Boolean ' To detect redundant calls

        ' IDisposable
        Protected Overridable Sub Dispose(disposing As Boolean)
            If Not Me.disposedValue Then
                If disposing Then
                    ' TODO: dispose managed state (managed objects).
                    Call gSource.ReleaseHdc(hdc)
                    Call gDrawing.Dispose()
                    Call gSource.Dispose()
                End If

                ' TODO: free unmanaged resources (unmanaged objects) and override Finalize() below.
                ' TODO: set large fields to null.
            End If
            Me.disposedValue = True
        End Sub

        ' TODO: override Finalize() only if Dispose(disposing As Boolean) above has code to free unmanaged resources.
        'Protected Overrides Sub Finalize()
        '    ' Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
        '    Dispose(False)
        '    MyBase.Finalize()
        'End Sub

        ' This code added by Visual Basic to correctly implement the disposable pattern.
        Public Sub Dispose() Implements IDisposable.Dispose
            ' Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
            Dispose(True)
            ' TODO: uncomment the following line if Finalize() is overridden above.
            ' GC.SuppressFinalize(Me)
        End Sub
#End Region
    End Class
End Namespace
