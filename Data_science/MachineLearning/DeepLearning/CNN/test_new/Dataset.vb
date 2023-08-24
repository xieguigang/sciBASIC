Imports System.IO
Imports System.Text
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Language.Java

Namespace CNN.Dataset

    Public Class Dataset

        Private records As IList(Of Record)

        Friend m_lableIndex As Integer = -1
        Friend maxLable As Double = -1

        Public Sub New(classIndex As Integer)
            m_lableIndex = classIndex
            records = New List(Of Record)()
        End Sub

        Public Sub New(datas As IList(Of Double()))
            Me.New()
            For Each data As Double() In datas
                append(New Record(Me, data))
            Next
        End Sub

        Private Sub New()
            m_lableIndex = -1
            records = New List(Of Record)()
        End Sub

        Public Overridable Function size() As Integer
            Return records.Count
        End Function

        Public Overridable ReadOnly Property LableIndex As Integer
            Get
                Return m_lableIndex
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

        Dim m_attrs As Double()
        Dim m_lable As Double?
        Dim m_lableIndex As Integer = -1

        Public Overridable ReadOnly Property Attrs As Double()
            Get
                Return m_attrs
            End Get
        End Property

        Public Overridable ReadOnly Property Lable As Double?
            Get
                If m_lableIndex = -1 Then
                    Return Nothing
                End If
                Return m_lable
            End Get
        End Property

        Friend Sub New(attrs As Double(), lable As Double?)
            m_attrs = attrs
            m_lable = lable
        End Sub

        Public Sub New(ds As Dataset, data As Double())
            If ds.m_lableIndex = -1 Then
                m_attrs = data
            Else
                m_lable = data(ds.m_lableIndex)
                m_lableIndex = ds.m_lableIndex

                If m_lable.Value > ds.maxLable Then
                    ds.maxLable = m_lable.Value
                End If
                If ds.m_lableIndex = 0 Then
                    m_attrs = Arrays.copyOfRange(data, 1, data.Length)
                Else
                    m_attrs = Arrays.copyOfRange(data, 0, data.Length - 1)
                End If
            End If
        End Sub

        Public Overrides Function ToString() As String
            Dim sb As StringBuilder = New StringBuilder()
            sb.Append("attrs:")
            sb.Append(String.Join(", ", m_attrs))
            sb.Append("lable:")
            sb.Append(m_lable)
            Return sb.ToString()
        End Function
    End Class
End Namespace
