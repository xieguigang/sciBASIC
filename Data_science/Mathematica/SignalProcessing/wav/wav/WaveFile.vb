#Region "Microsoft.VisualBasic::322c91349958618537eb41d11b6bfda4, Data_science\Mathematica\SignalProcessing\wav\wav\WaveFile.vb"

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

    '   Total Lines: 104
    '    Code Lines: 36 (34.62%)
    ' Comment Lines: 54 (51.92%)
    '    - Xml Docs: 75.93%
    ' 
    '   Blank Lines: 14 (13.46%)
    '     File Size: 3.76 KB


    ' Class WaveFile
    ' 
    '     Properties: data, fileSize, fmt, format, magic
    ' 
    '     Function: Open
    ' 
    '     Sub: (+2 Overloads) Dispose
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Data.IO

''' <summary>
''' The wav file model
''' </summary>
''' <remarks>
''' Waveform Audio File Format (WAVE, or WAV due to its filename extension; pronounced /wæv/ or /weɪv/) 
''' is an audio file format standard, developed by IBM and Microsoft, for storing an audio bitstream on
''' personal computers. It is the main format used on Microsoft Windows systems for uncompressed audio. 
''' The usual bitstream encoding is the linear pulse-code modulation (LPCM) format.
'''
''' WAV Is an application Of the Resource Interchange File Format (RIFF) bitstream format method For 
''' storing data In chunks, And thus Is similar To the 8SVX And the Audio Interchange File Format (AIFF) 
''' format used On Amiga And Macintosh computers, respectively.
''' </remarks>
Public Class WaveFile : Implements IDisposable

    Private disposedValue As Boolean

    ''' <summary>
    ''' Contains the letters "RIFF" in ASCII form (0x52494646 big-endian form).
    ''' Resource Interchange File Format.
    ''' </summary>
    ''' <returns></returns>
    Public Property magic As String

    ''' <summary>
    ''' ``36 + SubChunk2Size``, or more precisely:
    ''' 
    ''' ```
    ''' 4 + (8 + SubChunk1Size) + (8 + SubChunk2Size)
    ''' ```
    ''' 
    ''' This Is the size of the rest of the chunk 
    ''' following this number.  This Is the size Of the 
    ''' entire file In bytes minus 8 bytes For the
    ''' two fields Not included In this count:
    ''' ChunkID And ChunkSize.
    ''' </summary>
    ''' <returns></returns>
    Public Property fileSize As Integer

    ''' <summary>
    ''' Contains the letters "WAVE" (0x57415645 big-endian form).
    ''' </summary>
    ''' <returns></returns>
    Public Property format As String

    ''' <summary>
    ''' Subchunk1
    ''' </summary>
    ''' <returns></returns>
    Public Property fmt As FMTSubChunk

    ''' <summary>
    ''' Subchunk2
    ''' </summary>
    ''' <returns></returns>
    Public Property data As SampleDataChunk

    Public Shared Function Open(wav As BinaryDataReader, Optional lazy As Boolean = False) As WaveFile
        Dim file As New WaveFile With {
            .magic = wav.ReadString(4),
            .fileSize = wav.ReadInt32,
            .format = wav.ReadString(4),
            .fmt = FMTSubChunk.ParseChunk(wav),
            .data = If(lazy, New LazyDataChunk(wav, .fmt), DataSubChunk.ParseData(wav, format:= .fmt))
        }

        If Not lazy Then
            Call wav.Dispose()
        End If

        Return file
    End Function

    Protected Overridable Sub Dispose(disposing As Boolean)
        If Not disposedValue Then
            If disposing Then
                ' TODO: dispose managed state (managed objects)
                If TypeOf data Is LazyDataChunk Then
                    Call DirectCast(data, LazyDataChunk).Close()
                End If
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
