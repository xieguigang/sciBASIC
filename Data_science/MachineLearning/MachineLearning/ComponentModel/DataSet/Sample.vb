#Region "Microsoft.VisualBasic::7ee0cb77fe51666652281800f97b92aa, sciBASIC#\Data_science\MachineLearning\MachineLearning\ComponentModel\DataSet\Sample.vb"

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

    '   Total Lines: 99
    '    Code Lines: 55
    ' Comment Lines: 31
    '   Blank Lines: 13
    '     File Size: 3.41 KB


    '     Class Sample
    ' 
    '         Properties: ID, status, target, vector
    ' 
    '         Constructor: (+3 Overloads) Sub New
    ' 
    '         Function: decodeVector, ToString
    ' 
    '         Sub: encodeVector
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel.Repository
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports Microsoft.VisualBasic.Net.Http

Namespace ComponentModel.StoreProcedure

    ''' <summary>
    ''' The training dataset
    ''' </summary>
    Public Class Sample : Implements INamedValue

        ''' <summary>
        ''' 可选的数据集唯一标记信息
        ''' </summary>
        ''' <returns></returns>
        <XmlAttribute("id")>
        Public Property ID As String Implements IKeyedEntity(Of String).Key

        ''' <summary>
        ''' Neuron network input parameters
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks>
        ''' 属性值可能会很长,为了XML文件的美观,在这里使用element
        ''' 
        ''' 20200318 there is a bugs in xml serialization of the double array elements
        ''' and the numeric value is also not suitable use text as file save format
        ''' so the data for this property is save with base64 encoded of the numeric 
        ''' array.
        ''' </remarks>
        <XmlElement>
        Public Property status As String

        ''' <summary>
        ''' The network expected output values
        ''' </summary>
        ''' <returns></returns>
        <XmlAttribute>
        Public Property target As Double()

        <XmlIgnore>
        Public ReadOnly Property vector As Double()
            Get
                Return decodeVector.ToArray
            End Get
        End Property

        ''' <summary>
        ''' Create a new training dataset
        ''' </summary>
        ''' <param name="values">Neuron network input parameters</param>
        ''' <param name="targets">The network expected output values</param>
        Public Sub New(values#(), targets#(), Optional inputName$ = Nothing)
            Me.target = targets
            Me.ID = inputName
            Me.encodeVector(values)
        End Sub

        ''' <summary>
        ''' Create a new empty training dataset
        ''' </summary>
        Sub New()
        End Sub

        Sub New(samples As IEnumerable(Of Double))
            Call Me.encodeVector(samples)
        End Sub

        Private Iterator Function decodeVector() As IEnumerable(Of Double)
            Using buffer = status.Base64RawBytes.UnGzipStream
                For Each block As Byte() In buffer.ToArray.Split(8)
                    Yield BitConverter.ToDouble(block, Scan0)
                Next
            End Using
        End Function

        Private Sub encodeVector(data As IEnumerable(Of Double))
            Using buffer As New MemoryStream
                For Each x As Double In data
                    buffer.Write(BitConverter.GetBytes(x), Scan0, 8)
                Next

                status = buffer _
                    .GZipStream _
                    .ToArray _
                    .ToBase64String
            End Using
        End Sub

        Public Overrides Function ToString() As String
            Return $"{vector.AsVector.ToString} => {target.AsVector.ToString}"
        End Function
    End Class
End Namespace
