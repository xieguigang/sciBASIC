Imports rand = Microsoft.VisualBasic.Math.RandomExtensions

Namespace Math

    ''' <summary>
    ''' This class provides a method to randomize the contents of an array
    ''' @author Salindor. Modified by O.Gonzalez-Recio in January 2010, to make 'temp' to be a list, instead a vector
    ''' 
    ''' </summary>
    Public NotInheritable Class Permutator

        Private Sub New()
        End Sub

        ''' <summary>
        ''' This function does all the work.  It randomizes the array into the new
        ''' array it returns.  Granted if you were using this for real, you would
        ''' most likely want to use something other than the default java randomizer.
        ''' or you would at least want to seed it properly </summary>
        ''' <param name="a"> the original array </param>
        ''' <returns> the new shuffled array </returns>
        Public Shared Function Permute(a As Integer()) As Integer()
            'temp object we are going to use to return
            Dim ret = New Integer(a.Length - 1) {}

            'going to use a vector because they have element remove pre-implmented which
            'makes it easy for us
            'int temp[] = new int[a.length];
            Dim temp As List(Of Integer) = New List(Of Integer)()

            'copy the contents of the array into the vector, 
            For index = 0 To a.Length - 1
                temp.Insert(index, a(index))
            Next

            'now that all the prework is done, here is the beautiful part
            Dim i = 0 'index we are writting to
            While i < ret.Length
                Dim v As Integer = rand.NextDouble * temp.Count 'generate a random number from 0- (size-1)
                If v = temp.Count Then
                    Continue While 'just in case, paranoid
                End If
                ret(i) = temp(v)
                temp.RemoveAt(v) 'uncomment for sampling w/o replacement
                'ret[i] = temp.get(v);                  //uncomment for sampling w replacement
                '               System.out.println(ret[i]);
                i += 1
            End While

            Return ret
        End Function
    End Class
End Namespace