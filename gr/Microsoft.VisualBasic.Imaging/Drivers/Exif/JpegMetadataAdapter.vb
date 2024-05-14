#Region "Microsoft.VisualBasic::cb37be7a57753d64404175d0021b7fba, gr\Microsoft.VisualBasic.Imaging\Drivers\Exif\JpegMetadataAdapter.vb"

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

    '   Total Lines: 165
    '    Code Lines: 113
    ' Comment Lines: 16
    '   Blank Lines: 36
    '     File Size: 6.33 KB


    '     Class JpegMetadataAdapter
    ' 
    '         Properties: Metadata
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: CreateJpegBitmapEncoderWithMetadata, CreateMetadata, ReadMetadata, Save, TryPadAndSave
    '                   TrySave
    ' 
    '         Sub: PadAndSave, SetMetadata
    ' 
    ' 
    ' /********************************************************************************/

#End Region

#If NET48 Then

Imports System.Collections.ObjectModel
Imports System.IO
Imports System.Threading
Imports System.Windows.Media.Imaging

Namespace Driver

    ''' <summary>
    ''' # Exif
    ''' </summary>
    ''' <remarks>
    ''' https://github.com/mwijnands/JpegMetadata
    ''' </remarks>
    Public Class JpegMetadataAdapter

        ReadOnly _filePath$

        Public ReadOnly Property Metadata() As JpegMetadata

        Public Sub New(filePath As String)
            _filePath = filePath
            _Metadata = ReadMetadata(filePath)
        End Sub

        Public Function Save() As Boolean
            Try
                If TrySave(_filePath, _Metadata) Then
                    Return True
                End If

                Return TryPadAndSave(_filePath, _Metadata)
            Catch
                Return False
            End Try
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="filePath"></param>
        ''' <returns></returns>
        ''' <remarks>
        ''' Png is not working for this exif tag data writer
        ''' </remarks>
        Private Function ReadMetadata(filePath As String) As JpegMetadata
            Using jpegStream = New FileStream(filePath, FileMode.Open, FileAccess.Read)
                Dim decoder = BitmapDecoder.Create(jpegStream, BitmapCreateOptions.None, BitmapCacheOption.[Default])

                If Not decoder.CodecInfo.FileExtensions.Contains("jpg") Then
                    Throw New ArgumentException("File is not a JPEG.")
                End If

                Dim jpegFrame = decoder.Frames(0)
                Dim metadata = DirectCast(jpegFrame.Metadata, BitmapMetadata)

                Return CreateMetadata(metadata)
            End Using
        End Function

        Private Function TrySave(filePath As String, metadata As JpegMetadata) As Boolean
            Using jpegStream = New FileStream(filePath, FileMode.Open, FileAccess.ReadWrite)
                Dim decoder = New JpegBitmapDecoder(jpegStream, BitmapCreateOptions.None, BitmapCacheOption.[Default])
                Dim jpegFrame = decoder.Frames(0)
                Dim metadataWriter = jpegFrame.CreateInPlaceBitmapMetadataWriter()

                SetMetadata(metadataWriter, metadata)

                Return metadataWriter.TrySave()
            End Using
        End Function

        Private Function TryPadAndSave(filePath As String, metadata As JpegMetadata) As Boolean
            Dim result As New JpegMetadataSaveResult(filePath, metadata)
            Dim thread As New Thread(Sub() Call PadAndSave(result))

            thread.SetApartmentState(ApartmentState.STA)
            thread.Start()
            thread.Join()

            Return result.IsSuccess
        End Function

        Private Sub PadAndSave(result As JpegMetadataSaveResult)
            Try
                Dim tempFileName As String = Path.GetTempFileName()

                Using jpegStream = New FileStream(result.FilePath, FileMode.Open, FileAccess.Read)
                    Dim decoder = New JpegBitmapDecoder(jpegStream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.None)
                    Dim jpegFrame = decoder.Frames(0)

                    If jpegFrame Is Nothing Then
                        Return
                    End If

                    Dim encoder = CreateJpegBitmapEncoderWithMetadata(jpegFrame, result.Metadata)

                    Using tempFileStream = File.Open(tempFileName, FileMode.Create, FileAccess.ReadWrite)
                        encoder.Save(tempFileStream)
                    End Using
                End Using

                File.Copy(tempFileName, result.FilePath, True)

                Try
                    File.Delete(tempFileName)
                    ' Not a problem if temporary file can't be deleted.
                Catch ex As IOException
                End Try

                result.IsSuccess = True
                ' Ignore exception on this thread and don't set IsSuccess property.
            Catch ex As Exception
            End Try
        End Sub

        Private Function CreateJpegBitmapEncoderWithMetadata(jpegFrame As BitmapFrame, metadata As JpegMetadata) As JpegBitmapEncoder
            Dim frameMetadata = DirectCast(jpegFrame.Metadata, BitmapMetadata)

            If frameMetadata Is Nothing Then
                frameMetadata = New BitmapMetadata("jpeg")
            End If

            Dim metadataCopy = frameMetadata.Clone()
            Dim padding As UInteger = CUInt(4096)

            metadataCopy.SetQuery("/app1/ifd/PaddingSchema:Padding", padding)
            metadataCopy.SetQuery("/app1/ifd/exif/PaddingSchema:Padding", padding)
            metadataCopy.SetQuery("/xmp/PaddingSchema:Padding", padding)

            SetMetadata(metadataCopy, metadata)

            Dim newJpegFrame = BitmapFrame.Create(jpegFrame, jpegFrame.Thumbnail, metadataCopy, jpegFrame.ColorContexts)
            Dim encoder As New JpegBitmapEncoder()

            Call encoder.Frames.Add(newJpegFrame)

            Return encoder
        End Function

        <DebuggerStepThrough>
        Private Shared Function CreateMetadata(metadata As BitmapMetadata) As JpegMetadata
            Return New JpegMetadata() With {
                .Title = metadata.Title Or EmptyString,
                .Subject = metadata.Subject Or EmptyString,
                .Rating = metadata.Rating,
                .Keywords = metadata.Keywords.AsList,
                .Comments = metadata.Comment Or EmptyString,
                .Author = metadata.Author.AsList
            }
        End Function

        <DebuggerStepThrough>
        Private Shared Sub SetMetadata(destination As BitmapMetadata, source As JpegMetadata)
            destination.Title = source.Title
            destination.Subject = source.Subject
            destination.Rating = source.Rating
            destination.Author = New ReadOnlyCollection(Of String)(source.Author)
            destination.Keywords = New ReadOnlyCollection(Of String)(source.Keywords)
            destination.Comment = source.Comments
        End Sub
    End Class
End Namespace
#End If
