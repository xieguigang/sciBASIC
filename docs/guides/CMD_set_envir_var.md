# SET

Display, set, or remove CMD environment variables. Changes made with SET will remain only for the duration of the current CMD session.

###### Syntax

```cmd
SET variable
SET variable=string
SET /A "variable=expression"
SET "variable="
SET /P variable=[promptString]
SET "
```

###### Key

```
variable    : A new or existing environment variable name e.g. _num
string      : A text string to assign to the variable.
expression  : Arithmetic expression
```

Arithmetic expressions (``SET /a``)
The expression to be evaluated can include the following operators:

```
   +   Add                set /a "_num=_num+5"
   +=  Add variable       set /a "_num+=5"
   -   Subtract (or unary)set /a "_num=_num-5"
   -=  Subtract variable  set /a "_num-=5"
   *   Multiply           set /a "_num=_num*5"
   *=  Multiply variable  set /a "_num*=5"
   /   Divide             set /a "_num=_num/5"
   /=  Divide variable    set /a "_num/=5"
   %   Modulus            set /a "_num=5%%2"
   %%= Modulus            set /a "_num%%=5" 
   !   Logical negation  0 (FALSE) ⇨ 1 (TRUE) and any non-zero value (TRUE) ⇨ 0 (FALSE)
   ~   One's complement (bitwise negation) 
   &   AND                set /a "_num=5&3"    0101 AND 0011 = 0001 (decimal 1)
   &=  AND variable       set /a "_num&=3"
   |   OR                 set /a "_num=5|3"    0101 OR 0011 = 0111 (decimal 7)
   |=  OR variable        set /a "_num|=3"
   ^   XOR                set /a "_num=5^3"    0101 XOR 0011 = 0110 (decimal 6)
   ^=  XOR variable       set /a "_num=^3"
   <<  Left Shift.    (sign bit ⇨ 0)
   >>  Right Shift.   (Fills in the sign bit such that a negative number always remains negative.)
                       Neither ShiftRight nor ShiftLeft will detect overflow.
   <<= Left Shift variable     set /a "_num<<=2"
   >>= Right Shift variable    set /a "_num>>=2"

  ( )  Parenthesis group expressions  set /a "_num=(2+3)*5"
   ,   Commas separate expressions    set /a "_num=2,_result=_num*5"
```

If a variable name is specified as part of the expression, but is not defined in the
current environment, then ``SET /a`` will use a value of 0.

See ``SET /a`` examples below and this forum thread for more.
also see SetX, VarSearch and VarSubstring for more on variable manipulation.
Variable names are not case sensitive but the contents can be.

It is good practice to avoid using any delimiter characters (spaces, commas etc) in the variable name, for example IF DEFINED ``_variable`` will often fail if the variable name contains a delimiter character.

It is a common practice to prefix variable names with either an undescore or a dollar sign ``_variable`` or ``$variable``, these prefixes are not required but help to prevent any confusion with the standard built-in Windows Environment variables or any other other command strings.

Any extra spaces around either the variable name or the string, will not be ignored, SET is not forgiving of extra spaces like many other scripting languages.

##### Display a variable:
In most contexts, surround the variable name with %'s and the variable's value will be used 
e.g. To display the value of the _department variable with the ECHO command:

```
ECHO %_department%
```

If the variable name is not found in the current environment then SET will set %ERRORLEVEL% to 1 .
This can be detected using IF ERRORLEVEL ...

Including extra characters can be useful to show any white space:

```
ECHO [%_department%]
ECHO "%_department%"
```

Type SET without parameters to display all the current environment variables.

Type SET with a variable name to display that variable

```
SET _department
```

The SET command invoked with a string (and no equal sign) will display a wildcard list of all matching variables

Display variables that begin with 'P':

```
SET p
```

Display variables that begin with an underscore

```
SET _
```

##### Set a variable:
Example of storing a text string:
```
C:\> SET _dept=Sales and Marketing
C:\> set _
_dept=Sales and Marketing
```

Set a variable that contains a redirection character, note the position of the quotes which are not saved:
```
SET "_dept=Sales & Marketing"
```
One variable can be based on another, but this is not dynamic
E.g.
```
C:\> set xx=fish
C:\> set msg=%xx% chips
C:\> set msg
msg=fish chips

C:\> set xx=sausage
C:\> set msg
msg=fish chips

C:\> set msg=%xx% chips
C:\> set msg
msg=sausage chips
```
Avoid starting variable names with a number, this will avoid the variable being mis-interpreted as a parameter
``%123_myvar% < > %1 23_myvar``

To display undocumented system variables:
```
   SET "
```

##### Values with Spaces - using Double Quotes
There is no need to add quotation marks when assigning a value that includes spaces
```
SET _variable=one two three
```
For special characters like & you can surround the entire expression with quotation marks.
The variable contents will not include the surrounding quotes:
```
SET "_variable=one & two"
```
If you place quotation marks around the value, then those quotes will be stored:
```
SET _variable="one & two"
```

##### Variable names with spaces
A variable can contain spaces and also the variable name itself can contain spaces, therefore the following assignment:
```
SET _var =MyText
```
will create a variable called "_var " - note the trailing space

