Imports System.IO
Imports System.Text
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Language.Java

Namespace CNN.Dataset

    Public Class Dataset

        Private records As IList(Of Record)
        Friend lableIndexField As Integer
        Friend maxLable As Double = -1

        Public Sub New(classIndex As Integer)

            lableIndexField = classIndex
            records = New List(Of Record)()
        End Sub

        Public Sub New(datas As IList(Of Double()))
            Me.New()
            For Each data As Double() In datas
                append(New Record(Me, data))
            Next
        End Sub

        Private Sub New()
            lableIndexField = -1
            records = New List(Of Record)()
        End Sub

        Public Overridable Function size() As Integer
            Return records.Count
        End Function

        Public Overridable ReadOnly Property LableIndex As Integer
            Get
                Return lableIndexField
            End Get
        End Property

        Public Overridable Sub append(record As Record)
            records.Add(record)
        End Sub

        Public Overridable Sub clear()
            records.Clear()
        End Sub

        Public Overridable Sub append(attrs As Double(), lable As Double?)
            records.Add(New Record(attrs, lable))
        End Sub

        Public Overridable Function iter() As IEnumerator(Of Record)
            Return records.GetEnumerator()
        End Function

        Public Overridable Function getAttrs(index As Integer) As Double()
            Return records(index).Attrs
        End Function

        Public Overridable Function getLable(index As Integer) As Double?
            Return records(index).Lable
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
                    Dim record As Record = New Record(dataset, data)
                    dataset.append(record)
                End While
                [in].Close()
            Catch e As IOException
                Console.WriteLine(e.ToString())
                Console.Write(e.StackTrace)
                Return Nothing
            End Try
            Console.WriteLine("��������:" & dataset.size().ToString())
            Return dataset
        End Function

        Public Overridable Function getRecord(index As Integer) As Record
            Return records(index)
        End Function

    End Class


    Public Class Record

        Friend attrsField As Double()
        Friend lableField As Double?
        Friend lableIndexField As Integer = -1

        Friend Sub New(attrs As Double(), lable As Double?)
            attrsField = attrs
            lableField = lable
        End Sub

        Public Sub New(outerInstance As Dataset, data As Double())
            If outerInstance.lableIndexField = -1 Then
                attrsField = data
            Else
                lableField = data(outerInstance.lableIndexField)
                lableIndexField = outerInstance.lableIndexField

                If lableField.Value > outerInstance.maxLable Then
                    outerInstance.maxLable = lableField.Value
                End If
                If outerInstance.lableIndexField = 0 Then
                    attrsField = Arrays.copyOfRange(data, 1, data.Length)
                Else
                    attrsField = Arrays.copyOfRange(data, 0, data.Length - 1)
                End If
            End If
        End Sub

        Public Overridable ReadOnly Property Attrs As Double()
            Get
                Return attrsField
            End Get
        End Property

        Public Overrides Function ToString() As String
            Dim sb As StringBuilder = New StringBuilder()
            sb.Append("attrs:")
            sb.Append(String.Join(", ", attrsField))
            sb.Append("lable:")
            sb.Append(lableField)
            Return sb.ToString()
        End Function

        Public Overridable ReadOnly Property Lable As Double?
            Get
                If lableIndexField = -1 Then
                    Return Nothing
                End If
                Return lableField
            End Get
        End Property

    End Class


End Namespace
