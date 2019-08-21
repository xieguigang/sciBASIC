Imports System.Runtime.CompilerServices
Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Text.Xml.Models

Namespace ComponentModel.Evaluation

    ''' <summary>
    ''' 一个包含有多维度验证结果输出的样本验证结果
    ''' </summary>
    Public Structure Validate

        <XmlAttribute> Public Property actuals As Double()
        <XmlAttribute> Public Property predicts As Double()

        Public ReadOnly Property err As Double
            Get
                Dim predicts = Me.predicts

                Return actuals _
                    .Select(Function(x, i) Math.Abs(x - predicts(i))) _
                    .Average
            End Get
        End Property

        ''' <summary>
        ''' 验证的结果的维度的数量, <see cref="actuals"/>和<see cref="predicts"/>这两个向量应该都是等长的
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property width As Integer
            Get
                If actuals.Length <> predicts.Length Then
                    Throw New DataMisalignedException
                Else
                    Return actuals.Length
                End If
            End Get
        End Property

        Public Shared Iterator Function ROC(data As IEnumerable(Of Validate),
                                            Optional threshold As Sequence = Nothing,
                                            Optional outputLabels$() = Nothing) As IEnumerable(Of NamedCollection(Of Validation))

            Dim dataArray = data.ToArray
            Dim width = dataArray(Scan0).width

            If outputLabels.IsNullOrEmpty Then
                outputLabels = width _
                    .SeqIterator _
                    .Select(Function(i) $"output_i") _
                    .ToArray
            End If

#Disable Warning
            For i As Integer = 0 To width - 1
                Yield New NamedCollection(Of Validation) With {
                    .name = outputLabels(i),
                    .value = Validation.ROC(Of Validate)(
                        entity:=dataArray,
                        getValidate:=Function(x, cutoff) x.actuals(i) >= cutoff,
                        getPredict:=Function(x, cutoff) x.predicts(i) >= cutoff,
                        threshold:=threshold Or Validation.normalRange
                    ).ToArray
                }
            Next
#Enable Warning
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function AUC(data As IEnumerable(Of Validate), Optional outputLabels$() = Nothing) As IEnumerable(Of NamedValue(Of Double))
            Return data.AUC(names:=outputLabels)
        End Function
    End Structure
End Namespace