##### Prompt for user input
The /P switch allows you to set a variable equal to a line of input entered by the user. 
The Prompt string is displayed before the user input is read.
```
@echo off
Set /P _dept=Please enter Department || Set _dept=NothingChosen
If "%_dept%"=="NothingChosen" goto :sub_error
If /i "%_dept%"=="finance" goto sub_finance
If /i "%_dept%"=="hr" goto sub_hr
goto:eof

:sub_finance
echo You chose the finance dept
goto:eof

:sub_hr
echo You chose the hr dept

:sub_error
echo Nothing was chosen
```

The Prompt string can be empty. If the user does not enter anything (just presses return) then the variable will be unchanged and an errorlevel will be set.
To place the first line of a file into a variable:
```
Set /P _MyVar=<MyFilename.txt
```
The CHOICE command is an alternative to ``SET /P`` (but accepts only one character/keypress.)

##### Delete a variable
Type SET with just the variable name and an equals sign:
```
SET _department=
```
Better still, to be sure there is no trailing space after the = place the expression in parentheses or quotes:
(``SET _department=``)
  or
```
SET "_department="
```

##### Arithmetic expressions (SET /a)
Placing expressions in "quotes" is optional for simple arithmetic but required for any expression using logical operators.

Any SET /A calculation that returns a fractional result will be rounded down to the nearest whole integer.

Examples:
```
   SET /A "_result=2+4"
   (=6)

   SET /A "_result=5"
   (=5)
   SET /A "_result+=5"
   (=10)

   SET /A "_result=2<<3"
   (=16)   { 2 Lsh 3 = binary 10 Lsh 3 = binary 10000 = decimal 16 }
   
   SET /A "_result=5%%2"
   (=1)    { 5/2 = 2 + 2 remainder 1 = 1 }

   SET /A "_var1=_var2=_var3=10"
   (sets 3 variables to the same value - undocumented syntax.)
```
In a batch script, the Modulus operator (%) must be doubled up to (%%).

SET /A will treat any character string in the expression as an environment variable name. This allows you to do arithmetic with environment variables without having to type any % signs to get the values. SET /A _result=5 + _MyVar

Multiple calculations can be performed in one line, by separating each calculation with commas, for example:
```
_year=1999
Set /a _century=_year/100, _next=_century+1
```
The numbers must all be within the range of 32 bit signed integer numbers (-2,147,483,648 through 2,147,483,647) to handle larger numbers use PowerShell or VBScript.

##### Leading Zero will specify Octal
Numeric values are decimal numbers, unless prefixed by 
0x for hexadecimal numbers,
0 for octal numbers.

So ``0x10 = 020 = 16`` decimal

The octal notation can be confusing - all numeric values that start with zeros are treated as octal but 08 and 09 are not valid octal digits.
For example SET /a _month=07 will return the value 7, but SET /a _month=09 will return an error.

##### Permanent changes
Changes made using the SET command are NOT permanent, they apply to the current CMD prompt only and remain only until the CMD window is closed.
To permanently change a variable at the command line use SetX
or with the GUI - Control Panel | System | Environment | System/User Variables

Changing a variable permanently with SetX will not affect any CMD prompt that is already open. 
Only new CMD prompts will get the new setting.

You can of course use SetX in conjunction with SET to change both at the same time:
```
Set _Library=T:\Library\ 
SetX _Library T:\Library\ /m
```

##### Change the environment for other sessions
Neither SET nor SetX will affect other CMD sessions that are already running on the machine . This as a good thing, particularly on multi-user machines, your scripts won't have to contend with a dynamically changing environment while they are running.

It is possible to add permanent environment variables to the registry (``HKCU\Environment``), but this is an undocumented (and likely unsupported) technique and still it will not take effect until the users next login.

System environment variables can be found in the registry here:
```
HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\Environment
```
##### CALL SET
The CALL SET syntax allows a variable substring to be evaluated, the CALL page has more detail on this technique, in most cases a better approach is to use Setlocal EnableDelayedExpansion

##### Autoexec.bat
Any SET statement in c:\autoexec.bat will be parsed at boot time
Variables set in this way are not available to 32 bit gui programs - they won't appear in the control panel.
They will appear at the CMD prompt.

If autoexec.bat CALLS any secondary batch files, the additional batch files will NOT be parsed at boot.
This behaviour can be useful on a dual boot PC.

##### Errorlevels
When CMD Command Extensions are enabled (the default)

If the variable was successfully changed %ERRORLEVEL% = 0
No variable found/invalid name = 1
SET /A Unbalanced parentheses = 1073750988
SET /A Missing operand = 1073750989
SET /A Syntax error = 1073750990
SET /A Invalid number = 1073750991
SET /A Number larger than 32-bits = 1073750992
SET /A Division by zero = 1073750993

SET is an internal command. If Command Extensions are disabled all SET commands are disabled other than simple assignments like: ``_variable=MyText``

The CMD shell will fail to read an environment variable if it contains more than 8,191 characters.
```bash
# I got my mind set on you
# I got my mind set on you... - Rudy Clark (James Ray/George Harrison)
```