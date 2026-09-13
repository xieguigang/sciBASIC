#Region "Microsoft.VisualBasic::17c43a61ffc7404600b1be600763ab9c, Microsoft.VisualBasic.Core\src\Extensions\Math\Random\Permutator.vb"

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

    '   Total Lines: 53
    '    Code Lines: 25 (47.17%)
    ' Comment Lines: 20 (37.74%)
    '    - Xml Docs: 55.00%
    ' 
    '   Blank Lines: 8 (15.09%)
    '     File Size: 2.25 KB


    '     Class Permutator
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: Permute
    ' 
    ' 
    ' /********************************************************************************/

#End Region

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
