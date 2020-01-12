Fibers
======

Fibers are the fundamental concurrency component in Fibrous.  In the most basic sense, they are a queue of delegates that get executed in order using the thread pool.  The main method for a fiber is Enqueue.  This can be used directly, but is used behind the scenes to allow other functionality like subscribing to a channel.

There are two Fiber types:  Fiber and AsyncFiber

Fiber
-----

AsyncFiber
----------