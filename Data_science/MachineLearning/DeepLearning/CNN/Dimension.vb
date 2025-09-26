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

        Public Shared ReadOnly Property One As Dimension
            Get
                Return New Dimension(1, 1)
            End Get
        End Property

        Public Sub New(x As Integer, y As Integer)
            Me.x = x
            Me.y = y
        End Sub

        Sub New()
        End Sub

        Public Overrides Function ToString() As String
            Return $"size[x:{x}, y:{y}]"
        End Function

        Public Overridable Function divide(scaleSize As Dimension) As Dimension
            Dim x As Integer = Me.x / scaleSize.x
            Dim y As Integer = Me.y / scaleSize.y

            If x * scaleSize.x <> Me.x OrElse y * scaleSize.y <> Me.y Then
                Call $"{Me.ToString} is not matched with scale {scaleSize.ToString}?".Warning
            End If

            Return New Dimension(x, y)
        End Function

        Public Overridable Function subtract(size As Dimension, append As Integer) As Dimension
            Dim x = Me.x - size.x + append
            Dim y = Me.y - size.y + append

            Return New Dimension(x, y)
        End Function
    End Class
End Namespace
