# [VB.NET是怎样做到的（搬家版）](http://www.cnhackhy.com/forum.php?mod=viewthread&tid=911&extra=page%3D2)

VB.net能够实现很多C#不能做到的功能，如**When**语句、**Optional**参数、局部**Static**变量、对象实例访问静态方法、**Handles**绑定事件、**On Error**处理异常、**Object**直接后期绑定等等。VB和C#同属.net的语言，编译出来的是同样的CIL，但为什么VB支持很多有趣的特性呢。我们一起来探究一下。

### 局部静态变量

VB支持用**Static**关键字声明局部变量，这样在过程结束的时候可以保持变量的数值：
```vb.net
Public Sub Test1()
   Static i As Integer
   i += 1  ' 实现一个过程调用计数器
End Sub
```
我们实现了一个简单的过程计数器。每调用一次Test，计数器的数值就增加1。其实还有很多情况我们希望保持变量的数值。而C#的static是不能用在过程内部的。因此要实现过程计数器，我们必须声明一个类级别的变量。这样做明显不如VB好。因为无法防止其他过程修改计数器变量。这就和对象封装一个道理，本来应该是一个方法的局部变量，现在我要被迫把它独立出来，显然是不好的设计。那么VB是怎么生成局部静态变量的呢？将上述代码返汇编，我们可以清楚地看到在VB生成的CIL中，i不是作为局部变量，而是作为类的Field出现的：

```il
.field private specialname int32 $STATIC$Test1$2001$i
```

也就是说，i被改名作为一个类的字段，但被冠以specialname。在代码中试图访问$STATIC$Test1$2001$i是不可能的，因为它不是一个有效的标识符。但是在IL中，将这个变量加一的代码却与一般的类字段完全一样，是通过ldfld加载的。我觉得这个方法十分聪明，把静态变量变成生命周期一样的类字段，但是又由编译器来控制访问的权限，让它成为一个局部变量。同时也解释了VB为什么要用两个不同的关键字来声明静态变量——Static和Shared。

由于局部静态变量的实质是类的字段，所以它和真正的局部变量还是有所不同的。比如在多线程条件下，对局部静态变量的访问就和访问字段相同。

### MyClass关键字

VB.net支持一项很有意思的功能——MyClass。大部分人使用MyClass可能仅限于调用本类其他构造函数时。其实MyClass可以产生一些很独特的用法。MyClass永远按类的成员为不可重写的状态进行调用，即当类的方法被重写后，用MyClass仍能得到自身的版本。下面这个例子和VB帮助中所举的例子非常相似
```vb.net
Public Class MyClassBase
   Protected Overridable Sub Greeting()
      Console.WriteLine("Hello form Base")
   End Sub

   Public Sub UseMe()
      Me.Greeting()
   End Sub

   Public Sub UseMyClass()
      MyClass.Greeting()
   End Sub
End Class

Public Class MyClassSub
   Inherits MyClassBase

   Protected Overrides Sub Greeting()
      Console.WriteLine("Hello form Sub")
   End Sub
End Class
```

我们用一段代码来测试：
```vb.net
Dim o As MyClassBase = New MyClassSub()

o.UseMe()
o.UseMyClass()
```
结果是UseMe执行了子类的版本，而UseMyClass还是执行了基类本身的版本，尽管这是一个虚拟方法。观其IL，可以看到其简单的实现原理：

Me用的调用指令是callvirt

```il
IL_0001: callvirt  instance void App1.MyClassBase::Greeting()
```

而MyClass调用的是call
```il
IL_0001:  call     instance void App1.MyClassBase::Greeting()
```

奇怪的是，如此简单的一个功能，我竟然无法用C#实现，C#怎样也不允许我按非虚函数的方式调用一个虚函数。C++可以用类名::方法名的方式访问自身版本的函数，但C#的类名只能用来访问静态的成员。这真是C#一个奇怪的限制。

### Handles和WithEvents

VB除了可以用C#那样的方法来处理事件响应以外，还有从VB5继承下来的独特的事件处理方式——WithEvents。

