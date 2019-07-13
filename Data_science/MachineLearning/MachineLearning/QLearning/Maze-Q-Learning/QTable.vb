#Region "Microsoft.VisualBasic::3a91ac14412d89010bc4ce5abdfc7eb4, Data_science\MachineLearning\MachineLearning\QLearning\Maze-Q-Learning\QTable.vb"

    ' Author:
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 



    ' /********************************************************************************/

    ' Summaries:

    ' Class QTable
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    '     Function: __getMapString
    ' 
    '     Sub: PrintQTable
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.MachineLearning.QLearning

Public Class QTable : Inherits QTable(Of Char())

    Sub New(ar As Integer)
        Call MyBase.New(ar)
    End Sub

    ''' <summary>
    ''' printQtable is included for debugging purposes and uses the
    ''' action labels used in the maze class (even though the Qtable
    ''' is written so that it can more generic).
    ''' </summary>
    Public Sub PrintQTable()
        Dim [iterator] As IEnumerator = Table.Keys.GetEnumerator()
        Do While [iterator].MoveNext
            Dim key() As Char = CType([iterator].Current, Char())
            Dim qvalues() As Single = GetValues(key)

            Console.Write(AscW(key(0)) & "" & AscW(key(1)) & "" & AscW(key(2)))
            Console.WriteLine("  UP   RIGHT  DOWN  LEFT")
            Console.Write(AscW(key(3)) & "" & AscW(key(4)) & "" & AscW(key(5)))
            Console.WriteLine(": " & qvalues(0) & "   " & qvalues(1) & "   " & qvalues(2) & "   " & qvalues(3))
            Console.WriteLine(AscW(key(6)) & "" & AscW(key(7)) & "" & AscW(key(8)))
        Loop
    End Sub

    Protected Overrides Function __getMapString(map() As Char) As String
        Dim result As String = ""
        For x As Integer = 0 To map.Length - 1
            result &= "" & AscW(map(x))
            If x > 0 AndAlso x Mod 3 = 0 Then
                result += vbLf
            End If
        Next x
        Return result
    End Function
End Class
