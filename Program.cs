using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<AppDbContext>(options 
=> options.UseSqlite("Data Source=produtos.db"));
var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
}

app.MapGet("/", () => "API de Produtos - Minimal API com .NET 10");

app.MapGet("/status", () => new 
{
    status = "online",
    mensagem = "API Funcionando",
    dataHora = DateTime.Now
});


app.MapGet("/produtos", async (AppDbContext db) => 
{
    var produtos = await db.Produtos.ToListAsync();
    return Results.Ok(produtos);
});

app.MapGet("/produtos/{id:int}",  async(int id, AppDbContext db) =>
{
    var produto = await db.Produtos.FindAsync(id);
    if(produto == null)
        return Results.NotFound();

    return Results.Ok(produto);
} );

app.MapPost("/produtos", async (Produto produtoReq, AppDbContext db) =>
{
   db.Produtos.Add(produtoReq);
   await db.SaveChangesAsync();
   return Results.Created($"/produtos/{produtoReq.Id}",produtoReq);
});


app.MapPut("/produtos/{id:int}", async (int id, Produto produtoAtualizado, AppDbContext db) =>
{
    var produto = await db.Produtos.FindAsync(id);
    if(produto == null)
        return Results.NotFound();

    produto.Nome = produtoAtualizado.Nome;
    produto.Preco = produtoAtualizado.Preco;
    produto.Ativo = produtoAtualizado.Ativo;

    await db.SaveChangesAsync();

    return Results.Ok(produto);
});

app.MapDelete("/produtos/{id:int}", async (int id, AppDbContext db) =>
{
    var produto = await db.Produtos.FindAsync(id);
    if(produto == null)
        return Results.NotFound();

    db.Produtos.Remove(produto);
    await db.SaveChangesAsync();

    return Results.NoContent();
});


app.Run();
