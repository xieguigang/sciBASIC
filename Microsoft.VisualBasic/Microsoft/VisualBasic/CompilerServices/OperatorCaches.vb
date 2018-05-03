Imports System.Runtime.InteropServices
Imports Microsoft.VisualBasic.CompilerServices.ConversionResolution

Namespace Microsoft.VisualBasic.CompilerServices
    Friend Class OperatorCaches
        ' Methods
        Private Sub New()
        End Sub


        ' Fields
        Friend Shared ReadOnly ConversionCache As FixedList = New FixedList
        Friend Shared ReadOnly UnconvertibleTypeCache As FixedExistanceList = New FixedExistanceList

        ' Nested Types
        Friend NotInheritable Class FixedExistanceList
            ' Methods
            Friend Sub New()
                Me.New(50)
            End Sub

            Friend Sub New(Size As Integer)
                Me.m_Size = Size
                Me.m_List = New Entry(((Me.m_Size - 1) + 1) - 1) {}
                Dim num As Integer = (Me.m_Size - 2)
                Dim i As Integer = 0
                Do While (i <= num)
                    Me.m_List(i).Next = (i + 1)
                    i += 1
                Loop
                Dim j As Integer = (Me.m_Size - 1)
                Do While (j >= 1)
                    Me.m_List(j).Previous = (j - 1)
                    j = (j + -1)
                Loop
                Me.m_List(0).Previous = (Me.m_Size - 1)
                Me.m_Last = (Me.m_Size - 1)
            End Sub

            Friend Sub Insert(Type As Type)
                If (Me.m_Count < Me.m_Size) Then
                    Dim numRef As Integer
                    numRef = CInt(AddressOf Me.m_Count) = (numRef + 1)
                End If
                Dim last As Integer = Me.m_Last
                Me.m_First = last
                Me.m_Last = Me.m_List(Me.m_Last).Previous
                Me.m_List(last).Type = Type
            End Sub

            Friend Function Lookup(Type As Type) As Boolean
                Dim first As Integer = Me.m_First
                Dim i As Integer
                For i = 0 To Me.m_Count - 1
                    If (Type Is Me.m_List(first).Type) Then
                        Me.MoveToFront(first)
                        Return True
                    End If
                    first = Me.m_List(first).Next
                Next i
                Return False
            End Function

            Private Sub MoveToFront(Item As Integer)
                If (Item <> Me.m_First) Then
                    Dim [next] As Integer = Me.m_List(Item).Next
                    Dim previous As Integer = Me.m_List(Item).Previous
                    Me.m_List(previous).Next = [next]
                    Me.m_List([next]).Previous = previous
                    Me.m_List(Me.m_First).Previous = Item
                    Me.m_List(Me.m_Last).Next = Item
                    Me.m_List(Item).Next = Me.m_First
                    Me.m_List(Item).Previous = Me.m_Last
                    Me.m_First = Item
                End If
            End Sub


            ' Fields
            Private Const DefaultSize As Integer = 50
            Private m_Count As Integer
            Private m_First As Integer
            Private m_Last As Integer
            Private ReadOnly m_List As Entry()
            Private ReadOnly m_Size As Integer

            ' Nested Types
            <StructLayout(LayoutKind.Sequential)>
            Private Structure Entry
                Friend Type As Type
                Friend [Next] As Integer
                Friend Previous As Integer
            End Structure
        End Class

        Friend NotInheritable Class FixedList
            ' Methods
            Friend Sub New()
                Me.New(50)
            End Sub

            Friend Sub New(Size As Integer)
                Me.m_Size = Size
                Me.m_List = New Entry(((Me.m_Size - 1) + 1) - 1) {}
                Dim num As Integer = (Me.m_Size - 2)
                Dim i As Integer = 0
                Do While (i <= num)
                    Me.m_List(i).Next = (i + 1)
                    i += 1
                Loop
                Dim j As Integer = (Me.m_Size - 1)
                Do While (j >= 1)
                    Me.m_List(j).Previous = (j - 1)
                    j = (j + -1)
                Loop
                Me.m_List(0).Previous = (Me.m_Size - 1)
                Me.m_Last = (Me.m_Size - 1)
            End Sub

            Friend Sub Insert(TargetType As Type, SourceType As Type, Classification As ConversionClass, OperatorMethod As Method)
                If (Me.m_Count < Me.m_Size) Then
                    Dim numRef As Integer
                    numRef = CInt(AddressOf Me.m_Count) = (numRef + 1)
                End If
                Dim last As Integer = Me.m_Last
                Me.m_First = last
                Me.m_Last = Me.m_List(Me.m_Last).Previous
                Me.m_List(last).TargetType = TargetType
                Me.m_List(last).SourceType = SourceType
                Me.m_List(last).Classification = Classification
                Me.m_List(last).OperatorMethod = OperatorMethod
            End Sub

            Friend Function Lookup(TargetType As Type, SourceType As Type, ByRef Classification As ConversionClass, ByRef OperatorMethod As Method) As Boolean
                Dim first As Integer = Me.m_First
                Dim i As Integer
                For i = 0 To Me.m_Count - 1
                    If ((TargetType Is Me.m_List(first).TargetType) AndAlso (SourceType Is Me.m_List(first).SourceType)) Then
                        Classification = Me.m_List(first).Classification
                        OperatorMethod = Me.m_List(first).OperatorMethod
                        Me.MoveToFront(first)
                        Return True
                    End If
                    first = Me.m_List(first).Next
                Next i
                Classification = ConversionClass.Bad
                OperatorMethod = Nothing
                Return False
            End Function

            Private Sub MoveToFront(Item As Integer)
                If (Item <> Me.m_First) Then
                    Dim [next] As Integer = Me.m_List(Item).Next
                    Dim previous As Integer = Me.m_List(Item).Previous
                    Me.m_List(previous).Next = [next]
                    Me.m_List([next]).Previous = previous
                    Me.m_List(Me.m_First).Previous = Item
                    Me.m_List(Me.m_Last).Next = Item
                    Me.m_List(Item).Next = Me.m_First
                    Me.m_List(Item).Previous = Me.m_Last
                    Me.m_First = Item
                End If
            End Sub


            ' Fields
            Private Const DefaultSize As Integer = 50
            Private m_Count As Integer
            Private m_First As Integer
            Private m_Last As Integer
            Private ReadOnly m_List As Entry()
            Private ReadOnly m_Size As Integer

            ' Nested Types
            <StructLayout(LayoutKind.Sequential)> _
            Private Structure Entry
                Friend TargetType As Type
                Friend SourceType As Type
                Friend Classification As ConversionClass
                Friend OperatorMethod As Method
                Friend [Next] As Integer
                Friend Previous As Integer
            End Structure
        End Class
    End Class
End Namespace

