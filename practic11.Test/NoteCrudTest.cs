using Microsoft.EntityFrameworkCore;
using Xunit;
using practic11;
using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.DependencyModel;

namespace practic11.Test
{
    public class NoteCrudTests
    {
        private readonly DataContext _db;
        private readonly string _testDbPath = "test.db";

        public NoteCrudTests()
        {
            var options = new DbContextOptionsBuilder<DataContext>()
                .UseSqlite($"Data Source={_testDbPath}")
                .Options;

            _db = new DataContext(options);

            _db.Database.EnsureDeleted();
            _db.Database.EnsureCreated();
        }

        public void Dispose()
        {
            _db.Dispose();
            if (File.Exists(_testDbPath))
            {
                File.Delete(_testDbPath);
            }
        }

        //  Create

        [Fact]
        public async Task Create_SaveCorrectNoteToDataBase()
        {
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

        //  Read by Text

        [Fact]
        public async Task ReadByText_ReturnCorectNotes()
        {
            await NoteCrud.Create("хлеб черный");
            await NoteCrud.Create("хлеб белый");
            await NoteCrud.Create("молоко");
            await NoteCrud.Create("яйца");

            var res = await NoteCrud.Read("хлеб");

            Assert.True(res.Count == 2);
            Assert.All(res, x => Assert.Contains("хлеб", x.Text));
        }

        [Fact]
        public async Task ReadByText_EmptyString_ReturnAllNotes()
        {
            await NoteCrud.Create("хлеб черный");
            await NoteCrud.Create("хлеб белый");
            await NoteCrud.Create("молоко");
            await NoteCrud.Create("яйца");

            var res = await NoteCrud.Read("");
            Assert.True(res.Count == 4);
    
        }

        [Fact]
        public async Task ReadByText_MissingString_ReturnEmptyList()
        {
            await NoteCrud.Create("хлеб черный");
            await NoteCrud.Create("хлеб белый");
            await NoteCrud.Create("молоко");
            await NoteCrud.Create("яйца");

            var res = await NoteCrud.Read("горошек");
            Assert.Empty(res);
        }

        //  Read by Id

        [Fact]
        public async Task ReadById_CorrectId_ReturnCorrectNote()
        {
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
            var note = await NoteCrud.Create("яйца");
            var origId = note.Id;
            var origTime = note.Time;

            await NoteCrud.Update(note, "хлеб");
            var res = await NoteCrud.Read(origId);

            Assert.Equal("хлеб", res.Text);
            Assert.Equal(origId, note.Id);
            Assert.Equal(origTime, note.Time);

        }

        //  Delete

        [Fact]
        public async Task Delete_DeleteNoteFromDataBase()
        {
            var note = await NoteCrud.Create("хлеб");
            var id = note.Id;
            await NoteCrud.Delete(note);
            var deleted = await NoteCrud.Read(id);
            Assert.Null(deleted);
        }

    }
}