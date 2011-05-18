Critical Paths in .NET: Extension Method
========================================

This is a rewrite of [Critical Path Method Implementation in C#](http://www.codeproject.com/KB/recipes/CriticalPathMethod.aspx).  It has the following features over and above the original:

* It uses generics to avoid type dependency.  Extra metadata is stored internally
* The public method is an extension method on IEnumerable, so any list works
* Task information can come in any order.  Pass in a lambda to tell the engine how to get a task's predecessors and another lambda to get the task length and it will figure out the rest
* If a loop is detected in the path, an exception will be thrown 

It is used in a production web site and is stable at this point



