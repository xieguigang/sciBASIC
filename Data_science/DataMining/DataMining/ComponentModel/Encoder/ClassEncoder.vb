#Region "Microsoft.VisualBasic::1278cb38aaddd6612ebec571926ae901, sciBASIC#\Data_science\DataMining\DataMining\ComponentModel\Encoder\ClassEncoder.vb"

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

    '   Total Lines: 114
    '    Code Lines: 72
    ' Comment Lines: 21
    '   Blank Lines: 21
    '     File Size: 3.53 KB


    '     Class ClassEncoder
    ' 
    '         Properties: Colors
    ' 
    '         Constructor: (+2 Overloads) Sub New
    '         Function: (+2 Overloads) AddClass, GetColor, PopulateFactors, Union
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports stdNum = System.Math

Namespace ComponentModel.Encoder

    Public Class ClassEncoder

        Dim m_colors As New Dictionary(Of String, ColorClass)
        Dim labels As New List(Of String)

        ''' <summary>
        ''' get unique class label list
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks>
        ''' apply for save to file
        ''' </remarks>
        Public ReadOnly Property Colors As ColorClass()
            Get
                Return m_colors.Values.ToArray
            End Get
        End Property

        Sub New()
        End Sub

        Sub New(vector As IEnumerable(Of ColorClass))
            For Each item In vector
                Call AddClass(item)
            Next
        End Sub

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="color"></param>
        ''' <returns></returns>
        ''' <remarks>
        ''' apply for load from file
        ''' </remarks>
        Public Function AddClass(color As ColorClass) As ClassEncoder
            If Not m_colors.ContainsKey(color.name) Then
                m_colors.Add(color.name, color)
            End If

            labels.Add(color.name)

            Return Me
        End Function

        Public Function AddClass(label As String) As ClassEncoder
            If Not m_colors.ContainsKey(label) Then
                Dim enumInt As Integer

                If m_colors.Count = 0 Then
                    enumInt = 0
                Else
                    enumInt = m_colors _
                        .Values _
                        .Select(Function(a) a.enumInt) _
                        .Max
                End If

                m_colors.Add(label, New ColorClass With {.color = "#000000", .enumInt = enumInt + 1, .name = label})
            End If

            labels.Add(label)

            Return Me
        End Function

        Public Function GetColor(value As Double) As ColorClass
            Dim min = m_colors.Values _
                .Select(Function(cls)
                            Return (ds:=stdNum.Abs(cls.enumInt - value), cls)
                        End Function) _
                .OrderBy(Function(a) a.ds) _
                .First

            Return min.cls
        End Function

        Public Iterator Function PopulateFactors() As IEnumerable(Of ColorClass)
            For Each label As String In labels
                Dim template As ColorClass = m_colors(label)
                Dim factor As New ColorClass With {
                    .color = template.color,
                    .enumInt = template.enumInt,
                    .name = template.name
                }

                Yield factor
            Next
        End Function

        ''' <summary>
        ''' union of two factor collection
        ''' </summary>
        ''' <param name="classList"></param>
        ''' <param name="newLabels"></param>
        ''' <returns></returns>
        Public Shared Function Union(classList As IEnumerable(Of ColorClass), newLabels As IEnumerable(Of String)) As ColorClass()
            Dim encoder As New ClassEncoder

            For Each cls In classList
                encoder.AddClass(cls)
            Next
            For Each label As String In newLabels
                encoder.AddClass(label)
            Next

            Return encoder.PopulateFactors.ToArray
        End Function
    End Class
End Namespace