我喜欢称这种事件处理方式为静态的事件处理，书写响应事件的方法时就已经决定该方法响应的是哪一个事件，而C#则是在代码中绑定事件的。比如下面这个最简单的例子：
```vb.net
Public Class HandlerClass
   Public WithEvents MyObj As EventClass


   Private Sub MyObj_MyEvent(ByVal sender As Object, ByVal e As System.EventArgs) Handles MyObj.MyEvent
      MsgBox("hello")
   End Sub

   Public Sub New()
      MyObj = New EventClass
   End Sub
End Class
```
代码中用到的EventClass是这样的：
```vb.net
Public Class EventClass
   Public Event MyEvent As EventHandler

   Protected Overridable Sub OnMyEvent(ByVal e As EventArgs)
      RaiseEvent MyEvent(Me, e)
   End Sub

   Public Sub Test()
      OnMyEvent(New EventArgs)
   End Sub
End Class
```
我们来复习一下，这段代码隐式地给EventClass编写了两个方法——Add_MyEvent(EventHandler)和Remove_MyEvent(EventHandler)，实际上任何使用事件的上下文都是通过调用这两个方法来绑定事件和解除绑定的。C#还允许你书写自己的事件绑定/解除绑定的代码。

那么WithEvents是怎么工作的呢？VB.net的编译器在编译时自动将
```vb.net
Public WithEvents MyObj As EventClass
```
翻译成下面这个过程：
```vb.net
Private _MyObj As EventClass

Public Property MyObj() As EventClass
    Get
        Return _MyObj
    End Get
    Set(ByVal Value As EventClass)
        If Not (Me._MyObj Is Nothing) Then
            RemoveHandler _MyObj.MyEvent, AddressOf MyObj_MyEvent
        End If
        Me._MyObj = Value
        If Me._MyObj Is Nothing Then Exit Property

        AddHandler _MyObj.MyEvent, AddressOf MyObj_MyEvent
    End Set
End Property
```

由此可见，当对WithEvents变量赋值的时候，会自动触发这个属性以绑定事件。我们所用的大部分事件响应都是1对1的，即一个过程响应一个事件，所以这种WithEvents静态方法是非常有用的，它可以显著增强代码可读性，同时也让VB.net中的事件处理非常方便，不像C#那样离开了窗体设计器就必须手工绑定事件。

不过在分析这段IL的时候，我也发现了VB.net在翻译时小小的问题，就是**ldarg.0**出现得过多，这是频繁使用Me或this的表现，所以我们在编码过程中一定要注意，除了使用到Me/this本身引用以外，使用它的成员时不要带上Me/this，比如**Me.MyInt = 1**就改成**MyInt = 1**，这样的小习惯会为你带来很大的性能收益。

### 类型转换运算符

在Visual Basic 2005中将加入一个新的运算符——TryCast，相当于C#的as运算符。我一直希望VB有这样一个运算符。VB目前的类型转换运算符主要有CType和DirectCast。他们的用法几乎一样。我详细比较了一下这两个运算符，得出以下结论：

+ 在转换成引用类型时，两者没有什么区别，都是直接调用castclass指令，除非重载了类型转换运算符CType。DirectCast运算符是不能重载的。
+ 转换成值类型时，CType会调用VB指定的类型转换函数（如果有的话），比如将String转换为Int32时，就会自动调用**VisualBasic.CompilerServices.IntegerType.FromString**，而将Object转换为Int32则会调用FromObject。其他数值类型转换为Int32时，CType也会调用类型本身的转换方法实施转换。DirectCast运算符则很简单，直接将对象拆箱成所需类型。

所以在用于值类型时，CType没有DirectCast快速但可以支持更多的转换。在C#中，类型转换则为（type)运算符和as运算符。(type)运算符的工作方式与VB的DirectCast很相似，也是直接拆箱或castclass的，但是如果遇到支持的类型转换（如long到int），(type)运算符也会调用相应的转换方法，但不支持从String到int的转换。C#另一个运算符as则更加智能，它只要判断对象的运行实例能否转成目标类型，然后就可以省略castclass指令，直接按已知类型进行操作，而且编译器还可以自动对as进行优化，比如节省一个对象引用等。所以在将Object转换成所需的类型时，as是最佳选择。

由于as有很多优点，Visual Basic 2005将这一特性吸收了过来，用TryCast运算符就可以获得和as一样的效果，而且语法与DirectCast或CType一样。

### 实现接口

