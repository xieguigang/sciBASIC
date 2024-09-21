Imports System.Drawing
Imports System.IO
Imports Microsoft.VisualBasic.Imaging.BitmapImage

Namespace Imaging

#If NET8_0_OR_GREATER Then
    Public MustInherit Class Image

        Public MustOverride ReadOnly Property Size As Size
        Public ReadOnly Property Width As Integer
            Get
                Return Size.Width
            End Get
        End Property
        Public ReadOnly Property Height As Integer
            Get
                Return Size.Height
            End Get
        End Property

        Public Sub Save(s As Stream, format As ImageFormats)
            Throw New NotImplementedException
        End Sub

        Public Shared Function FromStream(s As Stream) As Bitmap
            Throw New NotImplementedException
        End Function
    End Class

    Public Class Bitmap : Inherits Image

        Public Overrides ReadOnly Property Size As Size
            Get
                Return memoryBuffer.Size
            End Get
        End Property

        ReadOnly memoryBuffer As BitmapBuffer

        Sub New(data As BitmapBuffer)
            memoryBuffer = data
        End Sub

    End Class
#End If
End Namespace