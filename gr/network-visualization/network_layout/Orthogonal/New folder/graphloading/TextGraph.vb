

' 
'  To change this license header, choose License Headers in Project Properties.
'  To change this template file, choose Tools | Templates
'  and open the template in the editor.
' 
Namespace Orthogonal.graphloading

    ''' 
    ''' <summary>
    ''' @author santi
    ''' </summary>
    Public Class TextGraph

        Public Shared Function loadGraph(fileName As String) As Integer()()
            '			StreamReader br = new StreamReader(fileName);

            '			int n = 0;
            '			int i = 0;
            '			int[][] graph = null;
            '			while (true)
            '			{
            '				string line = br.ReadLine();
            '				if (string.ReferenceEquals(line, null))
            '				{
            '					return graph;
            '				}
            '				if (graph == null)
            '				{
            '					StringTokenizer st = new StringTokenizer(line, ", \t");
            '					while (st.hasMoreTokens())
            '					{
            '						n++;
            '						st.nextToken();
            '					}

            '					graph = RectangularArrays.RectangularIntArray(n, n);
            '				}
            '				StringTokenizer st = new StringTokenizer(line, ", \t");
            '				for (int j = 0;j < n;j++)
            '				{
            '					graph[i][j] = int.Parse(st.nextToken());
            '				}
            '				i++;
            '			}
            Throw New NotImplementedException()
        End Function
    End Class

End Namespace
