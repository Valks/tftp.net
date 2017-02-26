# tftp.net
C# TFTP Server and Client

Project Description

This projects implements the TFTP (Trivial File Transfer) protocol (client/server) in an easy-to-use C#/.NET library. 

The main focus is to provide an API that allows you to easily access TFTP servers (or serve TFTP clients) in your own C# applications. If you're looking for a fully-fledged GUI client, you should probably look into other projects. However, if you're looking for code that allows you to implement your own TFTP client/server in only a few lines of C# code, you've come to the right place.

Download:

I recommend you just pull the latest version from Codeplex (see tab Source Code). Building the library in your Visual Studio should work without problems. Contact me if you're having any issues.

NEW:

Now supports TFTP timeout interval option (RFC 2349).
Now supports TFTP transfer size option (RFC 2349).
Now supports TFTP option extension (RFC 2347).
Now supports TFTP block size option (RFC 2348).

At the moment the library features:

- Complete TFTP protocol implementation (as defined in RFC 1350, RFC 2347 and RFC 2349)
- TFTP client components 
- TFTP server components 
- Unit-Tested code using NUnit
- Sample TFTP server
- Sample TFTP client

Last edited Dec 24, 2014 at 1:43 AM by Callisto, version 14
