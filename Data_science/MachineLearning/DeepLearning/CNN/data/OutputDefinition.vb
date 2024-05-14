#Region "Microsoft.VisualBasic::e0bae53a5df75fc3625caf6c9316d79d, Data_science\MachineLearning\DeepLearning\CNN\data\OutputDefinition.vb"

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

    '   Total Lines: 53
    '    Code Lines: 23
    ' Comment Lines: 21
    '   Blank Lines: 9
    '     File Size: 1.47 KB


    '     Class OutputDefinition
    ' 
    '         Properties: depth, len, outX, outY
    ' 
    '         Constructor: (+2 Overloads) Sub New
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Serialization.JSON

Namespace CNN.data

    ''' <summary>
    ''' This class will hold the definitions that bridge two layers.
    ''' So you can set values in one layer and use them in the next layer.
    ''' 
    ''' @author Daniel Persson (mailto.woden@gmail.com)
    ''' </summary>
    ''' <remarks>
    ''' width, height and depth
    ''' </remarks>
    Public Class OutputDefinition

        ''' <summary>
        ''' the image width
        ''' </summary>
        ''' <returns></returns>
        Public Property outX As Integer
        ''' <summary>
        ''' the image height
        ''' </summary>
        ''' <returns></returns>
        Public Property outY As Integer
        ''' <summary>
        ''' the data depth channel, example as 3 probably stands for rgb channels
        ''' </summary>
        ''' <returns></returns>
        Public Property depth As Integer

        Public ReadOnly Property len As Integer
            Get
                Return outX * outY * depth
            End Get
        End Property

        Sub New()
        End Sub

        Sub New(w As Integer, h As Integer, depth As Integer)
            Me.outX = w
            Me.outY = h
            Me.depth = depth
        End Sub

        Public Overrides Function ToString() As String
            Return $"w:{outX};h:{outY};d:{depth};len={len}"
        End Function

    End Class

End Namespace
