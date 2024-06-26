﻿#Region "Microsoft.VisualBasic::baf7b3165daa6c9638051182ce469f0f, Data_science\DataMining\UMAP\UmapProject.vb"

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

    '   Total Lines: 151
    '    Code Lines: 95 (62.91%)
    ' Comment Lines: 25 (16.56%)
    '    - Xml Docs: 96.00%
    ' 
    '   Blank Lines: 31 (20.53%)
    '     File Size: 4.51 KB


    ' Class UMAPProject
    ' 
    '     Properties: clusters, dimension, embedding, graph, labels
    '                 samples
    ' 
    '     Function: CreateProjection, ReadFile, SetLabels
    ' 
    '     Sub: WriteFile
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Runtime
Imports System.Text
Imports Microsoft.VisualBasic.ApplicationServices.Debugging
Imports Microsoft.VisualBasic.Serialization.BinaryDumping

''' <summary>
''' binary file data model for save the UMAP embedding result
''' </summary>
Public Class UMAPProject

    ''' <summary>
    ''' KNN graph 
    ''' </summary>
    ''' <returns></returns>
    Public Property graph As Double()()
    ''' <summary>
    ''' the UMAP embedding result
    ''' </summary>
    ''' <returns></returns>
    Public Property embedding As Double()()
    Public Property labels As String()
    ''' <summary>
    ''' width of the vector inside the <see cref="embedding"/> result.
    ''' </summary>
    ''' <returns></returns>
    Public Property dimension As Integer

    ''' <summary>
    ''' number of samples
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property samples As Integer
        Get
            Return embedding.Length
        End Get
    End Property

    Public Property clusters As String()

    Public Function SetLabels(labels As IEnumerable(Of String), Optional clusters As IEnumerable(Of String) = Nothing) As UMAPProject
        _labels = labels.ToArray

        If clusters IsNot Nothing Then
            _clusters = clusters.ToArray
        End If

        Return Me
    End Function

    ''' <summary>
    ''' extract matrix data from umap result
    ''' </summary>
    ''' <param name="umap"></param>
    ''' <returns></returns>
    Public Shared Function CreateProjection(umap As Umap) As UMAPProject
        Return New UMAPProject With {
            .dimension = umap.dimension,
            .embedding = umap.GetEmbedding.ToArray,
            .graph = umap.GetGraph.ToArray
        }
    End Function

    Public Shared Sub WriteFile(proj As UMAPProject, file As Stream)
        Dim wr As New BinaryWriter(file, Encoding.UTF8)
        Dim encoder As New NetworkByteOrderBuffer

        Call wr.Write("UMAP")
        Call wr.Write(proj.dimension)
        Call wr.Write(proj.samples)

        For Each str As String In proj.labels
            Call wr.Write(str)
        Next

        If proj.clusters Is Nothing OrElse proj.clusters.Length = 0 Then
            Call wr.Write(0)
        Else
            Call wr.Write(proj.clusters.Length)

            For Each str As String In proj.clusters
                Call wr.Write(str)
            Next
        End If

        Dim sizeof As Integer = HeapSizeOf.double * proj.samples

        For Each vec As Double() In proj.graph
            Call wr.Write(encoder.encode(vec), 0, sizeof)
        Next

        sizeof = HeapSizeOf.double * proj.dimension

        For Each vec As Double() In proj.embedding
            Call wr.Write(encoder.encode(vec), 0, sizeof)
        Next

        Call wr.Flush()
    End Sub

    Public Shared Function ReadFile(file As Stream) As UMAPProject
        Dim rd As New BinaryReader(file, Encoding.UTF8)
        Dim decoder As New NetworkByteOrderBuffer

        If rd.ReadString <> "UMAP" Then
            Throw New InvalidProgramException("invalid magic number!")
        End If

        Dim dims As Integer = rd.ReadInt32
        Dim samples As Integer = rd.ReadInt32
        Dim labels As String() = New String(samples - 1) {}

        For i As Integer = 0 To samples - 1
            labels(i) = rd.ReadString
        Next

        Dim n As Integer = rd.ReadInt32
        Dim clusters As String() = New String(n - 1) {}

        For i As Integer = 0 To n - 1
            clusters(i) = rd.ReadString
        Next

        Dim buf As Byte() = New Byte(HeapSizeOf.double * samples - 1) {}
        ' graph is (N x N) size
        Dim graph As Double()() = New Double(samples - 1)() {}

        For i As Integer = 0 To samples - 1
            rd.Read(buf, Scan0, buf.Length)
            graph(i) = decoder.decode(buf)
        Next

        Dim umap As Double()() = New Double(samples - 1)() {}

        buf = New Byte(HeapSizeOf.double * dims - 1) {}

        For i As Integer = 0 To samples - 1
            rd.Read(buf, Scan0, buf.Length)
            umap(i) = decoder.decode(buf)
        Next

        Return New UMAPProject With {
            .clusters = clusters,
            .dimension = dims,
            .embedding = umap,
            .graph = graph,
            .labels = labels
        }
    End Function

End Class
