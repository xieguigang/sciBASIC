#Region "Microsoft.VisualBasic::6945eba6d7909a581cd5b3953b40efb2, Data\BinaryData\HDF5\structure\Layout.vb"

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

    '   Total Lines: 106
    '    Code Lines: 68 (64.15%)
    ' Comment Lines: 24 (22.64%)
    '    - Xml Docs: 66.67%
    ' 
    '   Blank Lines: 14 (13.21%)
    '     File Size: 3.77 KB


    '     Class Layout
    ' 
    '         Properties: chunkSize, dataAddress, dataset, dimensionLength, fields
    '                     isCompoundStruct, isEmpty, maxDimensionLength, numberOfDimensions
    ' 
    '         Function: ToString
    ' 
    '         Sub: addField, printValues
    ' 
    ' 
    ' /********************************************************************************/

#End Region

'
' * Mostly copied from NETCDF4 source code.
' * refer : http://www.unidata.ucar.edu
' * 
' * Modified by iychoi@email.arizona.edu
' 
Imports System.IO
Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.Data.IO.HDF5.dataset
Imports Microsoft.VisualBasic.Data.IO.HDF5.type
Imports Microsoft.VisualBasic.Language

Namespace struct

    ''' <summary>
    ''' 
    ''' </summary>
    Public Class Layout : Implements IFileDump

        ''' <summary>
        ''' 
        ''' </summary>
        Dim fieldList As New List(Of LayoutField)

        Public Property dataAddress As Long
        Public Property chunkSize As Integer()
        Public Property numberOfDimensions As Integer
        Public Property dimensionLength As Integer()
        Public Property maxDimensionLength As Integer()
        Public Property dataset As Hdf5Dataset

        ''' <summary>
        ''' 如果这个列表不是空的，则说明目标对象是<see cref="DataTypes.DATATYPE_COMPOUND"/>类型的结构体
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property fields() As IEnumerable(Of LayoutField)
            Get
                Return Me.fieldList
            End Get
        End Property

        ''' <summary>
        ''' 是否是<see cref="DataTypes.DATATYPE_COMPOUND"/>复合类型？
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property isCompoundStruct As Boolean
            Get
                Return Not fieldList.IsNullOrEmpty
            End Get
        End Property

        ''' <summary>
        ''' All of the values in this <see cref="Layout"/> object is empty!
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property isEmpty As Boolean
            Get
                Return dataAddress <= 0 AndAlso
                    chunkSize.IsNullOrEmpty AndAlso
                    numberOfDimensions <= 0 AndAlso
                    dimensionLength.IsNullOrEmpty AndAlso
                    maxDimensionLength.IsNullOrEmpty AndAlso
                    fieldList.IsNullOrEmpty
            End Get
        End Property

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub addField(name As String, offset As Integer, ndims As Integer, dataType As Integer, byteLength As Integer)
            fieldList += New LayoutField(name, offset, ndims, dataType, byteLength)
        End Sub

        Private Sub printValues(console As TextWriter) Implements IFileDump.printValues
            Dim dims As Integer = numberOfDimensions
            Dim dlength As Integer() = dimensionLength
            Dim maxdlength As Integer() = maxDimensionLength

            Call console.WriteLine("dimensions : " & dims)

            For i As Integer = 0 To dims - 1
                If chunkSize.Length > i Then
                    console.WriteLine("chunk size[" & i & "] : " & chunkSize(i))
                End If

                If dlength.Length > i Then
                    console.WriteLine("dimension length[" & i & "] : " & dlength(i))
                End If

                If maxdlength.Length > i Then
                    console.WriteLine("max dimension length[" & i & "] : " & maxdlength(i))
                End If
            Next
        End Sub

        Public Overrides Function ToString() As String
            If isEmpty Then
                Return "null"
            Else
                With New StringBuilder
                    Call printValues(New System.IO.StringWriter(.ByRef))
                    Return .ToString
                End With
            End If
        End Function
    End Class
End Namespace
