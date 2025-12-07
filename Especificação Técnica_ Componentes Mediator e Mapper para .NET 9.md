# Especificação Técnica: Componentes Mediator e Mapper para .NET 9

**Autor:** Biss Lee
**Data:** 24 de outubro de 2025
**Versão:** 1.0

## 1. Introdução

Este documento detalha a especificação técnica para a criação de componentes **Mediator** e **Mapper** para ecossistemas .NET 9, com foco em alta performance, boas práticas de desenvolvimento, Clean Code e adesão aos princípios SOLID. A solução proposta utilizará **Source Generators** para eliminar o uso de reflexão em tempo de execução, garantindo máxima performance, segurança de tipos e compatibilidade com compilação AOT (Ahead-of-Time) e trimming.

As referências para esta especificação incluem a análise de bibliotecas consagradas como MediatR e AutoMapper, bem como alternativas modernas e performáticas como o Mapperly, que se baseia em source generators [1, 2].

## 2. Princípios de Design

Os componentes serão construídos sobre os seguintes pilares:

| Princípio | Descrição |
| :--- | :--- |
| **Performance** | Utilização de Source Generators para gerar o código de mapeamento e dispatch em tempo de compilação, eliminando o custo da reflexão em tempo de execução. |
| **Segurança de Tipos** | A geração de código em tempo de compilação garante que erros de mapeamento ou de resolução de handlers sejam capturados durante o desenvolvimento, não em produção. |
| **Clean Code & SOLID** | A arquitetura das bibliotecas seguirá os princípios SOLID para garantir um código limpo, manutenível e extensível. |
| **Facilidade de Uso** | APIs intuitivas e configuração mínima, com extensões para injeção de dependência que simplificam a integração com aplicações ASP.NET Core. |
| **Extensibilidade** | O design permitirá a extensão através de comportamentos de pipeline (Mediator) e conversores customizados (Mapper) sem alterar o núcleo da biblioteca. |




## 3. Componente Mediator

O componente Mediator facilitará a comunicação desacoplada entre componentes de software, implementando o padrão Mediator e CQRS (Command Query Responsibility Segregation).

### 3.1. Abstrações (`YourProject.Mediator.Abstractions`)

Este pacote conterá todas as interfaces e contratos necessários para utilizar o Mediator.

#### 3.1.1. Mensagens

As mensagens são o coração do Mediator, representando tanto comandos (operações de escrita) quanto queries (operações de leitura).

- **`IRequest<TResponse>`**: Uma interface marcadora para qualquer requisição que retorna um valor. A resposta será encapsulada em um tipo `Result<TResponse>` para prover um tratamento de erros robusto.

  ```csharp
  namespace YourProject.Mediator.Abstractions;

  public interface IRequest<TResponse> { }
  ```

- **`ICommand` / `ICommand<TResponse>`**: Interfaces específicas para comandos. `ICommand` representa uma operação sem retorno de valor (fire-and-forget), enquanto `ICommand<TResponse>` retorna um resultado.

- **`IQuery<TResponse>`**: Interface para queries, que sempre devem retornar um valor.

#### 3.1.2. Handlers

Handlers são responsáveis por processar as mensagens.

- **`IRequestHandler<TRequest, TResponse>`**: Interface para handlers que processam uma `IRequest<TResponse>` e retornam um `Result<TResponse>`.

  ```csharp
  namespace YourProject.Mediator.Abstractions;

  public interface IRequestHandler<in TRequest, TResponse> where TRequest : IRequest<TResponse>
  {
      Task<Result<TResponse>> Handle(TRequest request, CancellationToken cancellationToken);
  }
  ```

- **`ICommandHandler<TCommand>` / `ICommandHandler<TCommand, TResponse>`**: Interfaces específicas para handlers de comandos.

- **`IQueryHandler<TQuery, TResponse>`**: Interface específica para handlers de queries.

#### 3.1.3. Notificações

Para cenários de "fire-and-forget", onde múltiplos handlers podem reagir a um evento.

- **`INotification`**: Interface marcadora para notificações.

  ```csharp
  namespace YourProject.Mediator.Abstractions;

  public interface INotification { }
  ```

- **`INotificationHandler<TNotification>`**: Interface para handlers de notificações.

  ```csharp
  namespace YourProject.Mediator.Abstractions;

  public interface INotificationHandler<in TNotification> where TNotification : INotification
  {
      Task Handle(TNotification notification, CancellationToken cancellationToken);
  }
  ```

