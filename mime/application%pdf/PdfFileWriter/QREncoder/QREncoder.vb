#Region "Microsoft.VisualBasic::2ee85ef0f082ce4ce5a0fd6846af4d16, mime\application%pdf\PdfFileWriter\QREncoder\QREncoder.vb"

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

    '   Total Lines: 1516
    '    Code Lines: 803
    ' Comment Lines: 364
    '   Blank Lines: 349
    '     File Size: 52.15 KB


    ' Enum ErrorCorrection
    ' 
    '     H, L, M, Q
    ' 
    '  
    ' 
    ' 
    ' 
    ' Enum EncodingMode
    ' 
    '     [Byte], AlphaNumeric, Append, FNC1First, FNC1Second
    '     Kanji, Numeric, Terminator, Unknown10, Unknown11
    '     Unknown12, Unknown13, Unknown14, Unknown15, Unknown6
    '     Unknown7
    ' 
    '  
    ' 
    ' 
    ' 
    ' Class QREncoder
    ' 
    '     Properties: ErrorCorrection, ModuleSize, QRCodeDimension, QRCodeImageDimension, QRCodeMatrix
    '                 QRCodeVersion, QuietZone
    ' 
    '     Function: ConvertQRCodeMatrixToPixels, DataLengthBits, EvaluationCondition1, EvaluationCondition2, EvaluationCondition3
    '               EvaluationCondition4, TestHorizontalDarkLight, TestVerticalDarkLight
    ' 
    '     Sub: AddFormatInformation, ApplyMask, ApplyMask0, ApplyMask1, ApplyMask2
    '          ApplyMask3, ApplyMask4, ApplyMask5, ApplyMask6, ApplyMask7
    '          BuildBaseMatrix, CalculateErrorCorrection, (+4 Overloads) Encode, EncodeData, Initialization
    '          InterleaveBlocks, LoadMatrixWithData, PolynominalDivision, SaveBitsToCodewordsArray, SelectBastMask
    '          SetDataCodewordsLength
    ' 
    ' /********************************************************************************/

#End Region

'
'
'	QR Code Encoder Library
'
'	QR Code encoder.
'
'	Author: Uzi Granot
'	Original Version: 1.0
'	Date: June 30, 2018
'	Copyright (C) 2018-2019 Uzi Granot. All Rights Reserved
'	For full version history please look at QREncoder.cs
'
'	QR Code Library C# class library and the attached test/demo
'  applications are free software.
'	Software developed by this author is licensed under CPOL 1.02.
'	Some portions of the QRCodeVideoDecoder are licensed under GNU Lesser
'	General Public License v3.0.
'
'	The solution is made of 3 projects:
'	1. QRCodeEncoderLibrary: QR code encoding.
'	2. QRCodeEncoderDemo: Create QR Code images.
'	3. QRCodeConsoleDemo: Demo app for net standard
'
'	The main points of CPOL 1.02 subject to the terms of the License are:
'
'	Source Code and Executable Files can be used in commercial applications;
'	Source Code and Executable Files can be redistributed; and
'	Source Code can be modified to create derivative works.
'	No claim of suitability, guarantee, or any warranty whatsoever is
'	provided. The software is provided "as-is".
'	The Article accompanying the Work may not be distributed or republished
'	without the Author's consent
'
'

Imports System
Imports System.Text
Imports stdNum = System.Math

'namespace QRCodeEncoderLibrary

''' <summary>
''' QR Code error correction code enumeration
''' </summary>
Public Enum ErrorCorrection
    ''' <summary>
    ''' Low (01)
    ''' </summary>
    L

    ''' <summary>
    ''' Medium (00)
    ''' </summary>
    M

    ''' <summary>
    ''' Medium-high (11)
    ''' </summary>
    Q

    ''' <summary>
    ''' High (10)
    ''' </summary>
    H
End Enum

''' <summary>
''' QR Code encoding modes
''' </summary>
Public Enum EncodingMode
    ''' <summary>
    ''' Terminator
    ''' </summary>
    Terminator

    ''' <summary>
    ''' Numeric
    ''' </summary>
    Numeric

    ''' <summary>
    ''' Alpha numeric
    ''' </summary>
    AlphaNumeric

    ''' <summary>
    ''' Append
    ''' </summary>
    Append

    ''' <summary>
    ''' byte encoding
    ''' </summary>
    [Byte]

    ''' <summary>
    ''' FNC1 first
    ''' </summary>
    FNC1First

    ''' <summary>
    ''' Unknown encoding constant
    ''' </summary>
    Unknown6

    ''' <summary>
    ''' Unknown encoding constant
    ''' </summary>
    Unknown7

    ''' <summary>
    ''' Kanji encoding (not implemented by this software)
    ''' </summary>
    Kanji

    ''' <summary>
    ''' FNC1 second
    ''' </summary>
    FNC1Second

    ''' <summary>
    ''' Unknown encoding constant
    ''' </summary>
    Unknown10

    ''' <summary>
    ''' Unknown encoding constant
    ''' </summary>
    Unknown11

    ''' <summary>
    ''' Unknown encoding constant
    ''' </summary>
    Unknown12

    ''' <summary>
    ''' Unknown encoding constant
    ''' </summary>
    Unknown13

    ''' <summary>
    ''' Unknown encoding constant
    ''' </summary>
    Unknown14

    ''' <summary>
    ''' Unknown encoding constant
    ''' </summary>
    Unknown15
End Enum

