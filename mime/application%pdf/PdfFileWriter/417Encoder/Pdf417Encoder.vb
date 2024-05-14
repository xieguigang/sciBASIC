#Region "Microsoft.VisualBasic::74f17e7be61e13776d980c41829760a7, mime\application%pdf\PdfFileWriter\417Encoder\Pdf417Encoder.vb"

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

    '   Total Lines: 1290
    '    Code Lines: 689
    ' Comment Lines: 374
    '   Blank Lines: 227
    '     File Size: 46.88 KB


    ' Enum EncodingControl
    ' 
    '     Auto, ByteOnly, TextAndByte
    ' 
    '  
    ' 
    ' 
    ' 
    ' Enum ErrorCorrectionLevel
    ' 
    '     AutoHigh, AutoLow, AutoMedium, AutoNormal, Level_0
    '     Level_1, Level_2, Level_3, Level_4, Level_5
    '     Level_6, Level_7, Level_8
    ' 
    '  
    ' 
    ' 
    ' 
    ' Class Pdf417Encoder
    ' 
    ' 
    '     Enum EncodingMode
    ' 
    '         [Byte], Numeric, Text
    ' 
    ' 
    ' 
    '     Enum TextEncodingMode
    ' 
    '         Lower, Mixed, Punct, ShiftPunct, ShiftUpper
    '         Upper
    ' 
    '  
    ' 
    ' 
    ' 
    '  
    ' 
    '     Properties: BarColumns, DataColumns, DataRows, DefaultDataColumns, EncodingControl
    '                 ErrorCorrection, GlobalLabelIDCharacterSet, GlobalLabelIDGeneralPurpose, GlobalLabelIDUserDefined, ImageHeight
    '                 ImageWidth, NarrowBarWidth, QuietZone, RowHeight
    ' 
    '     Function: ConvertBarcodeMatrixToPixels, CountBytes, CountDigits, CountPunctuation, CountText
    '               CreateBarcodeMatrix, SetDataColumns, SetDataRows, WidthToHeightRatio
    ' 
    '     Sub: CalculateErrorCorrection, CodewordToModules, DataEncoding, (+2 Overloads) Encode, EncodeByteSegment
    '          EncodeNumericSegment, EncodeTextSegment, SetDataRowsAndColumns, SetErrorCorrectionLevel, SetImageWidthAndHeight
    ' 
    ' /********************************************************************************/

#End Region

'
'
'	PDF417 Barcode Encoder
'
'	Pdf417Encoder class
'
'	Author: Uzi Granot
'	Version: 2.0
'	Date: May 7, 2019
'	Copyright (C) 2019 Uzi Granot. All Rights Reserved
'
'	PDF417 barcode encoder class and the attached test/demo
'  applications are free software.
'	Software developed by this author is licensed under CPOL 1.02.
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
'	Version History
'	---------------
'
'	Version 1.0 2019/04/01
'		Original version
'	Version 1.1 2019/04/15
'		Remove icones from form windows to solve VS 2019 security
'	Version 2.0 2019/05/07
'		Add support for .NET framework and .NET standard
'

Imports System
Imports System.Collections.Generic
Imports System.Numerics
Imports System.Text
Imports stdNum = System.Math

'namespace Pdf417EncoderLibrary

''' <summary>
''' PDF417 Encoding control
''' </summary>
Public Enum EncodingControl
    ''' <summary>
    ''' Auto encoding control
    ''' </summary>
    Auto

    ''' <summary>
    ''' Encode all as bytes
    ''' </summary>
    ByteOnly

    ''' <summary>
    ''' Encode all as text and bytes
    ''' </summary>
    TextAndByte
End Enum

''' <summary>
''' PDF417 Error correction level
''' </summary>
Public Enum ErrorCorrectionLevel
    ''' <summary>
    ''' Error correction level 0 (2 correction codewords)
    ''' </summary>
    Level_0
    ''' <summary>
    ''' Error correction level 1 (4 correction codewords)
    ''' </summary>
    Level_1
    ''' <summary>
    ''' Error correction level 2 (8 correction codewords)
    ''' </summary>
    Level_2
    ''' <summary>
    ''' Error correction level 3 (16 correction codewords)
    ''' </summary>
    Level_3
    ''' <summary>
    ''' Error correction level 4 (32 correction codewords)
    ''' </summary>
    Level_4
    ''' <summary>
    ''' Error correction level 5 (64 correction codewords)
    ''' </summary>
    Level_5
    ''' <summary>
    ''' Error correction level 6 (128 correction codewords)
    ''' </summary>
    Level_6
    ''' <summary>
    ''' Error correction level 7 (256 correction codewords)
    ''' </summary>
    Level_7
    ''' <summary>
    ''' Error correction level 8 (512 correction codewords)
    ''' </summary>
    Level_8
    ''' <summary>
    ''' Recommended level less one
    ''' </summary>
    AutoLow
    ''' <summary>
    ''' Recomended level based on number of codewords
    ''' </summary>
    AutoNormal
    ''' <summary>
    ''' Recommended level plus one
    ''' </summary>
    AutoMedium
    ''' <summary>
    ''' Recommended level plus two
    ''' </summary>
    AutoHigh
End Enum