#### 3.1.4. Pipeline

- **`IPipelineBehavior<TRequest, TResponse>`**: Define um comportamento no pipeline de execução de uma requisição, permitindo a implementação de cross-cutting concerns.

  ```csharp
  namespace YourProject.Mediator.Abstractions;

  public interface IPipelineBehavior<in TRequest, TResponse>
      where TRequest : IRequest<TResponse>
  {
      Task<Result<TResponse>> Handle(TRequest request, Func<Task<Result<TResponse>>> next, CancellationToken cancellationToken);
  }
  ```

#### 3.1.5. Mediator Principal

- **`IMediator`**: A interface principal para enviar requisições e publicar notificações.

  ```csharp
  namespace YourProject.Mediator.Abstractions;

  public interface IMediator
  {
      Task<Result<TResponse>> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default);
      Task Publish(INotification notification, CancellationToken cancellationToken = default);
  }
  ```

### 3.2. Implementação (`YourProject.Mediator`)

Esta será a implementação principal, utilizando um **Source Generator** para o trabalho pesado.

#### 3.2.1. Funcionamento do Source Generator

1.  **Detecção**: O gerador irá escanear os assemblies do projeto em tempo de compilação em busca de tipos que implementem `IRequestHandler<,>` e `INotificationHandler<>`.
2.  **Geração do Dispatcher**: Com base nos handlers encontrados, o gerador criará uma classe `Mediator` concreta e `partial` com um método `Send` e `Publish` altamente otimizado. Este método conterá um `switch` statement ou um dicionário que mapeia diretamente o tipo da mensagem para a instância do seu handler, sem usar reflexão.
3.  **Injeção de Dependência**: O gerador também produzirá um método de extensão para `IServiceCollection` (`AddYourMediator`) que registrará automaticamente todos os handlers e pipeline behaviors encontrados.

#### 3.2.2. Exemplo de Código Gerado (Simplificado)

```csharp
// Código gerado em tempo de compilação
public partial class Mediator : IMediator
{
    private readonly IServiceProvider _serviceProvider;

    public Mediator(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public Task<Result<TResponse>> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken)
    {
        return request switch
        {
            CreateProductCommand cmd => (_serviceProvider.GetRequiredService<IRequestHandler<CreateProductCommand, ProductId>>()).Handle(cmd, cancellationToken),
            GetProductQuery qry => (_serviceProvider.GetRequiredService<IRequestHandler<GetProductQuery, ProductDto>>()).Handle(qry, cancellationToken),
            _ => throw new InvalidOperationException("Handler não encontrado para o request.")
        };
    }
    // ... Lógica para Publish e Pipeline ...
}
```



## 4. Componente Mapper

O componente Mapper será responsável por mapear objetos de um tipo para outro, com foco em performance e facilidade de uso, inspirado no **Mapperly** [2].

### 4.1. Abstrações (`YourProject.Mapper.Abstractions`)

Este pacote será mínimo, pois a maior parte da "mágica" acontece em tempo de compilação.

- **`[Mapper]` Attribute**: Um atributo para marcar uma classe `partial` como um mapper.

  ```csharp
  namespace YourProject.Mapper.Abstractions;

  [AttributeUsage(AttributeTargets.Class)]
  public sealed class MapperAttribute : Attribute { }
  ```

- **`[MapProperty]` Attribute**: (Opcional) Para customizar o mapeamento entre propriedades com nomes diferentes.

### 4.2. Implementação (`YourProject.Mapper`)

O núcleo do Mapper será um **Source Generator**.

#### 4.2.1. Funcionamento do Source Generator

1.  **Detecção**: O gerador buscará por classes `partial` marcadas com o atributo `[Mapper]`.
2.  **Análise**: Para cada método de mapeamento abstrato (`partial`) definido pelo usuário na classe, o gerador analisará os tipos de origem e destino.
3.  **Geração de Código**: O gerador criará a implementação `partial` do método, contendo o código de mapeamento direto, propriedade por propriedade. Isso elimina completamente a necessidade de reflexão.
4.  **Convenções**: O mapeamento seguirá convenções, como mapear propriedades com o mesmo nome e realizar o "flattening" (ex: `source.Customer.Name` para `destination.CustomerName`).
5.  **Diagnósticos**: O gerador emitirá warnings de compilação para propriedades no tipo de destino que não foram mapeadas, forçando o desenvolvedor a tomar uma ação (mapear ou ignorar explicitamente).

