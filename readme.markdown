Rhino Security
==============

Rhino Security is part of the [Rhino Tools](http://hibernatingrhinos.com/) collection by [Ayende Rahien](http://ayende.com/blog).                                                     
       
What is this?
-------------

Rhino Security is a security framework to provide row level security for NHibernate. Rhino Security is perfect for people who want to set up user and group security in their NHibernate domain models. It supports ACL and role based security using  a model similar to this:

![yUML Rhino Security](http://yuml.me/diagram/scruffy/class/%5BUser%5D%3C1-*++%5BPermission%5D%2C%20%5BPermission%5D++-1%3E%5BOperation%5D%2C%20%5BOperation%5D++-%3E%5BOperation%5D%2C%20%5BUser%5D%3C*-%5BUserGroup%5D)
                                             
*Based on [this blog post](http://weblogs.asp.net/arturtrosin/archive/2009/04/02/rhino-tools-rhino-security-guide.aspx)*


Getting Started
---------------

Read the [How to Use.txt](http://github.com/ayende/rhino-security/raw/master/How%20to%20Use.txt) file for more information about getting started.
                 
More Information
----------------

More information about the library can be found [in the Rhino Security category on Ayende's blog](http://ayende.com/Blog/category/548.aspx).

Try [this Google search]("rhino security" nhibernate) to find a ton of other information about it.    


-----

Intro by [Tobin Harris](http://tobinharris.com), he askes that people contribute to make it better :)
