#Region "Microsoft.VisualBasic::a54222024ab7bcf07fd63ac8e0c5c1d7, sciBASIC#\Data\BinaryData\HDSPack\Extensions.vb"

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

    '   Total Lines: 58
    '    Code Lines: 45
    ' Comment Lines: 0
    '   Blank Lines: 13
    '     File Size: 1.78 KB


    ' Module Extensions
    ' 
    '     Function: ReadText, (+2 Overloads) WriteText
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
                              Optional encoding As Encodings = Encodings.UTF8) As Boolean

        Dim buffer As Stream = pack.OpenBlock(fileName)
        Dim bin As New StreamWriter(buffer, encoding.CodePage)

        For Each line As String In text
            Call bin.WriteLine(line)
        Next

        Call bin.Flush()

        If TypeOf buffer Is StreamBuffer Then
            Call buffer.Dispose()
        End If

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
                              Optional encoding As Encodings = Encodings.UTF8) As Boolean

        Dim buffer As Stream = pack.OpenBlock(fileName)
        Dim bin As New StreamWriter(buffer, encoding.CodePage)

        Call bin.WriteLine(text)
        Call bin.Flush()

        If TypeOf buffer Is StreamBuffer Then
            Call buffer.Dispose()
        End If

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
End Module
