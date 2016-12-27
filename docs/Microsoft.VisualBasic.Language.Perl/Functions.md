# Functions
_namespace: [Microsoft.VisualBasic.Language.Perl](./index.md)_



> 
>  ##### last
>  
>  The ``last`` command Is Like the ``break`` statement In C (As used In loops); **it immediately exits the Loop In question**. 
>  If the ``LABEL`` Is omitted, the command refers To the innermost enclosing Loop. The last EXPR form, available starting 
>  In Perl 5.18.0, allows a label name To be computed at run time, And Is otherwise identical To last ``LABEL``. 
>  
>  **The Continue block, If any, Is Not executed**
>  (Perl里面的``last``关键词相当于vb里面的``Exit Do``或者``Exit For``)
>  
>  匹配：``m/<regexp>/`` (还可以简写为 /<regexp>/, 略去 m)
>  替换：``s/<pattern>/<replacement>/``
>  转化：``tr/<pattern>/<replacemnt>/``
>  


### Methods

#### chomp
```csharp
Microsoft.VisualBasic.Language.Perl.Functions.chomp(Microsoft.VisualBasic.Dictionary{Microsoft.VisualBasic.ComponentModel.DataSourceModel.NamedValue{System.String}}@)
```
If ``VARIABLE`` is a hash, it chomps the hash's values, but not its keys, resetting the each iterator 
 in the process.
 
 You can actually chomp anything that's an ``lvalue``, including an assignment:
 
 ```perl
 chomp(my $cwd = `pwd`);
 chomp(my $answer = <STDIN>);
 ```

|Parameter Name|Remarks|
|--------------|-------|
|hash|-|


#### Pop``1
```csharp
Microsoft.VisualBasic.Language.Perl.Functions.Pop``1(``0[]@)
```
##### pop ARRAY
 
 **pop**
 Pops And returns the last value of the array, shortening the array by one element.
 
 Returns the undefined value If the array Is empty, although this may also happen at 
 other times. If ARRAY Is omitted, pops the @ARGV array In the main program, but the 
 @_ array In subroutines, just Like shift.
 
 Starting with Perl 5.14, an experimental feature allowed pop to take a scalar expression. 
 This experiment has been deemed unsuccessful, And was removed as of Perl 5.24.

|Parameter Name|Remarks|
|--------------|-------|
|array|-|


#### Push``1
```csharp
Microsoft.VisualBasic.Language.Perl.Functions.Push``1(``0[]@,System.Collections.Generic.IEnumerable{``0})
```
Treats ARRAY as a stack by appending the values of LIST to the end of ARRAY. The length of ARRAY 
 increases by the length of LIST. Has the same effect as
 
 ```perl
 for my $value (LIST) {
 $ARRAY[++$#ARRAY] = $value;
 }
 ```
 
 but Is more efficient. Returns the number of elements in the array following the completed push.
 Starting with Perl 5.14, an experimental feature allowed push to take a scalar expression. 
 This experiment has been deemed unsuccessful, And was removed as of Perl 5.24.

|Parameter Name|Remarks|
|--------------|-------|
|array|-|
|LIST|-|


#### system
```csharp
Microsoft.VisualBasic.Language.Perl.Functions.system(System.String)
```
Does exactly the same thing as exec, except that a fork is done first and the parent process waits for the 
 child process to exit. Note that argument processing varies depending on the number of arguments. If there 
 is more than one argument in LIST, or if LIST is an array with more than one value, starts the program 
 given by the first element of the list with arguments given by the rest of the list. If there is only one 
 scalar argument, the argument is checked for shell metacharacters, and if there are any, the entire argument 
 is passed to the system's command shell for parsing (this is /bin/sh -c on Unix platforms, but varies on 
 other platforms). If there are no shell metacharacters in the argument, it is split into words and passed 
 directly to execvp , which is more efficient. On Windows, only the system PROGRAM LIST syntax will reliably 
 avoid using the shell; system LIST , even with more than one element, will fall back to the shell if the 
 first spawn fails.
 Perl will attempt To flush all files opened For output before any operation that may Do a fork, but this 
 may Not be supported On some platforms (see perlport). To be safe, you may need To Set $ ($AUTOFLUSH In 
 English) Or Call the autoflush method Of IO:Handle on any open handles.
 The return value Is the exit status of the program as returned by the wait call. To get the actual exit 
 value, shift right by eight (see below). See also exec. This Is Not what you want to use to capture the 
 output from a command; for that you should use merely backticks Or qx//, as described in `STRING` in perlop. 
 Return value of -1 indicates a failure to start the program Or an error of the wait(2) system call 
 (inspect $! for the reason).
 If you 'd like to make system (and many other bits of Perl) die on error, have a look at the autodie pragma.
 Like exec, system allows you to lie to a program about its name if you use the system PROGRAM LIST syntax. 
 Again, see exec.

|Parameter Name|Remarks|
|--------------|-------|
|CLI|-|



