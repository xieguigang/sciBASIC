#Region "Microsoft.VisualBasic::e11f017f7752fe0e038947175846b73b, Data\BinaryData\netCDF\netCDFReader.vb"

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

    '   Total Lines: 404
    '    Code Lines: 179 (44.31%)
    ' Comment Lines: 185 (45.79%)
    '    - Xml Docs: 48.65%
    ' 
    '   Blank Lines: 40 (9.90%)
    '     File Size: 16.23 KB


    ' Class netCDFReader
    ' 
    '     Properties: dimensions, globalAttributes, recordDimension, variables, version
    ' 
    '     Constructor: (+3 Overloads) Sub New
    ' 
    '     Function: attributeExists, dataVariableExists, FindAttribute, (+2 Overloads) getDataVariable, getDataVariableAsString
    '               getDataVariableEntry, Open, ToString
    ' 
    '     Sub: (+2 Overloads) Dispose, getDataVariable, Print
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.Data.IO
Imports Microsoft.VisualBasic.DataStorage.netCDF.Components
Imports Microsoft.VisualBasic.DataStorage.netCDF.Data
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Serialization.JSON
Imports Microsoft.VisualBasic.Text
Imports System.Runtime.InteropServices


#If NETCOREAPP Then
Imports System.Data
#End If

'* Copyright 2004 University Corporation for Atmospheric Research/Unidata
'* 
'* Portions of this software were developed by the Unidata Program at the 
'* University Corporation for Atmospheric Research.
'* 
'* Access and use of this software shall impose the following obligations
'* and understandings on the user. The user is granted the right, without
'* any fee or cost, to use, copy, modify, alter, enhance and distribute
'* this software, and any derivative works thereof, and its supporting
'* documentation for any purpose whatsoever, provided that this entire
'* notice appears in all copies of the software, derivative works and
'* supporting documentation.  Further, UCAR requests that the user credit
'* UCAR/Unidata in any publications that result from the use of this
'* software or in any product that includes this software. The names UCAR
'* and/or Unidata, however, may not be used in any advertising or publicity
'* to endorse or promote any products or commercial entity unless specific
'* written permission is obtained from UCAR/Unidata. The user also
'* understands that UCAR/Unidata is not obligated to provide the user with
'* any support, consulting, training or assistance of any kind with regard
'* to the use, operation and performance of this software nor to provide
'* the user with any updates, revisions, new versions or "bug fixes."
'* 
'* THIS SOFTWARE IS PROVIDED BY UCAR/UNIDATA "AS IS" AND ANY EXPRESS OR
'* IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
'* WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
'* DISCLAIMED. IN NO EVENT SHALL UCAR/UNIDATA BE LIABLE FOR ANY SPECIAL,
'* INDIRECT OR CONSEQUENTIAL DAMAGES OR ANY DAMAGES WHATSOEVER RESULTING
'* FROM LOSS OF USE, DATA OR PROFITS, WHETHER IN AN ACTION OF CONTRACT,
'* NEGLIGENCE OR OTHER TORTIOUS ACTION, ARISING OUT OF OR IN CONNECTION
'* WITH THE ACCESS, USE OR PERFORMANCE OF THIS SOFTWARE.
'
'This is a wrapper class for the netCDF dll.
'
'Get the netCDF dll from ftp://ftp.unidata.ucar.edu/pub/netcdf/contrib/win32
'Put it somewhere in your path, or else in the bin subdirectory of your
'VB project.
'
'Then include this class file in your project. Use the netcdf functions 
'like this:
'res = NetCDF.nc_create(name, NetCDF.cmode.NC_CLOBBER, ncid)
'If (res <> 0) Then GoTo err
'
'NetCDF was ported to dll by John Caron (as far as I know).
'This VB.NET wrapper created by Ed Hartnett, 3/10/4
'
'Some notes:
'   Although the dll can be tested (and has passed for release 
'3.5.0 and 3.5.1 at least), the VB wrapper class has not been
'extensively tested. Use at your own risk. Writing test code to
'test the netCDF interface is a non-trivial task, and one I haven't
'undertaken. The tests run verify common use of netCDF, for example
'creation of dims, vars, and atts of various types, and ensuring that
'they can be written and read back. But I don't check type conversion,
'or boundery conditions. These are all tested in the dll, but not the
'VB wrapper.
'
'This class consists mearly of some defined enums, consts and declares, 
'all inside a class called NetCDF.
'
'Passing strings: when passing in a string to a function, use a string,
'when passing in a pointer to a string so that the function can fill it 
'(for example when requesting an attribute name, use a 
'System.Text.StringBuilder.
'
'Since VB doesn't have an unsigned byte, I've left those functions 
'out of the wrapper class. If you need to read unsigned bytes, read them as 
'shorts, and netcdf will automatically convert them for you.
'