''' <summary>
''' QR Code Encoder class
''' </summary>
Public Class QREncoder
    Inherits QREncoderTables

    ''' <summary>
    ''' Gets QR Code image dimension
    ''' </summary>
    Private _QRCodeMatrix As Boolean(,), _QRCodeVersion As Integer, _QRCodeDimension As Integer, _QRCodeImageDimension As Integer
    ''' <summary>
    ''' Version number
    ''' </summary>
    Public Const VersionNumber As String = "Ver 2.1.0 - 2019-05-26"

    ''' <summary>
    ''' QR code matrix.
    ''' </summary>
    Public Property QRCodeMatrix As Boolean(,)
        Get
            Return _QRCodeMatrix
        End Get
        Friend Set(value As Boolean(,))
            _QRCodeMatrix = value
        End Set
    End Property

    ''' <summary>
    ''' Gets QR Code matrix version
    ''' </summary>
    Public Property QRCodeVersion As Integer
        Get
            Return _QRCodeVersion
        End Get
        Friend Set(value As Integer)
            _QRCodeVersion = value
        End Set
    End Property

    ''' <summary>
    ''' Gets QR Code matrix dimension in bits
    ''' </summary>
    Public Property QRCodeDimension As Integer
        Get
            Return _QRCodeDimension
        End Get
        Friend Set(value As Integer)
            _QRCodeDimension = value
        End Set
    End Property

    Public Property QRCodeImageDimension As Integer
        Get
            Return _QRCodeImageDimension
        End Get
        Friend Set(value As Integer)
            _QRCodeImageDimension = value
        End Set
    End Property

    ' internal variables
    Friend DataSegArray As Byte()()
    Friend EncodedDataBits As Integer
    Friend MaxCodewords As Integer
    Friend MaxDataCodewords As Integer
    Friend MaxDataBits As Integer
    Friend ErrCorrCodewords As Integer
    Friend BlocksGroup1 As Integer
    Friend DataCodewordsGroup1 As Integer
    Friend BlocksGroup2 As Integer
    Friend DataCodewordsGroup2 As Integer
    Friend MaskCode As Integer
    Friend EncodingSegMode As EncodingMode()
    Friend CodewordsArray As Byte()
    Friend CodewordsPtr As Integer
    Friend BitBuffer As UInteger
    Friend BitBufferLen As Integer
    Friend BaseMatrix As Byte(,)
    Friend MaskMatrix As Byte(,)
    Friend ResultMatrix As Byte(,)

    ''' <summary>
    ''' QR Code error correction code (L, M, Q, H)
    ''' </summary>
    Public Property ErrorCorrection As ErrorCorrection
        Get
            Return _ErrorCorrection
        End Get
        Set(value As ErrorCorrection)
            ' test error correction
            If value < ErrorCorrection.L OrElse value > ErrorCorrection.H Then
                Throw New ArgumentException("Error correction is invalid. Must be L, M, Q or H. Default is M")
            End If

            ' save error correction level
            _ErrorCorrection = value
            Return
        End Set
    End Property

    Private _ErrorCorrection As ErrorCorrection = ErrorCorrection.M

    ''' <summary>
    ''' Module size (Default: 2)
    ''' </summary>
    Public Property ModuleSize As Integer
        Get
            Return _ModuleSize
        End Get
        Set(value As Integer)
            If value < 1 OrElse value > 100 Then Throw New ArgumentException("Module size error. Default is 2.")
            _ModuleSize = value

            ' quiet zone must be at least 4 times module size
            If _QuietZone < 4 * value Then _QuietZone = 4 * value

            ' recalculate image dimension
            QRCodeImageDimension = 2 * _QuietZone + QRCodeDimension * _ModuleSize
            Return
        End Set
    End Property

    Private _ModuleSize As Integer = 2

    ''' <summary>
    ''' Quiet zone around the barcode in pixels (Default: 8)
    ''' Must be at least 4 times module size
    ''' </summary>
    Public Property QuietZone As Integer
        Get
            Return _QuietZone
        End Get
        Set(value As Integer)
            If value < 4 * _ModuleSize OrElse value > 400 Then Throw New ArgumentException("Quiet zone must be at least 4 times the module size. Default is 8.")
            _QuietZone = value

            ' recalculate image dimension
            QRCodeImageDimension = 2 * _QuietZone + QRCodeDimension * _ModuleSize
            Return
        End Set
    End Property

    Private _QuietZone As Integer = 8

    ''' <summary>
    ''' Encode one string into QRCode boolean matrix
    ''' </summary>
    ''' <param name="StringDataSegment">string data segment</param>
    Public Sub Encode(StringDataSegment As String)
        ' empty
        If String.IsNullOrEmpty(StringDataSegment) Then Throw New ArgumentNullException("String data segment is null or missing")

        ' convert string to byte array
        Dim BinaryData = Encoding.UTF8.GetBytes(StringDataSegment)

        ' encode data
        Encode(New Byte()() {BinaryData})
        Return
    End Sub

    ''' <summary>
    ''' Encode array of strings into QRCode boolean matrix
    ''' </summary>
    ''' <param name="StringDataSegments">string data segments</param>
    Public Sub Encode(StringDataSegments As String())
        ' empty
        If StringDataSegments Is Nothing OrElse StringDataSegments.Length = 0 Then Throw New ArgumentNullException("String data segments are null or empty")

        ' loop for all segments
        For SegIndex = 0 To StringDataSegments.Length - 1
            ' convert string to byte array
            If Equals(StringDataSegments(SegIndex), Nothing) Then Throw New ArgumentNullException("One of the string data segments is null or empty")
        Next

        ' create bytes arrays
        Dim TempDataSegArray = New Byte(StringDataSegments.Length - 1)() {}

        ' loop for all segments
        For SegIndex = 0 To StringDataSegments.Length - 1
            ' convert string to byte array
            TempDataSegArray(SegIndex) = Encoding.UTF8.GetBytes(StringDataSegments(SegIndex))
        Next

        ' convert string to byte array
        Encode(TempDataSegArray)
        Return
    End Sub

    ''' <summary>
    ''' Encode one data segment into QRCode boolean matrix
    ''' </summary>
    ''' <param name="SingleDataSeg">Data segment byte array</param>
    ''' <remarks>QR Code boolean matrix</remarks>
    Public Sub Encode(SingleDataSeg As Byte())
        ' test data segments array
        If SingleDataSeg Is Nothing OrElse SingleDataSeg.Length = 0 Then Throw New ArgumentNullException("Single data segment argument is null or empty")

        ' encode data
        Encode(New Byte()() {SingleDataSeg})
        Return
    End Sub

    ''' <summary>
    ''' Encode data segments array into QRCode boolean matrix
    ''' </summary>
    ''' <param name="DataSegArray">Data array of byte arrays</param>
    ''' <remarks>QR Code boolean matrix</remarks>
    Public Sub Encode(DataSegArray As Byte()())
        ' test data segments array
        If DataSegArray Is Nothing OrElse DataSegArray.Length = 0 Then Throw New ArgumentNullException("Data segments argument is null or empty")

        ' reset result variables
        QRCodeMatrix = Nothing
        QRCodeVersion = 0
        QRCodeDimension = 0
        QRCodeImageDimension = 0

        ' loop for all segments
        Dim Bytes = 0

        For SegIndex = 0 To DataSegArray.Length - 1
            ' input string length
            Dim DataSeg = DataSegArray(SegIndex)

            If DataSeg Is Nothing Then
                DataSegArray(SegIndex) = New Byte(-1) {}
            Else
                Bytes += DataSeg.Length
            End If
        Next

        If Bytes = 0 Then Throw New ArgumentException("There is no data to encode.")

        ' save data segments array
        Me.DataSegArray = DataSegArray

        ' initialization
        Initialization()

        ' encode data
        EncodeData()

        ' calculate error correction
        CalculateErrorCorrection()

        ' iterleave data and error correction codewords
        InterleaveBlocks()

        ' build base matrix
        BuildBaseMatrix()

        ' load base matrix with data and error correction codewords
        LoadMatrixWithData()

        ' data masking
        SelectBastMask()

        ' add format information (error code level and mask code)
        AddFormatInformation()

        ' output matrix each element is one module
        QRCodeMatrix = New Boolean(QRCodeDimension - 1, QRCodeDimension - 1) {}

        ' convert result matrix to output matrix
        ' Black=true, White=false
        For Row = 0 To QRCodeDimension - 1

            For Col = 0 To QRCodeDimension - 1
                If (ResultMatrix(Row, Col) And 1) <> 0 Then QRCodeMatrix(Row, Col) = True
            Next
        Next

        ' exit
        Return
    End Sub

    ''' <summary>
    ''' convert black and white matrix to black and white image
    ''' </summary>
    ''' <returns>Black and white image in pixels</returns>
    Public Function ConvertQRCodeMatrixToPixels() As Boolean(,)
        If QRCodeMatrix Is Nothing Then Throw New ApplicationException("QRCode must be encoded first")

        ' output matrix size in pixels all matrix elements are white (false)
        Dim ImageDimension = QRCodeImageDimension
        Dim BWImage = New Boolean(ImageDimension - 1, ImageDimension - 1) {}
        Dim XOffset = _QuietZone
        Dim YOffset = _QuietZone

        ' convert result matrix to output matrix
        For Row = 0 To QRCodeDimension - 1

            For Col = 0 To QRCodeDimension - 1
                ' bar is black
                If QRCodeMatrix(Row, Col) Then
                    For Y = 0 To ModuleSize - 1

                        For X = 0 To ModuleSize - 1
                            BWImage(YOffset + Y, XOffset + X) = True
                        Next
                    Next
                End If

                XOffset += ModuleSize
            Next

            XOffset = _QuietZone
            YOffset += ModuleSize
        Next

        Return BWImage
    End Function


    
    ' Initialization
    

    Friend Sub Initialization()
        ' create encoding mode array
        EncodingSegMode = New EncodingMode(DataSegArray.Length - 1) {}

        ' reset total encoded data bits
        EncodedDataBits = 0

        ' loop for all segments
        For SegIndex = 0 To DataSegArray.Length - 1
            ' input string length
            Dim DataSeg = DataSegArray(SegIndex)
            Dim DataLength = DataSeg.Length

            ' find encoding mode
            Dim EncodingMode As EncodingMode = EncodingMode.Numeric

            For Index = 0 To DataLength - 1
                Dim Code As Integer = EncodingTable(DataSeg(Index))
                If Code < 10 Then Continue For

                If Code < 45 Then
                    EncodingMode = EncodingMode.AlphaNumeric
                    Continue For
                End If

                EncodingMode = EncodingMode.Byte
                Exit For
            Next

            ' calculate required bit length
            Dim DataBits = 4

            Select Case EncodingMode
                Case EncodingMode.Numeric
                    DataBits += 10 * (DataLength / 3)

                    If DataLength Mod 3 = 1 Then
                        DataBits += 4
                    ElseIf DataLength Mod 3 = 2 Then
                        DataBits += 7
                    End If

                Case EncodingMode.AlphaNumeric
                    DataBits += 11 * (DataLength / 2)
                    If (DataLength And 1) <> 0 Then DataBits += 6
                Case EncodingMode.Byte
                    DataBits += 8 * DataLength
            End Select

            EncodingSegMode(SegIndex) = EncodingMode
            EncodedDataBits += DataBits
        Next

        ' find best version
        Dim TotalDataLenBits = 0
        QRCodeVersion = 1

        While QRCodeVersion <= 40
            ' number of bits on each side of the QR code square
            QRCodeDimension = 17 + 4 * QRCodeVersion
            QRCodeImageDimension = 2 * _QuietZone + QRCodeDimension * _ModuleSize
            SetDataCodewordsLength()
            TotalDataLenBits = 0

            For Seg = 0 To EncodingSegMode.Length - 1
                TotalDataLenBits += DataLengthBits(EncodingSegMode(Seg))
            Next

            If EncodedDataBits + TotalDataLenBits <= MaxDataBits Then Exit While
            QRCodeVersion += 1
        End While

        If QRCodeVersion > 40 Then Throw New ApplicationException("Input data string is too long")
        EncodedDataBits += TotalDataLenBits
        Return
    End Sub

    
    ' QRCode: Convert data to bit array
    
    Friend Sub EncodeData()
        ' codewords array
        CodewordsArray = New Byte(MaxCodewords - 1) {}

        ' reset encoding members
        CodewordsPtr = 0
        BitBuffer = 0
        BitBufferLen = 0

        ' loop for all segments
        For SegIndex = 0 To DataSegArray.Length - 1
            ' input string length
            Dim DataSeg = DataSegArray(SegIndex)
            Dim DataLength = DataSeg.Length

            ' first 4 bits is mode indicator
            ' numeric code indicator is 0001, alpha numeric 0010, byte 0100
            SaveBitsToCodewordsArray(EncodingSegMode(SegIndex), 4)

            ' character count
            SaveBitsToCodewordsArray(DataLength, DataLengthBits(EncodingSegMode(SegIndex)))

            ' switch based on encode mode
            Select Case EncodingSegMode(SegIndex)
                ' numeric mode
                Case EncodingMode.Numeric
                    ' encode digits in groups of 3
                    Dim NumEnd As Integer = DataLength / 3 * 3

                    For Index = 0 To NumEnd - 1 Step 3
                        SaveBitsToCodewordsArray(100 * EncodingTable(DataSeg(Index)) + 10 * EncodingTable(DataSeg(Index + 1)) + EncodingTable(DataSeg(Index + 2)), 10)
                    Next

                    ' we have one digit remaining
                    If DataLength - NumEnd = 1 Then
                        SaveBitsToCodewordsArray(EncodingTable(DataSeg(NumEnd)), 4)

                        ' we have two digits remaining
                    ElseIf DataLength - NumEnd = 2 Then
                        SaveBitsToCodewordsArray(10 * EncodingTable(DataSeg(NumEnd)) + EncodingTable(DataSeg(NumEnd + 1)), 7)
                    End If

                ' alphanumeric mode
                Case EncodingMode.AlphaNumeric
                    ' encode digits in groups of 2
                    Dim AlphaNumEnd As Integer = DataLength / 2 * 2

                    For Index = 0 To AlphaNumEnd - 1 Step 2
                        SaveBitsToCodewordsArray(45 * EncodingTable(DataSeg(Index)) + EncodingTable(DataSeg(Index + 1)), 11)
                    Next

                    ' we have one character remaining
                    If DataLength - AlphaNumEnd = 1 Then SaveBitsToCodewordsArray(EncodingTable(DataSeg(AlphaNumEnd)), 6)


                ' byte mode					
                Case EncodingMode.Byte
                    ' append the data after mode and character count
                    For Index = 0 To DataLength - 1
                        SaveBitsToCodewordsArray(DataSeg(Index), 8)
                    Next
            End Select
        Next

        ' set terminator
        If EncodedDataBits < MaxDataBits Then SaveBitsToCodewordsArray(0, If(MaxDataBits - EncodedDataBits < 4, MaxDataBits - EncodedDataBits, 4))

        ' flush bit buffer
        If BitBufferLen > 0 Then CodewordsArray(stdNum.Min(Threading.Interlocked.Increment(CodewordsPtr), CodewordsPtr - 1)) = CByte(BitBuffer >> 24)

        ' add extra padding if there is still space
        Dim PadEnd = MaxDataCodewords - CodewordsPtr

        For PadPtr = 0 To PadEnd - 1
            CodewordsArray(CodewordsPtr + PadPtr) = CByte(If((PadPtr And 1) = 0, &HEC, &H11))
        Next

        ' exit
        Return
    End Sub

    
    ' Save data to codeword array
    
    Friend Sub SaveBitsToCodewordsArray(Data As Integer, Bits As Integer)
        BitBuffer = BitBuffer Or CUInt(Data) << 32 - BitBufferLen - Bits
        BitBufferLen += Bits

        While BitBufferLen >= 8
            CodewordsArray(stdNum.Min(Threading.Interlocked.Increment(CodewordsPtr), CodewordsPtr - 1)) = CByte(BitBuffer >> 24)
            BitBuffer <<= 8
            BitBufferLen -= 8
        End While

        Return
    End Sub

    
    ' Calculate Error Correction
    
    Friend Sub CalculateErrorCorrection()
        ' set generator polynomial array
        Dim Generator = GenArray(ErrCorrCodewords - 7)

        ' error correcion calculation buffer
        Dim BufSize = stdNum.Max(DataCodewordsGroup1, DataCodewordsGroup2) + ErrCorrCodewords
        Dim ErrCorrBuff = New Byte(BufSize - 1) {}

        ' initial number of data codewords
        Dim DataCodewords = DataCodewordsGroup1
        Dim BuffLen = DataCodewords + ErrCorrCodewords

        ' codewords pointer
        Dim DataCodewordsPtr = 0

        ' codewords buffer error correction pointer
        Dim CodewordsArrayErrCorrPtr = MaxDataCodewords

        ' loop one block at a time
        Dim TotalBlocks = BlocksGroup1 + BlocksGroup2

        For BlockNumber = 0 To TotalBlocks - 1
            ' switch to group2 data codewords
            If BlockNumber = BlocksGroup1 Then
                DataCodewords = DataCodewordsGroup2
                BuffLen = DataCodewords + ErrCorrCodewords
            End If

            ' copy next block of codewords to the buffer and clear the remaining part
            Array.Copy(CodewordsArray, DataCodewordsPtr, ErrCorrBuff, 0, DataCodewords)
            Array.Clear(ErrCorrBuff, DataCodewords, ErrCorrCodewords)

            ' update codewords array to next buffer
            DataCodewordsPtr += DataCodewords

            ' error correction polynomial division
            PolynominalDivision(ErrCorrBuff, BuffLen, Generator, ErrCorrCodewords)

            ' save error correction block			
            Array.Copy(ErrCorrBuff, DataCodewords, CodewordsArray, CodewordsArrayErrCorrPtr, ErrCorrCodewords)
            CodewordsArrayErrCorrPtr += ErrCorrCodewords
        Next

        Return
    End Sub

    
    ' Polynomial division for error correction
    

    Friend Shared Sub PolynominalDivision(Polynomial As Byte(), PolyLength As Integer, Generator As Byte(), ErrCorrCodewords As Integer)
        Dim DataCodewords = PolyLength - ErrCorrCodewords

        ' error correction polynomial division
        For Index = 0 To DataCodewords - 1
            ' current first codeword is zero
            If Polynomial(Index) = 0 Then Continue For

            ' current first codeword is not zero
            Dim Multiplier As Integer = IntToExp(Polynomial(Index))

            ' loop for error correction coofficients
            For GeneratorIndex = 0 To ErrCorrCodewords - 1
                Polynomial(Index + 1 + GeneratorIndex) = Polynomial(Index + 1 + GeneratorIndex) Xor ExpToInt(Generator(GeneratorIndex) + Multiplier)
            Next
        Next

        Return
    End Sub

    
    ' Interleave data and error correction blocks
    
    Friend Sub InterleaveBlocks()
        ' allocate temp codewords array
        Dim TempArray = New Byte(MaxCodewords - 1) {}

        ' total blocks
        Dim TotalBlocks = BlocksGroup1 + BlocksGroup2

        ' create array of data blocks starting point
        Dim Start = New Integer(TotalBlocks - 1) {}

        For Index = 1 To TotalBlocks - 1
            Start(Index) = Start(Index - 1) + If(Index <= BlocksGroup1, DataCodewordsGroup1, DataCodewordsGroup2)
        Next

        ' step one. iterleave base on group one length
        Dim PtrEnd = DataCodewordsGroup1 * TotalBlocks

        ' iterleave group one and two
        Dim Ptr As Integer
        Dim Block = 0

        For Ptr = 0 To PtrEnd - 1
            TempArray(Ptr) = CodewordsArray(Start(Block))
            Start(Block) += 1
            Block += 1
            If Block = TotalBlocks Then Block = 0
        Next

        ' interleave group two
        If DataCodewordsGroup2 > DataCodewordsGroup1 Then
            ' step one. iterleave base on group one length
            PtrEnd = MaxDataCodewords
            Block = BlocksGroup1

            While Ptr < PtrEnd
                TempArray(Ptr) = CodewordsArray(Start(Block))
                Start(Block) += 1
                Block += 1
                If Block = TotalBlocks Then Block = BlocksGroup1
                Ptr += 1
            End While
        End If

        ' create array of error correction blocks starting point
        Start(0) = MaxDataCodewords

        For Index = 1 To TotalBlocks - 1
            Start(Index) = Start(Index - 1) + ErrCorrCodewords
        Next

        ' step one. iterleave base on group one length

        ' iterleave all groups
        PtrEnd = MaxCodewords
        Block = 0

        While Ptr < PtrEnd
            TempArray(Ptr) = CodewordsArray(Start(Block))
            Start(Block) += 1
            Block += 1
            If Block = TotalBlocks Then Block = 0
            Ptr += 1
        End While

        ' save result
        CodewordsArray = TempArray
        Return
    End Sub

    
    ' Load base matrix with data and error correction codewords
    
    Friend Sub LoadMatrixWithData()
        ' input array pointer initialization
        Dim Ptr = 0
        Dim PtrEnd = 8 * MaxCodewords

        ' bottom right corner of output matrix
        Dim Row = QRCodeDimension - 1
        Dim Col = QRCodeDimension - 1

        ' step state
        Dim State = 0

        While True
            ' current module is data
            If (BaseMatrix(Row, Col) And NonData) = 0 Then
                ' load current module with
                If (CodewordsArray(Ptr >> 3) And 1 << 7 - (Ptr And 7)) <> 0 Then BaseMatrix(Row, Col) = DataBlack
                If Threading.Interlocked.Increment(Ptr) = PtrEnd Then Exit While

                ' current module is non data and vertical timing line condition is on
            ElseIf Col = 6 Then
                Col -= 1
            End If

            ' update matrix position to next module
            Select Case State
                ' going up: step one to the left
                Case 0
                    Col -= 1
                    State = 1
                    Continue While

                ' going up: step one row up and one column to the right
                Case 1
                    Col += 1
                    Row -= 1
                    ' we are not at the top, go to state 0
                    If Row >= 0 Then
                        State = 0
                        Continue While
                    End If
                    ' we are at the top, step two columns to the left and start going down
                    Col -= 2
                    Row = 0
                    State = 2
                    Continue While

                ' going down: step one to the left
                Case 2
                    Col -= 1
                    State = 3
                    Continue While

                ' going down: step one row down and one column to the right
                Case 3
                    Col += 1
                    Row += 1
                    ' we are not at the bottom, go to state 2
                    If Row < QRCodeDimension Then
                        State = 2
                        Continue While
                    End If
                    ' we are at the bottom, step two columns to the left and start going up
                    Col -= 2
                    Row = QRCodeDimension - 1
                    State = 0
                    Continue While
            End Select
        End While

        Return
    End Sub

    
    ' Select Mask
    
    Friend Sub SelectBastMask()
        Dim BestScore = Integer.MaxValue
        MaskCode = 0

        For TestMask = 0 To 8 - 1
            ' apply mask
            ApplyMask(TestMask)

            ' evaluate 4 test conditions
            Dim Score As Integer = EvaluationCondition1()
            If Score >= BestScore Then Continue For
            Score += EvaluationCondition2()
            If Score >= BestScore Then Continue For
            Score += EvaluationCondition3()
            If Score >= BestScore Then Continue For
            Score += EvaluationCondition4()
            If Score >= BestScore Then Continue For

            ' save as best mask so far
            ResultMatrix = MaskMatrix
            MaskMatrix = Nothing
            BestScore = Score
            MaskCode = TestMask
        Next

        Return
    End Sub

    
    ' Evaluation condition #1
    ' 5 consecutive or more modules of the same color
    
    Friend Function EvaluationCondition1() As Integer
        Dim Score = 0

        ' test rows
        For Row = 0 To QRCodeDimension - 1
            Dim Count = 1

            For Col = 1 To QRCodeDimension - 1
                ' current cell is not the same color as the one before
                If ((MaskMatrix(Row, Col - 1) Xor MaskMatrix(Row, Col)) And 1) <> 0 Then
                    If Count >= 5 Then Score += Count - 2
                    Count = 0
                End If

                Count += 1
            Next

            ' last run
            If Count >= 5 Then Score += Count - 2
        Next

        ' test columns
        For Col = 0 To QRCodeDimension - 1
            Dim Count = 1

            For Row = 1 To QRCodeDimension - 1
                ' current cell is not the same color as the one before
                If ((MaskMatrix(Row - 1, Col) Xor MaskMatrix(Row, Col)) And 1) <> 0 Then
                    If Count >= 5 Then Score += Count - 2
                    Count = 0
                End If

                Count += 1
            Next

            ' last run
            If Count >= 5 Then Score += Count - 2
        Next

        Return Score
    End Function

    
    ' Evaluation condition #2
    ' same color in 2 by 2 area
    
    Friend Function EvaluationCondition2() As Integer
        Dim Score = 0
        ' test rows
        For Row = 1 To QRCodeDimension - 1

            For Col = 1 To QRCodeDimension - 1
                ' all are black
                If (MaskMatrix(Row - 1, Col - 1) And MaskMatrix(Row - 1, Col) And MaskMatrix(Row, Col - 1) And MaskMatrix(Row, Col) And 1) <> 0 Then
                    Score += 3

                    ' all are white
                ElseIf ((MaskMatrix(Row - 1, Col - 1) Or MaskMatrix(Row - 1, Col) Or MaskMatrix(Row, Col - 1) Or MaskMatrix(Row, Col)) And 1) = 0 Then
                    Score += 3
                End If
            Next
        Next

        Return Score
    End Function

    
    ' Evaluation condition #3
    ' pattern dark, light, dark, dark, dark, light, dark
    ' before or after 4 light modules
    
    Friend Function EvaluationCondition3() As Integer
        Dim Score = 0

        ' test rows
        For Row = 0 To QRCodeDimension - 1
            Dim Start = 0

            ' look for a lignt run at least 4 modules
            For Col = 0 To QRCodeDimension - 1
                ' current cell is white
                If (MaskMatrix(Row, Col) And 1) = 0 Then Continue For

                ' more or equal to 4
                If Col - Start >= 4 Then
                    ' we have 4 or more white
                    ' test for pattern before the white space
                    If Start >= 7 AndAlso TestHorizontalDarkLight(Row, Start - 7) Then Score += 40

                    ' test for pattern after the white space
                    If QRCodeDimension - Col >= 7 AndAlso TestHorizontalDarkLight(Row, Col) Then
                        Score += 40
                        Col += 6
                    End If
                End If

                ' assume next one is white
                Start = Col + 1
            Next

            ' last run
            If QRCodeDimension - Start >= 4 AndAlso Start >= 7 AndAlso TestHorizontalDarkLight(Row, Start - 7) Then Score += 40
        Next

        ' test columns
        For Col = 0 To QRCodeDimension - 1
            Dim Start = 0

            ' look for a lignt run at least 4 modules
            For Row = 0 To QRCodeDimension - 1
                ' current cell is white
                If (MaskMatrix(Row, Col) And 1) = 0 Then Continue For

                ' more or equal to 4
                If Row - Start >= 4 Then
                    ' we have 4 or more white
                    ' test for pattern before the white space
                    If Start >= 7 AndAlso TestVerticalDarkLight(Start - 7, Col) Then Score += 40

                    ' test for pattern after the white space
                    If QRCodeDimension - Row >= 7 AndAlso TestVerticalDarkLight(Row, Col) Then
                        Score += 40
                        Row += 6
                    End If
                End If

                ' assume next one is white
                Start = Row + 1
            Next

            ' last run
            If QRCodeDimension - Start >= 4 AndAlso Start >= 7 AndAlso TestVerticalDarkLight(Start - 7, Col) Then Score += 40
        Next

        ' exit
        Return Score
    End Function

    
    ' Evaluation condition #4
    ' blak to white ratio
    

    Friend Function EvaluationCondition4() As Integer
        ' count black cells
        Dim Black = 0

        For Row = 0 To QRCodeDimension - 1

            For Col = 0 To QRCodeDimension - 1
                If (MaskMatrix(Row, Col) And 1) <> 0 Then Black += 1
            Next
        Next

        ' ratio
        Dim Ratio = Black / (QRCodeDimension * QRCodeDimension)

        ' there are more black than white
        If Ratio > 0.55 Then
            Return CInt(20.0 * (Ratio - 0.5)) * 10
        ElseIf Ratio < 0.45 Then
            Return CInt(20.0 * (0.5 - Ratio)) * 10
        End If

        Return 0
    End Function

    
    ' Test horizontal dark light pattern
    
    Friend Function TestHorizontalDarkLight(Row As Integer, Col As Integer) As Boolean
        Return (MaskMatrix(Row, Col) And Not MaskMatrix(Row, Col + 1) And MaskMatrix(Row, Col + 2) And MaskMatrix(Row, Col + 3) And MaskMatrix(Row, Col + 4) And Not MaskMatrix(Row, Col + 5) And MaskMatrix(Row, Col + 6) And 1) <> 0
    End Function

    
    ' Test vertical dark light pattern
    
    Friend Function TestVerticalDarkLight(Row As Integer, Col As Integer) As Boolean
        Return (MaskMatrix(Row, Col) And Not MaskMatrix(Row + 1, Col) And MaskMatrix(Row + 2, Col) And MaskMatrix(Row + 3, Col) And MaskMatrix(Row + 4, Col) And Not MaskMatrix(Row + 5, Col) And MaskMatrix(Row + 6, Col) And 1) <> 0
    End Function

    
    ' Add format information
    ' version, error correction code plus mask code
    
    Friend Sub AddFormatInformation()
        Dim Mask As Integer

        ' version information
        If QRCodeVersion >= 7 Then
            Dim Pos = QRCodeDimension - 11
            Dim VerInfo = VersionCodeArray(QRCodeVersion - 7)

            ' top right
            Mask = 1

            For Row = 0 To 6 - 1

                For Col = 0 To 3 - 1
                    ResultMatrix(Row, Pos + Col) = If((VerInfo And Mask) <> 0, FixedBlack, FixedWhite)
                    Mask <<= 1
                Next
            Next

            ' bottom left
            Mask = 1

            For Col = 0 To 6 - 1

                For Row = 0 To 3 - 1
                    ResultMatrix(Pos + Row, Col) = If((VerInfo And Mask) <> 0, FixedBlack, FixedWhite)
                    Mask <<= 1
                Next
            Next
        End If

        ' error correction code and mask number
        Dim FormatInfoPtr = 0 ' M is the default

        Select Case _ErrorCorrection
            Case ErrorCorrection.L
                FormatInfoPtr = 8
            Case ErrorCorrection.Q
                FormatInfoPtr = 24
            Case ErrorCorrection.H
                FormatInfoPtr = 16
        End Select

        Dim FormatInfo = FormatInfoArray(FormatInfoPtr + MaskCode)

        ' load format bits into result matrix
        Mask = 1

        For Index = 0 To 15 - 1
            Dim FormatBit As Integer = If((FormatInfo And Mask) <> 0, FixedBlack, FixedWhite)
            Mask <<= 1

            ' top left corner
            ResultMatrix(FormatInfoOne(Index, 0), FormatInfoOne(Index, 1)) = CByte(FormatBit)

            ' bottom left and top right corners
            Dim Row = FormatInfoTwo(Index, 0)
            If Row < 0 Then Row += QRCodeDimension
            Dim Col = FormatInfoTwo(Index, 1)
            If Col < 0 Then Col += QRCodeDimension
            ResultMatrix(Row, Col) = CByte(FormatBit)
        Next

        Return
    End Sub
    
    ' Set encoded data bits length
    

    Friend Function DataLengthBits(EncodingMode As EncodingMode) As Integer
        ' Data length bits
        Select Case EncodingMode
            ' numeric mode
            Case EncodingMode.Numeric
                Return If(QRCodeVersion < 10, 10, If(QRCodeVersion < 27, 12, 14))

            ' alpha numeric mode
            Case EncodingMode.AlphaNumeric
                Return If(QRCodeVersion < 10, 9, If(QRCodeVersion < 27, 11, 13))

            ' byte mode
            Case EncodingMode.Byte
                Return If(QRCodeVersion < 10, 8, 16)
        End Select

        Throw New ApplicationException("Encoding mode error")
    End Function

    
    ' Set data and error correction codewords length
    

    Friend Sub SetDataCodewordsLength()
        ' index shortcut
        Dim BlockInfoIndex = (QRCodeVersion - 1) * 4 + _ErrorCorrection

        ' Number of blocks in group 1
        BlocksGroup1 = ECBlockInfo(BlockInfoIndex, BLOCKS_GROUP1)

        ' Number of data codewords in blocks of group 1
        DataCodewordsGroup1 = ECBlockInfo(BlockInfoIndex, DATA_CODEWORDS_GROUP1)

        ' Number of blocks in group 2
        BlocksGroup2 = ECBlockInfo(BlockInfoIndex, BLOCKS_GROUP2)

        ' Number of data codewords in blocks of group 2
        DataCodewordsGroup2 = ECBlockInfo(BlockInfoIndex, DATA_CODEWORDS_GROUP2)

        ' Total number of data codewords for this version and EC level
        MaxDataCodewords = BlocksGroup1 * DataCodewordsGroup1 + BlocksGroup2 * DataCodewordsGroup2
        MaxDataBits = 8 * MaxDataCodewords

        ' total data plus error correction bits
        MaxCodewords = MaxCodewordsArray(QRCodeVersion)

        ' Error correction codewords per block
        ErrCorrCodewords = (MaxCodewords - MaxDataCodewords) / (BlocksGroup1 + BlocksGroup2)

        ' exit
        Return
    End Sub

    
    ' Build Base Matrix
    

    Friend Sub BuildBaseMatrix()
        ' allocate base matrix
        BaseMatrix = New Byte(QRCodeDimension + 5 - 1, QRCodeDimension + 5 - 1) {}

        ' top left finder patterns
        For Row = 0 To 9 - 1

            For Col = 0 To 9 - 1
                BaseMatrix(Row, Col) = FinderPatternTopLeft(Row, Col)
            Next
        Next

        ' top right finder patterns
        Dim Pos = QRCodeDimension - 8

        For Row = 0 To 9 - 1

            For Col = 0 To 8 - 1
                BaseMatrix(Row, Pos + Col) = FinderPatternTopRight(Row, Col)
            Next
        Next

        ' bottom left finder patterns
        For Row = 0 To 8 - 1

            For Col = 0 To 9 - 1
                BaseMatrix(Pos + Row, Col) = FinderPatternBottomLeft(Row, Col)
            Next
        Next

        ' Timing pattern
        For Z = 8 To QRCodeDimension - 8 - 1
            BaseMatrix(6, Z) = If((Z And 1) = 0, FixedBlack, FixedWhite)
            BaseMatrix(Z, 6) = BaseMatrix(6, Z)
        Next

        ' alignment pattern
        If QRCodeVersion > 1 Then
            Dim AlignPos = AlignmentPositionArray(QRCodeVersion)
            Dim AlignmentDimension = AlignPos.Length

            For Row = 0 To AlignmentDimension - 1

                For Col = 0 To AlignmentDimension - 1
                    If Col = 0 AndAlso Row = 0 OrElse Col = AlignmentDimension - 1 AndAlso Row = 0 OrElse Col = 0 AndAlso Row = AlignmentDimension - 1 Then Continue For
                    Dim PosRow As Integer = AlignPos(Row)
                    Dim PosCol As Integer = AlignPos(Col)

                    For ARow = -2 To 3 - 1

                        For ACol = -2 To 3 - 1
                            BaseMatrix(PosRow + ARow, PosCol + ACol) = AlignmentPattern(ARow + 2, ACol + 2)
                        Next
                    Next
                Next
            Next
        End If

        ' reserve version information
        If QRCodeVersion >= 7 Then
            ' position of 3 by 6 rectangles
            Pos = QRCodeDimension - 11

            ' top right
            For Row = 0 To 6 - 1

                For Col = 0 To 3 - 1
                    BaseMatrix(Row, Pos + Col) = FormatWhite
                Next
            Next

            ' bottom right
            For Col = 0 To 6 - 1

                For Row = 0 To 3 - 1
                    BaseMatrix(Pos + Row, Col) = FormatWhite
                Next
            Next
        End If

        Return
    End Sub

    
    ' Apply Mask
    

    Friend Sub ApplyMask(Mask As Integer)
        MaskMatrix = CType(BaseMatrix.Clone(), Byte(,))

        Select Case Mask
            Case 0
                ApplyMask0()
            Case 1
                ApplyMask1()
            Case 2
                ApplyMask2()
            Case 3
                ApplyMask3()
            Case 4
                ApplyMask4()
            Case 5
                ApplyMask5()
            Case 6
                ApplyMask6()
            Case 7
                ApplyMask7()
        End Select

        Return
    End Sub

    
    ' Apply Mask 0
    ' (row + column) % 2 == 0
    

    Friend Sub ApplyMask0()
        For Row = 0 To QRCodeDimension - 1 Step 2

            For Col = 0 To QRCodeDimension - 1 Step 2
                If (MaskMatrix(Row, Col) And NonData) = 0 Then MaskMatrix(Row, Col) = MaskMatrix(Row, Col) Xor 1
                If (MaskMatrix(Row + 1, Col + 1) And NonData) = 0 Then MaskMatrix(Row + 1, Col + 1) = MaskMatrix(Row + 1, Col + 1) Xor 1
            Next
        Next

        Return
    End Sub

    
    ' Apply Mask 1
    ' row % 2 == 0
    

    Friend Sub ApplyMask1()
        For Row = 0 To QRCodeDimension - 1 Step 2

            For Col = 0 To QRCodeDimension - 1
                If (MaskMatrix(Row, Col) And NonData) = 0 Then MaskMatrix(Row, Col) = MaskMatrix(Row, Col) Xor 1
            Next
        Next

        Return
    End Sub

    
    ' Apply Mask 2
    ' column % 3 == 0
    

    Friend Sub ApplyMask2()
        For Row = 0 To QRCodeDimension - 1

            For Col = 0 To QRCodeDimension - 1 Step 3
                If (MaskMatrix(Row, Col) And NonData) = 0 Then MaskMatrix(Row, Col) = MaskMatrix(Row, Col) Xor 1
            Next
        Next

        Return
    End Sub

    
    ' Apply Mask 3
    ' (row + column) % 3 == 0
    

    Friend Sub ApplyMask3()
        For Row = 0 To QRCodeDimension - 1 Step 3

            For Col = 0 To QRCodeDimension - 1 Step 3
                If (MaskMatrix(Row, Col) And NonData) = 0 Then MaskMatrix(Row, Col) = MaskMatrix(Row, Col) Xor 1
                If (MaskMatrix(Row + 1, Col + 2) And NonData) = 0 Then MaskMatrix(Row + 1, Col + 2) = MaskMatrix(Row + 1, Col + 2) Xor 1
                If (MaskMatrix(Row + 2, Col + 1) And NonData) = 0 Then MaskMatrix(Row + 2, Col + 1) = MaskMatrix(Row + 2, Col + 1) Xor 1
            Next
        Next

        Return
    End Sub

    
    ' Apply Mask 4
    ' ((row / 2) + (column / 3)) % 2 == 0
    

    Friend Sub ApplyMask4()
        For Row = 0 To QRCodeDimension - 1 Step 4

            For Col = 0 To QRCodeDimension - 1 Step 6
                If (MaskMatrix(Row, Col) And NonData) = 0 Then MaskMatrix(Row, Col) = MaskMatrix(Row, Col) Xor 1
                If (MaskMatrix(Row, Col + 1) And NonData) = 0 Then MaskMatrix(Row, Col + 1) = MaskMatrix(Row, Col + 1) Xor 1
                If (MaskMatrix(Row, Col + 2) And NonData) = 0 Then MaskMatrix(Row, Col + 2) = MaskMatrix(Row, Col + 2) Xor 1
                If (MaskMatrix(Row + 1, Col) And NonData) = 0 Then MaskMatrix(Row + 1, Col) = MaskMatrix(Row + 1, Col) Xor 1
                If (MaskMatrix(Row + 1, Col + 1) And NonData) = 0 Then MaskMatrix(Row + 1, Col + 1) = MaskMatrix(Row + 1, Col + 1) Xor 1
                If (MaskMatrix(Row + 1, Col + 2) And NonData) = 0 Then MaskMatrix(Row + 1, Col + 2) = MaskMatrix(Row + 1, Col + 2) Xor 1
                If (MaskMatrix(Row + 2, Col + 3) And NonData) = 0 Then MaskMatrix(Row + 2, Col + 3) = MaskMatrix(Row + 2, Col + 3) Xor 1
                If (MaskMatrix(Row + 2, Col + 4) And NonData) = 0 Then MaskMatrix(Row + 2, Col + 4) = MaskMatrix(Row + 2, Col + 4) Xor 1
                If (MaskMatrix(Row + 2, Col + 5) And NonData) = 0 Then MaskMatrix(Row + 2, Col + 5) = MaskMatrix(Row + 2, Col + 5) Xor 1
                If (MaskMatrix(Row + 3, Col + 3) And NonData) = 0 Then MaskMatrix(Row + 3, Col + 3) = MaskMatrix(Row + 3, Col + 3) Xor 1
                If (MaskMatrix(Row + 3, Col + 4) And NonData) = 0 Then MaskMatrix(Row + 3, Col + 4) = MaskMatrix(Row + 3, Col + 4) Xor 1
                If (MaskMatrix(Row + 3, Col + 5) And NonData) = 0 Then MaskMatrix(Row + 3, Col + 5) = MaskMatrix(Row + 3, Col + 5) Xor 1
            Next
        Next

        Return
    End Sub

    
    ' Apply Mask 5
    ' ((row * column) % 2) + ((row * column) % 3) == 0
    

    Friend Sub ApplyMask5()
        For Row = 0 To QRCodeDimension - 1 Step 6

            For Col = 0 To QRCodeDimension - 1 Step 6

                For Delta = 0 To 6 - 1
                    If (MaskMatrix(Row, Col + Delta) And NonData) = 0 Then MaskMatrix(Row, Col + Delta) = MaskMatrix(Row, Col + Delta) Xor 1
                Next

                For Delta = 1 To 6 - 1
                    If (MaskMatrix(Row + Delta, Col) And NonData) = 0 Then MaskMatrix(Row + Delta, Col) = MaskMatrix(Row + Delta, Col) Xor 1
                Next

                If (MaskMatrix(Row + 2, Col + 3) And NonData) = 0 Then MaskMatrix(Row + 2, Col + 3) = MaskMatrix(Row + 2, Col + 3) Xor 1
                If (MaskMatrix(Row + 3, Col + 2) And NonData) = 0 Then MaskMatrix(Row + 3, Col + 2) = MaskMatrix(Row + 3, Col + 2) Xor 1
                If (MaskMatrix(Row + 3, Col + 4) And NonData) = 0 Then MaskMatrix(Row + 3, Col + 4) = MaskMatrix(Row + 3, Col + 4) Xor 1
                If (MaskMatrix(Row + 4, Col + 3) And NonData) = 0 Then MaskMatrix(Row + 4, Col + 3) = MaskMatrix(Row + 4, Col + 3) Xor 1
            Next
        Next

        Return
    End Sub

    
    ' Apply Mask 6
    ' (((row * column) % 2) + ((row * column) mod 3)) mod 2 == 0
    

    Friend Sub ApplyMask6()
        For Row = 0 To QRCodeDimension - 1 Step 6

            For Col = 0 To QRCodeDimension - 1 Step 6

                For Delta = 0 To 6 - 1
                    If (MaskMatrix(Row, Col + Delta) And NonData) = 0 Then MaskMatrix(Row, Col + Delta) = MaskMatrix(Row, Col + Delta) Xor 1
                Next

                For Delta = 1 To 6 - 1
                    If (MaskMatrix(Row + Delta, Col) And NonData) = 0 Then MaskMatrix(Row + Delta, Col) = MaskMatrix(Row + Delta, Col) Xor 1
                Next

                If (MaskMatrix(Row + 1, Col + 1) And NonData) = 0 Then MaskMatrix(Row + 1, Col + 1) = MaskMatrix(Row + 1, Col + 1) Xor 1
                If (MaskMatrix(Row + 1, Col + 2) And NonData) = 0 Then MaskMatrix(Row + 1, Col + 2) = MaskMatrix(Row + 1, Col + 2) Xor 1
                If (MaskMatrix(Row + 2, Col + 1) And NonData) = 0 Then MaskMatrix(Row + 2, Col + 1) = MaskMatrix(Row + 2, Col + 1) Xor 1
                If (MaskMatrix(Row + 2, Col + 3) And NonData) = 0 Then MaskMatrix(Row + 2, Col + 3) = MaskMatrix(Row + 2, Col + 3) Xor 1
                If (MaskMatrix(Row + 2, Col + 4) And NonData) = 0 Then MaskMatrix(Row + 2, Col + 4) = MaskMatrix(Row + 2, Col + 4) Xor 1
                If (MaskMatrix(Row + 3, Col + 2) And NonData) = 0 Then MaskMatrix(Row + 3, Col + 2) = MaskMatrix(Row + 3, Col + 2) Xor 1
                If (MaskMatrix(Row + 3, Col + 4) And NonData) = 0 Then MaskMatrix(Row + 3, Col + 4) = MaskMatrix(Row + 3, Col + 4) Xor 1
                If (MaskMatrix(Row + 4, Col + 2) And NonData) = 0 Then MaskMatrix(Row + 4, Col + 2) = MaskMatrix(Row + 4, Col + 2) Xor 1
                If (MaskMatrix(Row + 4, Col + 3) And NonData) = 0 Then MaskMatrix(Row + 4, Col + 3) = MaskMatrix(Row + 4, Col + 3) Xor 1
                If (MaskMatrix(Row + 4, Col + 5) And NonData) = 0 Then MaskMatrix(Row + 4, Col + 5) = MaskMatrix(Row + 4, Col + 5) Xor 1
                If (MaskMatrix(Row + 5, Col + 4) And NonData) = 0 Then MaskMatrix(Row + 5, Col + 4) = MaskMatrix(Row + 5, Col + 4) Xor 1
                If (MaskMatrix(Row + 5, Col + 5) And NonData) = 0 Then MaskMatrix(Row + 5, Col + 5) = MaskMatrix(Row + 5, Col + 5) Xor 1
            Next
        Next

        Return
    End Sub

    
    ' Apply Mask 7
    ' (((row + column) % 2) + ((row * column) mod 3)) mod 2 == 0
    

    Friend Sub ApplyMask7()
        For Row = 0 To QRCodeDimension - 1 Step 6

            For Col = 0 To QRCodeDimension - 1 Step 6
                If (MaskMatrix(Row, Col) And NonData) = 0 Then MaskMatrix(Row, Col) = MaskMatrix(Row, Col) Xor 1
                If (MaskMatrix(Row, Col + 2) And NonData) = 0 Then MaskMatrix(Row, Col + 2) = MaskMatrix(Row, Col + 2) Xor 1
                If (MaskMatrix(Row, Col + 4) And NonData) = 0 Then MaskMatrix(Row, Col + 4) = MaskMatrix(Row, Col + 4) Xor 1
                If (MaskMatrix(Row + 1, Col + 3) And NonData) = 0 Then MaskMatrix(Row + 1, Col + 3) = MaskMatrix(Row + 1, Col + 3) Xor 1
                If (MaskMatrix(Row + 1, Col + 4) And NonData) = 0 Then MaskMatrix(Row + 1, Col + 4) = MaskMatrix(Row + 1, Col + 4) Xor 1
                If (MaskMatrix(Row + 1, Col + 5) And NonData) = 0 Then MaskMatrix(Row + 1, Col + 5) = MaskMatrix(Row + 1, Col + 5) Xor 1
                If (MaskMatrix(Row + 2, Col) And NonData) = 0 Then MaskMatrix(Row + 2, Col) = MaskMatrix(Row + 2, Col) Xor 1
                If (MaskMatrix(Row + 2, Col + 4) And NonData) = 0 Then MaskMatrix(Row + 2, Col + 4) = MaskMatrix(Row + 2, Col + 4) Xor 1
                If (MaskMatrix(Row + 2, Col + 5) And NonData) = 0 Then MaskMatrix(Row + 2, Col + 5) = MaskMatrix(Row + 2, Col + 5) Xor 1
                If (MaskMatrix(Row + 3, Col + 1) And NonData) = 0 Then MaskMatrix(Row + 3, Col + 1) = MaskMatrix(Row + 3, Col + 1) Xor 1
                If (MaskMatrix(Row + 3, Col + 3) And NonData) = 0 Then MaskMatrix(Row + 3, Col + 3) = MaskMatrix(Row + 3, Col + 3) Xor 1
                If (MaskMatrix(Row + 3, Col + 5) And NonData) = 0 Then MaskMatrix(Row + 3, Col + 5) = MaskMatrix(Row + 3, Col + 5) Xor 1
                If (MaskMatrix(Row + 4, Col) And NonData) = 0 Then MaskMatrix(Row + 4, Col) = MaskMatrix(Row + 4, Col) Xor 1
                If (MaskMatrix(Row + 4, Col + 1) And NonData) = 0 Then MaskMatrix(Row + 4, Col + 1) = MaskMatrix(Row + 4, Col + 1) Xor 1
                If (MaskMatrix(Row + 4, Col + 2) And NonData) = 0 Then MaskMatrix(Row + 4, Col + 2) = MaskMatrix(Row + 4, Col + 2) Xor 1
                If (MaskMatrix(Row + 5, Col + 1) And NonData) = 0 Then MaskMatrix(Row + 5, Col + 1) = MaskMatrix(Row + 5, Col + 1) Xor 1
                If (MaskMatrix(Row + 5, Col + 2) And NonData) = 0 Then MaskMatrix(Row + 5, Col + 2) = MaskMatrix(Row + 5, Col + 2) Xor 1
                If (MaskMatrix(Row + 5, Col + 3) And NonData) = 0 Then MaskMatrix(Row + 5, Col + 3) = MaskMatrix(Row + 5, Col + 3) Xor 1
            Next
        Next

        Return
    End Sub
End Class
