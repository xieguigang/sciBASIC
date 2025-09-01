Namespace ShapleyValue

    Public Class RandomPermutations


        Public Shared Function getRandomOld(size As Long) As IList(Of Integer)
            'long n= 13;
            'System.out.println(FactorialUtil.factorial(size));

            Dim random As Long = randf.NextDouble * FactorialUtil.factorial(size)
            'random = FactorialUtil.factorial(n) -1;
            'random = 1;
            'System.out.println("random="+random);
            Dim ints As IList(Of Integer) = New List(Of Integer)()
            Dim outputs As IList(Of Integer) = New List(Of Integer)()
            For i As Integer = 1 To size
                ints.Add(i)
            Next

            Dim temp As Integer
            Dim reste As Long = 0
            For divisor As Long = size - 1 To 2 Step -1
                'System.out.println("divisor "+divisor);
                Dim result As Long = random / FactorialUtil.factorial(divisor)
                reste = random Mod FactorialUtil.factorial(divisor)
                'System.out.println(" "+divisor+"!*"+result);
                random = reste
                temp = ints(result)
                outputs.Add(temp)
                ints.RemoveAt(temp)

            Next
            'System.out.println(" 1!*"+reste);
            temp = ints(reste)
            outputs.Add(temp)
            ints.RemoveAt(temp)
            outputs.Add(ints(0))


            Return outputs
        End Function

        Private Shared Function getRandom(min As Integer, max As Integer) As Integer
            Dim randomNum = randf.NextInteger(min, max + 1)
            Return randomNum
        End Function

        Public Shared Function getRandom(size As Long) As IList(Of Integer)

            Dim res As IList(Of Integer) = New List(Of Integer)()
            Dim temp As IList(Of Integer) = New List(Of Integer)()
            For i As Integer = 1 To size
                temp.Add(i)
            Next

            While temp.Count > 0
                Dim random = getRandom(0, temp.Count - 1)
                res.Add(temp(random))
                temp.RemoveAt(random)
            End While

            Return res
        End Function

    End Class

End Namespace
