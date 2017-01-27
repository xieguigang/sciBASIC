# Extensions
_namespace: [Microsoft.VisualBasic](./index.md)_

Common extension methods library for convenient the programming job.



### Methods

#### Add``1
```csharp
Microsoft.VisualBasic.Extensions.Add``1(Microsoft.VisualBasic.Language.List{``0}@,``0[])
```
Adds the elements of the specified collection to the end of the List`1.
 (会自动跳过空集合，这个方法是安全的)

|Parameter Name|Remarks|
|--------------|-------|
|list|-|
|value|The collection whose elements should be added to the end of the List`1.|


#### AddHandle``1
```csharp
Microsoft.VisualBasic.Extensions.AddHandle``1(System.Collections.Generic.IEnumerable{``0}@,System.Int32)
```
为列表中的对象添加对象句柄值

|Parameter Name|Remarks|
|--------------|-------|
|source|-|


#### AddRange``1
```csharp
Microsoft.VisualBasic.Extensions.AddRange``1(System.Collections.Generic.LinkedList{``0},System.Collections.Generic.IEnumerable{``0})
```
Add a linked list of a collection of specific type of data.

|Parameter Name|Remarks|
|--------------|-------|
|list|-|
|data|-|


#### Append``1
```csharp
Microsoft.VisualBasic.Extensions.Append``1(``0[],System.Collections.Generic.IEnumerable{``0})
```
Add given elements into an array object and then returns the target array object **`buffer`**.

|Parameter Name|Remarks|
|--------------|-------|
|buffer|-|
|value|-|


#### CheckDuplicated``2
```csharp
Microsoft.VisualBasic.Extensions.CheckDuplicated``2(System.Collections.Generic.IEnumerable{``0},System.Func{``0,``1})
```
函数只返回有重复的数据

|Parameter Name|Remarks|
|--------------|-------|
|source|-|
|getKey|-|


#### CLIPath
```csharp
Microsoft.VisualBasic.Extensions.CLIPath(System.String)
```
If the path string value is already wrappered by quot, then this function will returns the original string (DO_NOTHING).
 (假若命令行之中的文件名参数之中含有空格的话，则可能会造成错误，需要添加一个双引号来消除歧义)

|Parameter Name|Remarks|
|--------------|-------|
|Path|-|


#### CLIToken
```csharp
Microsoft.VisualBasic.Extensions.CLIToken(System.String)
```
@``M:Microsoft.VisualBasic.Extensions.CLIPath(System.String)``函数为了保持对Linux系统的兼容性会自动替换\为/符号，这个函数则不会执行这个替换

|Parameter Name|Remarks|
|--------------|-------|
|Token|-|


#### Constrain``2
```csharp
Microsoft.VisualBasic.Extensions.Constrain``2(System.Collections.Generic.IEnumerable{``1})
```
Constrain the inherits class type into the base type.
 (基类集合与继承类的集合约束)

#### CopyTo``1
```csharp
Microsoft.VisualBasic.Extensions.CopyTo``1(``0,``0@)
```
Copy the value in **`value`** into target variable **`target`** and then return the target variable.

|Parameter Name|Remarks|
|--------------|-------|
|value|-|
|target|-|


#### CopyTypeDef``1
```csharp
Microsoft.VisualBasic.Extensions.CopyTypeDef``1(Microsoft.VisualBasic.Language.List{``0})
```


|Parameter Name|Remarks|
|--------------|-------|
|IList|仅仅是起到类型复制的作用|


#### CopyTypeDef``2
```csharp
Microsoft.VisualBasic.Extensions.CopyTypeDef``2(System.Collections.Generic.Dictionary{``0,``1})
```


|Parameter Name|Remarks|
|--------------|-------|
|source|仅仅是起到类型复制的作用|


#### DateToString
```csharp
Microsoft.VisualBasic.Extensions.DateToString(System.DateTime)
```
Format the datetime value in the format of yy/mm/dd hh:min

|Parameter Name|Remarks|
|--------------|-------|
|dat|-|


#### Description
```csharp
Microsoft.VisualBasic.Extensions.Description(System.Enum)
```
Get the description data from a enum type value, if the target have no @``T:System.ComponentModel.DescriptionAttribute`` attribute data
 then function will return the string value from the ToString() function.

