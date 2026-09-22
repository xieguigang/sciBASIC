#Region "Microsoft.VisualBasic::0880a8b54f1632c3131ca010da4854ae, Data_science\MachineLearning\MachineLearning\ComponentModel\DataSet\Sample.vb"

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

    '   Total Lines: 324
    '    Code Lines: 201 (62.04%)
    ' Comment Lines: 75 (23.15%)
    '    - Xml Docs: 96.00%
    ' 
    '   Blank Lines: 48 (14.81%)
    '     File Size: 11.75 KB


    '     Class MLDataFrame
    ' 
    '         Properties: featureLabels, featureNames, N_attributes, N_tot, samples
    ' 
    '         Function: ToString
    ' 
    '     Class SampleData
    ' 
    '         Properties: features, id, labels
    ' 
    '         Constructor: (+6 Overloads) Sub New
    ' 
    '         Function: CheckInvalidNaN, CreateDataSet, Load, ToString, TransformDataset
    ' 
    '         Sub: Save
    ' 
    '     Class Sample
    ' 
    '         Properties: ID, label, target, vector
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
Imports System.Text
Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel.Repository
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math
Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports Microsoft.VisualBasic.Net.Http
Imports Microsoft.VisualBasic.Serialization
Imports Microsoft.VisualBasic.Serialization.BinaryDumping
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace ComponentModel.StoreProcedure

    ''' <summary>
    ''' a collection of the samples data
    ''' </summary>
    Public Class MLDataFrame

        ''' <summary>
        ''' a collection of the samples data.
        ''' </summary>
        ''' <returns>An array of the <see cref="SampleData"/> objects.</returns>
        Public Property samples As SampleData()
        ''' <summary>
        ''' the column name of the <see cref="SampleData.features"/> 
        ''' </summary>
        ''' <returns>An array of the feature column names.</returns>
        Public Property featureNames As String()
        ''' <summary>
        ''' the column name of the <see cref="SampleData.labels"/>
        ''' </summary>
        ''' <returns>An array of the label column names.</returns>
        Public Property featureLabels As String()

        ''' <summary>
        ''' The number of the feature attributes in each sample.
        ''' </summary>
        ''' <returns>The length of the <see cref="featureNames"/> list.</returns>
        Public ReadOnly Property N_attributes As Integer
            Get
                Return featureNames.Length
            End Get
        End Property

        ''' <summary>
        ''' The total number of the samples in this data frame.
        ''' </summary>
        ''' <returns>The length of the <see cref="samples"/> array.</returns>
        Public ReadOnly Property N_tot As Integer
            Get
                Return samples.Length
            End Get
        End Property

        ''' <summary>
        ''' Display the feature names and the label names of this data frame.
        ''' </summary>
        ''' <returns>A string which is combined by the two json text of the names.</returns>
        Public Overrides Function ToString() As String
            Return featureNames.GetJson & " -> " & featureLabels.GetJson
        End Function

    End Class

    ''' <summary>
    ''' the in-memory sample data object
    ''' </summary>
    Public Class SampleData : Implements INamedValue

        ''' <summary>
        ''' the unique id
        ''' </summary>
        ''' <returns>The unique reference id of this sample data.</returns>
        Public Property id As String Implements INamedValue.Key
        ''' <summary>
        ''' The sample features vector, which is the input of the machine learning model.
        ''' </summary>
        ''' <returns>An array of the feature values.</returns>
        Public Property features As Double()
        ''' <summary>
        ''' The sample label values, which is the expected output of the machine learning model.
        ''' </summary>
        ''' <returns>An array of the label values.</returns>
        Public Property labels As Double()

        ''' <summary>
        ''' Create a new empty sample data object.
        ''' </summary>
        Sub New()
        End Sub

        ''' <summary>
        ''' Create a new sample data object with a single label value.
        ''' </summary>
        ''' <param name="id">The unique reference id of this sample.</param>
        ''' <param name="features">The sample features vector.</param>
        ''' <param name="label">The single label value of this sample.</param>
        Sub New(id As String, features As Double(), label As Double)
            Me.id = id
            Me.features = features
            Me.labels = {label}
        End Sub

        ''' <summary>
        ''' make data copy from the given sample object, this constructor will assign the id, 
        ''' features and labels from the given sample data object.
        ''' </summary>
        ''' <param name="sample">The source <see cref="Sample"/> object.</param>
        Sub New(sample As Sample)
            id = sample.ID
            features = sample.vector
            labels = sample.target
        End Sub

        ''' <summary>
        ''' create the dataset for predictions, so no label data
        ''' </summary>
        ''' <param name="data">The input feature vector of this sample.</param>
        Sub New(data As Double())
            Me.features = data
        End Sub

        ''' <summary>
        ''' Create a new sample data object with a single label value, and no id is assigned.
        ''' </summary>
        ''' <param name="features">The sample features vector.</param>
        ''' <param name="label">The single label value of this sample.</param>
        Sub New(features As Double(), label As Double)
            Me.features = features
            Me.labels = {label}
        End Sub

        ''' <summary>
        ''' Create a new sample data object with multiple label values, and no id is assigned.
        ''' </summary>
        ''' <param name="features">The sample features vector.</param>
        ''' <param name="labels">The label values of this sample.</param>
        Sub New(features As Double(), labels As Double())
            Me.features = features
            Me.labels = labels
        End Sub

        ''' <summary>
        ''' Check whether an invalid ``NaN`` value is exists in the 
        ''' <see cref="features"/> or the <see cref="labels"/> vector?
        ''' </summary>
        ''' <returns>
        ''' ``True`` when a ``NaN`` value is detected, otherwise ``False``.
        ''' </returns>
        Public Function CheckInvalidNaN() As Boolean
            Return features.Any(Function(d) d.IsNaNImaginary) OrElse labels.Any(Function(d) d.IsNaNImaginary)
        End Function

        ''' <summary>
        ''' Display the sample id in string format.
        ''' </summary>
        ''' <returns>
        ''' The <see cref="id"/> value; a string in format like ``*(NaN!) id`` 
        ''' will be returned when the sample data contains an invalid ``NaN`` value.
        ''' </returns>
        Public Overrides Function ToString() As String
            If CheckInvalidNaN() Then
                Return $"*(NaN!) {id}"
            End If

            Return id
        End Function

        ''' <summary>
        ''' Create a training <see cref="DataSet"/> object from a collection of 
        ''' the <see cref="SampleData"/> objects.
        ''' </summary>
        ''' <param name="ds">A collection of the <see cref="SampleData"/> objects.</param>
        ''' <returns>
        ''' A <see cref="DataSet"/> object, in which the feature names are generated 
        ''' in format ``x%d`` and the output names are generated in format ``y%d``.
        ''' </returns>
        Public Shared Function CreateDataSet(ds As IEnumerable(Of SampleData)) As DataSet
            Dim samples As New SampleList With {
                .items = ds _
                    .Select(Function(si) New Sample(si.features, si.labels, si.id)) _
                    .ToArray
            }
            Dim featureNames As String() = samples(0).vector _
                .Select(Function(di, i) $"x{i + 1}") _
                .ToArray
            Dim norm As NormalizeMatrix = NormalizeMatrix.CreateFromSamples(samples, featureNames, estimateQuantile:=False)

            Return New DataSet With {
                .DataSamples = samples,
                .output = samples(0).target _
                    .Select(Function(di, i) $"y{i + 1}") _
                    .ToArray,
                .NormalizeMatrix = norm
            }
        End Function

        ''' <summary>
        ''' make dataset normalization
        ''' </summary>
        ''' <param name="trainset">The source sample data collection.</param>
        ''' <param name="is_generative">
        ''' Whether the label values should also be normalized? The label 
        ''' normalization is only applied when both this flag and the 
        ''' <paramref name="is_training"/> flag are enabled.
        ''' </param>
        ''' <param name="is_training">
        ''' Whether the <paramref name="trainset"/> is the training data? When it 
        ''' is ``False`` the label value will not be rescaled.
        ''' </param>
        ''' <returns>
        ''' A sequence of the <see cref="SampleData"/> objects whose feature 
        ''' values are divided by the maximum value of each feature column.
        ''' </returns>
        Public Shared Iterator Function TransformDataset(trainset As SampleData(), is_generative As Boolean, is_training As Boolean) As IEnumerable(Of SampleData)
            Dim featureMax As Double() = New Double(trainset(0).features.Length - 1) {}
            Dim labelMax As Double() = Nothing

            If is_training Then
                labelMax = New Double(trainset(0).labels.Length - 1) {}
            End If

            For i As Integer = 0 To trainset.Length - 1
                Dim d As SampleData = trainset(i)

                For j As Integer = 0 To d.features.Length - 1
                    If d.features(j) > featureMax(j) Then
                        featureMax(j) = d.features(j)
                    End If
                Next

                If is_training Then
                    For j As Integer = 0 To d.labels.Length - 1
                        If d.labels(j) > labelMax(j) Then
                            labelMax(j) = d.labels(j)
                        End If
                    Next
                End If
            Next

            For i As Integer = 0 To trainset.Length - 1
                Dim label As Double() = trainset(i).labels

                ' data is normalized by feature columns
                ' labelMax and featureMax is vector 
                If is_training AndAlso is_generative Then
                    label = SIMD.Divide.f64_op_divide_f64(label, labelMax)
                End If

                Yield New SampleData With {
                    .features = SIMD.Divide.f64_op_divide_f64(trainset(i).features, featureMax),
                    .labels = label,
                    .id = trainset(i).id
                }
            Next
        End Function

        ''' <summary>
        ''' Write the sample data collection into a binary <see cref="Stream"/>.
        ''' </summary>
        ''' <param name="data">A collection of the <see cref="SampleData"/> objects that will be written.</param>
        ''' <param name="file">The target output <see cref="Stream"/>.</param>
        ''' <remarks>
        ''' The binary layout of the output stream is: the feature vector size 
        ''' (int32), the label vector size (int32), and then each sample is 
        ''' written as its id string buffer, the features vector and the labels 
        ''' vector in network byte order.
        ''' </remarks>
        Public Shared Sub Save(data As IEnumerable(Of SampleData), file As Stream)
            Dim wr As New BinaryWriter(file)
            Dim encode As New NetworkByteOrderBuffer
            Dim feature_size As Integer = -1
            Dim label_size As Integer = -1

            For Each sample As SampleData In data
                If feature_size = -1 Then
                    feature_size = sample.features.Length
                    label_size = sample.labels.Length

                    Call wr.Write(BitConverter.GetBytes(feature_size))
                    Call wr.Write(BitConverter.GetBytes(label_size))
                End If

                Call wr.Write(New Buffer(Encoding.ASCII.GetBytes(sample.id)).Serialize)
                Call wr.Write(encode.encode(sample.features))
                Call wr.Write(encode.encode(sample.labels))
            Next

            Call wr.Flush()
        End Sub

        ''' <summary>
        ''' Read the sample data collection back from a binary <see cref="Stream"/> 
        ''' which was written by the <see cref="Save"/> method.
        ''' </summary>
        ''' <param name="file">The input <see cref="Stream"/> which contains the binary sample data.</param>
        ''' <returns>A sequence of the <see cref="SampleData"/> objects.</returns>
        Public Shared Iterator Function Load(file As Stream) As IEnumerable(Of SampleData)
            Dim rd As New BinaryReader(file)
            Dim decode As New NetworkByteOrderBuffer
            Dim feature_size As Integer = BitConverter.ToInt32(rd.ReadBytes(RawStream.INT32), Scan0)
            Dim label_size As Integer = BitConverter.ToInt32(rd.ReadBytes(RawStream.INT32), Scan0)

            Do While file.Position < file.Length
                Dim sbuf As Buffer = Buffer.Parse(rd)
                Dim id As String = Encoding.ASCII.GetString(sbuf.buffer)
                Dim features As Double() = decode.decode(rd.ReadBytes(feature_size * RawStream.DblFloat))
                Dim labels As Double() = decode.decode(rd.ReadBytes(label_size * RawStream.DblFloat))

                Yield New SampleData With {
                    .id = id,
                    .features = features,
                    .labels = labels
                }
            Loop
        End Function
    End Class

    ''' <summary>
    ''' The training dataset, a data point with known label
    ''' </summary>
    Public Class Sample : Implements INamedValue

        ''' <summary>
        ''' 可选的数据集唯一标记信息
        ''' </summary>
        ''' <returns>The optional unique reference id of this sample.</returns>
        <XmlAttribute("id")>
        Public Property ID As String Implements IKeyedEntity(Of String).Key

        ''' <summary>
        ''' Neuron network input parameters
        ''' </summary>
        ''' <returns>
        ''' The base64 encoded (and gzip compressed) text which represents the 
        ''' input feature vector.
        ''' </returns>
        ''' <remarks>
        ''' 属性值可能会很长,为了XML文件的美观,在这里使用element
        ''' 
        ''' 20200318 there is a bugs in xml serialization of the double array elements
        ''' and the numeric value is also not suitable use text as file save format
        ''' so the data for this property is save with base64 encoded of the numeric 
        ''' array.
        ''' </remarks>
        <XmlElement>
        Public Property label As String

        ''' <summary>
        ''' The network expected output values
        ''' </summary>
        ''' <returns>An array of the expected output values of this sample.</returns>
        <XmlAttribute>
        Public Property target As Double()

        ''' <summary>
        ''' sample features data
        ''' </summary>
        ''' <returns>An array of the input feature values of this sample.</returns>
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

        ''' <summary>
        ''' Create a new sample data object which only contains the input vector.
        ''' </summary>
        ''' <param name="samples">The neuron network input parameters.</param>
        Sub New(samples As IEnumerable(Of Double))
            Call Me.encodeVector(samples)
        End Sub

        Private Iterator Function decodeVector() As IEnumerable(Of Double)
            Using buffer = label.Base64RawBytes.UnGzipStream
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

                label = buffer _
                    .GZipStream _
                    .ToArray _
                    .ToBase64String
            End Using
        End Sub

        ''' <summary>
        ''' Display this training sample as a mapping expression.
        ''' </summary>
        ''' <returns>A string in format like ``input vector => output vector``.</returns>
        Public Overrides Function ToString() As String
            Return $"{vector.AsVector.ToString} => {target.AsVector.ToString}"
        End Function
    End Class
End Namespace