#### 4.2.2. Exemplo de Uso e Código Gerado

**Código do Desenvolvedor:**

```csharp
// Definição dos modelos
public class User { public int Id { get; set; } public string FullName { get; set; } }
public class UserDto { public int Id { get; set; } public string FullName { get; set; } }

// Definição do Mapper
[Mapper]
public partial class UserMapper
{
    public partial UserDto ToDto(User user);
}
```

**Código Gerado pelo Source Generator:**

```csharp
// Código gerado em tempo de compilação
public partial class UserMapper
{
    public partial UserDto ToDto(User user)
    {
        if (user == null) throw new ArgumentNullException(nameof(user));
        
        var target = new UserDto
        {
            Id = user.Id,
            FullName = user.FullName,
        };
        
        return target;
    }
}
```

### 4.3. Extensões para Injeção de Dependência

Um método de extensão `AddMappers` será fornecido para registrar todos os mappers gerados no container de DI como singletons.

```csharp
// YourProject.Mapper.Extensions.DependencyInjection
public static IServiceCollection AddMappers(this IServiceCollection services, Assembly assembly)
{
    // O Source Generator pode gerar o corpo deste método para registrar os mappers
    services.AddSingleton<UserMapper>();
    // ... outros mappers
    return services;
}
```


## 5. Aplicação dos Princípios SOLID

A tabela abaixo detalha como os princípios SOLID serão aplicados no design dos componentes.

| Princípio | Aplicação no Mediator | Aplicação no Mapper |
| :--- | :--- | :--- |
| **Single Responsibility** | Cada handler tem a responsabilidade única de processar um tipo de mensagem. O Mediator apenas despacha a mensagem. | Cada classe de mapper é responsável por um conjunto coeso de mapeamentos (ex: `UserMapper`). Conversores customizados lidam com lógicas de conversão específicas. |
| **Open/Closed** | O sistema é aberto à extensão (adicionando novos handlers e behaviors) e fechado para modificação (o núcleo do Mediator não muda). | É possível adicionar novos mapeamentos e conversores sem alterar a engine de mapeamento. |
| **Liskov Substitution** | As implementações concretas dos handlers e behaviors podem ser substituídas por suas interfaces sem quebrar o sistema. | - |
| **Interface Segregation** | Interfaces granulares como `IRequest`, `ICommand`, `IQuery`, `INotification` garantem que as classes implementem apenas o que precisam. | Interfaces como `ITypeConverter` são pequenas e focadas. A API principal é simples (`IMapper`). |
| **Dependency Inversion** | O código da aplicação depende de abstrações (`IMediator`, `IMapper`), não das implementações concretas. A injeção de dependência é usada extensivamente. | O código da aplicação depende da abstração do método de mapeamento, cuja implementação é gerada e injetada. |

## 6. Estrutura dos Pacotes NuGet

A solução será distribuída nos seguintes pacotes NuGet:

| Pacote | Dependências | Descrição |
| :--- | :--- | :--- |
| `YourProject.Mediator.Abstractions` | - | Contém as interfaces (`IMediator`, `IRequest`, `IRequestHandler`, etc). |
| `YourProject.Mediator` | `YourProject.Mediator.Abstractions` | Implementação principal com o Source Generator. |
| `YourProject.Mediator.Extensions.DependencyInjection` | `YourProject.Mediator` | Métodos de extensão para fácil registro no `IServiceCollection`. |
| `YourProject.Mapper.Abstractions` | - | Contém os atributos (`[Mapper]`, etc). |
| `YourProject.Mapper` | `YourProject.Mapper.Abstractions` | Implementação principal com o Source Generator. |
| `YourProject.Mapper.Extensions.DependencyInjection` | `YourProject.Mapper` | Métodos de extensão para registro dos mappers no `IServiceCollection`. |

## 7. Referências

[1] J. Bogard, "AutoMapper Usage Guidelines", _Jimmy Bogard's Blog_, 2019. [Online]. Disponível: https://www.jimmybogard.com/automapper-usage-guidelines/.

[2] Riok, "Mapperly - A .NET source generator for generating object mappings", _Mapperly Docs_. [Online]. Disponível: https://mapperly.riok.app/.

