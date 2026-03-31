using Microsoft.EntityFrameworkCore;
using Xunit;
using practic11;

namespace practic11.Test;



public class NoteCrudTests
{
    private DataContext _db;

    public NoteCrudTests()
    {
        ResetDatabase();
    }

    private void ResetDatabase()
    {
        _db?.Dispose();
        _db = new DataContext();
        _db.Database.EnsureCreated();

        _db.Database.ExecuteSqlRaw("DELETE FROM Notes");
        _db.ChangeTracker.Clear();
    }

    //  Create

    [Fact]
    public async Task Create_SaveCorrectNoteToDataBase()
    {
        ResetDatabase();

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
        ResetDatabase();

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
        ResetDatabase();

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
        ResetDatabase();

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
        ResetDatabase();

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
        ResetDatabase();

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
        ResetDatabase();

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
        ResetDatabase();

        var note = await NoteCrud.Create("хлеб");
        var id = note.Id;
        await NoteCrud.Delete(note);
        var deleted = await NoteCrud.Read(id);
        Assert.Null(deleted);
    }
}
