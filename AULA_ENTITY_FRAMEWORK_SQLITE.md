# Aula: Primeiros passos com Entity Framework Core e SQLite

## Ideia da aula

Na aula passada a API guardava os produtos em uma lista:

```csharp
List<Produto> produtos = new List<Produto>();
```

Isso foi bom para aprender rotas, GET, POST, PUT e DELETE. Mas a lista vive apenas na memoria. Quando a API para, os dados somem.

Nesta aula, a ideia e trocar a lista por um banco SQLite usando Entity Framework Core.

A progressao sera:

1. Antes: `List<Produto> produtos`
2. Agora: `db.Produtos`
3. Antes: `produtos.Add(produto)`
4. Agora: `db.Produtos.Add(produto)` e `db.SaveChangesAsync()`

## Objetivo

Ao final da aula, o aluno deve entender:

- O que e persistencia de dados.
- Por que uma lista em memoria nao resolve tudo.
- O que e o Entity Framework Core.
- O que e um `DbContext`.
- Como gravar dados em SQLite.
- Como adaptar os endpoints ja conhecidos para usar banco.

## 1. O problema da lista em memoria

Na primeira versao da API, os produtos ficavam assim:

```csharp
List<Produto> produtos = new List<Produto>
{
    new Produto { Id = 1, Nome = "Produto A", Preco = 10.99m, Ativo = true },
    new Produto { Id = 2, Nome = "Produto B", Preco = 20.50m, Ativo = true },
    new Produto { Id = 3, Nome = "Produto C", Preco = 15.75m, Ativo = false }
};
```

Ela funcionava, mas tinha uma limitacao:

- Criou produto.
- Parou a API.
- Rodou de novo.
- O produto criado desapareceu.

Isso acontece porque a lista fica na memoria do programa.

## 2. O que vamos usar agora

Vamos usar:

- `Produto`: a mesma classe da aula passada.
- `AppDbContext`: classe que representa o banco.
- SQLite: banco simples em arquivo.
- EF Core: biblioteca que liga as classes C# ao banco.

O banco sera criado em um arquivo chamado:

```text
produtos.db
```

## 3. Pacote instalado

Para usar EF Core com SQLite, foi instalado:

```bash
dotnet add package Microsoft.EntityFrameworkCore.Sqlite
```

Esse pacote permite usar:

```csharp
options.UseSqlite("Data Source=produtos.db")
```

## 4. A entidade continua simples

Arquivo: `Produto.cs`

```csharp
public class Produto
{
    public int Id { get; set; }
    public string Nome { get; set; } = "";
    public decimal Preco { get; set; }
    public bool Ativo { get; set; }
}
```

Por convencao, o EF Core entende que:

- `Id` e a chave primaria.
- `Nome`, `Preco` e `Ativo` viram colunas.
- `Produto` vira uma tabela controlada pelo `DbSet<Produto>`.

## 5. Criando o contexto do banco

Arquivo: `AppDbContext.cs`

```csharp
using Microsoft.EntityFrameworkCore;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Produto> Produtos => Set<Produto>();
}
```

Pense no `AppDbContext` como a ponte entre a API e o banco.

A linha mais importante para esta aula e:

```csharp
public DbSet<Produto> Produtos => Set<Produto>();
```

Ela representa a tabela de produtos.

## 6. Configurando o SQLite no Program.cs

No `Program.cs`, antes de criar o `app`, registramos o banco:

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=produtos.db"));

