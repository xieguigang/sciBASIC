Imports System.IO
Imports System.Text
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Language.Java
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.MachineLearning.ComponentModel.StoreProcedure

Namespace CNN.Dataset

    Public Class Dataset

        Private records As IList(Of SampleData)

        Friend m_lableIndex As Integer = -1
        Friend maxLable As Double = -1

        Public Sub New(classIndex As Integer)
            m_lableIndex = classIndex
            records = New List(Of SampleData)()
        End Sub

        Private Sub New()
            m_lableIndex = -1
            records = New List(Of SampleData)()
        End Sub

        Public Overridable Function size() As Integer
            Return records.Count
        End Function

        Public Overridable ReadOnly Property LableIndex As Integer
            Get
                Return m_lableIndex
            End Get
        End Property

        Public Overridable Sub append(record As SampleData)
            records.Add(record)
        End Sub

        Public Overridable Sub clear()
            records.Clear()
        End Sub

        Public Overridable Sub append(attrs As Double(), lable As Double?)
            records.Add(New SampleData(attrs, lable))
        End Sub

        Public Overridable Function iter() As IEnumerator(Of SampleData)
            Return records.GetEnumerator()
        End Function

        Public Overridable Function getAttrs(index As Integer) As Double()
            Return records(index).features
        End Function

        Public Overridable Function getLable(index As Integer) As Double?
            Return records(index).labels(0)
        End Function

        Public Shared Function load(filePath As String, tag As String, lableIndex As Integer) As Dataset
            Dim dataset As New Dataset(lableIndex)
            Dim file As Stream = IO.File.OpenRead(filePath)
            Dim sep = tag.ToArray
            Try

                Dim [in] As StreamReader = New StreamReader(file)
                Dim line As Value(Of String) = ""
                While Not (line = [in].ReadLine()) Is Nothing
                    Dim datas = line.Value.Split(sep)
                    If datas.Length = 0 Then
                        Continue While
                    End If
                    Dim data = New Double(datas.Length - 1) {}
                    For i = 0 To datas.Length - 1
                        data(i) = Double.Parse(datas(i))
                    Next

                    If lableIndex < 0 Then
                        dataset.append(New SampleData(data, -1))
                    Else
                        Dim label As Double = data(lableIndex)
                        data = data.Take(lableIndex).JoinIterates(data.Skip(lableIndex)).ToArray
                        dataset.append(New SampleData(data, label))
                    End If
                End While
                [in].Close()
            Catch e As IOException
                Console.WriteLine(e.ToString())
                Console.Write(e.StackTrace)
                Return Nothing
            End Try
            Console.WriteLine("get data set with data records:" & dataset.size().ToString())
            Return dataset
        End Function

        Public Overridable Function getRecord(index As Integer) As SampleData
            Return records(index)
        End Function

    End Class
End Namespace
