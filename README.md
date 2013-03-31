GoldCPP
=======
A C++ Gold Parser engine  
https://github.com/pylorak/GoldCPP


Credits
-----------------------------------------
Reference engine by Devin Cook  
C++ implementation by Károly Pados  
GoldCPP License: WTFPL

This project includes UTF8-CPP by Nemanja Trifunovic, licensed separately.
See included source headers of UTF8-CPP for its license.
http://sourceforge.net/projects/utfcpp/


What is this?
-----------------------------------------
This is an almost direct port of the reference Gold Parser engine to C++.
The reason it is "direct" is because my end-goal was not to write 
a C++ gold parser just for the fun. I just happened to need a grammar parser
for one of my larger C++ projects, so I needed a parser as a tool preferably
fast to get on with my original project. Porting an existing engine was the
fastest solution.

So, should you try to cross-reference Devin's reference code
with mine, you will often be able to match large parts 1:1. Of course some
C++-ification was done to the design, but such changes were kept minimal 
on purpose to be able to easily update the C++ version whenever Devin makes
changes to his original code. In the end, I hope this will make GoldCPP easy to
update whenever Devin decides to improve the reference engine.


How to compile?
-----------------------------------------
Just compile everything statically into your project.
No tricks necessary, except that the code needs to be complied in C++11 mode.
Tested with GCC 4.7.2 and VC++2012. No unicode support under VC++ though.


How to use?
-----------------------------------------
The Parser object's interface is pretty much like that of the 
reference implementation, so if you can use that, you're good to go.
There is also a SimpleParser class included which shows you how to use
the Parser class (and wraps it at the same time). Furthermore, you can check
out "example.cpp" for a complete and runnable application.


Unicode support?
-----------------------------------------
Yes! Though C++ does not come with automatic unicode support such as .Net
languages, I tried to also port the unicode capability. The end result
is that all internal processing happens in UTF-16. This basically gives you 
immediate access to all codepoints in the unicode BMP. If this does not suit
your needs, you can easily identify all strings in the code by searching for
the macros defined in "String.h" and make changes as appropriate for you.

Inputting and outputting UTF-16 is a PITA, unfortunately, and most cross-
platform applications use UTF-8 for that. This is why I've included UTF8-CPP,
so that you can easily convert between these string representations.

Unfortunately, unicode is not supported under VC++, because its compiler
lacks support for unicode string literals. You can still build a unicode
version of GoldCPP with MinGW or Cygwin.


Memory management?
-----------------------------------------
Another thing .Net has but C++ doesn't is automatic memory management.
So here's the solution that I've come up with. During parse, Token and Reduction
objects are wrapped into standard C++ shared_ptrs, so that you can manipulate 
trees to your heart's content without having to worry about pointer management.
Other objects, most notably those created while loading the parse tables, use
simple unmanaged pointers and are owned by the Parser object. They are taken
care of correctly during destruction so you won't have memory leaks, but it 
means that any non-managed pointer you encounter is only valid during the 
lifetime of the owning Parser object. So, follow this simple workflow
and you won't have invalid pointer problems: 1. Create Parser  2. Do all your processing  3. Destroy Parser
