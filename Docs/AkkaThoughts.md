Thoughts on Akka.net
====================

- Akka is complicated...  over-complicated for most needs.
- If careful, you can decouple your logic from the concurrency framework, but the API pushes you toward entanglement
- Error handling through supervision has never sounded like a good idea.  You entangle your error handling with the framework
- Akka is great if you need distributed actors.  All the complexity of the framework is required to provide this functionality.  
- The Become state machines are interesting and potentially very useful
- 

