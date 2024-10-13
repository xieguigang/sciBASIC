#Region "Microsoft.VisualBasic::c2512fac2139a9c89745e245e9b9bd06, Data_science\Mathematica\Math\Math\Algebra\Matrix.NET\Extensions\Serialization.vb"

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

    '   Total Lines: 55
    '    Code Lines: 32 (58.18%)
    ' Comment Lines: 13 (23.64%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 10 (18.18%)
    '     File Size: 1.71 KB


    '     Module Serialization
    ' 
    '         Function: Load
    ' 
    '         Sub: Save
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ApplicationServices.Debugging
Imports Microsoft.VisualBasic.Serialization.BinaryDumping

Namespace LinearAlgebra.Matrix

    ''' <summary>
    ''' data serialization of the numeric matrix
    ''' </summary>
    Public Module Serialization

        ReadOnly network As New NetworkByteOrderBuffer

        ''' <summary>
        ''' Save the current numeric matrix into a binary file
        ''' </summary>
        ''' <param name="m"></param>
        ''' <param name="s"></param>
        <Extension>
        Public Sub Save(m As NumericMatrix, s As Stream)
            Dim bin As New BinaryWriter(s)

            Call bin.Write(m.RowDimension)
            Call bin.Write(m.ColumnDimension)

            For Each row As Double() In m.Array
                Call bin.Write(network.GetBytes(row))
            Next

            Call bin.Flush()
        End Sub

        ''' <summary>
        ''' read matrix data from a binary file
        ''' </summary>
        ''' <param name="s"></param>
        ''' <returns></returns>
        Public Function Load(s As Stream) As NumericMatrix
            Dim bin As New BinaryReader(s)
            Dim rows = bin.ReadInt32
            Dim cols = bin.ReadInt32
            Dim m As New List(Of Double())
            Dim buffer_size = HeapSizeOf.double * cols
            Dim buf As Byte()

            For i As Integer = 0 To rows - 1
                buf = bin.ReadBytes(buffer_size)
                m.Add(network.decode(buf))
            Next

            Return New NumericMatrix(m)
        End Function
    End Module
End Namespace