''' <summary>
''' PDF417 Encoder class
''' </summary>
Public Class Pdf417Encoder
    Inherits Pdf417EncoderTables

    ' encoding mode

    Private Enum EncodingMode
        [Byte]
        Text
        Numeric
    End Enum

    ' text encoding sub-mode
    Private Enum TextEncodingMode
        Upper
        Lower
        Mixed
        Punct
        ShiftUpper
        ShiftPunct
    End Enum

    ''' <summary>
    ''' Version number and date
    ''' </summary>
    Public Const VersionNumber As String = "Rev 2.2.0 - 2019-05-26"

    ''' <summary>
    ''' Data rows
    ''' </summary>
    Public Property DataRows As Integer

    ''' <summary>
    ''' Data columns
    ''' </summary>
    Public Property DataColumns As Integer

    ''' <summary>
    ''' Returns the barcode width in terms of narrow bars
    ''' </summary>
    Public Property BarColumns As Integer

    ''' <summary>
    ''' Barcode image width in pixels
    ''' </summary>
    Public Property ImageWidth As Integer

    ''' <summary>
    ''' Barcode image height in pixels
    ''' </summary>
    Public Property ImageHeight As Integer

    ''' <summary>
    ''' Barcode matrix (each bar is one bool item)
    ''' </summary>
    Public Pdf417BarcodeMatrix As Boolean(,)

    ' Pdf417 constants
    Private Const MaxCodewords As Integer = 929
    Private Const [MOD] As Integer = MaxCodewords
    Private Const ModulesInCodeword As Integer = 17
    Private Const DataRowsMin As Integer = 3
    Private Const DataRowsMax As Integer = 90
    Private Const DataColumnsMin As Integer = 1
    Private Const DataColumnsMax As Integer = 30
    Private Shared ReadOnly StartCodeword As Boolean() = {True, True, True, True, True, True, True, True, False, True, False, True, False, True, False, False, False}
    Private Const StartPatternLen As Integer = ModulesInCodeword
    Private Shared ReadOnly StopCodeword As Boolean() = {True, True, True, True, True, True, True, False, True, False, False, False, True, False, True, False, False, True}
    Private Const StopPatternLen As Integer = ModulesInCodeword + 1

    ' Control codewords
    Private Const SwitchToTextMode As Integer = 900
    Private Const SwitchToByteMode As Integer = 901
    Private Const SwitchToNumericMode As Integer = 902
    Private Const ShiftToByteMode As Integer = 913
    Private Const SwitchToByteModeForSix As Integer = 924

    ' User-Defined GLis:
    ' Codeword 925 followed by one data codeword.
    ' The data codeword has a range of 0 to 899.
    Private Const GliUserDefined As Integer = 925

    ' General Purpose GLis:
    ' Codeword 926 followed by two data codewords.
    ' The two data codewords have a range of 0 to 899 each.
    Private Const GliGeneralPurpose As Integer = 926

    ' International character set:
    ' Codeword 927 followed by a single data codeword.
    ' The data codeword has a range of 0 to 899.
    ' In this class the data codeword is a function
    ' of ISO 8859 part number. The class supports
    ' ISO-8859-n (1-9, 13, 15) The default is ISO-8859-1.
    ' The codeword value is Part + 2.
    Private Const GliCharacterSet As Integer = 927

    ' saved barcode string and binary data
    Private BarcodeStringData As String
    Private BarcodeBinaryData As Byte()
    Private BarcodeDataLength As Integer
    Private BarcodeDataPos As Integer

    ' encoding input data into codewords
    Private _EncodingMode As EncodingMode
    Private _TextEncodingMode As TextEncodingMode
    Private DataCodewords As List(Of Integer)

    ' error correction level, length and codewords
    Private ErrorCorrectionLevel As ErrorCorrectionLevel
    Private ErrorCorrectionLength As Integer
    Private ErrCorrCodewords As Integer()

    ''' <summary>
    ''' Encoding control (Default: Auto)
    ''' </summary>
    Public Property EncodingControl As EncodingControl
        Get
            Return _EncodingControl
        End Get
        Set(value As EncodingControl)
            ' test symbol encoding
            If value <> EncodingControl.Auto AndAlso value <> EncodingControl.ByteOnly AndAlso value <> EncodingControl.TextAndByte Then Throw New ArgumentException("PDF417 Encoding control must be Auto," & Microsoft.VisualBasic.Constants.vbCrLf & "ByteOnly or TextAndByte. Default is Auto.")

            ' save error correction level
            _EncodingControl = value
            Return
        End Set
    End Property

    Private _EncodingControl As EncodingControl = EncodingControl.Auto

    ''' <summary>
    ''' Error correction level (Default: AutoNormal)
    ''' </summary>
    Public Property ErrorCorrection As ErrorCorrectionLevel
        Get
            Return _ErrorCorrection
        End Get
        Set(value As ErrorCorrectionLevel)
            ' test error correction
            If value < ErrorCorrectionLevel.Level_0 OrElse value > ErrorCorrectionLevel.AutoHigh Then
                Throw New ArgumentException("PDF417 Error correction request is invalid." & Microsoft.VisualBasic.Constants.vbCrLf & "Default is Auto normal.")
            End If

            ' save error correction level
            _ErrorCorrection = value
            Return
        End Set
    End Property

    Private _ErrorCorrection As ErrorCorrectionLevel = ErrorCorrectionLevel.AutoNormal

    ''' <summary>
    ''' Narrow bar width in pixels (Default: 2)
    ''' </summary>
    Public Property NarrowBarWidth As Integer
        Get
            Return _BarWidthPix
        End Get
        Set(value As Integer)
            If value < 1 OrElse value > 100 Then
                Throw New ArgumentException("PDF417 Narrow bar width must be one or more" & Microsoft.VisualBasic.Constants.vbCrLf & "Default is two.")
            End If

            _BarWidthPix = value

            ' row height must be at least 3 times the width of narrow bar
            If _RowHeightPix < 3 * value Then _RowHeightPix = 3 * value

            ' quiet zone must be at least 2 times the width of narrow bar
            If _QuietZonePix < 2 * value Then _QuietZonePix = 2 * value

            ' set image width and height
            SetImageWidthAndHeight()
            Return
        End Set
    End Property

    Private _BarWidthPix As Integer = 2

    ''' <summary>
    ''' Row height in pixels (Default: 6)
    ''' </summary>
    Public Property RowHeight As Integer
        Get
            Return _RowHeightPix
        End Get
        Set(value As Integer)
            If value < 3 * _BarWidthPix OrElse value > 300 Then
                Throw New ArgumentException("PDF417 Row height must be at least 3 times the narrow bar width." & Microsoft.VisualBasic.Constants.vbCrLf & "Default is six.")
            End If

            _RowHeightPix = value

            ' set image width and height
            SetImageWidthAndHeight()
            Return
        End Set
    End Property

    Private _RowHeightPix As Integer = 6

    ''' <summary>
    ''' Quiet zone around the barcode in pixels (Default: 4)
    ''' </summary>
    Public Property QuietZone As Integer
        Get
            Return _QuietZonePix
        End Get
        Set(value As Integer)
            If value < 2 * _BarWidthPix OrElse value > 200 Then
                Throw New ArgumentException("PDF417 Quiet zone must be at least 2 times the narrow bar width." & Microsoft.VisualBasic.Constants.vbCrLf & "Default is four.")
            End If

            _QuietZonePix = value

            ' set image width and height
            SetImageWidthAndHeight()
            Return
        End Set
    End Property

    Private _QuietZonePix As Integer = 4

    ''' <summary>
    ''' Default number of data columns (Default: 3)
    ''' </summary>
    Public Property DefaultDataColumns As Integer
        Get
            Return _DefaultDataColumns
        End Get
        Set(value As Integer)
            If value < DataColumnsMin OrElse value > DataColumnsMax Then
                Throw New ArgumentException("PDF417 Default data columns must be 1 to 30." & Microsoft.VisualBasic.Constants.vbCrLf & "Default is three.")
            End If

            _DefaultDataColumns = value
            Return
        End Set
    End Property

    Private _DefaultDataColumns As Integer = 3

    ''' <summary>
    ''' ISO character set ISO-8859-n (n=1 to 9, 13 and 15) (Default: null)
    ''' </summary>
    Public Property GlobalLabelIDCharacterSet As String
        Get
            Return _GlobalLabelIDCharacterSet
        End Get
        Set(value As String)

            If String.IsNullOrWhiteSpace(value) Then
                _GlobalLabelIDCharacterSet = Nothing
                Return
            End If

            Dim Part As Integer = Nothing

            If String.Compare(value.Substring(0, 9), "ISO-8859-", True) <> 0 OrElse Not Integer.TryParse(value.Substring(9), Part) OrElse Part < 1 OrElse Part > 9 AndAlso Part <> 13 AndAlso Part <> 15 Then
                Throw New ArgumentException("PDF417 Character set code in error." & Microsoft.VisualBasic.Constants.vbCrLf & "Must be ISO-8859-n (1-9, 13, 15)." & Microsoft.VisualBasic.Constants.vbCrLf & "Default is ISO-8859-1.")
            End If

            _GlobalLabelIDCharacterSet = value
            _GlobalLabelIDCharacterSetNo = Part + 2
            Return
        End Set
    End Property
    ' Global Label Identifier character set
    Private _GlobalLabelIDCharacterSet As String
    Private _GlobalLabelIDCharacterSetNo As Integer

    ''' <summary>
    ''' Global label ID user defined (Default: 0)
    ''' </summary>
    Public Property GlobalLabelIDUserDefined As Integer
        Get
            Return _GlobalLabelIDUserDefined
        End Get
        Set(value As Integer)
            If value <> 0 AndAlso (value < 810900 OrElse value > 811799) Then Throw New ArgumentException("PDF417 Global label identifier user defined value." & Microsoft.VisualBasic.Constants.vbCrLf & "Must be 810900 to 811799 or zero" & Microsoft.VisualBasic.Constants.vbCrLf & "Default is not used or zero value")
            _GlobalLabelIDUserDefined = value
            Return
        End Set
    End Property

    Private _GlobalLabelIDUserDefined As Integer = 0

    ''' <summary>
    ''' Global label ID general purpose (Default: 0)
    ''' </summary>
    Public Property GlobalLabelIDGeneralPurpose As Integer
        Get
            Return _GlobalLabelIDGeneralPurpose
        End Get
        Set(value As Integer)
            If value <> 0 AndAlso (value < 900 OrElse value > 810899) Then Throw New ArgumentException("PDF417 Global label identifier general purpose value." & Microsoft.VisualBasic.Constants.vbCrLf & "Must be 900 to 810899 or zero" & Microsoft.VisualBasic.Constants.vbCrLf & "Default is not used or zero value")
            _GlobalLabelIDGeneralPurpose = value
            Return
        End Set
    End Property
    ' code word 926 value
    Private _GlobalLabelIDGeneralPurpose As Integer = 0

    ''' <summary>
    ''' Encode unicode string
    ''' </summary>
    ''' <param name="StringData">Input text string</param>
    Public Sub Encode(StringData As String)
        ' argument error
        If String.IsNullOrEmpty(StringData) Then Throw New ArgumentException("PDF417 Input barcode data string is null or empty.")

        ' save argument
        BarcodeStringData = StringData

        ' convert text string to byte array
        Dim ISO = Encoding.GetEncoding(If(_GlobalLabelIDCharacterSet, "ISO-8859-1"))
        Dim UtfBytes = Encoding.UTF8.GetBytes(StringData)
        Dim IsoBytes = Encoding.Convert(Encoding.UTF8, ISO, UtfBytes)

        ' encode binary data
        Encode(IsoBytes)
        Return
    End Sub

    ''' <summary>
    ''' Encode binary bytes array
    ''' </summary>
    ''' <param name="BinaryData">Input binary byte array</param>
    Public Sub Encode(BinaryData As Byte())
        ' reset barcode matrix
        Pdf417BarcodeMatrix = Nothing
        Me.DataRows = 0
        Me.DataColumns = 0

        ' test data segments array
        If BinaryData Is Nothing OrElse BinaryData.Length = 0 Then Throw New ArgumentNullException("PDF417 Input binary barcode data is null or empty")

        ' save data segments array
        BarcodeBinaryData = BinaryData

        ' data encoding
        ' convert the byte array into Pdf417 codewords
        DataEncoding()

        ' set error correction level
        SetErrorCorrectionLevel()

        ' set data columns and data rows based on the default data columns
        ' this calculation adds DataColumns - 1 for rounding up
        Dim dataColumns = _DefaultDataColumns
        Dim dataRows As Integer = (DataCodewords.Count + ErrorCorrectionLength + dataColumns - 1) / dataColumns

        ' if data rows exceeds the maximum allowed,
        ' adjust data columns to reduce the number of rows
        If dataRows > DataRowsMax Then
            dataRows = DataRowsMax
            dataColumns = (DataCodewords.Count + ErrorCorrectionLength + dataRows - 1) / dataRows
            If dataColumns > DataColumnsMax Then Throw New ApplicationException("PDF417 barcode data overflow")
        End If

        ' save data rows and data columns in the class
        SetDataRowsAndColumns(dataRows, dataColumns)
        Return
    End Sub

    ' 
    ' Documentation for the quadratic equation solution below
    ' -------------------------------------------------------
    ' ImageWidth = _BarWidthPix * (ModulesInCodeword * (DataColumns + 4) + 1) + 2 * _QuietZonePix;
    ' ImageHeight = _RowHeightPix * DataRows + 2 * _QuietZonePix;
    ' 
    ' Ratio = ImageWidth / ImageHeight;
    ' 
    ' Ratio * (_RowHeightPix * DataRows + 2 * _QuietZonePix) = _BarWidthPix * (ModulesInCodeword * (DataColumns + 4) + 1) + 2 * _QuietZonePix
    ' 
    ' Ratio * _RowHeightPix * DataRows + 2 * Ratio * _QuietZonePix =
    ' 	 _BarWidthPix * ModulesInCodeword * (DataColumns + 4) + _BarWidthPix + 2 * _QuietZonePix
    ' 
    ' Ratio * _RowHeightPix * DataRows + 2 * Ratio * _QuietZonePix =
    ' 	 _BarWidthPix * ModulesInCodeword * DataColumns + 4 * _BarWidthPix * ModulesInCodeword + _BarWidthPix + 2 * _QuietZonePix
    ' 
    ' Ratio * _RowHeightPix * DataRows - _BarWidthPix * ModulesInCodeword * DataColumns =
    ' 	 4 * _BarWidthPix * ModulesInCodeword + _BarWidthPix + 2 * _QuietZonePix - 2 * Ratio * _QuietZonePix
    ' 
    ' DataRows * DataColumns = DataCodewords.Count + ErrorCorrectionLength
    ' 
    ' Total = DataCodewords.Count + ErrorCorrectionLength
    ' 
    ' DataRows * DataColumns = Total
    ' 
    ' A = _BarWidthPix * ModulesInCodeword
    ' B = _BarWidthPix * (4 * ModulesInCodeword + 1) + 2 * _QuietZonePix * (1 - Ratio)
    ' C = Ratio * _RowHeightPix
    ' 
    ' C * DataRows - A * DataColumns = B;
    ' C * Total / DataColumns - A * DataColumns = B;
    ' A * DataColumns**2 + B * DataColumns - C * Total = 0;
    ' 

    ''' <summary>
    ''' Adjust rows and columns to achive width to height ratio
    ''' </summary>
    ''' <param name="Ratio">Requested width to height ratio</param>
    ''' <returns>Success or failure result</returns>
    Public Function WidthToHeightRatio(Ratio As Double) As Boolean
        Try
            ' total of data and error correction but no padding
            Dim Total = DataCodewords.Count + ErrorCorrectionLength
            Dim A As Double = _BarWidthPix * ModulesInCodeword
            Dim B = _BarWidthPix * (4 * ModulesInCodeword + 1) - 2 * _QuietZonePix * (Ratio - 1)
            Dim C = Total * Ratio * _RowHeightPix

            ' initial guess for columns
            Dim Columns = (-B + stdNum.Sqrt(B * B + 4 * A * C)) / (2 * A)

            ' calculated data columns and rows to meet the width to height ratio
            ' this calculation adds DataColumns - 1 for rounding up
            Dim dataColumns As Integer = stdNum.Round(Columns, 0, MidpointRounding.AwayFromZero)
            If dataColumns < DataColumnsMin OrElse dataColumns > DataColumnsMax Then Return False
            Dim dataRows As Integer = (Total + dataColumns - 1) / dataColumns
            If dataRows < DataRowsMin OrElse dataRows > DataRowsMax Then Return False

            ' save data rows and data columns in the class
            SetDataRowsAndColumns(dataRows, dataColumns)
            Return True
        Catch
            Return False
        End Try
    End Function

    ''' <summary>
    ''' Set number of data columns and data rows 
    ''' </summary>
    ''' <param name="dataColumns">Data columns</param>
    ''' <returns>Success or failure result</returns>
    Public Function SetDataColumns(dataColumns As Integer) As Boolean
        Try
            ' columns outside valid range
            If dataColumns < DataColumnsMin OrElse dataColumns > DataColumnsMax Then Return False

            ' calculate rows
            Dim dataRows As Integer = (DataCodewords.Count + ErrorCorrectionLength + dataColumns - 1) / dataColumns
            If dataRows < DataRowsMin OrElse dataRows > DataRowsMax Then Return False

            ' save data rows and data columns in the class
            SetDataRowsAndColumns(dataRows, dataColumns)
            Pdf417BarcodeMatrix = Nothing
            Return True
        Catch
            Return False
        End Try
    End Function

    ''' <summary>
    ''' Set number of data rows and data columns 
    ''' </summary>
    ''' <param name="dataRows">Data rowss</param>
    ''' <returns>Success or failure result</returns>
    Public Function SetDataRows(dataRows As Integer) As Boolean
        Try
            ' rows outside valid range
            If dataRows < DataRowsMin OrElse dataRows > DataRowsMax Then Return False

            ' calculate columns
            Dim dataColumns As Integer = (DataCodewords.Count + ErrorCorrectionLength + dataRows - 1) / dataRows
            If dataColumns < DataColumnsMin OrElse dataColumns > DataColumnsMax Then Return False

            ' set rows and columns
            SetDataRowsAndColumns(dataRows, dataColumns)
            Return True
        Catch
            Return False
        End Try
    End Function

    Private Sub SetDataRowsAndColumns(dataRows As Integer, dataColumns As Integer)
        ' test for change
        If dataColumns <> Me.DataColumns OrElse dataRows <> Me.DataRows Then
            ' set rows and columns
            Me.DataRows = dataRows
            Me.DataColumns = dataColumns
            Pdf417BarcodeMatrix = Nothing
            SetImageWidthAndHeight()
        End If

        Return
    End Sub

    Private Sub SetImageWidthAndHeight()
        BarColumns = ModulesInCodeword * (DataColumns + 4) + 1
        ImageWidth = _BarWidthPix * BarColumns + 2 * _QuietZonePix
        ImageHeight = _RowHeightPix * DataRows + 2 * _QuietZonePix
        Return
    End Sub

    ''' <summary>
    ''' convert black and white matrix to black and white image
    ''' </summary>
    ''' <returns>Black and white image in pixels</returns>
    Public Function ConvertBarcodeMatrixToPixels() As Boolean(,)
        ' create barcode matrix
        If Pdf417BarcodeMatrix Is Nothing Then Pdf417BarcodeMatrix = CreateBarcodeMatrix()

        ' Pdf417Matrix width and height
        Dim MatrixWidth = BarColumns
        Dim MatrixHeight = DataRows

        ' output matrix size in pixels all matrix elements are white (false)
        Dim BWImage = New Boolean(ImageHeight - 1, ImageWidth - 1) {}
        Dim XOffset = _QuietZonePix
        Dim YOffset = _QuietZonePix

        ' convert result matrix to output matrix
        For Row = 0 To MatrixHeight - 1

            For Col = 0 To MatrixWidth - 1
                ' bar is black
                If Pdf417BarcodeMatrix(Row, Col) Then
                    For Y = 0 To RowHeight - 1

                        For X = 0 To _BarWidthPix - 1
                            BWImage(YOffset + Y, XOffset + X) = True
                        Next
                    Next
                End If

                XOffset += _BarWidthPix
            Next

            XOffset = _QuietZonePix
            YOffset += RowHeight
        Next

        Return BWImage
    End Function

    ''' <summary>
    ''' Create black and white boolean matrix of the barcode image
    ''' </summary>
    ''' <returns>bool matrix [row, col] true=black, false=white</returns>
    Public Function CreateBarcodeMatrix() As Boolean(,)
        ' create empty codewords array
        Dim Codewords = New Integer(DataRows * DataColumns - 1) {}

        ' length of data codewords plus length codeword
        Dim DataLength = DataCodewords.Count

        ' move data codewords
        For Index = 0 To DataLength - 1
            Codewords(Index) = DataCodewords(Index)
        Next

        ' calculate padding
        Dim PaddingCodewordsCount = DataRows * DataColumns - DataCodewords.Count - ErrorCorrectionLength

        ' add padding if needed
        For Index = 0 To PaddingCodewordsCount - 1
            Codewords(stdNum.Min(Threading.Interlocked.Increment(DataLength), DataLength - 1)) = SwitchToTextMode
        Next

        ' set length (codewords plus padding)
        Codewords(0) = DataLength

        ' calculate error correction and move it to codewords array
        CalculateErrorCorrection(Codewords)

        ' create output boolean matrix (Black=true, White=false)
        ' not including quiet zone
        Dim Pdf417Matrix = New Boolean(DataRows - 1, BarColumns - 1) {}

        ' last data row
        Dim LastRow = DataRows - 1

        ' fill output boolean matrix
        Dim StopCol = BarColumns - StopPatternLen
        Dim RightRowIndCol = StopCol - ModulesInCodeword

        For Row = 0 To DataRows - 1
            ' move start codeword at the start of each row
            For Col = 0 To StartPatternLen - 1
                Pdf417Matrix(Row, Col) = StartCodeword(Col)
            Next

            ' move stop codeword at the end of each row
            For Col = 0 To StopPatternLen - 1
                Pdf417Matrix(Row, StopCol + Col) = StopCodeword(Col)
            Next

            ' calculate left and right row indicators
            Dim LeftRowInd As Integer = 30 * (Row / 3)
            Dim RightRowInd = LeftRowInd
            Dim Cluster = Row Mod 3

            ' cluster 0
            If Cluster = 0 Then
                LeftRowInd += LastRow / 3
                RightRowInd += DataColumns - 1

                ' cluster 1
            ElseIf Cluster = 1 Then
                LeftRowInd += ErrorCorrectionLevel * 3 + LastRow Mod 3

                ' cluster 2
                RightRowInd += LastRow / 3
            Else
                LeftRowInd += DataColumns - 1
                RightRowInd += ErrorCorrectionLevel * 3 + LastRow Mod 3
            End If

            ' move left and right row indicators to barcode matrix
            CodewordToModules(Row, StartPatternLen, LeftRowInd, Pdf417Matrix)
            CodewordToModules(Row, RightRowIndCol, RightRowInd, Pdf417Matrix)

            ' loop for all codewords of this row
            For Col = 0 To DataColumns - 1
                ' code word pointer
                Dim Ptr = DataColumns * Row + Col

                ' move codeword to barcode matrix
                CodewordToModules(Row, ModulesInCodeword * (Col + 2), Codewords(Ptr), Pdf417Matrix)
            Next
        Next

        ' save barcode matrix
        Pdf417BarcodeMatrix = Pdf417Matrix

        ' exit
        Return Pdf417Matrix
    End Function

    ' set error correction level 
    ' it is either absolute or relative to data length
    Private Sub SetErrorCorrectionLevel()
        ' fixed error correction
        If _ErrorCorrection >= ErrorCorrectionLevel.Level_0 AndAlso _ErrorCorrection <= ErrorCorrectionLevel.Level_8 Then
            ' error correction level
            ErrorCorrectionLevel = _ErrorCorrection
        Else
            ' recommended normal values
            Dim DataLength = DataCodewords.Count

            If DataLength <= 40 Then
                ErrorCorrectionLevel = ErrorCorrectionLevel.Level_2
            ElseIf DataLength <= 160 Then
                ErrorCorrectionLevel = ErrorCorrectionLevel.Level_3
            ElseIf DataLength <= 320 Then
                ErrorCorrectionLevel = ErrorCorrectionLevel.Level_4
            ElseIf DataLength <= 863 Then
                ErrorCorrectionLevel = ErrorCorrectionLevel.Level_5
            Else
                ErrorCorrectionLevel = ErrorCorrectionLevel.Level_6
            End If

            If _ErrorCorrection = ErrorCorrectionLevel.AutoLow Then
                ErrorCorrectionLevel -= 1
            ElseIf _ErrorCorrection = ErrorCorrectionLevel.AutoMedium Then
                ErrorCorrectionLevel += 1
            ElseIf _ErrorCorrection = ErrorCorrectionLevel.AutoHigh Then
                ErrorCorrectionLevel += 2
            End If
        End If

        ' number of error correction codewords
        ErrorCorrectionLength = 1 << ErrorCorrectionLevel + 1
        Return
    End Sub

    ' perform data encoding.
    ' input text string or binary array to Pdf417 codewords
    Private Sub DataEncoding()
        ' create empty codewords array
        DataCodewords = New List(Of Integer)()

        ' add the data codewords length codeword
        DataCodewords.Add(0)

        ' character set
        If Not Equals(_GlobalLabelIDCharacterSet, Nothing) Then
            Dim Part = Integer.Parse(_GlobalLabelIDCharacterSet.Substring(9))
            DataCodewords.Add(GliCharacterSet)
            DataCodewords.Add(Part + 2)
        End If

        ' (G2 + 1) * 900 + G3
        Dim G3 As Integer = Nothing

        ' general purpose
        If _GlobalLabelIDGeneralPurpose <> 0 Then
            DataCodewords.Add(GliGeneralPurpose)
            Dim G2 = stdNum.DivRem(_GlobalLabelIDGeneralPurpose, 900, G3) - 1
            DataCodewords.Add(G2)
            DataCodewords.Add(G3)
        End If

        ' user defined
        If _GlobalLabelIDUserDefined <> 0 Then
            DataCodewords.Add(GliUserDefined)
            DataCodewords.Add(_GlobalLabelIDUserDefined - 810900)
        End If

        ' barcode data pointer
        BarcodeDataPos = 0
        BarcodeDataLength = BarcodeBinaryData.Length

        ' requested data encoding is byte only
        If _EncodingControl = EncodingControl.ByteOnly Then
            EncodeByteSegment(BarcodeDataLength)
            Return
        End If

        ' User selected encoding mode is auto or text plus byte (no numeric)
        ' set the default encoding mode and text sub mode
        _EncodingMode = EncodingMode.Text
        _TextEncodingMode = TextEncodingMode.Upper

        ' scan the barcode data
        While BarcodeDataPos < BarcodeDataLength
            ' test for numeric encoding if request is auto
            If _EncodingControl = EncodingControl.Auto Then
                ' count consequtive digits at this point
                Dim Digits As Integer = CountDigits()

                If Digits >= 13 Then
                    EncodeNumericSegment(Digits)
                    Continue While
                End If
            End If

            ' count text
            Dim TextChars As Integer = CountText()

            If TextChars >= 5 Then
                EncodeTextSegment(TextChars)
                Continue While
            End If

            ' count binary
            Dim Bytes As Integer = CountBytes()

            ' encode binary
            EncodeByteSegment(Bytes)
        End While

        Return
    End Sub

    ' count digits at the data pointer position
    Private Function CountDigits() As Integer
        Dim Ptr As Integer
        Ptr = BarcodeDataPos

        While Ptr < BarcodeDataLength AndAlso BarcodeBinaryData(Ptr) >= Asc("0"c) AndAlso BarcodeBinaryData(Ptr) <= Asc("9"c)
            Ptr += 1
        End While

        Return Ptr - BarcodeDataPos
    End Function

    ' count ASCII text characters at the data pointer position
    Private Function CountText() As Integer
        Dim DigitsCount = 0
        Dim Ptr As Integer

        For Ptr = BarcodeDataPos To BarcodeDataLength - 1
            ' current character
            Dim Chr As Integer = BarcodeBinaryData(Ptr)

            ' not part of text subset
            If Chr < Asc(" "c) AndAlso Chr <> 13 AndAlso Chr <> 10 AndAlso Chr <> 9 OrElse Chr > Asc("~"c) Then
                Exit For
            End If

            ' not a digits
            If Chr < Asc("0"c) OrElse Chr > Asc("9"c) Then
                DigitsCount = 0
                Continue For
            End If

            ' digit
            DigitsCount += 1

            ' we have less than 13 digits
            If DigitsCount < 13 Then Continue For

            ' terminate text mode if there is a block of 13 digits
            Ptr -= 12
            Exit For
        Next

        ' return textbytes
        Return Ptr - BarcodeDataPos
    End Function

    ' count punctuation marks at the data pointer position
    Private Function CountPunctuation(CurrentTextCount As Integer) As Integer
        Dim Count = 0

        While CurrentTextCount > 0
            Dim NextChr As Integer = BarcodeBinaryData(BarcodeDataPos + Count)
            Dim NextCode As Integer = TextToPunct(NextChr)
            If NextCode = 127 Then Return 0
            Count += 1
            If Count = 3 Then Return 3
        End While

        Return 0
    End Function

    ' count bytes at the data pointer position
    Private Function CountBytes() As Integer
        Dim TextCount = 0
        Dim Ptr As Integer

        For Ptr = BarcodeDataPos To BarcodeDataLength - 1
            ' current character
            Dim Chr As Integer = BarcodeBinaryData(Ptr)

            ' not part of text subset
            If Chr < Asc(" "c) AndAlso Chr <> 13 AndAlso Chr <> 10 AndAlso Chr <> 9 OrElse Chr > Asc("~"c) Then
                TextCount = 0
                Continue For
            End If

            ' update text count
            TextCount += 1

            ' we have less than 5 text characters
            If TextCount < 5 Then Continue For

            ' terminate binary mode if there is a block of 5 text characters
            Ptr -= 4
            Exit For
        Next

        Return Ptr - BarcodeDataPos
    End Function

    ' encode numeric data segment
    Private Sub EncodeNumericSegment(TotalCount As Integer)
        ' set numeric mode
        DataCodewords.Add(SwitchToNumericMode)
        _EncodingMode = EncodingMode.Numeric

        While TotalCount > 0
            ' work in segment no more than 44 digits
            Dim SegCount = stdNum.Min(TotalCount, 44)

            ' build a string with initial value 1
            Dim SegStr As StringBuilder = New StringBuilder("1")

            ' add data digits
            For Index = 0 To SegCount - 1
                SegStr.Append(Microsoft.VisualBasic.ChrW(BarcodeBinaryData(stdNum.Min(Threading.Interlocked.Increment(BarcodeDataPos), BarcodeDataPos - 1))))
            Next

            ' convert to big integer
            Dim Temp As BigInteger = BigInteger.Parse(SegStr.ToString())

            ' find the highest factor
            Dim Fact As Integer
            Fact = 0

            While Fact < 15 AndAlso Temp >= FactBigInt900(Fact)
                Fact += 1
            End While

            ' convert to module 900
            For Index = Fact - 1 To 0 + 1 Step -1
                DataCodewords.Add(CInt(BigInteger.DivRem(Temp, FactBigInt900(Index), Temp)))
            Next

            DataCodewords.Add(CInt(Temp))

            ' update total count
            TotalCount -= SegCount
        End While

        Return
    End Sub

    ' encode text segment
    Private Sub EncodeTextSegment(TotalCount As Integer)
        ' note first time this is the default
        If _EncodingMode <> EncodingMode.Text Then
            DataCodewords.Add(SwitchToTextMode)
            _EncodingMode = EncodingMode.Text
            _TextEncodingMode = TextEncodingMode.Upper
        End If

        Dim Temp As List(Of Integer) = New List(Of Integer)()
        Dim Code As Integer

        While TotalCount > 0
            Dim Chr As Integer = BarcodeBinaryData(stdNum.Min(Threading.Interlocked.Increment(BarcodeDataPos), BarcodeDataPos - 1))
            TotalCount -= 1

            Select Case _TextEncodingMode
                Case TextEncodingMode.Upper