|Parameter Name|Remarks|
|--------------|-------|
|value|-|


#### DriverRun
```csharp
Microsoft.VisualBasic.Extensions.DriverRun(Microsoft.VisualBasic.ComponentModel.DataSourceModel.IObjectModel_Driver)
```
Run the driver in a new thread, NOTE: from this extension function calls, then run thread is already be started, 
 so that no needs of calling the method @``M:System.Threading.Thread.Start`` again.
 (使用线程的方式启动，在函数调用之后，线程是已经启动了的，所以不需要再次调用@``M:System.Threading.Thread.Start``方法了)

|Parameter Name|Remarks|
|--------------|-------|
|driver|The object which is implements the interface @``T:Microsoft.VisualBasic.ComponentModel.DataSourceModel.IObjectModel_Driver``|


#### Enums``1
```csharp
Microsoft.VisualBasic.Extensions.Enums``1
```
Enumerate all of the enum values in the specific @``T:System.Enum`` type data.(只允许枚举类型，其他的都返回空集合)

#### FillBlank
```csharp
Microsoft.VisualBasic.Extensions.FillBlank(System.Drawing.Image@,System.Drawing.Brush)
```
Fill the newly created image data with the specific color brush

|Parameter Name|Remarks|
|--------------|-------|
|Image|-|
|FilledColor|-|


#### FirstNotEmpty
```csharp
Microsoft.VisualBasic.Extensions.FirstNotEmpty(System.String[])
```
Returns the first not null or empty string.

|Parameter Name|Remarks|
|--------------|-------|
|args|-|


#### FlushMemory
```csharp
Microsoft.VisualBasic.Extensions.FlushMemory
```
Rabbish collection to free the junk memory.(垃圾回收)

#### FormatTime
```csharp
Microsoft.VisualBasic.Extensions.FormatTime(System.TimeSpan)
```
``days, hh:mm:ss.ms``

|Parameter Name|Remarks|
|--------------|-------|
|t|-|


#### Free``1
```csharp
Microsoft.VisualBasic.Extensions.Free``1(``0@)
```
Free this variable pointer in the memory.(销毁本对象类型在内存之中的指针)

|Parameter Name|Remarks|
|--------------|-------|
|obj|-|


#### FuzzyMatching
```csharp
Microsoft.VisualBasic.Extensions.FuzzyMatching(System.String,System.String,System.Boolean,System.Double)
```
Fuzzy match two string, this is useful for the text query or searching.

|Parameter Name|Remarks|
|--------------|-------|
|Query|-|
|Subject|-|


#### Get``1
```csharp
Microsoft.VisualBasic.Extensions.Get``1(``0[],System.Int32,``0)
```
This is a safely method for gets the value in a array, if the index was outside of the boundary, then the default value will be return.
 (假若下标越界的话会返回默认值)

|Parameter Name|Remarks|
|--------------|-------|
|array|-|
|index|-|
|[default]|Default value for return when the array object is nothing or index outside of the boundary.|


#### GetAnonymousTypeList``1
```csharp
Microsoft.VisualBasic.Extensions.GetAnonymousTypeList``1(System.Collections.Generic.IEnumerable{``0})
```
You can using this method to create a empty list for the specific type of anonymous type object.
 (使用这个方法获取得到匿名类型的列表数据集合对象)

|Parameter Name|Remarks|
|--------------|-------|
|typedef|The temp object which was created anonymous.
 (匿名对象的集合，这个是用来复制匿名类型的，虽然没有引用这个参数，但是却可以直接通过拓展来得到匿名类型生成列表对象)
 |


#### getBoolean
```csharp
Microsoft.VisualBasic.Extensions.getBoolean(System.String)
```
Convert the string value into the boolean value, this is useful to the text format configuration file into data model.
 (请注意，空值字符串为False)

|Parameter Name|Remarks|
|--------------|-------|
|str|-|


