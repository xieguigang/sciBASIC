#Region "Microsoft.VisualBasic::1e21ab1839f3e8e938e3801e9aaa66f8, Microsoft.VisualBasic.Core\src\Language\Language\Perl\Tools.vb"

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

    '   Total Lines: 210
    '    Code Lines: 63 (30.00%)
    ' Comment Lines: 131 (62.38%)
    '    - Xml Docs: 87.79%
    ' 
    '   Blank Lines: 16 (7.62%)
    '     File Size: 10.19 KB


    '     Module Functions
    ' 
    '         Function: chomp, Pop, system
    ' 
    '         Sub: (+3 Overloads) chomp, (+2 Overloads) Push
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports System.Array
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.ComponentModel.Collection

Namespace Language.Perl

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <remarks>
    ''' ##### last
    ''' 
    ''' The ``last`` command Is Like the ``break`` statement In C (As used In loops); **it immediately exits the Loop In question**. 
    ''' If the ``LABEL`` Is omitted, the command refers To the innermost enclosing Loop. The last EXPR form, available starting 
    ''' In Perl 5.18.0, allows a label name To be computed at run time, And Is otherwise identical To last ``LABEL``. 
    ''' 
    ''' **The Continue block, If any, Is Not executed**
    ''' (Perl里面的``last``关键词相当于vb里面的``Exit Do``或者``Exit For``)
    ''' 
    ''' 匹配：``m/&lt;regexp>/`` (还可以简写为 /&lt;regexp>/, 略去 m)
    ''' 替换：``s/&lt;pattern>/&lt;replacement>/``
    ''' 转化：``tr/&lt;pattern>/&lt;replacemnt>/``
    ''' </remarks>
    Public Module Functions

        ''' <summary>
        ''' Treats ARRAY as a stack by appending the values of LIST to the end of ARRAY. The length of ARRAY 
        ''' increases by the length of LIST. Has the same effect as
        ''' 
        ''' ```perl
        ''' for my $value (LIST) {
        '''     $ARRAY[++$#ARRAY] = $value;
        ''' }
        ''' ```
        ''' 
        ''' but Is more efficient. Returns the number of elements in the array following the completed push.
        ''' Starting with Perl 5.14, an experimental feature allowed push to take a scalar expression. 
        ''' This experiment has been deemed unsuccessful, And was removed as of Perl 5.24.
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="array"></param>
        ''' <param name="x"></param>
        ''' 
        <Extension>
        Public Sub Push(Of T)(ByRef array As T(), x As T)
            ReDim Preserve array(array.Length)
            array(array.Length - 1) = x
        End Sub

        ''' <summary>
        ''' Treats ARRAY as a stack by appending the values of LIST to the end of ARRAY. The length of ARRAY 
        ''' increases by the length of LIST. Has the same effect as
        ''' 
        ''' ```perl
        ''' for my $value (LIST) {
        '''     $ARRAY[++$#ARRAY] = $value;
        ''' }
        ''' ```
        ''' 
        ''' but Is more efficient. Returns the number of elements in the array following the completed push.
        ''' Starting with Perl 5.14, an experimental feature allowed push to take a scalar expression. 
        ''' This experiment has been deemed unsuccessful, And was removed as of Perl 5.24.
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="array"></param>
        ''' <param name="LIST"></param>
        <Extension>
        Public Sub Push(Of T)(ByRef array As T(), LIST As IEnumerable(Of T))
            Dim source As T() = LIST.ToArray
            Dim tmp As T() = New T(array.Length + source.Length - 1) {}

            Call ConstrainedCopy(array, Scan0, tmp, Scan0, array.Length)
            Call ConstrainedCopy(source, Scan0, tmp, array.Length, source.Length)

            array = tmp
        End Sub

        ''' <summary>
        ''' ##### pop ARRAY
        ''' 
        ''' **pop**
        ''' Pops And returns the last value of the array, shortening the array by one element.
        ''' 
        ''' Returns the undefined value If the array Is empty, although this may also happen at 
        ''' other times. If ARRAY Is omitted, pops the @ARGV array In the main program, but the 
        ''' @_ array In subroutines, just Like shift.
        ''' 
        ''' Starting with Perl 5.14, an experimental feature allowed pop to take a scalar expression. 
        ''' This experiment has been deemed unsuccessful, And was removed as of Perl 5.24.
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="array"></param>
        ''' <returns></returns>
        <Extension>
        Public Function Pop(Of T)(ByRef array As T()) As T
            If array.IsNullOrEmpty Then
                Return Nothing
            End If

            Dim last As T = array(array.Length - 1)
            ReDim Preserve array(array.Length - 1)
        End Function

        ''' <summary>
        ''' Does exactly the same thing as exec, except that a fork is done first and the parent process waits for the 
        ''' child process to exit. Note that argument processing varies depending on the number of arguments. If there 
        ''' is more than one argument in LIST, or if LIST is an array with more than one value, starts the program 
        ''' given by the first element of the list with arguments given by the rest of the list. If there is only one 
        ''' scalar argument, the argument is checked for shell metacharacters, and if there are any, the entire argument 
        ''' is passed to the system's command shell for parsing (this is /bin/sh -c on Unix platforms, but varies on 
        ''' other platforms). If there are no shell metacharacters in the argument, it is split into words and passed 
        ''' directly to execvp , which is more efficient. On Windows, only the system PROGRAM LIST syntax will reliably 
        ''' avoid using the shell; system LIST , even with more than one element, will fall back to the shell if the 
        ''' first spawn fails.
        ''' Perl will attempt To flush all files opened For output before any operation that may Do a fork, but this 
        ''' may Not be supported On some platforms (see perlport). To be safe, you may need To Set $ ($AUTOFLUSH In 
        ''' English) Or Call the autoflush method Of IO:Handle on any open handles.
        ''' The return value Is the exit status of the program as returned by the wait call. To get the actual exit 
        ''' value, shift right by eight (see below). See also exec. This Is Not what you want to use to capture the 
        ''' output from a command; for that you should use merely backticks Or qx//, as described in `STRING` in perlop. 
        ''' Return value of -1 indicates a failure to start the program Or an error of the wait(2) system call 
        ''' (inspect $! for the reason).
        ''' If you 'd like to make system (and many other bits of Perl) die on error, have a look at the autodie pragma.
        ''' Like exec, system allows you to lie to a program about its name if you use the system PROGRAM LIST syntax. 
        ''' Again, see exec.
        ''' </summary>
        ''' <param name="CLI"></param>
        ''' <returns></returns>
        Public Function system(CLI As String) As Integer
            Return Interaction.Shell(CLI, AppWinStyle.NormalFocus, True)
        End Function

        ''' <summary>
        ''' This safer version of chop removes any trailing string that corresponds to the current value of ``$/`` 
        ''' (also known as ``$INPUT_RECORD_SEPARATOR`` in the English module). It returns the total number of characters 
        ''' removed from all its arguments. It's often used to remove the newline from the end of an input record 
        ''' when you're worried that the final record may be missing its newline. When in paragraph mode (``$/ = ''`` ), 
        ''' it removes all trailing newlines from the string. When in slurp mode (``$/ = undef`` ) or fixed-length 
        ''' record mode (``$/`` is a reference to an integer or the like; see perlvar), chomp won't remove anything.
        ''' </summary>
        ''' <param name="s"></param>
        ''' <returns></returns>
        <Extension>
        Public Function chomp(ByRef s As String) As String
            s = s.Trim(vbCr, vbLf)
            s = s.Trim(vbTab, " "c)

            Return s
        End Function

        ''' <summary>
        ''' If you chomp a ``list`` or ``array``, each element is chomped, and the total number of characters removed is returned.
        ''' </summary>
        ''' <param name="source"></param>
        <Extension>
        Public Sub chomp(ByRef source As IEnumerable(Of String))
            Dim array As String() = source.ToArray

            For i As Integer = 0 To array.Length - 1
                array(i).chomp
            Next

            source = array
        End Sub

        ''' <summary>
        ''' If ``VARIABLE`` is a hash, it chomps the hash's values, but not its keys, resetting the each iterator 
        ''' in the process.
        ''' 
        ''' You can actually chomp anything that's an ``lvalue``, including an assignment:
        ''' 
        ''' ```perl
        ''' chomp(my $cwd = `pwd`);
        ''' chomp(my $answer = &lt;STDIN>);
        ''' ```
        ''' </summary>
        ''' <param name="hash"></param>
        <Extension>
        Public Sub chomp(ByRef hash As Dictionary(Of String, String))
            For Each key As String In hash.Keys
                hash(key) = hash(key).chomp
            Next
        End Sub

        ''' <summary>
        ''' If ``VARIABLE`` is a hash, it chomps the hash's values, but not its keys, resetting the each iterator 
        ''' in the process.
        ''' 
        ''' You can actually chomp anything that's an ``lvalue``, including an assignment:
        ''' 
        ''' ```perl
        ''' chomp(my $cwd = `pwd`);
        ''' chomp(my $answer = &lt;STDIN>);
        ''' ```
        ''' </summary>
        ''' <param name="hash"></param>
        <Extension>
        Public Sub chomp(ByRef hash As Dictionary(Of NamedValue(Of String)))
            For Each key As String In hash.Keys
                Dim x As NamedValue(Of String) = hash(key)
                hash(key) = New NamedValue(Of String) With {
                    .Name = key,
                    .Value = x.Value.chomp,
                    .Description = x.Description
                }
            Next
        End Sub
    End Module
End Namespace
