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






app.Run();
