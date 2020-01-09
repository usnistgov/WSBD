-----------------------------------
Important note regarding ThreadJobs
-----------------------------------

A job will only catch a thread interrupted exception on cancellation if it is a straight 'catch' block --- adding 
exception filters (i.e., a When clause in VB.Net) will disable the exception handler.

-rjm / 3 Mar 2011
