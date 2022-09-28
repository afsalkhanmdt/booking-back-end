using Microsoft.EntityFrameworkCore;
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<BookingDb>(opt => opt.UseInMemoryDatabase("BookingDb"));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();
builder.Services.AddCors();

var app = builder.Build();

app.UseCors(builder =>
{
    builder
    .AllowAnyOrigin()
    .AllowAnyMethod()
    .AllowAnyHeader();
});

app.MapGet("/", () => "Hello World");

app.MapGet("/rooms", async (BookingDb db) =>
    await db.Rooms.Include(r => r.amenities).ToListAsync());


app.MapGet("/rooms/{id}", async (int id, BookingDb db) =>
    await db.Rooms.FindAsync(id)
        is Room room
            ? Results.Ok(room)
            : Results.NotFound());

app.MapPost("/rooms", async (Room room, BookingDb db) =>
{
    db.Rooms.Add(room);
    await db.SaveChangesAsync();

    return Results.Created($"/rooms/{room.id}", room);
});

app.MapPut("/rooms/{id}", async (int id, Room inputRoom, BookingDb db) =>
{
    var room = await db.Rooms.FindAsync(id);

    if (room is null) return Results.NotFound();

    room.roomNumber = inputRoom.roomNumber;
    room.adultCapacity = inputRoom.adultCapacity;
    room.childCapacity = inputRoom.childCapacity;
    room.price = inputRoom.price;

    await db.SaveChangesAsync();

    return Results.NoContent();
});

app.MapDelete("/rooms/{id}", async (int id, BookingDb db) =>
{
    if (await db.Rooms.FindAsync(id) is Room room)
    {
        db.Rooms.Remove(room);
        await db.SaveChangesAsync();
        return Results.Ok(room);
    }

    return Results.NotFound();
});

app.Run();

class Room
{
    public int id { get; set; }
    public int roomNumber { get; set; }
    public int adultCapacity { get; set; }
    public int childCapacity { get; set; }
    public int price { get; set; }
    public ICollection<Amenities> amenities { get; set; }
}

class Amenities
{
    public int id { get; set; }
    public string? text { get; set; }
}

class BookingDb : DbContext
{
    public BookingDb(DbContextOptions<BookingDb> options)
        : base(options) { }

    public DbSet<Room> Rooms => Set<Room>();
}