VB.net采用的实现接口的语法是VB5发明的Implements，这个实现接口的语法在当今主流语言中独一无二。比如我有两个接口：
```vb.net
Interface Interface1
   Sub Test()
End Interface

Interface Interface2
   Sub Test()
End Interface
```
这两个接口有一个完全一样的成员Test。假设我需要用一个类同时实现两个接口会怎么样呢？先想想看，如果是Java，JScrip.NET这样的语言就只能用一个Test函数实现两个接口的Test成员。假如两个Test只是偶然重名，其内容必须要分别实现怎么办，于是一些解决接口重名的设计出现了……。在VB中，独特的Implements语句可以让你想怎么实现接口就怎么实现，比如下面的类Implementation用两个名字根本不一样的方法实现了两个接口。
```vb.net
Public Class Implementation
    Implements Interface1, Interface2

    Public Sub Hello() Implements Interface1.Test
    End Sub

    Private Sub Hi() Implements Interface2.Test
    End Sub
End Class
```
也就是说，VB允许用任意名字的函数实现接口中的成员，而且访问器可以是任意的，比如想用Public还是Private都可以。

C#在处理重名成员上提供了显式实现（explicit implementation）的语法，其实现上述两个接口的语法为
```csharp
public class Class1 : Interface1, Interface2 {
    public Class1() {
    }
    void Interface1.Test() {
    }
    void Interface2.Test() {
    }
}
```
注意这里，C#只能用接口名.成员名的名字来命名实现方法，而且访问器只能是private，不能公开显式实现的方法。

在考察了IL以后，我发现.NET支持隐式实现和显式实现两种方式。其中隐式实现只要在类里面放一个与接口成员方法名字一样的方法即可——这一种VB不支持。而显式实现则在方法的描述信息里加入：
```il
.override TestApp.Interface1::Test
```
无论是C#的显式实现还是VB的Implements语句都是这样的原理。也就是说.NET提供了换名实现接口成员的功能，但是只有VB将这个自由让给了用户，而其他语言还是采用了经典的语法。

### 默认属性和属性参数

在原先的VB6里，有一项奇特的功能——默认属性。在VB6中，对象的名称可以直接表示该对象的默认属性。比如TextBox的默认属性是Text，所以下面的代码
```vb.net
Text1.Text = "Hello"
```
就可以简化为
```vb.net
Text1 = "Hello"
```
这种简化给VB带来了很多麻烦，赋值运算就需要两个关键字——Let和Set，结果属性过程也需要Let和Set两种。而且这种特征在后期绑定的时候仍能工作。到了VB.NET，这项功能被大大限制了，现在只有带参数的属性才可以作为默认属性。如
```vb.net
List1.Item(0) = "Hello"
```
可以简化为
```vb.net
List1(0) = "Hello"
```
这种语法让有默认属性的对象看起来像是一个数组。那么VB怎么判断一个属性是否是默认属性呢？看下列代码
```vb.net
Public Class PropTest 
   Public Property P1(ByVal index As Integer) As String 
      Get 

      End Get 
      Set(ByVal Value As String) 

      End Set 
   End Property 

   Default Public Property P2(ByVal index As Integer) As String 
      Get 

      End Get 
      Set(ByVal Value As String) 

      End Set 
   End Property 
End Class
```
P1和P2两个属性基本上完全相同，唯一的不同是P2带有一个Default修饰符。反汇编这个类以后，可以发现两个属性完全相同，没有任何差异。但是PropTest类却被增加了一个自定义元属性**System.Reflection.DefaultMemberAttribute**。这个元属性指定的成员是InvokeMember所使用默认类型，也就是说后期绑定也可以使用默认属性。可是我试验将DefaultMember元属性手工添加到类型上却不能达到让某属性成为默认属性的功能。看来这项功能又是VB的一项“语法甜头”。但是，VB或C#的编译器对别人生成的类的默认属性应该只能通过**DefaultMemberAttribute**来判断，所以我将一个VB类只用**DefaultMemberAttribute**指定一个默认方法，不使用Default，然后将它编译以后给C#用，果然，C#将它识别为一个索引器（indexer）！

既然说到了C#的索引器，我们就顺便来研究一下VB和C#属性方面的不同。刚才的实验结果是VB的默认属性在C#中就是索引器。但是VB仍然可以用属性的语法来访问默认属性，而C#只能用数组的语法访问索引器。更特别的是，VB可以创建不是默认属性，但是带有参数的属性，如上面例子里的P1，而C#则不支持带参数的属性，如果将VB编写的，含有带参数属性的类给C#用，C#会提示“属性不受该语言支持，请用get_XXX和set_XXX的语法访问”。也就是说，带参数的属性是CLR的一项功能，但不符合CLS（通用语言规范），因此就会出现跨语言的障碍。这也更加深了我们对CLS的认识——如果你希望让你的代码跨语言工作，请一定要注意符合CLS。

