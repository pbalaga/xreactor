# xReactor for .NET
**Project moved from codeplex. No longer actively maintained.**

**Note on stability**
First release version of the project is only a few steps ahead as the core concepts are becoming more complete and mature. Changes to the presented functionality are still possible, though. 

**What can it do?**
This library combines Reactive Extensions and Linq Expressions to let you set up relations between properties in a manner as close to the reactive paradigm as possible. Designed with independence of any specific GUI technology on mind, it has a particular strength in WPF applications. It is also easy to plug into your application, no matter what MVVM framework you use.

In GUI programming there's often a need to listen to changes occurring on multiple nested objects. Of course, you could attach PropertyChanged event handlers yourself, but going through the same configuration steps repeatedly is tiresome and error-prone. This little framework is supposed to automatize these tasks.

xReactor allows you to write something like:

{code:c#}
React.To(() => AverageYearlyIncome * Years).Set(() => TotalIncome);
{code:c#}

The above makes the _TotalIncome_ always be set to the value of expression _AverageYearlyIncome * Years_ and does not require any further set-up or attaching event handlers from you. Every time either _AverageYearlyIncome_ or _Years_ property changes, the entire expression is re-evaluated and the result is assigned to the _TotalIncome_ property. The only requirement is that the class declaring _AverageYearlyIncome_ and _Years_ properties implements the INotifyPropertyChanged interface and raises appropriate notification events. xReactor has tools to take off the burden of repeating the typical INotifyPropertyChanged boilerplate, but more on that later.

## Quick Overview
To get started with basic features it's not needed to write any additional code for the sake of setting xReactor up. xReactor is also easily pluggable into existing projects as it does not restrict the structure of the model layer. You can use the _React.To(...)_ syntax with any objects implementing INotifyPropertyChanged, while still sticking to the MVVM framework of your choice.

The expression to be put in the _React.To(...)_ method can be quite complex, as pretty many language constructs are supported.

**Deep notifications**
{code:c#}
React.To(() => DocumentManager.ActiveDocument.Objects.Count)
     .Set(count =>Status.Text = "Number of objects is " + count.ToString());
{code:c#}

which would cause status be set whenever the expression in the _React.To(...)_ method call changes. That is when:
* the number of elements in the Objects collection changes
* when the Objects property is set somewhere to a different collection instance
* when ActiveDocument is changed
* when DocumentManager property is set to another instance

**Math expressions**
{code:c#}
React.To(() => Math.Cos(Angle) * X + Math.Sin(Angle) * Y).Set(() => Z);
{code:c#}

**Coalesce / Ternary conditional / Type operators**
{code:c#}
React.To(() => UserText ?? FallbackText)
     .Set(() => DisplayText);
React.To(() => string.IsNullOrEmpty(UserText) ? FallbackText : UserText)
     .Set(() => DisplayText);
React.To(() => UnknownObject is string)
     .Set(value => DisplayText = (string)value);
{code:c#}

**Using Arbitrary Rx Extension Methods**
{code:c#}
React.To(() => Math.Cos(Angle) * X + Math.Sin(Angle) * Y)
     .Where(val => val > 0).Select(val => val * 2)
     .Throttle(TimeSpan.FromSeconds(0.5))
     .Set(() => Z);
{code:c#}

**Tracking changes on individual collection items**
{code:c#}
React.To(() => People.TrackItems(person => person.Age))
     .Where(people => people.Any())
     .Select(people => people.Average(person => person.Age))
     .Set(() => AverageAge);
{code:c#}

_AverageAge_ will be kept in sync whenever _Age_ of any person in the collection changes.

**Automatic PropertyChanged raising**
{code:c#}
public double AverageAge
{
    get;
    private set;
}

React.To(() => People.TrackItems(person => person.Age))
     .Where(people => people.Any())
     .Select(people => people.Average(person => person.Age))
     .SetAndNotify(() => AverageAge);
{code:c#}

The _SetAndNotify_ method can be used to automatically raise property change notifications. Then the property definition is much more concise as it is not required to raise the _PropertyChanged_ event manually from within the property's setter. 

This approach seems to have some limitations, though. All changes to the property would have to be applied via the _SetAndNotify_ method not to miss the notifying procedure. That being said, "reactive" properties are usually results of some internal operations on other properties. This way, keeping the setter private prevents external actors from modifying the property from outside.

**Property<T> class**
The _React.To(...)_ syntax is quite flexible, particularly because it does not require deriving logic objects from a common specialized base class. This renders xReactor very easy to plug in, no matter what MVVM framework in use in a specific project. In fact, xReactor could be used as a glue layer between components written in different MVVM frameworks. However, such a custom specialized base class still ships along with xReactor.

Classes deriving from _ReactiveBase_ can facilitate some 'bonuses'  out of the box. One of these is the _Property<T>_ wrapper, which raises property changes, supports fluent-style validation and (of course) exposes a value via a standard CLR property:
{code:c#}
private Property<int> totalIncomeProperty;
public int TotalIncome
{
    get { return totalIncomeProperty.Value; }
    private set { totalIncomeProperty.Value = value; }
}}
{code:c#}

It is necessary to initialize the _Property<T>_ object before use, typically in constructor of the owner class.
{code:c#}
totalIncomeProperty = this.Create(() => TotalIncome, () => AverageYearlyIncome * Years)
{code:c#}

An additional advantage of this approach is possibility of ensuring that the value satisfies a set of conditions before it is set, e.g:
* ensure _TotalIncome_ is always non-negative; throw exception otherwise
{code:c#}
totalIncomeProperty.Require(total => total >= 0, "Must be non-negative");
{code:c#}
* ensure _TotalIncome_ is always non-negative; change the value to 0, if it is less
{code:c#}
totalIncomeProperty.Coerce(total => Math.Max(0, total), "Must be non-negative");
{code:c#}

**Full working samples**
These can be found in the latest source code. There are currently 2 simple GUI-based sample apps in the solution:
* xReactor.WpfSample : leverages the _ReactiveBase_ class
* xReactor.Samples.MVVMLight : as the name says, uses MVVMLight Toolkit

**How does it work?**
Implemented mechanisms leverage INotifyPropertyChanged interface and the System.Linq.Expressions. When you provide a reactive expression like this:
{code:c#}
() => AverageYearlyIncome * Years
{code:c#}

xReactor investigates the expression tree in search for properties, on which the entire expression value depends. It finds that these are _AverageYearlyIncome _ and _Years_ properties. If properties found are defined on an instance that implements INotifyPropertyChanged, event handlers are attached to it to listen to changes of  _AverageYearlyIncome _ and _Years_  properties. Every time such a change occurs, the expression is recalculated and assigned, i.e. to the _TotalIncome_ property. 

**Points of interest and remarks**
xReactor is already capable of handling more sophisticated expressions including method calls, lambdas and collection changes (including deep tracking of individual INPC items in a collection). For complete samples please refer to the latest source code. 

Currently, the most urgent goals for xReactor are:
* adding a NuGet package
* creating good documentation
