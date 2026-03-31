using Microsoft.EntityFrameworkCore;
using Xunit;
using practic11;

namespace practic11.Test;


public class NoteCrudTests
{
    private readonly DataContext _db;

    public NoteCrudTests()
    {
        _db = new DataContext();
        _db.Database.EnsureCreated();
    }

    private async Task ClearDatabase()
    {

        var entitys = _db.Model.GetEntityTypes();

        foreach (var entity in entitys)
        {
            var tableName = entity.GetTableName();
            if (tableName != null)
            {
                await _db.Database.ExecuteSqlRawAsync($"DELETE FROM \"{tableName}\"");
            }
        }

    }

    //  Create

    [Fact]
    public async Task Create_SaveCorrectNoteToDataBase()
    {
        await ClearDatabase();

        string text = "Тест";

        var note = await NoteCrud.Create(text);
        var saved = await _db.Notes.FirstOrDefaultAsync(x => x.Id == note.Id);

        Assert.NotNull(note);
        Assert.NotEqual(0, note.Id);
        Assert.Equal(text, note.Text);
        Assert.True(note.Time <= DateTime.Now);
        Assert.NotNull(saved);
        Assert.Equal(text, saved.Text);
        Assert.True(saved.Time <= DateTime.Now);
    }

    //  Read by text

    [Fact]
    public async Task ReadByText_ReturnCorectNotes()
    {
        await ClearDatabase();

        await NoteCrud.Create("хлеб черный");
        await NoteCrud.Create("хлеб белый");
        await NoteCrud.Create("молоко");
        await NoteCrud.Create("яйца");

        var res = await NoteCrud.Read("хлеб");

        Assert.Equal(2, res.Count);
        Assert.All(res, x => Assert.Contains("хлеб", x.Text));
    }

    [Fact]
    public async Task ReadByText_EmptyString_ReturnAllNotes()
    {
        await ClearDatabase();

        await NoteCrud.Create("хлеб черный");
        await NoteCrud.Create("хлеб белый");
        await NoteCrud.Create("молоко");
        await NoteCrud.Create("яйца");

        var res = await NoteCrud.Read("");
        Assert.Equal(4, res.Count);
    }

    [Fact]
    public async Task ReadByText_MissingString_ReturnEmptyList()
    {
        await ClearDatabase();

        await NoteCrud.Create("хлеб черный");
        await NoteCrud.Create("хлеб белый");
        await NoteCrud.Create("молоко");
        await NoteCrud.Create("яйца");

        var res = await NoteCrud.Read("горошек");
        Assert.Empty(res);
    }

    //  Read by id

    [Fact]
    public async Task ReadById_CorrectId_ReturnCorrectNote()
    {
        await ClearDatabase();

        var note = await NoteCrud.Create("хлеб черный");
        await NoteCrud.Create("хлеб белый");
        await NoteCrud.Create("молоко");
        await NoteCrud.Create("яйца");

        var res = await NoteCrud.Read(note.Id);

        Assert.Equal("хлеб черный", res.Text);
    }

    [Fact]
    public async Task ReadById_MissingId_ReturnNull()
    {
        await ClearDatabase();

        await NoteCrud.Create("хлеб черный");
        await NoteCrud.Create("хлеб белый");
        await NoteCrud.Create("молоко");
        await NoteCrud.Create("яйца");

        var res = await NoteCrud.Read(100);
        Assert.Null(res);
    }

    //  Update 

    [Fact]
    public async Task Update_CorrectUpdateNote()
    {
        await ClearDatabase();

        var note = await NoteCrud.Create("яйца");
        var origId = note.Id;
        var origTime = note.Time;

        await NoteCrud.Update(note, "хлеб");
        var res = await NoteCrud.Read(origId);

        Assert.NotNull(res);
        Assert.Equal("хлеб", res.Text);
        Assert.Equal(origId, res.Id);
        Assert.Equal(origTime, res.Time);
    }

    //  Delete

    [Fact]
    public async Task Delete_DeleteNoteFromDataBase()
    {
        await ClearDatabase();

        var note = await NoteCrud.Create("хлеб");
        var id = note.Id;
        await NoteCrud.Delete(note);
        var deleted = await NoteCrud.Read(id);
        Assert.Null(deleted);
    }
}