### 可选参数和按名传递

VB从4.0开始支持“可选参数”这一特性。就是说，函数或子程序的参数有些是可选的，调用的时候可以不输入。其实VB从1.0开始就有一些函数带有可选参数，只不过到了4.0才让用户自己开发这样的过程。在VB4里，可选参数可以不带默认值，而在VB.NET里，如果使用可选参数，则必须带有默认值。如 
```vb.net
Public Sub TestOptional(Optional i As Integer = 1)

End Sub
```
调用的时候，既可以写成TestOptional(2)，也可以写成TestOptional()，这种情况参数i自动等于1。如果过程有不止一个可选参数，则VB还提供一种简化操作的方法——按名传递参数。比如过程
```vb.net
Public Sub TestOptional(Optional i As Int32 = 1, Optional j As Int32 = 1, Optional k As Int32 = 1) 
End Sub 
```
如果只想指定k，让i和j使用默认值，就可以使用按名传递，如下 
```vb.net
TestOptional(k := 2) 
```
而且这种方式不受参数表顺序的限制 
```vb.net
TestOptional(k := 2, i := 3, j := 5) 
```
这些的确是相当方便的功能，C#就不支持上述两个特性。我们看看它是怎样在IL级别实现的。上述第一个方法在IL中的定义为 
```il
.method public instance void TestOptional([opt] int32 i) cil managed
{
.param [1] = int32(0x00000001)
.maxstack 8 
```
可见，参数被加上了[opt]修饰符，而且.param指定了参数的默认值。这是只有VB能识别的内容，C#会跳过他们。在调用的时候，VB若发现参数被省略，则自动读取*.param*部分的默认值，并显式传递给过程。这一部分完全由编译器处理，而且没有任何性能损失，和手工传递所有参数是完全一样的。至于按名传递，VB会自动调整参数的顺序，其结果与传统方式的传递也没有任何的不同。这说明我们可以放心地使用这项便利。而且带有可选参数的过程拿到C#中，顶多变成不可选参数，也不会造成什么其他的麻烦。 

PS.很多COM组件都使用了默认参数，而且有些过程的参数列表非常长，在VB里可以轻松地处理它们，而在C#中经常让开发者传参数传到吐血。 

### On Error语句和When语句

本次讨论的是异常处理语句。VB.NET推荐使用**Try...End Try**块来进行结构化的异常处理，但是为了确保兼容性，它也从以前版本的BASIC中借鉴了On Error语句。其实On Error并不能算是VB的优点，因为使用它会破坏程序的结构，让带有异常处理的程序难以看懂和调试。但是我一直很惊叹于VB的工程师是怎样实现它的，因为On Error可以让异常的跳转变得很灵活，不像Try那样受到限制。首先看看Try是怎样实现的：
```vb.net
Public Function F1() As Integer
   Try
      Dim n As Integer = 2 \ n
   Catch ex As Exception
      MsgBox(ex.Message)
   End Try
End Function
```
这是最简单的异常处理程序，通过Reflector反汇编（如果用ILDasm，不要选择“展开try-catch”），可以发现整个过程被翻译成19条指令。留意这一句：
```vb.net
.try L_0000 to L_0006 catch Exception L_0006 to L_0022
```
这就是典型的try块，在catch处直接指定要捕获的异常，然后指定catch区的位置，非常清晰。还要留意这两句：
```vb.net
L_0007: call ProjectData.SetProjectError
L_001b: call ProjectData.ClearProjectError
```
可以看出，这两句是在catch块的开头和末尾。深入这两个过程我发现它是在为Err对象记录异常。看来使用Err也是语法甜头，性能苦头，凭空添加了这两句（幸好都不太复杂）。

