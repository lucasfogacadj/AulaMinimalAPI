var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/", () => "API de Produtos - Minimal API com .NET 10");

app.MapGet("/status", () => new 
{
    status = "online",
    mensagem = "API Funcionando",
    dataHora = DateTime.Now
});

List<Produto> produtos = new List<Produto>
{
    new Produto { Id = 1, Nome = "Produto A", Preco = 10.99m, Ativo = true },
    new Produto { Id = 2, Nome = "Produto B", Preco = 20.50m, Ativo = true },
    new Produto { Id = 3, Nome = "Produto C", Preco = 15.75m, Ativo = false }
};

app.MapGet("/produtos", () => 
{
    return Results.Ok(produtos);
});

app.MapGet("/produtos/{id:int}", (int id) =>
{
    Produto? produto = null;
    for(int i = 0; i < produtos.Count; i++)
    {
        if(produtos[i].Id == id)
        {
            produto = produtos[i];
        }
    }
    if(produto == null)
    {
        return Results.NotFound();
    }
    else
    {
        return Results.Ok(produto);
    }
} );

app.MapPost("/produtos", (Produto produtoReq) =>
{
   produtos.Add(produtoReq); 
   return Results.Created($"/produtos/{produtoReq.Id}",produtoReq);
});


app.MapPut("/produtos/{id:int}",  (int id, Produto produtoAtualizado) =>
{
    int index = produtos.FindIndex(produto => produto.Id == id);
    if(index == -1)
    {
        return Results.NotFound();
    }
    else
    {
        produtos[index] = produtoAtualizado;
        return Results.Ok(produtos[index]);
    }
});

app.MapDelete("/produtos/{id:int}", (int id) =>
{
    int index = produtos.FindIndex(x => x.Id == id);
    if(index == -1)
    {
        return Results.NotFound();
    }
    else
    {
        produtos.RemoveAt(index);
        return Results.NoContent();
    }
});


app.Run();
