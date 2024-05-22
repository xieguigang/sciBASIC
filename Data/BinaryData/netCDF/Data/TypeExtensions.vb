#Region "Microsoft.VisualBasic::e18b034665be000463e79ecfc675a9aa, Data\BinaryData\netCDF\Data\TypeExtensions.vb"

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

    '   Total Lines: 100
    '    Code Lines: 70 (70.00%)
    ' Comment Lines: 21 (21.00%)
    '    - Xml Docs: 71.43%
    ' 
    '   Blank Lines: 9 (9.00%)
    '     File Size: 4.26 KB


    '     Module TypeExtensions
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: GetCDFTypeCode, num2str, sizeof, str2num, ToType
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection

Namespace Data

    <HideModuleName>
    Public Module TypeExtensions

        ReadOnly description As Dictionary(Of CDFDataTypes, String)
        ReadOnly enumParser As Dictionary(Of String, CDFDataTypes)

        Sub New()
            description = Enums(Of CDFDataTypes).ToDictionary(Function(type) type, Function(type) type.Description)
            enumParser = description.ReverseMaps
        End Sub

        <Extension>
        Public Function GetCDFTypeCode(type As Type) As CDFDataTypes
            Select Case type
                Case GetType(Double) : Return CDFDataTypes.NC_DOUBLE
                Case GetType(Single) : Return CDFDataTypes.NC_FLOAT
                Case GetType(Integer) : Return CDFDataTypes.NC_INT
                Case GetType(Short) : Return CDFDataTypes.NC_SHORT
                Case GetType(String) : Return CDFDataTypes.NC_CHAR
                Case GetType(Byte) : Return CDFDataTypes.NC_BYTE
                Case GetType(Long) : Return CDFDataTypes.NC_INT64
                Case GetType(Boolean) : Return CDFDataTypes.BOOLEAN
                Case Else
                    Return CDFDataTypes.undefined
            End Select
        End Function

        ''' <summary>
        ''' Parse a number into their respective type
        ''' </summary>
        ''' <param name="type">type - integer that represents the type</param>
        ''' <returns>parsed value of the type</returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function num2str(type As CDFDataTypes) As String
            ' ([default]:="undefined") istanbul ignore next 
            Return description.TryGetValue(type, [default]:="undefined")
        End Function

        ''' <summary>
        ''' Parse a number type identifier to his size in bytes
        ''' </summary>
        ''' <param name="type">type - integer that represents the type</param>
        ''' <returns>size of the type</returns>
        Public Function sizeof(type As CDFDataTypes) As Integer
            Select Case type
                Case CDFDataTypes.NC_BYTE, CDFDataTypes.BOOLEAN
                    Return 1
                Case CDFDataTypes.NC_CHAR
                    Return 1
                Case CDFDataTypes.NC_SHORT
                    Return 2
                Case CDFDataTypes.NC_INT
                    Return 4
                Case CDFDataTypes.NC_FLOAT
                    Return 4
                Case CDFDataTypes.NC_DOUBLE, CDFDataTypes.NC_INT64
                    Return 8
                Case Else
                    ' istanbul ignore next 
                    Return -1
            End Select
        End Function

        ''' <summary>
        ''' Reverse search of num2str
        ''' </summary>
        ''' <param name="type">type - string that represents the type</param>
        ''' <returns>parsed value of the type</returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function str2num(type As String) As CDFDataTypes
            Return enumParser.TryGetValue(LCase(type), [default]:=CDFDataTypes.undefined)
        End Function

        <Extension>
        Public Function ToType(type As CDFDataTypes) As Type
            Select Case type
                Case CDFDataTypes.NC_BYTE : Return GetType(Byte)
                Case CDFDataTypes.NC_CHAR : Return GetType(Char)
                Case CDFDataTypes.BOOLEAN
                    ' 20210212 bytes flags for maps boolean
                    Return GetType(Boolean)
                Case CDFDataTypes.NC_DOUBLE : Return GetType(Double)
                Case CDFDataTypes.NC_FLOAT : Return GetType(Single)
                Case CDFDataTypes.NC_INT : Return GetType(Integer)
                Case CDFDataTypes.NC_INT64 : Return GetType(Long)
                Case CDFDataTypes.NC_SHORT : Return GetType(Short)
                Case Else
                    ' istanbul ignore next
                    Return Utils.notNetcdf(True, $"non valid type {type}")
            End Select
        End Function
    End Module
End Namespace