''' <summary>
''' The dotCDF file of a CDF contains magic numbers and numerous
''' internal records are used to organize information about the 
''' contents Of the CDF (For both Single-file And multi-file 
''' CDFs).
''' </summary>
''' <remarks>
''' https://github.com/cheminfo-js/netcdfjs
''' </remarks>
Public Class netCDFReader : Implements IDisposable

    Dim buffer As BinaryDataReader
    Dim header As Header
    Dim globalAttributeTable As Dictionary(Of String, attribute)
    Dim variableTable As Dictionary(Of String, variable)

    Public Const Magic$ = "CDF"

    ''' <summary>
    ''' Version for the NetCDF format
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property version As String
        Get
            If (header.version = 1) Then
                Return "classic format"
            Else
                Return "64-bit offset format"
            End If
        End Get
    End Property

    ''' <summary>
    ''' Metadata for the record dimension
    ''' 
    '''  + `length`: Number of elements in the record dimension
    '''  + `id`: Id number In the list Of dimensions For the record dimension
    '''  + `name`: String with the name of the record dimension
    '''  + `recordStep`: Number with the record variables step size
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property recordDimension As recordDimension
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Get
            Return header.recordDimension
        End Get
    End Property

    ''' <summary>
    ''' List of dimensions with:
    ''' 
    '''  + `name`: String with the name of the dimension
    '''  + `size`: Number with the size of the dimension
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks>
    ''' 一个cdf文件之中只能够有一种<see cref="Dimension"/>可以是矩阵类型的么？
    ''' </remarks>
    Public ReadOnly Property dimensions As Dimension()
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Get
            Return header.dimensions
        End Get
    End Property

    ''' <summary>
    ''' List of global attributes with:
    ''' 
    '''  + `name`: String with the name of the attribute
    '''  + `type`: String with the type of the attribute
    '''  + `value`: A number Or String With the value Of the attribute
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property globalAttributes As attribute()
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Get
            Return header.globalAttributes
        End Get
    End Property

    ''' <summary>
    ''' Returns the value of an global attribute
    ''' </summary>
    ''' <param name="attributeName">attributeName</param>
    ''' <returns><see cref="attribute.getObjectValue"/> result of the netcdf attribute which 
    ''' is specific via the <paramref name="attributeName"/> Or undefined</returns>
    Default Public ReadOnly Property getAttribute(attributeName As String) As Object
        Get
            With globalAttributeTable.TryGetValue(attributeName)
                If .IsNothing Then
                    Return Nothing
                Else
                    Return .getObjectValue
                End If
            End With
        End Get
    End Property

    ''' <summary>
    ''' List of variables with:
    ''' 
    '''  + `name`: String with the name of the variable
    '''  + `dimensions`: Array with the dimension IDs of the variable
    '''  + `attributes`: Array with the attributes of the variable
    '''  + `type`: String with the type of the variable
    '''  + `size`: Number with the size of the variable
    '''  + `offset`: Number with the offset where of the variable begins
    '''  + `record`: True if Is a record variable, false otherwise
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property variables As variable()
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Get
            Return header.variables
        End Get
    End Property

    Sub New(buffer As BinaryDataReader, Optional ignoreDuplicated As Boolean = False)
        Dim version As Value(Of Byte) = Scan0

        buffer.ByteOrder = ByteOrder.BigEndian
        ' Test if file in support format
        Utils.notNetcdf(buffer.ReadString(3) <> Magic, $"should start with {Magic}")
        Utils.notNetcdf((version = buffer.ReadByte) > 2, "unknown version")

        Me.header = New Header(buffer, version)
        Me.buffer = buffer
        Me.globalAttributeTable = header _
            .globalAttributes _
            .ToDictionary(Function(att) att.name)

        Dim conflictsId As String() = header.checkVariableIdConflicts.ToArray

        If conflictsId.Length > 0 Then
            If ignoreDuplicated Then
                Me.variableTable = header.variables _
                    .GroupBy(Function(v) v.name) _
                    .ToDictionary(Function(var)
                                      Return var.Key
                                  End Function,
                                  Function(group)
                                      Return group.First
                                  End Function)
            Else
                Throw New DuplicateNameException(conflictsId.GetJson)
            End If
        Else
            Me.variableTable = header _
                .variables _
                .ToDictionary(Function(var)
                                  Return var.name
                              End Function)
        End If
    End Sub

    Sub New(file As Stream, Optional encoding As Encodings = Encodings.UTF8, Optional ignoreDuplicated As Boolean = False)
        Call Me.New(New BinaryDataReader(file, encoding), ignoreDuplicated)
    End Sub

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Sub New(path$, Optional encoding As Encodings = Encodings.UTF8, Optional ignoreDuplicated As Boolean = False)
        Call Me.New(path.OpenBinaryReader(encoding), ignoreDuplicated)
    End Sub

    Public Function FindAttribute(ParamArray synonym As String()) As Object
        For Each attributeName As String In synonym
            With globalAttributeTable.TryGetValue(attributeName)
                If Not .IsNothing Then
                    Return .getObjectValue
                End If
            End With
        Next

        Return Nothing
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Shared Function Open(filePath$, Optional encoding As Encodings = Encodings.UTF8) As netCDFReader
        Return New netCDFReader(filePath, encoding)
    End Function

    ''' <summary>
    ''' Returns the value of a variable as a string
    ''' </summary>
    ''' <param name="variableName">variableName</param>
    ''' <returns>Value of the variable as a string Or undefined</returns>
    ''' 
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Function getDataVariableAsString(variableName As String) As String
        Return getDataVariable(variableName).ToString
    End Function

    ''' <summary>
    ''' Retrieves the data for a given variable
    ''' </summary>
    ''' <returns>List with the variable values</returns>
    Public Function getDataVariable(variable As variable) As ICDFDataVector
        Dim values As Array

        ' go to the offset position
        Call buffer.Seek(variable.offset, SeekOrigin.Begin)

        If (variable.record) Then
            ' record variable case
            values = DataReader.record(buffer, variable, header.recordDimension)
        Else
            ' non-record variable case
            values = DataReader.nonRecord(buffer, variable)
        End If

        Return VectorHelper.FromAny(values, variable.type)
    End Function

    ''' <summary>
    ''' Retrieves the data for a given variable
    ''' </summary>
    ''' <param name="variableName">Name of the variable to search Or variable object</param>
    ''' <param name="value">List with the variable values</param>
    ''' 
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Sub getDataVariable(variableName As String, <Out> ByRef value As ICDFDataVector)
        value = getDataVariable(variableName, [overrides]:=value?.cdfDataType)
    End Sub

    ''' <summary>
    ''' Retrieves the data for a given variable
    ''' </summary>
    ''' <param name="variableName">Name of the variable to search Or variable object</param>
    ''' <returns>List with the variable values</returns>
    Public Function getDataVariable(variableName As String, Optional [overrides] As CDFDataTypes? = Nothing) As ICDFDataVector
        ' search the variable
        Dim variable As variable = variableTable.TryGetValue(variableName)
        ' throws if variable Not found
        Utils.notNetcdf(variable Is Nothing, $"variable Not found: {variableName}")
        If Not [overrides] Is Nothing Then
            variable.type = [overrides]
        End If

        Return getDataVariable(variable)
    End Function

    Public Function getDataVariableEntry(variableName As String) As variable
        Return variableTable.TryGetValue(variableName)
    End Function

    ''' <summary>
    ''' Check if a dataVariable exists
    ''' </summary>
    ''' <param name="variableName">Name of the variable to find</param>
    ''' <returns></returns>
    ''' 
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Function dataVariableExists(variableName As String) As Boolean
        Return variableTable.ContainsKey(variableName)
    End Function

    ''' <summary>
    ''' Check if an attribute exists
    ''' </summary>
    ''' <param name="attributeName">Name of the attribute to find</param>
    ''' <returns></returns>
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Function attributeExists(attributeName As String) As Boolean
        Return globalAttributeTable.ContainsKey(attributeName)
    End Function

    ''' <summary>
    ''' CDF file data summary
    ''' </summary>
    ''' <returns></returns>
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Overrides Function ToString() As String
        With New StringBuilder
            Call netCDF.toString(Me, New System.IO.StringWriter(.ByRef))
            Return .ToString
        End With
    End Function

    ''' <summary>
    ''' Print CDF file data summary on console screen std_output
    ''' </summary>
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Sub Print()
        Call Me.toString(New StreamWriter(Console.OpenStandardOutput))
    End Sub

#Region "IDisposable Support"
    Private disposedValue As Boolean ' 要检测冗余调用

    ' IDisposable
    Protected Overridable Sub Dispose(disposing As Boolean)
        If Not disposedValue Then
            If disposing Then
                ' TODO: 释放托管状态(托管对象)。
                Call buffer.Dispose()
            End If

            ' TODO: 释放未托管资源(未托管对象)并在以下内容中替代 Finalize()。
            ' TODO: 将大型字段设置为 null。
        End If
        disposedValue = True
    End Sub

    ' TODO: 仅当以上 Dispose(disposing As Boolean)拥有用于释放未托管资源的代码时才替代 Finalize()。
    'Protected Overrides Sub Finalize()
    '    ' 请勿更改此代码。将清理代码放入以上 Dispose(disposing As Boolean)中。
    '    Dispose(False)
    '    MyBase.Finalize()
    'End Sub

    ' Visual Basic 添加此代码以正确实现可释放模式。
    Public Sub Dispose() Implements IDisposable.Dispose
        ' 请勿更改此代码。将清理代码放入以上 Dispose(disposing As Boolean)中。
        Dispose(True)
        ' TODO: 如果在以上内容中替代了 Finalize()，则取消注释以下行。
        ' GC.SuppressFinalize(Me)
    End Sub
#End Region
End Class
