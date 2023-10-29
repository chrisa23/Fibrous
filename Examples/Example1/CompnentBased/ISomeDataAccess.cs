using System;

namespace Example1.CompnentBased
{
    public interface ISomeDataAccess
    {
        void SaveData(Payload payload);
    }


    public class SomeDataAccess : ISomeDataAccess
    {
        public void SaveData(Payload payload)
        {
            Console.WriteLine("Data Saved");
        }
    }
}