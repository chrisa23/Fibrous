﻿using System;

namespace Example1.ComponentBased
{
    public interface ISomeService
    {
        object GetLatest();
    }

    public class SomeService : ISomeService
    {
        public object GetLatest()
        {
            Console.WriteLine("GetLatest called");
            return null;
        }
    }
}