var app = builder.Build();
```

Com isso, as rotas podem receber `AppDbContext db` como parametro.

Exemplo:

```csharp
app.MapGet("/produtos", async (AppDbContext db) =>
{
    var produtos = await db.Produtos.ToListAsync();
    return Results.Ok(produtos);
});
```

## 7. Criando o banco automaticamente

Para deixar a aula mais simples, usamos:

```csharp
db.Database.EnsureCreated();
```

Essa linha cria o banco e as tabelas se ainda nao existirem.

Tambem adicionamos produtos iniciais se a tabela estiver vazia:

```csharp
if (!db.Produtos.Any())
{
    db.Produtos.AddRange(
        new Produto { Nome = "Produto A", Preco = 10.99m, Ativo = true },
        new Produto { Nome = "Produto B", Preco = 20.50m, Ativo = true },
        new Produto { Nome = "Produto C", Preco = 15.75m, Ativo = false }
    );

    db.SaveChanges();
}
```

Aqui ainda estamos bem perto da lista da aula passada. A diferenca e que agora os dados vao para o SQLite.

## 8. Comparando antes e depois

### Listar produtos

Antes:

```csharp
return Results.Ok(produtos);
```

Agora:

```csharp
var produtos = await db.Produtos.ToListAsync();
return Results.Ok(produtos);
```

### Buscar produto por id

Antes:

```csharp
Produto? produto = produtos.Find(produto => produto.Id == id);
```

Agora:

```csharp
var produto = await db.Produtos.FindAsync(id);
```

### Criar produto

Antes:

```csharp
produtos.Add(produto);
```

Agora:

```csharp
db.Produtos.Add(produto);
await db.SaveChangesAsync();
```

O `Add` prepara a inclusao. O `SaveChangesAsync` grava no banco.

### Atualizar produto

Antes, procuravamos a posicao na lista e trocavamos o objeto.

Agora, buscamos o produto no banco:

```csharp
var produto = await db.Produtos.FindAsync(id);
```

Depois alteramos os campos:

```csharp
produto.Nome = produtoAtualizado.Nome;
produto.Preco = produtoAtualizado.Preco;
produto.Ativo = produtoAtualizado.Ativo;
```

E gravamos:

```csharp
await db.SaveChangesAsync();
```

### Remover produto

Antes:

```csharp
produtos.RemoveAt(index);
```

Agora:

```csharp
db.Produtos.Remove(produto);
await db.SaveChangesAsync();
```

## 9. Rotas da API

A API continua com as mesmas rotas principais:

```text
GET    /produtos
GET    /produtos/{id}
POST   /produtos
PUT    /produtos/{id}
DELETE /produtos/{id}
```

Isso ajuda o aluno a perceber que o contrato da API quase nao mudou. O que mudou foi onde os dados ficam guardados.

## 10. Como testar

Rode a API:

```bash
dotnet run
```

Liste os produtos:

```http
GET http://localhost:5286/produtos
```

Crie um produto:

```http
POST http://localhost:5286/produtos
Content-Type: application/json

{
  "nome": "Teclado",
  "preco": 150.00,
  "ativo": true
}
```

Agora faca o experimento principal:

1. Crie um produto.
2. Pare a API.
3. Rode a API novamente.
4. Liste os produtos.

Se o produto continuar aparecendo, a persistencia funcionou.

## 11. Roteiro sugerido para explicar em sala

1. Abra o projeto da aula passada.
2. Mostre a lista em memoria.
3. Explique que a lista perde dados quando a API para.
4. Instale o pacote do SQLite.
5. Crie o `AppDbContext`.
6. Configure `AddDbContext` no `Program.cs`.
7. Troque o GET de lista primeiro.
8. Depois troque GET por id.
9. Depois POST.
10. Depois PUT e DELETE.
11. Teste parando e rodando a API.

## 12. O que fica para a proxima aula

Nesta primeira aula, deixamos de fora alguns conceitos para nao pesar a explicacao:

- DTOs, como `ProdutoRequest`.
- Validacoes mais completas.
- `OnModelCreating`.
- Migrations.
- Relacionamentos entre tabelas.
- Separacao em pastas como `Models`, `Data` e `Controllers`.

Depois que o aluno entender a troca de `List<Produto>` para `db.Produtos`, esses conceitos entram com muito mais naturalidade.

## 13. Exercicios

1. Criar uma rota `GET /produtos/ativos`.
2. Criar uma rota `GET /produtos/inativos`.
3. Criar uma rota `GET /produtos/busca/{nome}`.
4. Adicionar uma validacao simples para nao aceitar produto sem nome.
5. Adicionar uma validacao simples para nao aceitar preco menor ou igual a zero.
6. Apagar o arquivo `produtos.db`, rodar a API novamente e observar o banco sendo recriado.

## 14. Referencias oficiais

- EF Core SQLite Provider: https://learn.microsoft.com/ef/core/providers/sqlite/
- EF Core DbContext: https://learn.microsoft.com/ef/core/dbcontext-configuration/
