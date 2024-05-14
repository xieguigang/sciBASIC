#Region "Microsoft.VisualBasic::5c8364591be76ff733ee55883ef41c18, mime\application%vnd.openxmlformats-officedocument.spreadsheetml.sheet\Excel\XLSX\Writer\Cell\BasicFormulas.vb"

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

    '   Total Lines: 317
    '    Code Lines: 142
    ' Comment Lines: 152
    '   Blank Lines: 23
    '     File Size: 18.28 KB


    '     Class BasicFormulas
    ' 
    '         Function: (+2 Overloads) Average, (+2 Overloads) Ceil, (+2 Overloads) Floor, GetBasicFormula, GetVLookup
    '                   (+2 Overloads) Max, (+2 Overloads) Median, (+2 Overloads) Min, (+2 Overloads) Round, (+2 Overloads) Sum
    '                   (+4 Overloads) VLookup
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Globalization
Imports Microsoft.VisualBasic.MIME.Office.Excel.XLSX.Writer.Cell

Namespace XLSX.Writer

    ''' <summary>
    ''' Class for handling of basic Excel formulas
    ''' </summary>
    Public NotInheritable Class BasicFormulas
        ''' <summary>
        ''' Returns a cell with a average formula
        ''' </summary>
        ''' <param name="range">Cell range to apply the average operation to.</param>
        ''' <returns>Prepared Cell object, ready to be added to a worksheet.</returns>
        Public Shared Function Average(range As Range) As Cell
            Return Average(Nothing, range)
        End Function

        ''' <summary>
        ''' Returns a cell with a average formula
        ''' </summary>
        ''' <param name="target">Target worksheet of the average operation. Can be null if on the same worksheet.</param>
        ''' <param name="range">Cell range to apply the average operation to.</param>
        ''' <returns>Prepared Cell object, ready to be added to a worksheet.</returns>
        Public Shared Function Average(target As Worksheet, range As Range) As Cell
            Return GetBasicFormula(target, range, "AVERAGE", Nothing)
        End Function

        ''' <summary>
        ''' Returns a cell with a ceil formula
        ''' </summary>
        ''' <param name="address">Address to apply the ceil operation to.</param>
        ''' <param name="decimals">Number of decimals (digits).</param>
        ''' <returns>Prepared Cell object, ready to be added to a worksheet.</returns>
        Public Shared Function Ceil(address As Address, decimals As Integer) As Cell
            Return Ceil(Nothing, address, decimals)
        End Function

        ''' <summary>
        ''' Returns a cell with a ceil formula
        ''' </summary>
        ''' <param name="target">Target worksheet of the ceil operation. Can be null if on the same worksheet.</param>
        ''' <param name="address">Address to apply the ceil operation to.</param>
        ''' <param name="decimals">Number of decimals (digits).</param>
        ''' <returns>Prepared Cell object, ready to be added to a worksheet.</returns>
        Public Shared Function Ceil(target As Worksheet, address As Address, decimals As Integer) As Cell
            Return GetBasicFormula(target, New Range(address, address), "ROUNDUP", decimals.ToString(CultureInfo.InvariantCulture))
        End Function

        ''' <summary>
        ''' Returns a cell with a floor formula
        ''' </summary>
        ''' <param name="address">Address to apply the floor operation to.</param>
        ''' <param name="decimals">Number of decimals (digits).</param>
        ''' <returns>Prepared Cell object, ready to be added to a worksheet.</returns>
        Public Shared Function Floor(address As Address, decimals As Integer) As Cell
            Return Floor(Nothing, address, decimals)
        End Function

        ''' <summary>
        ''' Returns a cell with a floor formula
        ''' </summary>
        ''' <param name="target">Target worksheet of the floor operation. Can be null if on the same worksheet.</param>
        ''' <param name="address">Address to apply the floor operation to.</param>
        ''' <param name="decimals">Number of decimals (digits).</param>
        ''' <returns>Prepared Cell object, ready to be added to a worksheet.</returns>
        Public Shared Function Floor(target As Worksheet, address As Address, decimals As Integer) As Cell
            Return GetBasicFormula(target, New Range(address, address), "ROUNDDOWN", decimals.ToString(CultureInfo.InvariantCulture))
        End Function

        ''' <summary>
        ''' Returns a cell with a max formula
        ''' </summary>
        ''' <param name="range">Cell range to apply the max operation to.</param>
        ''' <returns>Prepared Cell object, ready to be added to a worksheet.</returns>
        Public Shared Function Max(range As Range) As Cell
            Return Max(Nothing, range)
        End Function

        ''' <summary>
        ''' Returns a cell with a max formula
        ''' </summary>
        ''' <param name="target">Target worksheet of the max operation. Can be null if on the same worksheet.</param>
        ''' <param name="range">Cell range to apply the max operation to.</param>
        ''' <returns>Prepared Cell object, ready to be added to a worksheet.</returns>
        Public Shared Function Max(target As Worksheet, range As Range) As Cell
            Return GetBasicFormula(target, range, "MAX", Nothing)
        End Function

        ''' <summary>
        ''' Returns a cell with a median formula
        ''' </summary>
        ''' <param name="range">Cell range to apply the median operation to.</param>
        ''' <returns>Prepared Cell object, ready to be added to a worksheet.</returns>
        Public Shared Function Median(range As Range) As Cell
            Return Median(Nothing, range)
        End Function

        ''' <summary>
        ''' Returns a cell with a median formula
        ''' </summary>
        ''' <param name="target">Target worksheet of the median operation. Can be null if on the same worksheet.</param>
        ''' <param name="range">Cell range to apply the median operation to.</param>
        ''' <returns>Prepared Cell object, ready to be added to a worksheet.</returns>
        Public Shared Function Median(target As Worksheet, range As Range) As Cell
            Return GetBasicFormula(target, range, "MEDIAN", Nothing)
        End Function

        ''' <summary>
        ''' Returns a cell with a min formula
        ''' </summary>
        ''' <param name="range">Cell range to apply the min operation to.</param>
        ''' <returns>Prepared Cell object, ready to be added to a worksheet.</returns>
        Public Shared Function Min(range As Range) As Cell
            Return Min(Nothing, range)
        End Function

        ''' <summary>
        ''' Returns a cell with a min formula
        ''' </summary>
        ''' <param name="target">Target worksheet of the min operation. Can be null if on the same worksheet.</param>
        ''' <param name="range">Cell range to apply the median operation to.</param>
        ''' <returns>Prepared Cell object, ready to be added to a worksheet.</returns>
        Public Shared Function Min(target As Worksheet, range As Range) As Cell
            Return GetBasicFormula(target, range, "MIN", Nothing)
        End Function

        ''' <summary>
        ''' Returns a cell with a round formula
        ''' </summary>
        ''' <param name="address">Address to apply the round operation to.</param>
        ''' <param name="decimals">Number of decimals (digits).</param>
        ''' <returns>Prepared Cell object, ready to be added to a worksheet.</returns>
        Public Shared Function Round(address As Address, decimals As Integer) As Cell
            Return Round(Nothing, address, decimals)
        End Function

        ''' <summary>
        ''' Returns a cell with a round formula
        ''' </summary>
        ''' <param name="target">Target worksheet of the round operation. Can be null if on the same worksheet.</param>
        ''' <param name="address">Address to apply the round operation to.</param>
        ''' <param name="decimals">Number of decimals (digits).</param>
        ''' <returns>Prepared Cell object, ready to be added to a worksheet.</returns>
        Public Shared Function Round(target As Worksheet, address As Address, decimals As Integer) As Cell
            Return GetBasicFormula(target, New Range(address, address), "ROUND", decimals.ToString(CultureInfo.InvariantCulture))
        End Function

        ''' <summary>
        ''' Returns a cell with a sum formula
        ''' </summary>
        ''' <param name="range">Cell range to get a sum of.</param>
        ''' <returns>Prepared Cell object, ready to be added to a worksheet.</returns>
        Public Shared Function Sum(range As Range) As Cell
            Return Sum(Nothing, range)
        End Function

        ''' <summary>
        ''' Returns a cell with a sum formula
        ''' </summary>
        ''' <param name="target">Target worksheet of the sum operation. Can be null if on the same worksheet.</param>
        ''' <param name="range">Cell range to get a sum of.</param>
        ''' <returns>Prepared Cell object, ready to be added to a worksheet.</returns>
        Public Shared Function Sum(target As Worksheet, range As Range) As Cell
            Return GetBasicFormula(target, range, "SUM", Nothing)
        End Function

        ''' <summary>
        ''' Function to generate a Vlookup as Excel function
        ''' </summary>
        ''' <param name="number">Numeric value for the lookup. Valid types are int, uint, long, ulong, float, double, byte, sbyte, decimal, short and ushort.</param>
        ''' <param name="range">Matrix of the lookup.</param>
        ''' <param name="columnIndex">Column index of the target column within the range (1 based).</param>
        ''' <param name="exactMatch">If true, an exact match is applied to the lookup.</param>
        ''' <returns>Prepared Cell object, ready to be added to a worksheet.</returns>
        Public Shared Function VLookup(number As Object, range As Range, columnIndex As Integer, exactMatch As Boolean) As Cell
            Return VLookup(number, Nothing, range, columnIndex, exactMatch)
        End Function

        ''' <summary>
        ''' Function to generate a Vlookup as Excel function
        ''' </summary>
        ''' <param name="number">Numeric value for the lookup.Valid types are int, uint, long, ulong, float, double, byte, sbyte, decimal, short and ushort.</param>
        ''' <param name="rangeTarget">Target worksheet of the matrix. Can be null if on the same worksheet.</param>
        ''' <param name="range">Matrix of the lookup.</param>
        ''' <param name="columnIndex">Column index of the target column within the range (1 based).</param>
        ''' <param name="exactMatch">If true, an exact match is applied to the lookup.</param>
        ''' <returns>Prepared Cell object, ready to be added to a worksheet.</returns>
        Public Shared Function VLookup(number As Object, rangeTarget As Worksheet, range As Range, columnIndex As Integer, exactMatch As Boolean) As Cell
            Return GetVLookup(Nothing, New Address(), number, rangeTarget, range, columnIndex, exactMatch, True)
        End Function

        ''' <summary>
        ''' Function to generate a Vlookup as Excel function
        ''' </summary>
        ''' <param name="address">Query address of a cell as string as source of the lookup.</param>
        ''' <param name="range">Matrix of the lookup.</param>
        ''' <param name="columnIndex">Column index of the target column within the range (1 based).</param>
        ''' <param name="exactMatch">If true, an exact match is applied to the lookup.</param>
        ''' <returns>Prepared Cell object, ready to be added to a worksheet.</returns>
        Public Shared Function VLookup(address As Address, range As Range, columnIndex As Integer, exactMatch As Boolean) As Cell
            Return VLookup(Nothing, address, Nothing, range, columnIndex, exactMatch)
        End Function

        ''' <summary>
        ''' Function to generate a Vlookup as Excel function
        ''' </summary>
        ''' <param name="queryTarget">Target worksheet of the query argument. Can be null if on the same worksheet.</param>
        ''' <param name="address">Query address of a cell as string as source of the lookup.</param>
        ''' <param name="rangeTarget">Target worksheet of the matrix. Can be null if on the same worksheet.</param>
        ''' <param name="range">Matrix of the lookup.</param>
        ''' <param name="columnIndex">Column index of the target column within the range (1 based).</param>
        ''' <param name="exactMatch">If true, an exact match is applied to the lookup.</param>
        ''' <returns>Prepared Cell object, ready to be added to a worksheet.</returns>
        Public Shared Function VLookup(queryTarget As Worksheet, address As Address, rangeTarget As Worksheet, range As Range, columnIndex As Integer, exactMatch As Boolean) As Cell
            Return GetVLookup(queryTarget, address, 0, rangeTarget, range, columnIndex, exactMatch, False)
        End Function

        ''' <summary>
        ''' Function to generate a Vlookup as Excel function
        ''' </summary>
        ''' <param name="queryTarget">Target worksheet of the query argument. Can be null if on the same worksheet.</param>
        ''' <param name="address">In case of a reference lookup, query address of a cell as string.</param>
        ''' <param name="number">In case of a numeric lookup, number for the lookup.</param>
        ''' <param name="rangeTarget">Target worksheet of the matrix. Can be null if on the same worksheet.</param>
        ''' <param name="range">Matrix of the lookup.</param>
        ''' <param name="columnIndex">Column index of the target column within the range (1 based).</param>
        ''' <param name="exactMatch">If true, an exact match is applied to the lookup.</param>
        ''' <param name="numericLookup">If true, the lookup is a numeric lookup, otherwise a reference lookup.</param>
        ''' <returns>Prepared Cell object, ready to be added to a worksheet.</returns>
        Private Shared Function GetVLookup(queryTarget As Worksheet, address As Address, number As Object, rangeTarget As Worksheet, range As Range, columnIndex As Integer, exactMatch As Boolean, numericLookup As Boolean) As Cell
            Dim rangeWidth = range.EndAddress.Column - range.StartAddress.Column + 1
            If columnIndex < 1 OrElse columnIndex > rangeWidth Then
                Throw New FormatException("The column index on range " & range.ToString() & " can only be between 1 and " & rangeWidth.ToString())
            End If
            Dim culture = CultureInfo.InvariantCulture
            Dim arg1, arg2, arg3, arg4 As String
            If numericLookup Then
                If number Is Nothing Then
                    Throw New FormatException("The lookup variable can only be a cell address or a numeric value. The passed value was null.")
                End If
                Dim t As Type = number.GetType()
                If t Is GetType(Byte) Then
                    arg1 = CByte(number).ToString("G", culture)
                ElseIf t Is GetType(SByte) Then
                    arg1 = CSByte(number).ToString("G", culture)
                ElseIf t Is GetType(Decimal) Then
                    arg1 = CDec(number).ToString("G", culture)
                ElseIf t Is GetType(Double) Then
                    arg1 = CDbl(number).ToString("G", culture)
                ElseIf t Is GetType(Single) Then
                    arg1 = CSng(number).ToString("G", culture)
                ElseIf t Is GetType(Integer) Then
                    arg1 = CInt(number).ToString("G", culture)
                ElseIf t Is GetType(UInteger) Then
                    arg1 = CUInt(number).ToString("G", culture)
                ElseIf t Is GetType(Long) Then
                    arg1 = CLng(number).ToString("G", culture)
                ElseIf t Is GetType(ULong) Then
                    arg1 = CULng(number).ToString("G", culture)
                ElseIf t Is GetType(Short) Then
                    arg1 = CShort(number).ToString("G", culture)
                ElseIf t Is GetType(UShort) Then
                    arg1 = CUShort(number).ToString("G", culture)
                Else
                    Throw New FormatException("The lookup variable can only be a cell address or a numeric value. The value '" & number.ToString() & "' is invalid.")
                End If
            Else
                If queryTarget IsNot Nothing Then
                    arg1 = queryTarget.SheetName & "!" & address.ToString()
                Else
                    arg1 = address.ToString()
                End If
            End If
            If rangeTarget IsNot Nothing Then
                arg2 = rangeTarget.SheetName & "!" & range.ToString()
            Else
                arg2 = range.ToString()
            End If
            arg3 = columnIndex.ToString("G", culture)
            If exactMatch Then
                arg4 = "TRUE"
            Else
                arg4 = "FALSE"
            End If
            Return New Cell("VLOOKUP(" & arg1 & "," & arg2 & "," & arg3 & "," & arg4 & ")", CellType.FORMULA)
        End Function

        ''' <summary>
        ''' Function to generate a basic Excel function with one cell range as parameter and an optional post argument
        ''' </summary>
        ''' <param name="target">Target worksheet of the cell reference. Can be null if on the same worksheet.</param>
        ''' <param name="range">Main argument as cell range. If applied on one cell, the start and end address are identical.</param>
        ''' <param name="functionName">Internal Excel function name.</param>
        ''' <param name="postArg">Optional argument.</param>
        ''' <returns>Prepared Cell object, ready to be added to a worksheet.</returns>
        Private Shared Function GetBasicFormula(target As Worksheet, range As Range, functionName As String, postArg As String) As Cell
            Dim arg1, arg2, prefix As String
            If Equals(postArg, Nothing) Then
                arg2 = ""
            Else
                arg2 = "," & postArg
            End If
            If target IsNot Nothing Then
                prefix = target.SheetName & "!"
            Else
                prefix = ""
            End If
            If range.StartAddress.Equals(range.EndAddress) Then
                arg1 = prefix & range.StartAddress.ToString()
            Else
                arg1 = prefix & range.ToString()
            End If
            Return New Cell(functionName & "(" & arg1 & arg2 & ")", CellType.FORMULA)
        End Function
    End Class
End Namespace