#### GetById``1
```csharp
Microsoft.VisualBasic.Extensions.GetById``1(System.Collections.Generic.IEnumerable{``0},System.String,System.StringComparison)
```
Get a specific item value from the target collction data using its UniqueID property，
 (请注意，请尽量不要使用本方法，因为这个方法的效率有些低，对于获取@``T:Microsoft.VisualBasic.ComponentModel.Collection.Generic.INamedValue``[
 ]类型的集合之中的某一个对象，请尽量先转换为字典对象，在使用该字典对象进行查找以提高代码效率，使用本方法的优点是可以选择忽略**`uid``[
 **参数之中的大小写，以及对集合之中的存在相同的Key的这种情况的容忍)

|Parameter Name|Remarks|
|--------------|-------|
|source|-|
|uid|-|
|IgnoreCase|-|


#### GetElementCounts``1
```csharp
Microsoft.VisualBasic.Extensions.GetElementCounts``1(System.Collections.Generic.IEnumerable{``0})
```
Gets the element counts in the target data collection, if the collection object is nothing or empty
 then this function will returns ZERO, others returns Collection.Count.(返回一个数据集合之中的元素的数目，
 假若这个集合是空值或者空的，则返回0，其他情况则返回Count拓展函数的结果)

|Parameter Name|Remarks|
|--------------|-------|
|Collection|-|


#### GetItem``1
```csharp
Microsoft.VisualBasic.Extensions.GetItem``1(System.Collections.Generic.IEnumerable{``0},System.Int32)
```
这个是一个安全的方法，假若下标越界或者目标数据源为空的话，则会返回空值

|Parameter Name|Remarks|
|--------------|-------|
|source|-|
|index|-|


#### GetLength``1
```csharp
Microsoft.VisualBasic.Extensions.GetLength``1(``0[])
```
0 for null object

|Parameter Name|Remarks|
|--------------|-------|
|array|-|


#### If``1
```csharp
Microsoft.VisualBasic.Extensions.If``1(System.Boolean,``0,``0)
```
Function test the Boolean expression and then decided returns which part of the value.
 (这个函数主要是用于Delegate函数指针类型或者Lambda表达式的)

|Parameter Name|Remarks|
|--------------|-------|
|expr|@``T:System.Boolean`` Expression|
|[True]|value returns this parameter if the value of the expression is True|
|[False]|value returns this parameter if the value of the expression is False|


#### InsertOrUpdate``1
```csharp
Microsoft.VisualBasic.Extensions.InsertOrUpdate``1(System.Collections.Generic.Dictionary{System.String,``0}@,``0)
```
Insert data or update the exists data in the dictionary, if the target object with @``P:Microsoft.VisualBasic.ComponentModel.DataSourceModel.Repository.IKeyedEntity`1.Key`` 
 is not exists in the dictionary, then will be insert, else the old value will be replaced with the parameter 
 value **`item`**.
 (向字典对象之中更新或者插入新的数据，假若目标字典对象之中已经存在了一个数据的话，则会将原有的数据覆盖，并返回原来的数据)

|Parameter Name|Remarks|
|--------------|-------|
|dict|-|
|item|-|


#### Invoke
```csharp
Microsoft.VisualBasic.Extensions.Invoke(System.Diagnostics.Process)
```
本方法会执行外部命令并等待其执行完毕，函数返回状态值

|Parameter Name|Remarks|
|--------------|-------|
|Process|-|


#### InvokeSet``1
```csharp
Microsoft.VisualBasic.Extensions.InvokeSet``1(``0@,``0)
```
Value assignment to the target variable.(将**`value`**参数里面的值赋值给**`var`**参数然后返回**`value`**)

|Parameter Name|Remarks|
|--------------|-------|
|var|-|
|value|-|


#### IsNaNImaginary
```csharp
Microsoft.VisualBasic.Extensions.IsNaNImaginary(System.Double)
```
The target parameter **`n`** value is NaN or not a real number or not?
 (判断目标实数是否为一个无穷数或者非计算的数字，产生的原因主要来自于除0运算结果或者达到了
 @``T:System.Double``的上限或者下限)

|Parameter Name|Remarks|
|--------------|-------|
|n|-|


#### IsNullOrEmpty
```csharp
Microsoft.VisualBasic.Extensions.IsNullOrEmpty(System.Text.StringBuilder)
```
The @``T:System.Text.StringBuilder`` object its content is nothing?

|Parameter Name|Remarks|
|--------------|-------|
|sBuilder|-|


#### IsNullOrEmpty``1
```csharp
Microsoft.VisualBasic.Extensions.IsNullOrEmpty``1(``0[])
```
This object array is a null object or contains zero count items.(判断某一个对象数组是否为空)

#### Join``1
```csharp
Microsoft.VisualBasic.Extensions.Join``1(``0,System.Collections.Generic.IEnumerable{``0})
```
X, ....

|Parameter Name|Remarks|
|--------------|-------|
|obj|-|
|collection|-|


#### JoinBy
```csharp
Microsoft.VisualBasic.Extensions.JoinBy(System.Collections.Generic.IEnumerable{System.Int32},System.String)
```
@``T:System.String``，这是一个安全的函数，当数组为空的时候回返回空字符串

|Parameter Name|Remarks|
|--------------|-------|
|values|-|
|delimiter|-|


#### Keys``2
```csharp
Microsoft.VisualBasic.Extensions.Keys``2(System.Collections.Generic.IEnumerable{System.Collections.Generic.KeyValuePair{``0,``1}})
```
Gets all keys value from the target @``T:Microsoft.VisualBasic.ComponentModel.KeyValuePair`` collection.

|Parameter Name|Remarks|
|--------------|-------|
|source|-|


#### KeysJson``1
```csharp
Microsoft.VisualBasic.Extensions.KeysJson``1(System.Collections.Generic.Dictionary{System.String,``0})
```
Returns all of the keys in a dictionary in json format

|Parameter Name|Remarks|
|--------------|-------|
|d|-|


#### LoadTextDoc``1
```csharp
Microsoft.VisualBasic.Extensions.LoadTextDoc``1(System.String,System.Text.Encoding,System.Func{System.String,System.Text.Encoding,``0},System.Boolean)
```
默认是加载Xml文件的

|Parameter Name|Remarks|
|--------------|-------|
|file|-|
|encoding|-|
|parser|default is Xml parser|
|ThrowEx|-|


#### MatrixToUltraLargeVector``1
```csharp
Microsoft.VisualBasic.Extensions.MatrixToUltraLargeVector``1(System.Collections.Generic.IEnumerable{``0[]})
```
Merge the target array collection into one collection.
 (将目标数组的集合合并为一个数组，这个方法是提供给超大的集合的，即元素的数目非常的多的，即超过了@``T:System.Int32``的上限值)

|Parameter Name|Remarks|
|--------------|-------|
|source|-|


#### MatrixTranspose``1
```csharp
Microsoft.VisualBasic.Extensions.MatrixTranspose``1(System.Collections.Generic.IEnumerable{``0[]})
```
矩阵转置： 将矩阵之中的元素进行行列位置的互换

|Parameter Name|Remarks|
|--------------|-------|
|MAT|为了方便理解和使用，矩阵使用数组的数组来表示的|


#### MatrixTransposeIgnoredDimensionAgreement``1
```csharp
Microsoft.VisualBasic.Extensions.MatrixTransposeIgnoredDimensionAgreement``1(System.Collections.Generic.IEnumerable{``0[]})
```
将矩阵之中的元素进行行列位置的互换，请注意，假若长度不一致的话，会按照最短的元素来转置，故而使用本函数可能会造成一些信息的丢失

|Parameter Name|Remarks|
|--------------|-------|
|MAT|-|


#### ModifyValue``1
```csharp
Microsoft.VisualBasic.Extensions.ModifyValue``1(System.Reflection.PropertyInfo,``0,System.Func{System.Object,System.Object})
```
Modify target object property value using a ``valueModifier``[specific value provider] and then return original instance object.
 (修改目标对象的属性之后返回目标对象)

|Parameter Name|Remarks|
|--------------|-------|
|obj|-|


#### Move
```csharp
Microsoft.VisualBasic.Extensions.Move(System.Double@,System.Int32)
```
变量**`p`**移动距离**`d`**然后返回其移动之前的值

|Parameter Name|Remarks|
|--------------|-------|
|p|-|
|d|-|


#### MoveNext
```csharp
Microsoft.VisualBasic.Extensions.MoveNext(System.Int32@)
```
**`p`** plus one and then return its previous value. (p++)

|Parameter Name|Remarks|
|--------------|-------|
|p|-|


#### NormalizeXMLString
```csharp
Microsoft.VisualBasic.Extensions.NormalizeXMLString(System.String)
```
对Xml文件之中的特殊字符进行转义处理

|Parameter Name|Remarks|
|--------------|-------|
|str|-|


#### NotNull``1
```csharp
Microsoft.VisualBasic.Extensions.NotNull``1(``0[])
```
Returns the first not nothing object.

|Parameter Name|Remarks|
|--------------|-------|
|args|-|


#### Offset
```csharp
Microsoft.VisualBasic.Extensions.Offset(System.Int64[]@,System.Int32)
```
All of the number value in the target array offset a integer value.

|Parameter Name|Remarks|
|--------------|-------|
|array|-|
|intOffset|-|


#### ParseDateTime
```csharp
Microsoft.VisualBasic.Extensions.ParseDateTime(System.String)
```
Parsing the dat value from the expression text, if any exception happend, a null date value will returned.
 (空字符串会返回空的日期)

|Parameter Name|Remarks|
|--------------|-------|
|s|-|


#### Pause
```csharp
Microsoft.VisualBasic.Extensions.Pause(System.String)
```
Pause the console program.

|Parameter Name|Remarks|
|--------------|-------|
|Prompted|-|


#### println
```csharp
Microsoft.VisualBasic.Extensions.println(System.String,System.Object[])
```
@``M:Microsoft.VisualBasic.Terminal.STDIO.printf(System.String,System.Object[])`` + @``M:System.Console.WriteLine(System.String)``

|Parameter Name|Remarks|
|--------------|-------|
|s$|-|
|args|-|


#### Remove``1
```csharp
Microsoft.VisualBasic.Extensions.Remove``1(System.Collections.Generic.Dictionary{System.String,``0}@,``0)
```
Remove target object from dictionary.

|Parameter Name|Remarks|
|--------------|-------|
|dict|-|
|item|-|


#### RemoveDuplicates``2
```csharp
Microsoft.VisualBasic.Extensions.RemoveDuplicates``2(System.Collections.Generic.IEnumerable{``0},System.Func{``0,``1})
```
移除重复的对象，这个函数是根据对象所生成的标签来完成的

|Parameter Name|Remarks|
|--------------|-------|
|source|-|
|getKey|得到对象的标签|


#### RemoveLast``1
```csharp
Microsoft.VisualBasic.Extensions.RemoveLast``1(Microsoft.VisualBasic.Language.List{``0}@)
```
Removes the last element in the List object.

|Parameter Name|Remarks|
|--------------|-------|
|list|-|


#### Removes``1
```csharp
Microsoft.VisualBasic.Extensions.Removes``1(Microsoft.VisualBasic.Language.List{``0}@,System.Collections.Generic.IEnumerable{``0})
```
Remove all of the element in the **`collection`** from target ``List``[list]

|Parameter Name|Remarks|
|--------------|-------|
|List|-|
|collection|-|


#### RunDriver
```csharp
Microsoft.VisualBasic.Extensions.RunDriver(Microsoft.VisualBasic.ComponentModel.DataSourceModel.IObjectModel_Driver)
```
非线程的方式启动，当前线程会被阻塞在这里直到运行完毕

|Parameter Name|Remarks|
|--------------|-------|
|driver|-|


#### SecondOrNull``1
```csharp
Microsoft.VisualBasic.Extensions.SecondOrNull``1(System.Collections.Generic.IEnumerable{``0})
```
Returns the second element in the source collection, if the collection 
 is nothing or elements count not enough, then will returns nothing.

|Parameter Name|Remarks|
|--------------|-------|
|source|-|


#### Sequence``1
```csharp
Microsoft.VisualBasic.Extensions.Sequence``1(System.Collections.Generic.IEnumerable{``0},System.Int32)
```
Gets the subscript index of a generic collection.(获取某一个集合的下标的集合)

|Parameter Name|Remarks|
|--------------|-------|
|source|目标集合对象|


_returns: A integer array of subscript index of the target generic collection._

#### ShadowCopy``1
```csharp
Microsoft.VisualBasic.Extensions.ShadowCopy``1(``0,``0@,``0@,``0@,``0@,``0@,``0@,``0@,``0@,``0@,``0@,``0@,``0@)
```
Copy the source value directly to the target variable and then return the source value.

|Parameter Name|Remarks|
|--------------|-------|
|source|-|


#### Shell
```csharp
Microsoft.VisualBasic.Extensions.Shell(System.String)
```
执行一个命令行语句，并返回一个IO重定向对象，以获取被执行的目标命令的标准输出

|Parameter Name|Remarks|
|--------------|-------|
|CLI|-|


#### Shuffles``1
```csharp
Microsoft.VisualBasic.Extensions.Shuffles``1(System.Collections.Generic.IEnumerable{``0})
```
Return a collection with randomize element position in ``source``[the original collection].
 (从原有序序列中获取一个随机元素的序列)

|Parameter Name|Remarks|
|--------------|-------|
|source|-|


#### Split``1
```csharp
Microsoft.VisualBasic.Extensions.Split``1(System.Collections.Generic.IEnumerable{``0},System.Int32,System.Boolean)
```
Data partitioning function.
 (将目标集合之中的数据按照**`parTokens`**参数分配到子集合之中，
 这个函数之中不能够使用并行化Linq拓展，以保证元素之间的相互原有的顺序)

|Parameter Name|Remarks|
|--------------|-------|
|source|-|
|parTokens|每一个子集合之中的元素的数目|


#### SplitIterator``1
```csharp
Microsoft.VisualBasic.Extensions.SplitIterator``1(System.Collections.Generic.IEnumerable{``0},System.Int32,System.Boolean)
```
Performance the partitioning operation on the input sequence.
 (请注意，这个函数只适用于数量较少的序列。对所输入的序列进行分区操作，**`parTokens`**函数参数是每一个分区里面的元素的数量)

|Parameter Name|Remarks|
|--------------|-------|
|source|-|
|parTokens|-|


#### SplitMV
```csharp
Microsoft.VisualBasic.Extensions.SplitMV(System.String,System.String,System.Int32)
```


|Parameter Name|Remarks|
|--------------|-------|
|DIR|The source directory.|
|moveTo|-|
|Split|-|


#### StdError
```csharp
Microsoft.VisualBasic.Extensions.StdError(System.Collections.Generic.IEnumerable{System.Double})
```
求取该数据集的标准差

|Parameter Name|Remarks|
|--------------|-------|
|data|-|


#### SwapItem``1
```csharp
Microsoft.VisualBasic.Extensions.SwapItem``1(Microsoft.VisualBasic.Language.List{``0}@,``0,``0)
```
Swap the two item position in the target ``List``[list].

|Parameter Name|Remarks|
|--------------|-------|
|List|-|
|obj_1|-|
|obj_2|-|


#### SwapWith``1
```csharp
Microsoft.VisualBasic.Extensions.SwapWith``1(``0@,``0@)
```
Swap the value in the two variables.

|Parameter Name|Remarks|
|--------------|-------|
|obj1|-|
|obj2|-|


#### TakeRandomly``1
```csharp
Microsoft.VisualBasic.Extensions.TakeRandomly``1(System.Collections.Generic.IEnumerable{``0},System.Int32)
```
随机的在目标集合中选取指定数目的子集合

|Parameter Name|Remarks|
|--------------|-------|
|source|-|
|counts|当目标数目大于或者等于目标集合的数目的时候，则返回目标集合|


#### Time
```csharp
Microsoft.VisualBasic.Extensions.Time(System.Action)
```
Returns the total executation time of the target **`work`**.
 (性能测试工具，函数之中会自动输出整个任务所经历的处理时长)

|Parameter Name|Remarks|
|--------------|-------|
|work|
 Function pointer of the task work that needs to be tested.(需要测试性能的工作对象)
 |


_returns: Returns the total executation time of the target **`work`**. ms_

#### ToBoolean
```csharp
Microsoft.VisualBasic.Extensions.ToBoolean(System.Int64)
```
0 -> False
 1 -> True

|Parameter Name|Remarks|
|--------------|-------|
|b|-|


#### ToDictionary``2
```csharp
Microsoft.VisualBasic.Extensions.ToDictionary``2(System.Collections.Generic.IEnumerable{System.Collections.Generic.KeyValuePair{``0,``1}},System.Boolean)
```
将目标键值对对象的集合转换为一个字典对象

|Parameter Name|Remarks|
|--------------|-------|
|source|-|
|remoteDuplicates|当这个参数为False的时候，出现重复的键名会抛出错误，当为True的时候，有重复的键名存在的话，可能会丢失一部分的数据|


#### ToStringArray``1
```csharp
Microsoft.VisualBasic.Extensions.ToStringArray``1(System.Collections.Generic.IEnumerable{``0})
```
Convert target object type collection into a string array using the Object.ToString() interface function.

|Parameter Name|Remarks|
|--------------|-------|
|Collection|-|


#### ToVector``1
```csharp
Microsoft.VisualBasic.Extensions.ToVector``1(System.Collections.Generic.IEnumerable{System.Collections.Generic.IEnumerable{``0}})
```
Merge the target array collection into one collection.(将目标数组的集合合并为一个数组)

|Parameter Name|Remarks|
|--------------|-------|
|source|-|


#### TrimNewLine
```csharp
Microsoft.VisualBasic.Extensions.TrimNewLine(System.String,System.String)
```
Replace the @``F:Microsoft.VisualBasic.Constants.vbCrLf`` with the specific string.

|Parameter Name|Remarks|
|--------------|-------|
|strText|-|
|VbCRLF_Replace|-|


#### TrimNull
```csharp
Microsoft.VisualBasic.Extensions.TrimNull(System.Collections.Generic.IEnumerable{System.String})
```
Remove all of the null object in the target object collection

|Parameter Name|Remarks|
|--------------|-------|
|source|-|


#### TrimNull``1
```csharp
Microsoft.VisualBasic.Extensions.TrimNull``1(System.Collections.Generic.IEnumerable{``0})
```
Remove all of the null object in the target object collection.
 (这个是一个安全的方法，假若目标集合是空值，则函数会返回一个空的集合)

|Parameter Name|Remarks|
|--------------|-------|
|source|-|


#### TryGetValue``2
```csharp
Microsoft.VisualBasic.Extensions.TryGetValue``2(System.Collections.Generic.Dictionary{``0,``1},``0,``1)
```
假若不存在目标键名，则返回空值，默认值为空值

|Parameter Name|Remarks|
|--------------|-------|
|hash|-|
|Index|-|
|[default]|-|


#### Union
```csharp
Microsoft.VisualBasic.Extensions.Union(System.String[],System.String[])
```
Get a sub set of the string data which is contains in both collection **`strArray1`** and **`strArray2`**

|Parameter Name|Remarks|
|--------------|-------|
|strArray1|-|
|strArray2|-|


#### Unlist``1
```csharp
Microsoft.VisualBasic.Extensions.Unlist``1(System.Collections.Generic.IEnumerable{System.Collections.Generic.IEnumerable{``0}})
```
Empty list will be skip and ignored.
 (这是一个安全的方法，空集合会被自动跳过，并且这个函数总是返回一个集合不会返回空值)

|Parameter Name|Remarks|
|--------------|-------|
|source|-|


#### Wait
```csharp
Microsoft.VisualBasic.Extensions.Wait(Microsoft.VisualBasic.Extensions.WaitHandle)
```
假若条件判断**`handle`**不为真的话，函数会一直阻塞线程，直到条件判断**`handle`**为真

|Parameter Name|Remarks|
|--------------|-------|
|handle|-|


#### π
```csharp
Microsoft.VisualBasic.Extensions.π(System.Collections.Generic.IEnumerable{System.Double})
```
获取一个实数集合中所有元素的积

|Parameter Name|Remarks|
|--------------|-------|
|source|-|



### Properties

#### BooleanValues
Convert the string value into the boolean value, this is useful to the text format configuration file into data model.
#### null
Nothing
#### Scan0
The first element in a collection.
