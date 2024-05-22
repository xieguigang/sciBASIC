#Region "Microsoft.VisualBasic::4947f57b5f5c5f39af6121c5af85f291, Data_science\DataMining\DataMining\Clustering\FuzzyCMeans\Classify.vb"

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

    '   Total Lines: 64
    '    Code Lines: 51 (79.69%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 13 (20.31%)
    '     File Size: 2.57 KB


    '     Class Classify
    ' 
    '         Properties: center, Id, members
    ' 
    '         Function: GetBuffer, Load
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Serialization
Imports Microsoft.VisualBasic.Serialization.Bencoding
Imports Microsoft.VisualBasic.Serialization.BinaryDumping

Namespace FuzzyCMeans

    Public Class Classify

        Public Property Id As Integer
        Public Property members As New List(Of FuzzyCMeansEntity)
        Public Property center As Double()

        Public Shared Function GetBuffer(x As FuzzyCMeansEntity) As Byte()
            Using ms As New MemoryStream, bin As New BinaryWriter(ms)
                Dim membership As String = Bencoding.ToBEncodeString(x.memberships)

                Static buffer As New NetworkByteOrderBuffer

                Call bin.Write(x.uid)
                Call bin.Write(x.cluster)
                Call bin.Write(x.MarkClusterCenter.ToHtmlColor)
                Call bin.Write(membership)
                Call bin.Write(x.entityVector.Length)
                Call bin.Write(buffer.encode(x.entityVector))
                Call bin.Flush()

                Return ms.ToArray
            End Using
        End Function

        Public Shared Function Load(data As Byte()) As FuzzyCMeansEntity
            Static buffer As New NetworkByteOrderBuffer

            Using bin As New BinaryReader(New MemoryStream(data))
                Dim uid As String = bin.ReadString
                Dim cluster As Integer = bin.ReadInt32
                Dim color As String = bin.ReadString
                Dim memberBcode As String = bin.ReadString
                Dim vsize As Integer = bin.ReadInt32 * 8
                Dim v As Double() = buffer.decode(bin.ReadBytes(vsize))
                Dim memberships As BDictionary = BencodeDecoder.Decode(memberBcode)(Scan0)
                Dim memberData As New Dictionary(Of Integer, Double)

                For Each tuple As KeyValuePair(Of BString, BElement) In memberships
                    Dim i As Integer = Integer.Parse(tuple.Key.Value)
                    Dim value As Double = Double.Parse(DirectCast(tuple.Value, BString).Value)

                    Call memberData.Add(i, value)
                Next

                Return New FuzzyCMeansEntity With {
                    .uid = uid,
                    .MarkClusterCenter = color.TranslateColor,
                    .cluster = cluster,
                    .entityVector = v,
                    .memberships = memberData
                }
            End Using
        End Function
    End Class

End Namespace
