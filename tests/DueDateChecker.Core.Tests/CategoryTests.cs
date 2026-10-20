using DueDateChecker.Core.Data;
using DueDateChecker.Core.Domain;
using Microsoft.Data.Sqlite;

namespace DueDateChecker.Core.Tests;

public sealed class CategoryTests : IDisposable
{
    private readonly string _path = Path.Combine(Path.GetTempPath(), $"categories-{Guid.NewGuid():N}.db");
    private static readonly DateTimeOffset Now = new(2026, 10, 18, 0, 0, 0, TimeSpan.Zero);
    private SqliteConnection Open()
    {
        var connection = new SqliteConnection(new SqliteConnectionStringBuilder { DataSource = _path }.ToString());
        connection.Open();
        return connection;
    }
    private void Execute(string sql)
    {
        using var connection = Open();using var command = connection.CreateCommand();command.CommandText = sql;command.ExecuteNonQuery();
    }
    private void Legacy()
    {
        using var stream = typeof(DatabaseInitializer).Assembly.GetManifestResourceStream("DueDateChecker.Core.Data.Migrations.001_create_dues.sql")!;
        using var reader = new StreamReader(stream);
        Execute(reader.ReadToEnd()+"; PRAGMA user_version=1;");
    }
    private string Snapshot()
    {
        using var connection = Open();using var command = connection.CreateCommand();
        command.CommandText = "SELECT id,title,amount_centavos,currency,due_date,notes,is_paid,timezone,created_at,updated_at,deleted_at FROM dues ORDER BY id";
        using var reader = command.ExecuteReader();var rows = new List<string>();
        while (reader.Read()) rows.Add(string.Join("|", Enumerable.Range(0, reader.FieldCount).Select(reader.GetValue)));
        return string.Join("\n", rows);
    }
    private long Version()
    {
        using var connection = Open();using var command = connection.CreateCommand();command.CommandText="PRAGMA user_version";return (long)command.ExecuteScalar()!;
    }

    [Fact]
    public async Task Legacy_upgrade_preserves_every_stored_field_for_active_paid_and_deleted_rows()
    {
        Legacy();
        var ids = Enumerable.Range(0,3).Select(_=>Guid.NewGuid()).ToArray();
        using (var connection = Open())
        {
            for(var i=0;i<3;i++)
            {
                using var command=connection.CreateCommand();
                command.CommandText="INSERT INTO dues(id,title,amount_centavos,due_date,notes,is_paid,created_at,updated_at,deleted_at) VALUES(@id,'Legacy',12345,'2026-10-17','Original notes',@paid,'2026-10-01T12:00:00.0000000+08:00','2026-10-02T12:00:00.0000000+08:00',@deleted)";
                command.Parameters.AddWithValue("@id",ids[i].ToString());command.Parameters.AddWithValue("@paid",i==1?1:0);
                command.Parameters.AddWithValue("@deleted",i==2?(object)"2026-10-02T12:00:00.0000000+08:00":DBNull.Value);command.ExecuteNonQuery();
            }
        }
        var before=Snapshot();
        new DatabaseInitializer(_path).Initialize();new DatabaseInitializer(_path).Initialize();
        Assert.Equal(3,Version());Assert.Equal(before,Snapshot());
        var repository=new SqliteDueRepository(_path);
        foreach(var id in ids) Assert.Equal(DueCategory.Other,(await repository.GetAsync(id))!.Category);
        Assert.Equal(2,(await repository.ListActiveAsync()).Count);
        Assert.NotNull((await repository.GetAsync(ids[2]))!.DeletedAt);
    }

    [Theory]
    [InlineData(DueCategory.Bill)] [InlineData(DueCategory.Task)] [InlineData(DueCategory.Subscription)] [InlineData(DueCategory.Other)]
    public async Task Categories_round_trip_update_and_soft_delete_without_losing_identity(DueCategory category)
    {
        new DatabaseInitializer(_path).Initialize();var repository=new SqliteDueRepository(_path);
        var due=Due.Create("Category",Money.FromCentavos(0),new(2026,10,18),Now,category:category);
        await repository.AddAsync(due);Assert.Equal(due,await repository.GetAsync(due.Id));
        var edited=due with { Category=DueCategory.Other,IsPaid=true,UpdatedAt=Now.AddDays(1) };
        await repository.UpdateAsync(edited);Assert.Equal(edited,Assert.Single(await new SqliteDueRepository(_path).ListActiveAsync()));
        await repository.SoftDeleteAsync(due.Id,Now.AddDays(2));
        Assert.Empty(await repository.ListActiveAsync());
        Assert.Equal(edited with { UpdatedAt=Now.AddDays(2),DeletedAt=Now.AddDays(2) },await repository.GetAsync(due.Id));
        await Assert.ThrowsAsync<KeyNotFoundException>(()=>repository.UpdateAsync(edited with {Category=category}));
    }

    [Theory]
    [InlineData(-1)] [InlineData(4)] [InlineData(999)]
    public void Undefined_values_are_rejected_on_create_constructor_and_with(int code)
    {
        var value=(DueCategory)code;
        Assert.Throws<ArgumentOutOfRangeException>(()=>Due.Create("Invalid",Money.FromCentavos(0),new(2026,10,18),Now,category:value));
        Assert.Throws<ArgumentOutOfRangeException>(()=>new Due(Guid.NewGuid(),"Invalid",Money.FromCentavos(0),new(2026,10,18),null,false,Now,Now,null,value));
        var due=Due.Create("Valid",Money.FromCentavos(0),new(2026,10,18),Now);
        Assert.Equal(DueCategory.Other,due.Category);
        Assert.Throws<ArgumentOutOfRangeException>(()=>due with {Category=value});
    }

    [Fact]
    public async Task Storage_rejects_invalid_or_null_category_and_corrupt_read_fails_closed()
    {
        new DatabaseInitializer(_path).Initialize();var repository=new SqliteDueRepository(_path);
        var due=Due.Create("Valid",Money.FromCentavos(0),new(2026,10,18),Now);await repository.AddAsync(due);
        Assert.Throws<SqliteException>(()=>Execute("UPDATE dues SET category=99"));
        Assert.Throws<SqliteException>(()=>Execute("UPDATE dues SET category=NULL"));
        Assert.Equal(due,await repository.GetAsync(due.Id));
        Execute("PRAGMA ignore_check_constraints=ON; UPDATE dues SET category=99;");
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(()=>repository.GetAsync(due.Id));
    }

    [Fact]
    public void Migration_failure_preserves_version_and_legacy_data_without_resetting_database()
    {
        Legacy();
        Execute("INSERT INTO dues(id,title,amount_centavos,due_date,created_at,updated_at) VALUES('preserved','Legacy',0,'2026-10-18','original','original'); ALTER TABLE dues ADD COLUMN category INTEGER NOT NULL DEFAULT 0;");
        var before=Snapshot();
        Assert.Throws<SqliteException>(()=>new DatabaseInitializer(_path).Initialize());
        Assert.Equal(1,Version());Assert.Equal(before,Snapshot());
    }

    public void Dispose(){SqliteConnection.ClearAllPools();File.Delete(_path);}
}