接下来我编写了一个与此功能类似的函数，用的是On语句处理异常：
```vb.net
Public Function F2() As Integer
   On Error GoTo CATCHBLOCK
   Dim n As Integer = 2 \ n
   Exit Function
   
CATCHBLOCK:
   MsgBox(Err.Description)
End Function
```
这不比上一个过程复杂，但是反汇编以后，它的IL代码竟然有47条指令，刚才才19条啊！最主要的改变是try部分，现在它是这样：
```il
.try L_0000 to L_0022 filter L_0022 L_0036 to L_0060
```
注意，catch不见了，而出现了filter。我从没在C#生成的IL中见过filter。我查询了Meta Data一节的文档，filter大概能够进行一些过滤，满足一定条件才进入处理异常的块中，本例来说，L_0022指令开始就是过滤器，它是：
```il
L_0022: isinst Exception
L_0027: brfalse.s L_0033
L_0029: ldloc.s V_4
L_002b: brfalse.s L_0033
L_002d: ldloc.3 
L_002e: brtrue.s L_0033
L_0030: ldc.i4.1 
L_0031: br.s L_0034
L_0033: ldc.i4.0 
L_0034: endfilter
```
endfilter就是异常处理部分代码的开始。而L0030之前的代码是过滤器的判断部分，**V_4**和**V_3**是VB自己加入保存错误代码的变量。在整个反汇编中，我发现设计成处理异常部分的代码在IL里其实也是在try块中，也就是说程序的结构已经不是规整的**try...catch**块，产生异常的语句和处理异常的语句在一起，而真正处理异常的指令是一大堆繁冗拖沓的跳转语句。

下面看看我编写的第三个例子：
```vb.net
Public Function F3() As Integer
   On Error Resume Next
   Dim n As Integer = 2 \ n
End Function
```
这个值有2行的过程动用了VB强大的语法杀手——**On Error Resume** Next，它将忽略所有异常，让代码紧接产生异常的语句继续执行下去，猜猜这个功能产生了多少IL指令？答案是**50**条！比普通的**On Error**还要长。其实现我就不多说了，和前面的On语句差不多。不过50这个数字似乎提醒了大家，不要在程序里偷懒使用On Error处理异常，这样产生的代价是不可接受的。

最后一个例子是VB.NET的When语句，它可以实现对Catch部分的过滤：
```vb.net
Public Function F1() As Integer
   Dim n As Integer = 0
   Try
      Dim m As Integer = 2 \ n
   Catch ex As Exception When n = 0
      MsgBox(ex.Message)
   End Try
End Function
```
里面的When语句进行了对变量n的判断，仅当**n = 0**的时候才进入处理部分。

听到“过滤”两个字，我们已经猜出，它是用try...filter来实现的。没错。这里的filter主要是进行ex是否是Exception型，n是否等于零等，当过滤成功，就会转移到异常处理段进行处理。这次VB生成的代码要比On Error语句规则得多，结构相当清晰。

本次我们还借助On Error语句和When语句了解到try filter结构，它是C#不能生成的，因此，我发现它不能被常见的反编译器反编译（因为反编译器的编写者只知道C#，呵呵）。而且用了On Error后程序结构变得异常混乱，这在产生负面作用的时候，是不是能够变相起到保护我们代码的作用呢？

（九）实例访问共享成员

大家都知道静态成员在VB中叫做共享成员，虽然刚接受起来有点别扭，但“共享成员”的确是名副其实的：
```vb.net
Public Class Class1
   Public Shared i As Integer
   ' Other none-shared members
End Class
```
不但像在C#中那样，可以用Class1.i访问共享成员i，还可以用实例变量来访问:
```vb.net
Dim c1 As New Class1
c1.i = 100
```
就像i是c1的成员一样！当然只有一个i，任何实例去修改i的值都将导致所有i的值改变（因为其实只有一个）。甚至Me和MyClass也可以访问共享成员。
```vb.net
Me.i = 100
MyClass.i = 100
```
这在C#中是不可能做到的，一个纯正的C#程序员看到这些代码一定会觉得匪夷所思。为了揭示它的工作原理，我们可以做下列实验：
```vb.net
Dim c1 As Class1
c1.i = 100
```
注意，这里的c1为Nothing!，即使是Nothing的变量也可以访问共享成员，而且不会出错。接下来我们实验更极端的情况：
```vb.net
Dim o As Object = New Class1
o.i = 100
```
结果——失败，不能通过后期绑定访问共享成员。现在结果已经很明显，只有在VB明确了解对象类型的情况下，才能使用实例访问共享成员，VB会自动判断类型，然后将所有对共享成员访问的语句改写成
```vb.net
Class1.i = 100
```
这样的语法。Delphi也支持这一有趣的特征，而且李维在《Inside VCL》中将此说成Delphi.NET相对于.NET的扩展之一。
>"结果——失败，不能通过后期绑定访问共享成员"
那只是编辑器不允许而已，语法结构上并没有错！将Option Strict 设为 off 就不会有错误提示。正在从无知迈向菜鸟```
