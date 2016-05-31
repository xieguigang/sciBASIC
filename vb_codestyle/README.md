#Code style guidelines for Microsoft VisualBasic
------------------------------------------------
##Code Architecture of a VisualBasic CLI program

There is a VisualBasic application helper module that define in the namespace:
[Microsoft.VisualBasic.App](https://github.com/xieguigang/VisualBasic_AppFramework/blob/master/Microsoft.VisualBasic.Architecture.Framework/Extensions/App.vb)

**A special function named _main_ is the starting point of execution for all VisualBasic programs**. A VisualBasic CLI application should define the **Main** entry point in a Module which is named _Program_ and running from a Integer Function Main. By using the name of Program for the entry point module, this will makes more easily recognize of your program's entry point.

```vb.net
Module Program

    ''' <summary>
    ''' This is the main entry point of your VisualBasic application.
    ''' </summary>
    ''' <returns></returns>
    Public Function Main() As Integer
        Return GetType(CLI).RunCLI(App.CommandLine)
    End Function
End Module
```

By using a **Integer** _Function_ instead of _Sub_ in VisualBasic, this makes your code style is more standard compare with the main function from C++.

```c
	int main(int argc, char *argv[]) {
		// blablabla...
	}
```

Where, the type **CLI** is the CLI interface which it is a module that contains all of the CLI command of your application. And the extension function **RunCLI** is a CLI extension method from the VisualBasic App helper: [Microsoft.VisualBasic.App](https://github.com/xieguigang/VisualBasic_AppFramework/blob/master/Microsoft.VisualBasic.Architecture.Framework/Extensions/App.vb). The property value of **App.CommandLine** is the commandline argument of current application that user used for start this application and calling for some _CLI_ command which is exposed in **CLI** module.

###How to define the CLI module?
A **Module** is a static _Class_ type in the VisualBasic, and it usually used for _the API exportation and common method definition for a set of similarity or functional correlated utility functions_.

And then so that the CLI module in the VisualBasic can be explained as: **A module for exposed the CLI interface API to your user.**

Here is a example:

```vb.net
Partial Module CLI

    <ExportAPI("/Print", Usage:="/Print /in <inDIR> [/ext <ext> /out <out.Csv>]")>
    Public Function Print(args As CommandLine.CommandLine) As Integer
        Dim ext As String = args.GetValue("/ext", "*.*")
        Dim inDIR As String = args - "/in"
        Dim out As String = args.GetValue("/out", inDIR.TrimDIR & ".contents.Csv")
        Dim files As IEnumerable(Of String) =
            ls - l - r - wildcards(ext) <= inDIR
        Dim content As NamedValue(Of String)() =
            LinqAPI.Exec(Of NamedValue(Of String)) <= From file As String
                                                      In files
                                                      Let name As String = file.BaseName
                                                      Let genome As String = file.ParentDirName
                                                      Select New NamedValue(Of String)(genome, name)
        Return content.SaveTo(out).CLICode
    End Function
End Module
```

This example code can be found at: [github](https://github.com/SMRUCC/ncbi-localblast/tree/master/Tools/CLI)

###How to expose the CLI interface API in your application?

A wrapper for parsing the commandline from your user is already been defined in namespace: [**Microsoft.VisualBasic.CommandLine**](https://github.com/xieguigang/VisualBasic_AppFramework/tree/master/Microsoft.VisualBasic.Architecture.Framework/CommandLine)

And the **CLI** interface should define as in the format of this example:

```vb.net
Imports Microsoft.VisualBasic.CommandLine
Imports Microsoft.VisualBasic.CommandLine.Reflection

<ExportAPI("/Print", Usage:="/Print /in <inDIR> [/ext <ext> /out <out.Csv>]")>
Public Function CLI_API(args As CommandLine) As Integer
```

###Using the VisualBasic CommandLine Parser
For learn how to using the CommandLine Parser, we first lean the syntax of the VisualBasic commandline arguments.
A typical commandline arguments in VisualBasic is consist of two parts:
1. _Command Name_
2. _Arguments_

Here is a simple example:

	App.exe /API1 /test /msg "Hello World!!!" /test2-enable /test3-enable

Where in this CLI, token **App.exe** is the executable file name of your application; And **/API1** token, is the **Command Name**; And then the last tokens are the parameter arguments, using the commandline in VisualBasic just like function programming in VisualBasic:

```vb.net
Module App
    Public Function API1(test As Boolean, 
    			 msg As String, 
    			 test2Enable As Boolean, 
    			 test3Enable As Boolean) As Integer
End Module
```

You call your CLI command in the console terminal is just like call a function in the VisualBasic Code:

```vb.net
    Dim code As Integer = App.API1(True, "Hello World!!!", True, True)
```

**_NOTE:_ There is no order of the VisualBasic CLI arguments**, so that all of these CLI examples are equals to each other:

    App.exe /API1 /msg "Hello World!!!" /test2-enable /test3-enable /test
    App.exe /API1 /msg "Hello World!!!" /test /test2-enable /test3-enable
    App.exe /API1 /test /test2-enable /test3-enable /msg "Hello World!!!"
    App.exe /API1 /test2-enable /test /test3-enable /msg "Hello World!!!"

Simple Example of VisualBasic CLI application(Example source code at [here](https://github.com/xieguigang/VisualBasic_AppFramework/tree/master/Example/CLI_Example)):

```vb.net
Imports Microsoft.VisualBasic.CommandLine
Imports Microsoft.VisualBasic.CommandLine.Reflection

Module Program

    Public Function Main() As Integer
        Return GetType(CLI).RunCLI(App.CommandLine)
    End Function
End Module

Module CLI

    <ExportAPI("/API1",
         Info:="Puts the brief description of this API command at here.",
         Usage:="/API1 /msg ""Puts the CLI usage syntax at here""",
         Example:="/API1 /msg ""Hello world!!!""")>
    Public Function API1(args As CommandLine) As Integer
        Call Console.WriteLine(args("/msg"))
        Return 0
    End Function
End Module
```

Here are some mostly used function in VisualBasic CLI parser
Example CLI is:

	App.exe /Test-Example /b /n 52 /xml "~/test.Xml" /num_threads 96 /path "~/tmp/test.log"

|Function|Usage|Example|
|--------|-----|-------|
|CommandLine.GetBoolean(String) As Boolean|Get a boolean flag argument from the CLI|Dim b As Boolean = args.GetBoolean("/b")|
|CommandLine.GetInt32(String) As Integer|Get a parameter value as Integer|Dim n As Integer = args.GetInt32("/n")|
|CommandLine.GetObject(Of T)(String, System.Func(Of String, T)) As T|Get a parameter string value and then apply a string parser on it for load an .NET object|Dim x As T = args.GetObject(of T)("/xml", AddressOf LoadXml)|
|CommandLine.GetValue(Of T)(String, T, System.Func(Of String, T)) As T|Get a parameter value, if the parameter is not exist, then default value will be returns, this method is usually used on optional value|Dim n As Long = args.GetValue("/num_threads", 100L)|
|CommandLine.Item(String) As String|Default readonly property for read string value of a specific parameter|Dim path As String = args("/file")|

-------------------------------------
##List(Of T) operation in VisualBasic
For enable this language syntax feature and using the list feature in this section, you should imports the namespace **Microsoft.VisualBasic** at first

```vb.net
	Dim source As IEnumerable(Of <Type>)
	Dim list As New List(of <Type>)(source)
```

For Add a new instance

```vb.net
	list += New <Type> With {
    	.Property1 = value1,
        .Property2 = value2
    }
```

For Add a sequence of new elements
```vb.net
	list += From x As T
    		In source
    		Where True = <test>
            Select New <Type> With {
            	.Property1 = <expression>,
                .Property2 = <expression>
            }
```

if want to removes a specific element in the list
```vb.net
	list -= x
```
Or batch removes elements:
```vb.net
	list -= From x As T
    		In source
            Where True = <test>
            Select x
```

Here is some example of the list **+** operator
```vb.net
    ' This add operation makes the code more easily to read and understand:
    ' This function returns a list of RfamHit element and it also merge a
    ' list of uncertainly elements into the result list at the same time
    Public Function GetDataFrame() As RfamHit() Implements IRfamHits.GetDataFrame
        Return hits.ToList(Function(x) New RfamHit(x, Me)) + From x As Hit
                                                             In Uncertain
                                                             Select New RfamHit(x, Me)
    End Function
```
And using the **+** operator for add a new object into the list, this syntax can makes the code more readable instead of the poorly readable code from by using method **List(of T).Add**:
```vb.net
    genomes += New GenomeBrief With {
        .Name = title,
        .Size = last.Size,
        .Y = h1
    }

    ' Using the + operator to instead of this poorly readable function code
    genomes.Add(New GenomeBrief With {
            .Name = title,
            .Size = last.Size,
            .Y = h1
        })
```

##VisualBasic identifer names

####1. Directory type
If possible, then all of the directory path variable can be **UPCASE**, such as:
```vb.net
	Dim DIR As String = "/home/xieguigang/Downloads"
	Dim EXPORT As String = "/usr/lib/GCModeller/"
```

####2. Module variable
+ All of the module variable should in format like **_lowerUpper** if the variable is _private_
+ But if the variable is _Public_ or _Friend_ visible, then it should in format like **UpperUpper**

Here is some example:
```vb.net
	' Private
	Dim _fileName As String
	Dim _inDIR As Directory

	' Public
	Public ReadOnly Property FileName As String
	Public ReadOnly Property InDIR As Directory
```

####3. Local varaible and function parameter
If possible, all of the local varaible within a function or sub program and the parameters of a function, should be in format **lowerUpper**


####4. Function And Type name

For **_Public_** member function, the function name is recommended in formats **UpperUpper**, but if the function is **_Private, Friend, or Protected_** visible, then your function is recommended start with two underlines, likes **\_\_lowerUpper**. The definition of the _Class, Structure_ names is in the same rule as function name.

Here is some function name examples(Example picked from [here](https://github.com/SMRUCC/GCModeller.Core/Bio.Assembly/GenomicsContext/TFDensity.vb)):
```vb.net
	' Private
	Private Function __worker(Of T As I_GeneBrief)(genome As IGenomicsContextProvider(Of T),
                                               	getTF As Func(Of Strands, T()),
                                               	getRelated As Func(Of T, T(), Integer, T()),
                                               	numTotal As Integer,
                                               	ranges As Integer) As Density()
	' Public
	Public Function DensityCis(Of T As I_GeneBrief)(
                              	genome As IGenomicsContextProvider(Of T),
                              	TF As IEnumerable(Of String),
                              	Optional ranges As Integer = 10000) As Density()
```

![](https://raw.githubusercontent.com/xieguigang/VisualBasic_AppFramework/master/vb_codestyle/FunctionNames.png)

+ Interface type name should start with a upcase character **I**, like _IEnumerable_, _IList_, etc
+ Enum type name should end with a lower case character **s**, like _MethodTypes_, _FormatStyles_

At last, for improves of the code readable, try _**Make your identifier name short enough as possible**_


![Code standard overview example](https://raw.githubusercontent.com/xieguigang/VisualBasic_AppFramework/master/vb_codestyle/codeStandard.png)

##String manipulate
######1. String.Format
For formatted a string output, then recommended used **String.Format** function or string interpolate syntax in VisualBasic language.
And by using the **String.Format** function, then format control string is recommended puts in a constant variable instead of directly used in the format function:

```vb.net
	Const OutMsg As String = "Hello world, {0}, Right?"
	' blablabla.......
	Dim msg As String = String.Format(OutMsg, name)
```

######2. String contacts
For contacts a large amount of string tokens, the **StringBuilder** is recommended used for this job, **not recommend directly using _& operator_ to contacts a large string collection due to the reason of performance issue**.
```vb.net
	' Convert the input string to a byte array and compute the hash.
	Dim data As Byte() = md5Hash.ComputeHash(input)

    ' Create a new Stringbuilder to collect the bytes
    ' and create a string.
    Dim sBuilder As New StringBuilder()

    ' Loop through each byte of the hashed data
    ' and format each one as a hexadecimal string.
    For i As Integer = 0 To data.Length - 1
    	sBuilder.Append(data(i).ToString("x2"))
    Next i

    Return sBuilder.ToString() ' Return the hexadecimal string.
```

If you just want to contact the string, then a shared method **String.Join** is recommended used.
If the string tokens will be join by a specific delimiter, then using **String.Join** instead of **StringBuilder.Append**
```vb.net
	Dim tokens As String()
	Dim sb As New StringBuilder

	For Each s As String In tokens
		Call sb.Append(s & " ")
	Next
	Call sb.Remove(sb.Length -1)
```

Or just use **String.Join**, this method is more clean and readable than **StringBuilder.Append**:
```vb.net
	Dim tokens As String()
	Dim out As String = String.Join(" ", tokens)
```

######3. String interpolate
The string interpolate syntax in VisualBasic language is recommended used for **build _SQL_ statement and _CLI_ arguments as this syntax is very easily for understand and code readable**:
```vb.net
	Dim SQL As String = $"SELECT * FROM table WHERE id='{id}' AND ppi>{ppi}"
	Dim CLI As String = $"/start /port {port} /home {PathMapper.UserHOME}"
```
So, using this syntax feature makes your code very easy for reading and understand the code meaning, right?

####Linq Expression
All of the Linq Expression is recommended execute using [**LinqAPI**](https://github.com/xieguigang/VisualBasic_AppFramework/blob/master/Microsoft.VisualBasic.Architecture.Framework/Language/Linq.vb) if the output type of the expression is a known type:

![](https://raw.githubusercontent.com/xieguigang/VisualBasic_AppFramework/master/vb_codestyle/LinqStyle.png)

####Instantiation 
For define a new object, a short format is recommended:
```vb.net
Dim x As New <Type>
```

If the type you want to create object instance can be initialize from its property, then the With keyword is recommended to used:
```vb.net
Dim MyvaCog As MyvaCOG = 
    LinqAPI.Exec(Of MyvaCOG) <= From gene As GeneDumpInfo
                                In GenomeBrief
                                Select New MyvaCOG With {
                                    .COG = gene.COG,
                                    .QueryName = gene.LocusID,
                                    .QueryLength = gene.Length
                                }
```

##Appendix

Here are tables of names that i used in my programming, and continues updated....

>1.Some common used name for common types
<table>
	<tr><td>System.Type</td><td>Recommend Name</td><td>Example</td></tr>
	<tr><td>System.Text.StringBuilder</td>
    	<td>sb</td>
        <td>

    Dim sb As New StringBuilder
</td>
    </tr>
	<tr><td>System.String</td>
    	<td>s, str, name, sId, id, x</td>
        <td>
        
    Dim s As String
    Dim str As String
    Dim name As String
    Dim sId As String
    Dim id As String
    Dim x As String

</tr>
	<tr><td>System.Integer, System.Long</td>
    	<td>i, j, n, x</td>
        <td>

    Dim i As Integer
    Dim j As Integer
    Dim n As Integer
    Dim x As Integer

</tr>
	<tr><td>System.Object</td>
    	<td>x, o, obj, value</td>
        <td>

    Dim x As Object
    Dim o As Object
    Dim obj As Object
    Dim value As Object

</td>
	</tr>
</table>

2.Name for some meaning

<table>
   <tr><td>Meaning</td><td>Recommend Name</td><td>Example</td></tr>
   <tr><td>Commandline arguments</td>
       <td>args, CLI</td>
       <td>
<pre>Dim args As CommandLine
Dim CLI As String
Dim args As String()</pre>
</td></tr>
	<tr><td>SQL query</td>
    	<td>SQL, sql, query</td>
        <td>
<pre>Dim SQL As String = "SELECT * FROM table LIMIT 1;"</pre>
</td>
    </tr>
	<tr><td>Iterator</td>
    	<td>i, j, k, l</td>
        <td>
<pre>For i As Integer = 0 to [stop]
    For j As Integer = i to [stop]
        For k As Integer = j to [stop]
        Next
    Next
Next

Dim l As Integer = 100

Do while l = 100
    ' blablabla...
Loop</pre>
</td>
    </tr>
    <tr><td>Linq query expression</td><td>LQuery</td><td>
<pre>Dim LQuery = From path As String
             In ls -l -r -wildcards("*.Xml") <= DIR
             Where InStr(path.BaseName, "xcb") = 1
             Select path.LoadXml(Of KEGG.DBGET.Module)</pre>
</td>
</tr>
    <tr>
    	<td>Query Result/Function Returns</td><td>result, rtvl</td><td>
<pre>Dim result As [Module] = 
    LinqAPI.Exec(Of [Module]) <= 
          From path As String
          In ls -l -r -wildcards("*.Xml") <= DIR
          Where InStr(path.BaseName, "xcb") = 1
          Select path.LoadXml(Of KEGG.DBGET.Module)
        
Return result</pre>
</td>
</tr>
    <tr><td>Directory</td><td>DIR, out, inDIR</td><td>
<pre>Dim files As IEnumerable(Of String) = 
    ls -l -r -wildcards("*.Xml") <= DIR</pre>
</td>
</tr>
    <tr><td>json</td><td>json, JSON</td><td>
<pre>Dim JSON As String = (ls -l -r -wildcards("*.json") <= DIR).GetJson</pre>
</td>
</tr>
    <tr><td>data frame</td><td>df, ds, data</td><td>
<pre>Dim df As DataFrame = DataFrame.CreateObject(path)</pre>
</td>
</tr>
    <tr><td>Network objects</td><td>net, network, node, nodes, edge, edges</td><td>
<pre>Dim net As Network = Network.Load(DIR)

net += New Node With {
    .Identifier = "Linq"
}
net += New Edge With {
    .FromNode = "Linq",
    .ToNode = "SQL"
}
Return net >> Open("./test.net/")
</pre>
</td>
</tr>
</table>