_Select0_CasePdfFileWriter_Pdf417Encoder_TextEncodingMode_Upper:
                    Code = TextToUpper(Chr)

                    If Code <> 127 Then
                        Temp.Add(Code)
                        Continue While
                    End If

                    Code = TextToLower(Chr)

                    If Code <> 127 Then
                        Temp.Add(27) ' Lower Latch
                        Temp.Add(Code)
                        _TextEncodingMode = TextEncodingMode.Lower
                        Continue While
                    End If

                    Code = TextToMixed(Chr)

                    If Code <> 127 Then
                        Temp.Add(28) ' Mixed Latch
                        Temp.Add(Code)
                        _TextEncodingMode = TextEncodingMode.Mixed
                        Continue While
                    End If

                    Code = TextToPunct(Chr)

                    If Code <> 127 Then
                        ' count how many more punctuations after this one
                        Dim PunctCount = CountPunctuation(TotalCount)

                        ' if next character is panctuation too, we latch to punctuation
                        If PunctCount > 0 Then
                            Temp.Add(28) ' mixed latch
                            Temp.Add(25) ' punctuation latch
                            Temp.Add(Code)
                            _TextEncodingMode = TextEncodingMode.Punct
                            Continue While
                        End If

                        ' one to three punctuation marks at this point
                        Temp.Add(29) ' punctuation shift
                        Temp.Add(Code)
                        Continue While
                    End If

                    Throw New ApplicationException("Program error: Text upper submode.")
                Case TextEncodingMode.Lower
                    Code = TextToLower(Chr)

                    If Code <> 127 Then
                        Temp.Add(Code)
                        Continue While
                    End If

                    Code = TextToUpper(Chr)

                    If Code <> 127 Then
                        Temp.Add(27) ' upper shift
                        Temp.Add(Code)
                        Continue While
                    End If

                    Code = TextToMixed(Chr)

                    If Code <> 127 Then
                        Temp.Add(28) ' mixed Latch
                        Temp.Add(Code)
                        _TextEncodingMode = TextEncodingMode.Mixed
                        Continue While
                    End If

                    Code = TextToPunct(Chr)

                    If Code <> 127 Then
                        ' count how many more punctuations after this one
                        Dim PunctCount = CountPunctuation(TotalCount)

                        ' if next character is panctuation too, we latch to punctuation
                        If PunctCount > 0 Then
                            Temp.Add(28) ' mixed latch
                            Temp.Add(25) ' punctuation latch
                            Temp.Add(Code)
                            _TextEncodingMode = TextEncodingMode.Punct
                            Continue While
                        End If

                        ' one to three punctuation marks at this point
                        Temp.Add(29) ' punctuation shift
                        Temp.Add(Code)
                        Continue While
                    End If

                    Throw New ApplicationException("Program error: Text lower submode.")
                Case TextEncodingMode.Mixed
                    Code = TextToMixed(Chr)

                    If Code <> 127 Then
                        Temp.Add(Code)
                        Continue While
                    End If

                    Code = TextToLower(Chr)

                    If Code <> 127 Then
                        Temp.Add(27) ' lower Latch
                        Temp.Add(Code)
                        _TextEncodingMode = TextEncodingMode.Lower
                        Continue While
                    End If

                    Code = TextToUpper(Chr)

                    If Code <> 127 Then
                        Temp.Add(28) ' upper latch
                        Temp.Add(Code)
                        _TextEncodingMode = TextEncodingMode.Upper
                        Continue While
                    End If

                    Code = TextToPunct(Chr)

                    If Code <> 127 Then
                        ' count how many more punctuations after this one
                        Dim PunctCount = CountPunctuation(TotalCount)

                        ' if next character is panctuation too, we latch to punctuation
                        If PunctCount > 0 Then
                            Temp.Add(25) ' punctuation latch
                            Temp.Add(Code)
                            _TextEncodingMode = TextEncodingMode.Punct
                            Continue While
                        End If

                        ' single punctuation
                        Temp.Add(29) ' punctuation shift
                        Temp.Add(Code)
                        Continue While
                    End If

                    Throw New ApplicationException("Program error: Text mixed submode.")
                Case TextEncodingMode.Punct
                    Code = TextToPunct(Chr)

                    If Code <> 127 Then
                        Temp.Add(Code)
                        Continue While
                    End If

                    Temp.Add(29) ' upper latch
                    _TextEncodingMode = TextEncodingMode.Upper
                    GoTo _Select0_CasePdfFileWriter_Pdf417Encoder_TextEncodingMode_Upper
            End Select
        End While

        ' convert to codewords
        Dim TempEnd = Temp.Count And Not 1

        For Index = 0 To TempEnd - 1 Step 2
            DataCodewords.Add(30 * Temp(Index) + Temp(Index + 1))
        Next

        If (Temp.Count And 1) <> 0 Then
            DataCodewords.Add(30 * Temp(TempEnd) + 29)
        End If

        Return
    End Sub

    ' encode byte segment
    Private Sub EncodeByteSegment(Count As Integer)
        ' special case one time shift
        If Count = 1 AndAlso _EncodingMode = EncodingMode.Text Then
            DataCodewords.Add(ShiftToByteMode)
            DataCodewords.Add(BarcodeBinaryData(stdNum.Min(Threading.Interlocked.Increment(BarcodeDataPos), BarcodeDataPos - 1)))
            Return
        End If

        ' add shift to byte mode code
        DataCodewords.Add(If(Count Mod 6 = 0, SwitchToByteModeForSix, SwitchToByteMode))

        ' set byte encoding mode
        _EncodingMode = EncodingMode.Byte

        ' end position
        Dim EndPos = BarcodeDataPos + Count

        ' encode six data bytes into five codewords
        If Count >= 6 Then
            While EndPos - BarcodeDataPos >= 6
                ' load 6 data bytes into temp long integer
                Dim Temp = CLng(BarcodeBinaryData(stdNum.Min(Threading.Interlocked.Increment(BarcodeDataPos), BarcodeDataPos - 1))) << 40 Or CLng(BarcodeBinaryData(stdNum.Min(Threading.Interlocked.Increment(BarcodeDataPos), BarcodeDataPos - 1))) << 32 Or CLng(BarcodeBinaryData(stdNum.Min(Threading.Interlocked.Increment(BarcodeDataPos), BarcodeDataPos - 1))) << 24 Or CLng(BarcodeBinaryData(stdNum.Min(Threading.Interlocked.Increment(BarcodeDataPos), BarcodeDataPos - 1))) << 16 Or CLng(BarcodeBinaryData(stdNum.Min(Threading.Interlocked.Increment(BarcodeDataPos), BarcodeDataPos - 1))) << 8 Or BarcodeBinaryData(stdNum.Min(Threading.Interlocked.Increment(BarcodeDataPos), BarcodeDataPos - 1))

                ' convert to 4 digits base 900 number
                For Index = 4 To 0 + 1 Step -1
                    DataCodewords.Add(stdNum.DivRem(Temp, Fact900(Index), Temp))
                Next

                ' add the fifth one
                DataCodewords.Add(Temp)
            End While
        End If

        ' encode the last 5 oe less bytes
        While BarcodeDataPos < EndPos
            DataCodewords.Add(BarcodeBinaryData(stdNum.Min(Threading.Interlocked.Increment(BarcodeDataPos), BarcodeDataPos - 1)))
        End While

        Return
    End Sub

    ' calculate error correction codewords
    Private Sub CalculateErrorCorrection(Codewords As Integer())
        ' shortcut for the selected error correction table
        Dim ErrCorrTable = ErrorCorrectionTables(ErrorCorrectionLevel)

        ' create empty error correction array
        ErrCorrCodewords = New Integer(ErrorCorrectionLength - 1) {}

        ' pointer to last error correction codeword
        Dim ErrorCorrectionEnd = ErrorCorrectionLength - 1

        ' do the magic polynomial divide
        Dim DataCodewordsLength = Codewords(0)

        For CWIndex = 0 To DataCodewordsLength - 1
            Dim Temp = (Codewords(CWIndex) + ErrCorrCodewords(ErrorCorrectionEnd)) Mod [MOD]

            For Index = ErrorCorrectionEnd To 0 + 1 Step -1
                ErrCorrCodewords(Index) = ([MOD] + ErrCorrCodewords(Index - 1) - Temp * ErrCorrTable(Index)) Mod [MOD]
            Next

            ErrCorrCodewords(0) = ([MOD] - Temp * ErrCorrTable(0)) Mod [MOD]
        Next

        ' last step of the division
        For Index = ErrorCorrectionEnd To 0 Step -1
            ErrCorrCodewords(Index) = ([MOD] - ErrCorrCodewords(Index)) Mod [MOD]
        Next

        ' copy error codewords in reverse order
        For Index = 0 To ErrorCorrectionLength - 1
            Codewords(DataCodewordsLength + Index) = ErrCorrCodewords(ErrorCorrectionEnd - Index)
        Next

        ' move error correction
        Return
    End Sub

    ' convert one codeword to barcode modules
    Private Sub CodewordToModules(Row As Integer, Col As Integer, Codeword As Integer, Matrix As Boolean(,))
        ' leading black module
        Matrix(Row, Col) = True

        ' translate to modules
        Dim Modules As Integer = CodewordTable(Row Mod 3, Codeword)
        Dim Mask = &H4000

        For Index = 1 To ModulesInCodeword - 1
            If (Modules And Mask) <> 0 Then Matrix(Row, Col + Index) = True
            Mask >>= 1
        Next

        Return
    End Sub
End Class
