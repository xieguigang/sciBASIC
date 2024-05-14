#Region "Microsoft.VisualBasic::0965289e3eaf0038712d540a7382413e, Microsoft.VisualBasic.Core\src\Text\IO\TextFileEncodingDetector.vb"

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

    '   Total Lines: 406
    '    Code Lines: 224
    ' Comment Lines: 123
    '   Blank Lines: 59
    '     File Size: 21.10 KB


    '     Module TextFileEncodingDetector
    ' 
    '         Properties: TextCodings
    ' 
    '         Function: DetectBOMBytes, DetectSuspiciousUTF8SequenceLength, DetectTextByteArrayEncoding, (+2 Overloads) DetectTextFileEncoding, DetectUnicodeInByteSampleByHeuristics
    '                   IsCommonUSASCIIByte, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Text
Imports System.IO
Imports System.Text.RegularExpressions

Namespace Text

    ''' <summary>
    ''' Encoding fileEncoding = TextFileEncodingDetector.DetectTextFileEncoding("you file path",Encoding.Default);
    ''' </summary>
    ''' <remarks></remarks>
    Public Module TextFileEncodingDetector

        Public ReadOnly Property TextCodings As Dictionary(Of String, System.Text.Encoding) =
            New Dictionary(Of String, Encoding) From {
 _
            {"ascii", System.Text.Encoding.ASCII},
            {"unicode", System.Text.Encoding.Unicode},
            {"utf8", System.Text.Encoding.UTF8}
        }

        Public Function ToString(data As Generic.IEnumerable(Of Byte), Optional encoding As String = "") As String
            If Not String.IsNullOrEmpty(encoding) Then encoding = encoding.ToLower
            Dim TextEncoding As System.Text.Encoding = If(String.IsNullOrEmpty(encoding) OrElse Not TextCodings.ContainsKey(encoding), System.Text.Encoding.Default, TextCodings(encoding))
            Return TextEncoding.GetString(data.ToArray)
        End Function

        '
        '* Simple class to handle text file encoding woes (in a primarily English-speaking tech
        '* world).
        '*
        '* - This code is fully managed, no shady calls to MLang (the unmanaged codepage
        '* detection library originally developed for Internet Explorer).
        '*
        '* - This class does NOT try to detect arbitrary codepages/charsets, it really only
        '* aims to differentiate between some of the most common variants of Unicode
        '* encoding, and a "default" (western / ascii-based) encoding alternative provided
        '* by the caller.
        '*
        '* - As there is no "Reliable" way to distinguish between UTF-8 (without BOM) and
        '* Windows-1252 (in .Net, also incorrectly called "ASCII") encodings, we use a
        '* heuristic - so the more of the file we can sample the better the guess. If you
        '* are going to read the whole file into memory at some point, then best to pass
        '* in the whole byte byte array directly. Otherwise, decide how to trade off
        '* reliability against performance / memory usage.
        '*
        '* - The UTF-8 detection heuristic only works for western text, as it relies on
        '* the presence of UTF-8 encoded accented and other characters found in the upper
        '* ranges of the Latin-1 and (particularly) Windows-1252 codepages.
        '*
        '* - For more general detection routines, see existing projects / resources:
        '* - MLang - Microsoft library originally for IE6, available in Windows XP and later APIs now (I think?)
        '* - MLang .Net bindings: http://www.codeproject.com/KB/recipes/DetectEncoding.aspx
        '* - CharDet - Mozilla browser's detection routines
        '* - Ported to Java then .Net: http://www.conceptdevelopment.net/Localization/NCharDet/
        '* - Ported straight to .Net: http://code.google.com/p/chardetsharp/source/browse
        '*
        '* Copyright Tao Klerks, Jan 2010, tao@klerks.biz
        '* Licensed under the modified BSD license:
        '*
        '
        'Redistribution and use in source and binary forms, with or without modification, are
        'permitted provided that the following conditions are met:
        '
        '- Redistributions of source code must retain the above copyright notice, this list of
        'conditions and the following disclaimer.
        '- Redistributions in binary form must reproduce the above copyright notice, this list
        'of conditions and the following disclaimer in the documentation and/or other materials
        'provided with the distribution.
        '- The name of the author may not be used to endorse or promote products derived from
        'this software without specific prior written permission.
        '
        'THIS SOFTWARE IS PROVIDED BY THE AUTHOR ``AS IS'' AND ANY EXPRESS OR IMPLIED WARRANTIES,
        'INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR
        'A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR ANY
        'DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING,
        'BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR
        'PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY,
        'WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
        'ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY
        'OF SUCH DAMAGE.
        '
        '*
        '

        Const _defaultHeuristicSampleSize As Long = &H10000

        ''' <summary>
        ''' completely arbitrary - inappropriate for high numbers of files / high speed requirements
        ''' </summary>
        ''' <param name="InputFilename"></param>
        ''' <param name="DefaultEncoding"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function DetectTextFileEncoding(InputFilename As String, Optional DefaultEncoding As Encoding = Nothing) As Encoding
            Using textfileStream As FileStream = IO.File.OpenRead(InputFilename)
                If DefaultEncoding Is Nothing Then
                    DefaultEncoding = System.Text.Encoding.Default
                End If
                Return DetectTextFileEncoding(textfileStream, DefaultEncoding, _defaultHeuristicSampleSize)
            End Using
        End Function

        Public Function DetectTextFileEncoding(InputFileStream As FileStream, DefaultEncoding As Encoding, HeuristicSampleSize As Long) As Encoding
            If InputFileStream Is Nothing Then
                Throw New ArgumentNullException("Must provide a valid Filestream!", "InputFileStream")
            End If

            If Not InputFileStream.CanRead Then
                Throw New ArgumentException("Provided file stream is not readable!", "InputFileStream")
            End If

            If Not InputFileStream.CanSeek Then
                Throw New ArgumentException("Provided file stream cannot seek!", "InputFileStream")
            End If

            Dim encodingFound As Encoding = Nothing
            Dim originalPos As Long = InputFileStream.Position

            InputFileStream.Position = 0

            'First read only what we need for BOM detection

            Dim bomBytes As Byte() = New Byte(If(InputFileStream.Length > 4, 4, InputFileStream.Length) - 1) {}
            InputFileStream.Read(bomBytes, 0, bomBytes.Length)

            encodingFound = DetectBOMBytes(bomBytes)

            If encodingFound IsNot Nothing Then
                InputFileStream.Position = originalPos
                Return encodingFound
            End If

            'BOM Detection failed, going for heuristics now.
            ' create sample byte array and populate it
            Dim sampleBytes As Byte() = New Byte(If(HeuristicSampleSize > InputFileStream.Length, InputFileStream.Length, HeuristicSampleSize) - 1) {}

            Call Array.Copy(bomBytes, sampleBytes, bomBytes.Length)

            If InputFileStream.Length > bomBytes.Length Then
                InputFileStream.Read(sampleBytes, bomBytes.Length, sampleBytes.Length - bomBytes.Length)
            End If

            InputFileStream.Position = originalPos

            'test byte array content
            encodingFound = DetectUnicodeInByteSampleByHeuristics(sampleBytes)

            If encodingFound IsNot Nothing Then
                Return encodingFound
            Else
                Return DefaultEncoding
            End If
        End Function

        Public Function DetectTextByteArrayEncoding(TextData As Byte(), DefaultEncoding As Encoding) As Encoding
            If TextData Is Nothing Then
                Throw New ArgumentNullException("Must provide a valid text data byte array!", "TextData")
            End If

            Dim encodingFound As Encoding = DetectBOMBytes(TextData)

            If encodingFound IsNot Nothing Then
                Return encodingFound
            Else
                'test byte array content
                encodingFound = DetectUnicodeInByteSampleByHeuristics(TextData)

                If encodingFound IsNot Nothing Then
                    Return encodingFound
                Else
                    Return DefaultEncoding
                End If
            End If

        End Function

        Public Function DetectBOMBytes(BOMBytes As Byte()) As Encoding
            If BOMBytes Is Nothing Then
                Throw New ArgumentNullException("Must provide a valid BOM byte array!", "BOMBytes")
            End If

            If BOMBytes.Length < 2 Then
                Return Nothing
            End If

            If BOMBytes(0) = &HFF AndAlso BOMBytes(1) = &HFE AndAlso (BOMBytes.Length < 4 OrElse BOMBytes(2) <> 0 OrElse BOMBytes(3) <> 0) Then
                Return Encoding.Unicode
            End If

            If BOMBytes(0) = &HFE AndAlso BOMBytes(1) = &HFF Then
                Return Encoding.BigEndianUnicode
            End If

            If BOMBytes.Length < 3 Then
                Return Nothing
            End If

            If BOMBytes(0) = &HEF AndAlso BOMBytes(1) = &HBB AndAlso BOMBytes(2) = &HBF Then
                Return Encoding.UTF8
            End If

            If BOMBytes(0) = &H2B AndAlso BOMBytes(1) = &H2F AndAlso BOMBytes(2) = &H76 Then
                Return Encoding.UTF7
            End If

            If BOMBytes.Length < 4 Then
                Return Nothing
            End If

            If BOMBytes(0) = &HFF AndAlso BOMBytes(1) = &HFE AndAlso BOMBytes(2) = 0 AndAlso BOMBytes(3) = 0 Then
                Return Encoding.UTF32
            End If

            If BOMBytes(0) = 0 AndAlso BOMBytes(1) = 0 AndAlso BOMBytes(2) = &HFE AndAlso BOMBytes(3) = &HFF Then
                Return Encoding.GetEncoding(12001)
            End If

            Return Nothing
        End Function

        Public Function DetectUnicodeInByteSampleByHeuristics(SampleBytes As Byte()) As Encoding
            Dim oddBinaryNullsInSample As Long = 0
            Dim evenBinaryNullsInSample As Long = 0
            Dim suspiciousUTF8SequenceCount As Long = 0
            Dim suspiciousUTF8BytesTotal As Long = 0
            Dim likelyUSASCIIBytesInSample As Long = 0

            'Cycle through, keeping count of binary null positions, possible UTF-8
            ' sequences from upper ranges of Windows-1252, and probable US-ASCII
            ' character counts.

            Dim currentPos As Long = 0
            Dim skipUTF8Bytes As Integer = 0

            While currentPos < SampleBytes.Length
                'binary null distribution
                If SampleBytes(currentPos) = 0 Then
                    If currentPos Mod 2 = 0 Then
                        evenBinaryNullsInSample += 1
                    Else
                        oddBinaryNullsInSample += 1
                    End If
                End If

                'likely US-ASCII characters
                If IsCommonUSASCIIByte(SampleBytes(currentPos)) Then
                    likelyUSASCIIBytesInSample += 1
                End If

                'suspicious sequences (look like UTF-8)
                If skipUTF8Bytes = 0 Then
                    Dim lengthFound As Integer = DetectSuspiciousUTF8SequenceLength(SampleBytes, currentPos)

                    If lengthFound > 0 Then
                        suspiciousUTF8SequenceCount += 1
                        suspiciousUTF8BytesTotal += lengthFound
                        skipUTF8Bytes = lengthFound - 1
                    End If
                Else
                    skipUTF8Bytes -= 1
                End If

                currentPos += 1
            End While

            '1: UTF-16 LE - in english / european environments, this is usually characterized by a
            ' high proportion of odd binary nulls (starting at 0), with (as this is text) a low
            ' proportion of even binary nulls.
            ' The thresholds here used (less than 20% nulls where you expect non-nulls, and more than
            ' 60% nulls where you do expect nulls) are completely arbitrary.

            If ((evenBinaryNullsInSample * 2.0) / SampleBytes.Length) < 0.2 AndAlso ((oddBinaryNullsInSample * 2.0) / SampleBytes.Length) > 0.6 Then
                Return Encoding.Unicode
            End If

            '2: UTF-16 BE - in english / european environments, this is usually characterized by a
            ' high proportion of even binary nulls (starting at 0), with (as this is text) a low
            ' proportion of odd binary nulls.
            ' The thresholds here used (less than 20% nulls where you expect non-nulls, and more than
            ' 60% nulls where you do expect nulls) are completely arbitrary.

            If ((oddBinaryNullsInSample * 2.0) / SampleBytes.Length) < 0.2 AndAlso ((evenBinaryNullsInSample * 2.0) / SampleBytes.Length) > 0.6 Then
                Return Encoding.BigEndianUnicode
            End If

            '3: UTF-8 - Martin Dürst outlines a method for detecting whether something CAN be UTF-8 content
            ' using regexp, in his w3c.org unicode FAQ entry:
            ' http://www.w3.org/International/questions/qa-forms-utf-8
            ' adapted here for C#.
            Dim potentiallyMangledString As String = Encoding.ASCII.GetString(SampleBytes)
            Dim UTF8Validator As New Regex("\A(" & "[\x09\x0A\x0D\x20-\x7E]" & "|[\xC2-\xDF][\x80-\xBF]" & "|\xE0[\xA0-\xBF][\x80-\xBF]" & "|[\xE1-\xEC\xEE\xEF][\x80-\xBF]{2}" & "|\xED[\x80-\x9F][\x80-\xBF]" & "|\xF0[\x90-\xBF][\x80-\xBF]{2}" & "|[\xF1-\xF3][\x80-\xBF]{3}" & "|\xF4[\x80-\x8F][\x80-\xBF]{2}" & ")*\z")

            If UTF8Validator.IsMatch(potentiallyMangledString) Then
                'Unfortunately, just the fact that it CAN be UTF-8 doesn't tell you much about probabilities.
                'If all the characters are in the 0-127 range, no harm done, most western charsets are same as UTF-8 in these ranges.
                'If some of the characters were in the upper range (western accented characters), however, they would likely be mangled to 2-byte by the UTF-8 encoding process.
                ' So, we need to play stats.

                ' The "Random" likelihood of any pair of randomly generated characters being one
                ' of these "suspicious" character sequences is:
                ' 128 / (256 * 256) = 0.2%.
                '
                ' In western text data, that is SIGNIFICANTLY reduced - most text data stays in the <127
                ' character range, so we assume that more than 1 in 500,000 of these character
                ' sequences indicates UTF-8. The number 500,000 is completely arbitrary - so sue me.
                '
                ' We can only assume these character sequences will be rare if we ALSO assume that this
                ' IS in fact western text - in which case the bulk of the UTF-8 encoded data (that is
                ' not already suspicious sequences) should be plain US-ASCII bytes. This, I
                ' arbitrarily decided, should be 80% (a random distribution, eg binary data, would yield
                ' approx 40%, so the chances of hitting this threshold by accident in random data are
                ' VERY low).

                'suspicious sequences
                'all suspicious, so cannot evaluate proportion of US-Ascii
                If (suspiciousUTF8SequenceCount * 500000.0 / SampleBytes.Length >= 1) AndAlso (SampleBytes.Length - suspiciousUTF8BytesTotal = 0 OrElse likelyUSASCIIBytesInSample * 1.0 / (SampleBytes.Length - suspiciousUTF8BytesTotal) >= 0.8) Then
                    Return Encoding.UTF8
                End If
            End If

            Return Nothing
        End Function

        Private Function IsCommonUSASCIIByte(testByte As Byte) As Boolean
            'lf
            'cr
            'tab
            'common punctuation
            'digits
            'common punctuation
            'capital letters
            'common punctuation
            'lowercase letters
            If testByte = &HA OrElse testByte = &HD OrElse testByte = &H9 OrElse (testByte >= &H20 AndAlso testByte <= &H2F) OrElse (testByte >= &H30 AndAlso testByte <= &H39) OrElse (testByte >= &H3A AndAlso testByte <= &H40) OrElse (testByte >= &H41 AndAlso testByte <= &H5A) OrElse (testByte >= &H5B AndAlso testByte <= &H60) OrElse (testByte >= &H61 AndAlso testByte <= &H7A) OrElse (testByte >= &H7B AndAlso testByte <= &H7E) Then
                'common punctuation
                Return True
            Else
                Return False
            End If
        End Function

        Private Function DetectSuspiciousUTF8SequenceLength(SampleBytes As Byte(), currentPos As Long) As Integer
            Dim lengthFound As Integer = 0

            If SampleBytes.Length >= currentPos + 1 AndAlso SampleBytes(currentPos) = &HC2 Then
                If SampleBytes(currentPos + 1) = &H81 OrElse SampleBytes(currentPos + 1) = &H8D OrElse SampleBytes(currentPos + 1) = &H8F Then
                    lengthFound = 2
                ElseIf SampleBytes(currentPos + 1) = &H90 OrElse SampleBytes(currentPos + 1) = &H9D Then
                    lengthFound = 2
                ElseIf SampleBytes(currentPos + 1) >= &HA0 AndAlso SampleBytes(currentPos + 1) <= &HBF Then
                    lengthFound = 2
                End If
            ElseIf SampleBytes.Length >= currentPos + 1 AndAlso SampleBytes(currentPos) = &HC3 Then
                If SampleBytes(currentPos + 1) >= &H80 AndAlso SampleBytes(currentPos + 1) <= &HBF Then
                    lengthFound = 2
                End If
            ElseIf SampleBytes.Length >= currentPos + 1 AndAlso SampleBytes(currentPos) = &HC5 Then
                If SampleBytes(currentPos + 1) = &H92 OrElse SampleBytes(currentPos + 1) = &H93 Then
                    lengthFound = 2
                ElseIf SampleBytes(currentPos + 1) = &HA0 OrElse SampleBytes(currentPos + 1) = &HA1 Then
                    lengthFound = 2
                ElseIf SampleBytes(currentPos + 1) = &HB8 OrElse SampleBytes(currentPos + 1) = &HBD OrElse SampleBytes(currentPos + 1) = &HBE Then
                    lengthFound = 2
                End If
            ElseIf SampleBytes.Length >= currentPos + 1 AndAlso SampleBytes(currentPos) = &HC6 Then
                If SampleBytes(currentPos + 1) = &H92 Then
                    lengthFound = 2
                End If
            ElseIf SampleBytes.Length >= currentPos + 1 AndAlso SampleBytes(currentPos) = &HCB Then
                If SampleBytes(currentPos + 1) = &H86 OrElse SampleBytes(currentPos + 1) = &H9C Then
                    lengthFound = 2
                End If
            ElseIf SampleBytes.Length >= currentPos + 2 AndAlso SampleBytes(currentPos) = &HE2 Then
                If SampleBytes(currentPos + 1) = &H80 Then
                    If SampleBytes(currentPos + 2) = &H93 OrElse SampleBytes(currentPos + 2) = &H94 Then
                        lengthFound = 3
                    End If
                    If SampleBytes(currentPos + 2) = &H98 OrElse SampleBytes(currentPos + 2) = &H99 OrElse SampleBytes(currentPos + 2) = &H9A Then
                        lengthFound = 3
                    End If
                    If SampleBytes(currentPos + 2) = &H9C OrElse SampleBytes(currentPos + 2) = &H9D OrElse SampleBytes(currentPos + 2) = &H9E Then
                        lengthFound = 3
                    End If
                    If SampleBytes(currentPos + 2) = &HA0 OrElse SampleBytes(currentPos + 2) = &HA1 OrElse SampleBytes(currentPos + 2) = &HA2 Then
                        lengthFound = 3
                    End If
                    If SampleBytes(currentPos + 2) = &HA6 Then
                        lengthFound = 3
                    End If
                    If SampleBytes(currentPos + 2) = &HB0 Then
                        lengthFound = 3
                    End If
                    If SampleBytes(currentPos + 2) = &HB9 OrElse SampleBytes(currentPos + 2) = &HBA Then
                        lengthFound = 3
                    End If
                ElseIf SampleBytes(currentPos + 1) = &H82 AndAlso SampleBytes(currentPos + 2) = &HAC Then
                    lengthFound = 3
                ElseIf SampleBytes(currentPos + 1) = &H84 AndAlso SampleBytes(currentPos + 2) = &HA2 Then
                    lengthFound = 3
                End If
            End If

            Return lengthFound
        End Function
    End Module
End Namespace
