#Region "Microsoft.VisualBasic::4dec5e910552d6d871be1f4645e71463, mime\application%pdf\PdfFileWriter\Font\FontApi.vb"

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

    '   Total Lines: 656
    '    Code Lines: 260
    ' Comment Lines: 281
    '   Blank Lines: 115
    '     File Size: 22.83 KB


    ' Class FontApi
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    '     Function: BuildUnitMarix, DeleteObject, FormatMessage, GetFontData, GetFontDataApi
    '               GetGlyphIndices, GetGlyphIndicesApi, GetGlyphMetricsApi, GetGlyphMetricsApiByCode, GetGlyphMetricsApiByGlyphIndex
    '               GetGlyphOutline, GetKerningPairs, GetKerningPairsApi, GetOutlineTextMetrics, GetOutlineTextMetricsApi
    '               GetTextMetrics, GetTextMetricsApi, ReadByte, ReadChar, ReadInt16
    '               ReadInt16Array, ReadInt32, ReadInt32Array, ReadString, ReadUInt16
    '               ReadUInt32, ReadWinPoint, SelectObject
    ' 
    '     Sub: Align4, AllocateBuffer, Dispose, FreeBuffer, ThrowSystemErrorException
    ' 
    ' /********************************************************************************/

#End Region

'
'
'	PdfFileWriter
'	PDF File Write C# Class Library.
'
'	FontApi
'	Support for Windows API functions related to fonts and glyphs.
'
'	Uzi Granot
'	Version: 1.0
'	Date: April 1, 2013
'	Copyright (C) 2013-2019 Uzi Granot. All Rights Reserved
'
'	PdfFileWriter C# class library and TestPdfFileWriter test/demo
'  application are free software.
'	They is distributed under the Code Project Open License (CPOL).
'	The document PdfFileWriterReadmeAndLicense.pdf contained within
'	the distribution specify the license agreement and other
'	conditions and notes. You must read this document and agree
'	with the conditions specified in order to use this software.
'
'	For version history please refer to PdfDocument.cs
'
'

Imports System.Drawing
Imports System.Runtime.InteropServices
Imports System.Text
Imports stdNum = System.Math

