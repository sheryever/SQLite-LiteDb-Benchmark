using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using LiteDB;
using System;
using System.Data.SQLite;
using System.IO;
using Bogus;  // For generating fake data

[MemoryDiagnoser]
public class DatabaseBenchmark
{
    private SQLiteConnection sqliteConnection;
    private LiteDatabase liteDatabase;
    private const string SqliteDbFile = "test.db";
    private const string LiteDbFile = "test.litedb";
    private const int RecordCount = 150000;

    [GlobalSetup]
    public void Setup()
    {
        // Setup SQLite
        if (File.Exists(SqliteDbFile)) File.Delete(SqliteDbFile);
        sqliteConnection = new SQLiteConnection($"Data Source={SqliteDbFile};Version=3;");
        sqliteConnection.Open();
        using (var cmd = sqliteConnection.CreateCommand())
        {
            cmd.CommandText = @"CREATE TABLE Users (
                                Id INTEGER PRIMARY KEY AUTOINCREMENT, 
                                UserName TEXT, 
                                FullName TEXT, 
                                Phone TEXT)";
            cmd.ExecuteNonQuery();
        }

        // Setup LiteDB
        if (File.Exists(LiteDbFile)) File.Delete(LiteDbFile);
        liteDatabase = new LiteDatabase(LiteDbFile);
        var liteCollection = liteDatabase.GetCollection<User>("users");

        // Insert dummy data
        InsertDummyData();
    }

    private void InsertDummyData()
    {
        var faker = new Faker<User>()
            .RuleFor(u => u.UserName, f => f.Internet.UserName())
            .RuleFor(u => u.FullName, f => f.Name.FullName())
            .RuleFor(u => u.Phone, f => f.Phone.PhoneNumber());

        // Insert data into SQLite
        using (var transaction = sqliteConnection.BeginTransaction())
        {
            using (var cmd = sqliteConnection.CreateCommand())
            {
                cmd.CommandText = "INSERT INTO Users (UserName, FullName, Phone) VALUES (@UserName, @FullName, @Phone)";
                for (int i = 0; i < RecordCount; i++)
                {
                    var user = faker.Generate();
                    cmd.Parameters.AddWithValue("@UserName", user.UserName);
                    cmd.Parameters.AddWithValue("@FullName", user.FullName);
                    cmd.Parameters.AddWithValue("@Phone", user.Phone);
                    cmd.ExecuteNonQuery();
                }
            }
            transaction.Commit();
        }

        // Insert data into LiteDB
        var liteCollection = liteDatabase.GetCollection<User>("users");
        for (int i = 0; i < RecordCount; i++)
        {
            var user = faker.Generate();
            liteCollection.Insert(user);
        }
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        sqliteConnection.Close();
        liteDatabase.Dispose();
    }

    // SQLite Methods
    [Benchmark]
    public void SqliteInsert()
    {
        using (var cmd = sqliteConnection.CreateCommand())
        {
            cmd.CommandText = @"INSERT INTO Users (UserName, FullName, Phone) 
                                VALUES ('newuser', 'New User', '123-456-7890')";
            cmd.ExecuteNonQuery();
        }
    }

    [Benchmark]
    public void SqliteReadById()
    {
        using (var cmd = sqliteConnection.CreateCommand())
        {
            User user = null;
            cmd.CommandText = "SELECT * FROM Users WHERE Id = 50000";
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    user = new User
                    {
                        Id = reader.GetInt32(0),
                        UserName = reader.GetString(1),
                        FullName = reader.GetString(2),
                        Phone = reader.GetString(3)
                    };
                }
            }

            if (user == null)
            {
                throw new Exception("User not found");
            }
        }
    }

    [Benchmark]
    public void SqliteReadAll()
    {
        using (var cmd = sqliteConnection.CreateCommand())
        {
            cmd.CommandText = "SELECT * FROM Users";
            var users = new List<User>();
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read()) { 
                    users.Add(new User
                    {
                        Id = reader.GetInt32(0),
                        UserName = reader.GetString(1),
                        FullName = reader.GetString(2),
                        Phone = reader.GetString(3)
                    });

                }
            }
            if (users.Count() < 5){
                throw new Exception("Users not found");
            }
        }
    }

    [Benchmark]
    public void SqliteUpdate()
    {
        using (var cmd = sqliteConnection.CreateCommand())
        {
            cmd.CommandText = "UPDATE Users SET Phone = '987-654-3210' WHERE Id = 50000";
            cmd.ExecuteNonQuery();
        }
    }

    // LiteDB Methods
    [Benchmark]
    public void LiteDbInsert()
    {
        var usersCollection = liteDatabase.GetCollection<User>("users");
        usersCollection.Insert(new User { UserName = "newuser", FullName = "New User", Phone = "123-456-7890" });
    }

    [Benchmark]
    public void LiteDbReadById()
    {
        var usersCollection = liteDatabase.GetCollection<User>("users");
        var user = usersCollection.FindById(50000);
        if (user == null){
                throw new Exception("Users not found");
        }
    }

    [Benchmark]
    public void LiteDbReadAll()
    {
        var usersCollection = liteDatabase.GetCollection<User>("users");
        
        if (usersCollection.FindAll().Count() < 5){
            throw new Exception("Users not found");
        }
    }

    [Benchmark]
    public void LiteDbUpdate()
    {
        var usersCollection = liteDatabase.GetCollection<User>("users");
        var user = usersCollection.FindById(1);
        user.Phone = "987-654-3210";
        usersCollection.Update(user);
    }

    // User model
    public class User
    {
        //public ObjectId UserId { get; set; }
        public int Id { get; set; }
        public string UserName { get; set; }
        public string FullName { get; set; }
        public string Phone { get; set; }
    }

    public static void Main(string[] args)
    {
        var summary = BenchmarkRunner.Run<DatabaseBenchmark>();
    }
}
