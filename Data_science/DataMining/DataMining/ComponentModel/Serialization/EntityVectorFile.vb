#Region "Microsoft.VisualBasic::6f6cf241bc969fdbe96c852ba07d6544, Data_science\DataMining\DataMining\ComponentModel\Serialization\EntityVectorFile.vb"

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

    '   Total Lines: 73
    '    Code Lines: 55 (75.34%)
    ' Comment Lines: 3 (4.11%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 15 (20.55%)
    '     File Size: 2.46 KB


    '     Class EntityVectorFile
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: GetBinaryParser, GetSerializer, LoadVectors
    ' 
    '         Sub: SaveVector
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports Microsoft.VisualBasic.DataMining.KMeans
Imports Microsoft.VisualBasic.Linq

Namespace ComponentModel.Serialization

    ''' <summary>
    ''' helper module for IPC parallel or store the result data
    ''' </summary>
    Public NotInheritable Class EntityVectorFile

        Private Sub New()
        End Sub

        Public Shared Function GetSerializer() As Func(Of ClusterEntity(), Stream)
            Return Function(vec)
                       Dim ms As New MemoryStream
                       Call SaveVector(vec, ms)
                       Call ms.Flush()
                       Call ms.Seek(0, SeekOrigin.Begin)
                       Return ms
                   End Function
        End Function

        Public Shared Function GetBinaryParser() As Func(Of Stream, ClusterEntity())
            Return Function(s)
                       Return LoadVectors(s).ToArray
                   End Function
        End Function

        Public Shared Sub SaveVector(vectors As IEnumerable(Of ClusterEntity), file As Stream)
            Dim bin As New BinaryWriter(file)
            Dim pullAll As ClusterEntity() = vectors.SafeQuery.ToArray

            Call bin.Write(pullAll.Length)

            For Each vi As ClusterEntity In pullAll
                Call bin.Write(vi.uid)
                Call bin.Write(vi.cluster)
                Call bin.Write(vi.Length)

                For Each xi As Double In vi.entityVector
                    Call bin.Write(xi)
                Next
            Next

            Call bin.Flush()
        End Sub

        Public Shared Iterator Function LoadVectors(file As Stream) As IEnumerable(Of ClusterEntity)
            Dim bin As New BinaryReader(file)
            Dim n As Integer = bin.ReadInt32

            For i As Integer = 0 To n - 1
                Dim id As String = bin.ReadString
                Dim class_id As Integer = bin.ReadInt32
                Dim width As Integer = bin.ReadInt32
                Dim v As Double() = New Double(width - 1) {}

                For j As Integer = 0 To v.Length - 1
                    v(j) = bin.ReadDouble
                Next

                Yield New ClusterEntity With {
                    .cluster = class_id,
                    .uid = id,
                    .entityVector = v
                }
            Next
        End Function

    End Class
End Namespace