''' <summary>
''' Font API class
''' </summary>
''' <remarks>
''' Windows API callable by C# program
''' </remarks>
Public Class FontApi : Implements IDisposable

    Private BitMap As Bitmap
    Private GDI As Graphics
    Private GDIHandle As IntPtr
    Private FontHandle As IntPtr
    Private SavedFont As IntPtr
    Private Buffer As IntPtr
    Private BufPtr As Integer
    Private DesignHeight As Integer

    ''' <summary>
    ''' Device context constructor
    ''' </summary>
    ''' <param name="GDIHandle"></param>
    ''' <param name="FontHandle"></param>
    ''' <returns></returns>
    <DllImport("gdi32.dll", CharSet:=CharSet.Auto, CallingConvention:=CallingConvention.StdCall, SetLastError:=True)>
    Private Shared Function SelectObject(GDIHandle As IntPtr, FontHandle As IntPtr) As IntPtr
    End Function

    ''' <summary>
    ''' Font API constructor
    ''' </summary>
    ''' <param name="DesignFont">Design font</param>
    ''' <param name="DesignHeight">Design height</param>
    Public Sub New(DesignFont As Font, DesignHeight As Integer)
        ' save design height
        Me.DesignHeight = DesignHeight

        ' define device context
        BitMap = New Bitmap(1, 1)
        GDI = Graphics.FromImage(BitMap)
        GDIHandle = CType(GDI.GetHdc(), IntPtr)

        ' select the font into the device context
        FontHandle = DesignFont.ToHfont()
        SavedFont = SelectObject(GDIHandle, FontHandle)
    End Sub

    Private Const GGO_METRICS As UInteger = 0
    Private Const GGO_BITMAP As UInteger = 1
    Private Const GGO_NATIVE As UInteger = 2
    Private Const GGO_BEZIER As UInteger = 3
    Private Const GGO_GLYPH_INDEX As UInteger = 128

    ''' <summary>
    ''' Gets single glyph metric
    ''' </summary>
    ''' <param name="GDIHandle"></param>
    ''' <param name="CharIndex"></param>
    ''' <param name="GgoFormat"></param>
    ''' <param name="GlyphMetrics"></param>
    ''' <param name="Zero"></param>
    ''' <param name="Null"></param>
    ''' <param name="TransMatrix"></param>
    ''' <returns></returns>
    <DllImport("gdi32.dll", CharSet:=CharSet.Auto, CallingConvention:=CallingConvention.StdCall, SetLastError:=True)>
    Private Shared Function GetGlyphOutline(GDIHandle As IntPtr, CharIndex As Integer, GgoFormat As UInteger, GlyphMetrics As IntPtr, Zero As UInteger, Null As IntPtr, TransMatrix As IntPtr) As Integer
    End Function

    ''' <summary>
    ''' Gets glyph metric
    ''' </summary>
    ''' <param name="CharCode">Character code</param>
    ''' <returns>Character info class</returns>
    Public Function GetGlyphMetricsApiByCode(CharCode As Integer) As CharInfo
        ' get glyph index for char code
        Dim GlyphIndexArray = GetGlyphIndicesApi(CharCode, CharCode)

        ' get glyph outline
        Dim Info = GetGlyphMetricsApiByGlyphIndex(GlyphIndexArray(0))
        Info.CharCode = CharCode

        ' exit
        Return Info
    End Function

    ''' <summary>
    ''' Gets glyph metric
    ''' </summary>
    ''' <param name="GlyphIndex">Character code</param>
    ''' <returns>Character info class</returns>
    Public Function GetGlyphMetricsApiByGlyphIndex(GlyphIndex As Integer) As CharInfo
        ' build unit matrix
        Dim UnitMatrix As IntPtr = BuildUnitMarix()

        ' allocate buffer to receive glyph metrics information
        AllocateBuffer(20)

        ' get one glyph
        If GetGlyphOutline(GDIHandle, GlyphIndex, GGO_GLYPH_INDEX, Buffer, 0, IntPtr.Zero, UnitMatrix) < 0 Then ThrowSystemErrorException("Calling GetGlyphOutline failed")

        ' create WinOutlineTextMetric class
        Dim Info As CharInfo = New CharInfo(0, GlyphIndex, Me)

        ' free buffer for glyph metrics
        FreeBuffer()

        ' free unit matrix buffer
        Marshal.FreeHGlobal(UnitMatrix)

        ' exit
        Return Info
    End Function

    ''' <summary>
    ''' Gets array of glyph metrics
    ''' </summary>
    ''' <param name="CharValue">Character code</param>
    ''' <returns>Array of character infos</returns>
    Public Function GetGlyphMetricsApi(CharValue As Integer) As CharInfo()
        ' first character of the 256 block
        Dim FirstChar = CharValue And &HFF00

        ' use glyph index
        Dim UseGlyphIndex = FirstChar <> 0

        ' get character code to glyph index
        ' if GlyphIndex[x] is zero glyph is undefined
        Dim GlyphIndexArray = GetGlyphIndicesApi(FirstChar, FirstChar + 255)

        ' test for at least one valid glyph
        Dim Start As Integer
        Start = 0

        While Start < 256 AndAlso GlyphIndexArray(Start) = 0
            Start += 1
        End While

        If Start = 256 Then Return Nothing

        ' build unit matrix
        Dim UnitMatrix As IntPtr = BuildUnitMarix()

        ' allocate buffer to receive glyph metrics information
        AllocateBuffer(20)

        ' result array
        Dim CharInfoArray = New CharInfo(255) {}

        ' loop for all characters
        For CharCode As Integer = Start To 256 - 1
            ' charater not defined
            Dim GlyphIndex = GlyphIndexArray(CharCode)
            If GlyphIndex = 0 Then Continue For

            ' get one glyph
            If GetGlyphOutline(GDIHandle, FirstChar + CharCode, GGO_METRICS, Buffer, 0, IntPtr.Zero, UnitMatrix) < 0 Then
                ThrowSystemErrorException("Calling GetGlyphOutline failed")
            End If

            ' reset buffer pointer
            BufPtr = 0

            ' create WinOutlineTextMetric class
            CharInfoArray(CharCode) = New CharInfo(FirstChar + CharCode, GlyphIndex, Me)
        Next

        ' free buffer for glyph metrics
        FreeBuffer()

        ' free unit matrix buffer
        Marshal.FreeHGlobal(UnitMatrix)

        ' exit
        Return CharInfoArray
    End Function

    ''' <summary>
    ''' Get kerning pairs array
    ''' </summary>
    ''' <param name="GDIHandle"></param>
    ''' <param name="NumberOfPairs"></param>
    ''' <param name="PairArray"></param>
    ''' <returns></returns>
    <DllImport("gdi32.dll", CharSet:=CharSet.Auto, CallingConvention:=CallingConvention.StdCall, SetLastError:=True)>
    Private Shared Function GetKerningPairs(GDIHandle As IntPtr, NumberOfPairs As UInteger, PairArray As IntPtr) As UInteger
    End Function

    ''' <summary>
    ''' Gets kerning pairs array
    ''' </summary>
    ''' <param name="FirstChar">First character</param>
    ''' <param name="LastChar">Last character</param>
    ''' <returns>Array of kerning pairs</returns>
    Public Function GetKerningPairsApi(FirstChar As Integer, LastChar As Integer) As WinKerningPair()
        ' get number of pairs
        Dim Pairs As Integer = GetKerningPairs(GDIHandle, 0, IntPtr.Zero)
        If Pairs = 0 Then Return Nothing

        ' allocate buffer to receive outline text metrics information
        AllocateBuffer(8 * Pairs)

        ' get outline text metrics information
        If GetKerningPairs(GDIHandle, Pairs, Buffer) = 0 Then ThrowSystemErrorException("Calling GetKerningPairs failed")

        ' create list because the program will ignore pairs that are outside char range
        Dim TempList As List(Of WinKerningPair) = New List(Of WinKerningPair)()

        ' kerning pairs from buffer
        For Index = 0 To Pairs - 1
            Dim KPair As WinKerningPair = New WinKerningPair(Me)
            If AscW(KPair.First) >= FirstChar AndAlso AscW(KPair.First) <= LastChar AndAlso AscW(KPair.Second) >= FirstChar AndAlso AscW(KPair.Second) <= LastChar Then
                TempList.Add(KPair)
            End If
        Next

        ' free buffer for outline text metrics
        FreeBuffer()

        ' list is empty
        If TempList.Count = 0 Then Return Nothing

        ' sort list
        TempList.Sort()

        ' exit
        Return TempList.ToArray()
    End Function

    ''' <summary>
    ''' Get OUTLINETEXTMETRICW structure
    ''' </summary>
    ''' <param name="GDIHandle"></param>
    ''' <param name="BufferLength"></param>
    ''' <param name="Buffer"></param>
    ''' <returns></returns>
    <DllImport("gdi32.dll", CharSet:=CharSet.Auto, CallingConvention:=CallingConvention.StdCall, SetLastError:=True)>
    Private Shared Function GetOutlineTextMetrics(GDIHandle As IntPtr, BufferLength As Integer, Buffer As IntPtr) As Integer
    End Function

    ''' <summary>
    ''' Gets OUTLINETEXTMETRICW structure
    ''' </summary>
    ''' <returns>Outline text metric class</returns>
    Public Function GetOutlineTextMetricsApi() As WinOutlineTextMetric
        ' get buffer size
        Dim BufSize = GetOutlineTextMetrics(GDIHandle, 0, IntPtr.Zero)
        If BufSize = 0 Then ThrowSystemErrorException("Calling GetOutlineTextMetrics (get buffer size) failed")

        ' allocate buffer to receive outline text metrics information
        AllocateBuffer(BufSize)

        ' get outline text metrics information
        If GetOutlineTextMetrics(GDIHandle, BufSize, Buffer) = 0 Then ThrowSystemErrorException("Calling GetOutlineTextMetrics failed")

        ' create WinOutlineTextMetric class
        Dim WOTM As WinOutlineTextMetric = New WinOutlineTextMetric(Me)

        ' free buffer for outline text metrics
        FreeBuffer()

        ' exit
        Return WOTM
    End Function


    ''' <summary>
    ''' Get TEXTMETRICW structure
    ''' </summary>
    ''' <param name="GDIHandle"></param>
    ''' <param name="Buffer"></param>
    ''' <returns></returns>
    <DllImport("gdi32.dll", CharSet:=CharSet.Auto, CallingConvention:=CallingConvention.StdCall, SetLastError:=True)>
    Private Shared Function GetTextMetrics(GDIHandle As IntPtr, Buffer As IntPtr) As Integer
    End Function

    ''' <summary>
    ''' Gets TEXTMETRICW structure
    ''' </summary>
    ''' <returns>Text metric class</returns>
    Public Function GetTextMetricsApi() As WinTextMetric
        ' allocate buffer to receive outline text metrics information
        AllocateBuffer(57)

        ' get outline text metrics information
        If GetTextMetrics(GDIHandle, Buffer) = 0 Then ThrowSystemErrorException("Calling GetTextMetrics API failed.")

        ' create WinOutlineTextMetric class
        Dim WTM As WinTextMetric = New WinTextMetric(Me)

        ' free buffer for outline text metrics
        FreeBuffer()

        ' exit
        Return WTM
    End Function

    ''' <summary>
    ''' Get font data tables
    ''' </summary>
    ''' <param name="DeviceContextHandle"></param>
    ''' <param name="Table"></param>
    ''' <param name="Offset"></param>
    ''' <param name="Buffer"></param>
    ''' <param name="BufferLength"></param>
    ''' <returns></returns>
    <DllImport("gdi32.dll", CharSet:=CharSet.Auto, CallingConvention:=CallingConvention.StdCall, SetLastError:=True)>
    Private Shared Function GetFontData(DeviceContextHandle As IntPtr, Table As UInteger, Offset As UInteger, Buffer As IntPtr, BufferLength As UInteger) As UInteger
    End Function

    ''' <summary>
    ''' Gets font data tables
    ''' </summary>
    ''' <param name="TableTag">Table Tag</param>
    ''' <param name="Offset">Table offset</param>
    ''' <param name="BufSize">Table size</param>
    ''' <returns>Table info as byte array</returns>
    Public Function GetFontDataApi(TableTag As UInteger, Offset As Integer, BufSize As Integer) As Byte()
        ' empty table
        If BufSize = 0 Then Return Nothing

        ' allocate buffer to receive outline text metrics information
        AllocateBuffer(BufSize)

        ' microsoft tag is in little endian format
        Dim MSTag As UInteger = TableTag << 24 Or TableTag << 8 And &HFF0000 Or TableTag >> 8 And &HFF00 Or TableTag >> 24 And &HFF

        ' get outline text metrics information
        If CInt(GetFontData(GDIHandle, MSTag, Offset, Buffer, BufSize)) <> BufSize Then ThrowSystemErrorException("Get font data file header failed")

        ' copy api result buffer to managed memory buffer
        Dim DataBuffer = New Byte(BufSize - 1) {}
        Marshal.Copy(Buffer, DataBuffer, 0, BufSize)
        BufPtr = 0

        ' free unmanaged memory buffer
        FreeBuffer()

        ' exit
        Return DataBuffer
    End Function

    ''' <summary>
    ''' Get glyph indices array
    ''' </summary>
    ''' <param name="GDIHandle"></param>
    ''' <param name="CharBuffer"></param>
    ''' <param name="CharCount"></param>
    ''' <param name="GlyphArray"></param>
    ''' <param name="GlyphOptions"></param>
    ''' <returns></returns>
    <DllImport("gdi32.dll", CharSet:=CharSet.Auto, CallingConvention:=CallingConvention.StdCall, SetLastError:=True)>
    Private Shared Function GetGlyphIndices(GDIHandle As IntPtr, CharBuffer As IntPtr, CharCount As Integer, GlyphArray As IntPtr, GlyphOptions As UInteger) As Integer
    End Function

    ''' <summary>
    ''' Gets glyph indices array
    ''' </summary>
    ''' <param name="FirstChar">First character</param>
    ''' <param name="LastChar">Last character</param>
    ''' <returns>Array of glyph indices.</returns>
    Public Function GetGlyphIndicesApi(FirstChar As Integer, LastChar As Integer) As Integer()
        ' character count
        Dim CharCount = LastChar - FirstChar + 1

        ' allocate character table buffer in global memory (two bytes per char)
        Dim CharBuffer = Marshal.AllocHGlobal(2 * CharCount)

        ' create array of all character codes between FirstChar and LastChar (we use short because of Unicode)
        For CharPtr = FirstChar To LastChar
            Marshal.WriteInt16(CharBuffer, 2 * (CharPtr - FirstChar), CShort(CharPtr))
        Next

        ' allocate memory for result
        AllocateBuffer(2 * CharCount)

        ' get glyph numbers for all characters including non existing glyphs
        If GetGlyphIndices(GDIHandle, CharBuffer, CharCount, Buffer, 0) <> CharCount Then ThrowSystemErrorException("Calling GetGlypeIndices failed")

        ' get result array to managed code
        Dim GlyphIndex16 = ReadInt16Array(CharCount)

        ' free local buffer
        Marshal.FreeHGlobal(CharBuffer)

        ' free result buffer
        FreeBuffer()

        ' convert to int
        Dim GlyphIndex32 = New Integer(GlyphIndex16.Length - 1) {}

        For Index = 0 To GlyphIndex16.Length - 1
            GlyphIndex32(Index) = GlyphIndex16(Index)
        Next

        ' exit
        Return GlyphIndex32
    End Function

    ''' <summary>
    ''' Allocate API result buffer
    ''' </summary>
    ''' <param name="Size"></param>
    Private Sub AllocateBuffer(Size As Integer)
        ' allocate memory for result
        Buffer = Marshal.AllocHGlobal(Size)
        BufPtr = 0
        Return
    End Sub

    ''' <summary>
    ''' Free API result buffer
    ''' </summary>
    Private Sub FreeBuffer()
        ' free buffer
        Marshal.FreeHGlobal(Buffer)
        Buffer = IntPtr.Zero
        Return
    End Sub

    ''' <summary>
    ''' Align buffer pointer to 4 bytes boundry
    ''' </summary>
    Friend Sub Align4()
        BufPtr = BufPtr + 3 And Not 3
        Return
    End Sub

    ''' <summary>
    ''' Read point (x, y) from data buffer
    ''' </summary>
    ''' <returns></returns>
    Friend Function ReadWinPoint() As Point
        Return New Point(ReadInt32(), ReadInt32())
    End Function

    ''' <summary>
    ''' Read byte from data buffer
    ''' </summary>
    ''' <returns></returns>
    Friend Function ReadByte() As Byte
        Return Marshal.ReadByte(Buffer, stdNum.Min(Threading.Interlocked.Increment(BufPtr), BufPtr - 1))
    End Function

    ''' <summary>
    ''' Read character from data buffer
    ''' </summary>
    ''' <returns></returns>
    Friend Function ReadChar() As Char
        Dim Value As Char = Microsoft.VisualBasic.ChrW(Marshal.ReadInt16(Buffer, BufPtr))
        BufPtr += 2
        Return Value
    End Function

    ''' <summary>
    ''' Read short integer from data buffer
    ''' </summary>
    ''' <returns></returns>
    Friend Function ReadInt16() As Short
        Dim Value = Marshal.ReadInt16(Buffer, BufPtr)
        BufPtr += 2
        Return Value
    End Function

    ''' <summary>
    ''' Read unsigned short integer from data buffer
    ''' </summary>
    ''' <returns></returns>
    Friend Function ReadUInt16() As UShort
        Dim Value As UShort = Marshal.ReadInt16(Buffer, BufPtr)
        BufPtr += 2
        Return Value
    End Function

    ''' <summary>
    ''' Read short array from result buffer
    ''' </summary>
    ''' <param name="Size"></param>
    ''' <returns></returns>
    Friend Function ReadInt16Array(Size As Integer) As Short()
        ' create active characters array
        Dim Result = New Short(Size - 1) {}
        Marshal.Copy(Buffer, Result, 0, Size)
        Return Result
    End Function

    ''' <summary>
    ''' Read integers from data buffer
    ''' </summary>
    ''' <returns></returns>
    Friend Function ReadInt32() As Integer
        Dim Value = Marshal.ReadInt32(Buffer, BufPtr)
        BufPtr += 4
        Return Value
    End Function

    ''' <summary>
    ''' Read int array from result buffer
    ''' </summary>
    ''' <param name="Size"></param>
    ''' <returns></returns>
    Friend Function ReadInt32Array(Size As Integer) As Integer()
        ' create active characters array
        Dim Result = New Integer(Size - 1) {}
        Marshal.Copy(Buffer, Result, 0, Size)
        Return Result
    End Function

    ''' <summary>
    ''' Read unsigned integers from data buffer
    ''' </summary>
    ''' <returns></returns>
    Friend Function ReadUInt32() As UInteger
        Dim Value As UInteger = Marshal.ReadInt32(Buffer, BufPtr)
        BufPtr += 4
        Return Value
    End Function

    ''' <summary>
    ''' Read string (null terminated) from data buffer
    ''' </summary>
    ''' <returns></returns>
    Friend Function ReadString() As String
        Dim Ptr = Marshal.ReadInt32(Buffer, BufPtr)
        BufPtr += 4
        Dim Str As StringBuilder = New StringBuilder()

        While True
            Dim Chr As Char = Microsoft.VisualBasic.ChrW(Marshal.ReadInt16(Buffer, Ptr))
            If AscW(Chr) = 0 Then Exit While
            Str.Append(Chr)
            Ptr += 2
        End While

        Return Str.ToString()
    End Function

    <DllImport("Kernel32.dll", CharSet:=CharSet.Auto, CallingConvention:=CallingConvention.StdCall, SetLastError:=True)>
    Private Shared Function FormatMessage(dwFlags As UInteger, lpSource As IntPtr, dwMessageId As UInteger, dwLanguageId As UInteger, lpBuffer As IntPtr, nSize As UInteger, Arguments As IntPtr) As UInteger
    End Function

    ''' <summary>
    ''' Throw exception showing last system error
    ''' </summary>
    ''' <param name="AppMsg"></param>
    Friend Sub ThrowSystemErrorException(AppMsg As String)
        Const FORMAT_MESSAGE_FROM_SYSTEM As UInteger = &H1000

        ' error message
        Dim ErrMsg As StringBuilder = New StringBuilder(AppMsg)

        ' get last system error
        Dim ErrCode As UInteger = CUInt(Marshal.GetLastWin32Error()) ' GetLastError();

        If ErrCode <> 0 Then
            ' allocate buffer
            Dim ErrBuffer = Marshal.AllocHGlobal(1024)

            ' add error code
            ErrMsg.AppendFormat(Microsoft.VisualBasic.Constants.vbCrLf & "System error [0x{0:X8}]", ErrCode)

            ' convert error code to text
            Dim StrLen As Integer = FormatMessage(FORMAT_MESSAGE_FROM_SYSTEM, IntPtr.Zero, ErrCode, 0, ErrBuffer, 1024, IntPtr.Zero)

            If StrLen > 0 Then
                ErrMsg.Append(" ")
                ErrMsg.Append(Marshal.PtrToStringAuto(ErrBuffer, StrLen))

                While ErrMsg(ErrMsg.Length - 1) <= " "c
                    ErrMsg.Length -= 1
                End While
            End If

            ' free buffer

            ' unknown error
            Marshal.FreeHGlobal(ErrBuffer)
        Else
            ErrMsg.Append(Microsoft.VisualBasic.Constants.vbCrLf & "Unknown error.")
        End If

        ' exit
        Throw New ApplicationException(ErrMsg.ToString())
    End Sub

    ''' <summary>
    ''' Build unit matrix in unmanaged memory
    ''' </summary>
    ''' <returns></returns>
    Private Function BuildUnitMarix() As IntPtr
        ' allocate buffer for transformation matrix
        Dim UnitMatrix = Marshal.AllocHGlobal(16)

        ' set transformation matrix into unit matrix
        Marshal.WriteInt32(UnitMatrix, 0, &H10000)
        Marshal.WriteInt32(UnitMatrix, 4, 0)
        Marshal.WriteInt32(UnitMatrix, 8, 0)
        Marshal.WriteInt32(UnitMatrix, 12, &H10000)
        Return UnitMatrix
    End Function

    <DllImport("gdi32.dll", CharSet:=CharSet.Auto, CallingConvention:=CallingConvention.StdCall, SetLastError:=True)>
    Private Shared Function DeleteObject(Handle As IntPtr) As IntPtr
    End Function

    ''' <summary>
    ''' Dispose unmanaged resources
    ''' </summary>
    Public Sub Dispose() Implements IDisposable.Dispose
        ' free unmanaged buffer
        Marshal.FreeHGlobal(Buffer)

        ' restore original font
        SelectObject(GDIHandle, SavedFont)

        ' delete font handle
        DeleteObject(FontHandle)

        ' release device context handle
        GDI.ReleaseHdc(GDIHandle)

        ' release GDI resources
        GDI.Dispose()

        ' release bitmap
        BitMap.Dispose()
    End Sub
End Class
