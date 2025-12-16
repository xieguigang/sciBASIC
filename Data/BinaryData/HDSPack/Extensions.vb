#Region "Microsoft.VisualBasic::296725b55d0cb3cce28ecb9bcacf287a, Data\BinaryData\HDSPack\Extensions.vb"

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

    '   Total Lines: 117
    '    Code Lines: 79 (67.52%)
    ' Comment Lines: 20 (17.09%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 18 (15.38%)
    '     File Size: 4.22 KB


    ' Module Extensions
    ' 
    '     Function: LoadStream, ReadBinary, (+2 Overloads) ReadText, (+2 Overloads) WriteText
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.DataStorage.HDSPack.FileSystem
Imports Microsoft.VisualBasic.Text

<HideModuleName>
Public Module Extensions

    <Extension>
    Public Function WriteText(pack As StreamPack,
                              text As IEnumerable(Of String),
                              fileName As String,
                              Optional encoding As Encodings = Encodings.UTF8,
                              Optional allocate As Boolean = True) As Boolean

        Using buffer As New MemoryStream
            Dim bin As New StreamWriter(buffer, encoding.CodePage)

            For Each line As String In text
                Call bin.WriteLine(line)
            Next

            Call bin.Flush()

            Dim seek As Integer = If(allocate, buffer.Length, -1)
            Dim writeBin = pack.OpenBlock(fileName, buffer_size:=seek)
            writeBin.Write(buffer.ToArray, Scan0, buffer.Length)
            writeBin.Flush()

            If TypeOf writeBin Is StreamBuffer Then
                Call writeBin.Dispose()
            End If
        End Using

        Return True
    End Function

    ''' <summary>
    ''' unsafe write text data, you should check the 
    ''' <paramref name="fileName"/> is exists or not 
    ''' before call this write data function.
    ''' </summary>
    ''' <param name="pack"></param>
    ''' <param name="text"></param>
    ''' <param name="fileName"></param>
    ''' <param name="encoding"></param>
    ''' <returns></returns>
    <Extension>
    Public Function WriteText(pack As StreamPack,
                              text As String,
                              fileName As String,
                              Optional encoding As Encodings = Encodings.UTF8,
                              Optional allocate As Boolean = True) As Boolean

        Using buffer As New MemoryStream
            Dim bin As New StreamWriter(buffer, encoding.CodePage)

            Call bin.WriteLine(text)
            Call bin.Flush()

            Dim seek As Integer = If(allocate, buffer.Length, -1)
            Dim writeBin = pack.OpenBlock(fileName, buffer_size:=seek)
            writeBin.Write(buffer.ToArray, Scan0, buffer.Length)
            writeBin.Flush()

            If TypeOf writeBin Is StreamBuffer Then
                Call writeBin.Dispose()
            End If
        End Using

        Return True
    End Function

    <Extension>
    Public Function ReadText(pack As StreamPack, filename As String, Optional encoding As Encodings = Encodings.UTF8) As String
        If pack.GetObject(filename) Is Nothing Then
            Return Nothing
        Else
            Return New StreamReader(pack.OpenBlock(filename), encoding.CodePage).ReadToEnd
        End If
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Public Function ReadText(pack As StreamPack, file As StreamBlock, Optional encoding As Encodings = Encodings.UTF8) As String
        Return New StreamReader(pack.OpenBlock(file), encoding.CodePage).ReadToEnd
    End Function

    <Extension>
    Public Function LoadStream(pack As StreamPack, file As StreamBlock) As MemoryStream
        Dim s As Stream = pack.OpenBlock(file)
        Dim ms As New MemoryStream
        Call s.CopyTo(ms)
        Call ms.Flush()
        Call ms.Seek(Scan0, SeekOrigin.Begin)
        Return ms
    End Function

    ''' <summary>
    ''' Get in-memory stream buffer data from the archive
    ''' </summary>
    ''' <param name="pack"></param>
    ''' <param name="filename"></param>
    ''' <returns>
    ''' this function will returns nothing if the given resource 
    ''' which is assoctaed with <paramref name="filename"/> is 
    ''' not found inside the package file.
    ''' </returns>
    <Extension>
    Public Function ReadBinary(pack As StreamPack, filename As String) As MemoryStream
        If pack.GetObject(filename) Is Nothing Then
            Return Nothing
        Else
            Return pack.LoadStream(pack.GetObject(filename))
        End If
    End Function
End Module
