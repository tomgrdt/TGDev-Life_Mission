using SharedProjectDatabase.Functions;
using System;
using System.Collections.Generic;
using System.Text;

namespace LifeMissionModels.Database;

public static partial class DatabaseFunctions
{
    public static void CreateDatabase()
    {

    }

    public static void ImportJsonFile(string fileName) 
    {
        JsonConverter.DeserializeToDatabase(fileName);
    }
}
