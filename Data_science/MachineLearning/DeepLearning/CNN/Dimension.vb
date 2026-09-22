#Region "Microsoft.VisualBasic::16664688b37c6530f12a4000d3e9f4c5, Data_science\MachineLearning\DeepLearning\CNN\Dimension.vb"

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

    '   Total Lines: 56
    '    Code Lines: 33 (58.93%)
    ' Comment Lines: 12 (21.43%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 11 (19.64%)
    '     File Size: 1.59 KB


    '     Class Dimension
    ' 
    '         Properties: One
    ' 
    '         Constructor: (+2 Overloads) Sub New
    '         Function: divide, subtract, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace CNN

    ''' <summary>
    ''' The layer dimension data
    ''' </summary>
    ''' <remarks>
    ''' width and height
    ''' </remarks>
    Public Class Dimension

        ''' <summary>
        ''' the image dimension width
        ''' </summary>
        Public ReadOnly x As Integer
        ''' <summary>
        ''' the image dimension height
        ''' </summary>
        Public ReadOnly y As Integer

        ''' <summary>Gets a 1 x 1 dimension.</summary>
        Public Shared ReadOnly Property One As Dimension
            Get
                Return New Dimension(1, 1)
            End Get
        End Property

        ''' <summary>
        ''' Creates a dimension with the given width and height.
        ''' </summary>
        ''' <param name="x">The width.</param>
        ''' <param name="y">The height.</param>
        Public Sub New(x As Integer, y As Integer)
            Me.x = x
            Me.y = y
        End Sub

        ''' <summary>Creates an empty (0 x 0) dimension, used by the deserializer.</summary>
        Sub New()
        End Sub

        ''' <summary>Returns a readable description of the dimension.</summary>
        ''' <returns>A text of the form <c>size[x:W, y:H]</c>.</returns>
        Public Overrides Function ToString() As String
            Return $"size[x:{x}, y:{y}]"
        End Function

        ''' <summary>
        ''' Divides this dimension by another one component wise, for example to compute the output size of a pooling
        ''' layer.
        ''' </summary>
        ''' <param name="scaleSize">The divisor dimension.</param>
        ''' <returns>The component wise quotient. A warning is logged when the division is not exact.</returns>
        Public Overridable Function divide(scaleSize As Dimension) As Dimension
            Dim x As Integer = Me.x / scaleSize.x
            Dim y As Integer = Me.y / scaleSize.y

            If x * scaleSize.x <> Me.x OrElse y * scaleSize.y <> Me.y Then
                Call $"{Me.ToString} is not matched with scale {scaleSize.ToString}?".Warning
            End If

            Return New Dimension(x, y)
        End Function

        ''' <summary>
        ''' Subtracts another dimension component wise and adds <paramref name="append"/> to both components, following
        ''' the convolution output size formula.
        ''' </summary>
        ''' <param name="size">The dimension to subtract, for example the filter window size.</param>
        ''' <param name="append">A constant added to both components, normally 1.</param>
        ''' <returns>The resulting dimension.</returns>
        Public Overridable Function subtract(size As Dimension, append As Integer) As Dimension
            Dim x = Me.x - size.x + append
            Dim y = Me.y - size.y + append

            Return New Dimension(x, y)
        End Function
    End Class
End Namespace
