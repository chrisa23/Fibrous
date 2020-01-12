using System;

namespace PipelineExample
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